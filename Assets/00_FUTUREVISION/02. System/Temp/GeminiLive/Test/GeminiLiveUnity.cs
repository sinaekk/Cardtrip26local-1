using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity ↔ Gemini Live 실시간 음성 대화 샘플
/// - 마이크(16kHz, PCM16) → WebSocket JSON(Base64)로 전송
/// - 응답 오디오(24kHz, PCM16) 수신 후 AudioSource 재생
/// - 응답 텍스트 자막 수신 시 UI 갱신
/// - VAD(무음 감지) 또는 Space 키로 발화 단위 전송
/// </summary>
public class GeminiLiveUnity : MonoBehaviour
{
    [Header("Gemini Live API")]
    [Tooltip("Google AI Studio API Key")]
    public string apiKey = "YOUR_API_KEY";

    [Tooltip("Gemini Live WebSocket URL (v1beta). 예: wss://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-exp:streamGenerateContent?key={API_KEY}")]
    public string webSocketUrl =
        "wss://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-exp:streamGenerateContent?key={API_KEY}";

    [Tooltip("모델 이름 (서버 setup 메시지에 포함)")]
    public string modelName = "gemini-2.5-flash-exp";

    [Tooltip("출력 음성 언어 코드 (예: ko-KR, en-US)")]
    public string languageCode = "ko-KR";

    [Tooltip("프리빌트 보이스 이름 (환경/리전/권한에 따라 상이). 빈 값이면 기본 보이스")]
    public string voiceName = "";

    [Header("Mic Settings")]
    [Tooltip("입력 샘플레이트 (Gemini Live 권장 16000)")]
    public int micSampleRate = 16000;

    [Tooltip("마이크 디바이스 (null = 기본)")]
    public string microphoneDevice = null;

    [Tooltip("마이크 버퍼(초). loop=true로 계속 녹음")]
    public int micClipSeconds = 5;

    [Header("VAD (Voice Activity Detection)")]
    [Tooltip("무음 감지 임계값(0~1)")]
    public float silenceThreshold = 0.01f;

    [Tooltip("무음으로 간주할 지속 시간(초)")]
    public float silenceDuration = 0.6f;

    [Tooltip("자동 VAD 사용 (true면 자동으로 구간 전송, false면 Space 키로 수동)")]
    public bool useAutoVAD = true;

    [Header("Output / UI")]
    public AudioSource audioSource;
    public Text transcriptText;

    [Header("Debug")]
    public bool logIncomingJson = false;

    // Internal fields
    private ClientWebSocket ws;
    private CancellationTokenSource cts;
    private AudioClip micClip;
    private int lastMicSample = 0;

    // Outgoing mic PCM buffer (16kHz / PCM16 LE)
    private List<byte> micPcmBuffer = new List<byte>();

    // Incoming response audio buffer (24kHz / PCM16 LE)
    private List<byte> responsePcm24k = new List<byte>();
    private string lastTranscript = "";

    // VAD state
    private bool isSpeaking = false;
    private float silenceTimer = 0f;

    // Turn state
    private bool awaitingResponse = false;

    // Regex helpers (flexible field names)
    // "audio": {"data": "BASE64..."}  OR  possibly nested inside serverContent.data[]
    private static readonly Regex rxAudioBase64 = new Regex("\"audio\"\\s*:\\s*\\{[^}]*?\"data\"\\s*:\\s*\"([^\"]+)\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    // "outputTranscription": {"text":"..."} or "output_transcription": {"text":"..."}
    private static readonly Regex rxTranscript = new Regex("(?:outputTranscription|output_transcription)\"?\\s*:\\s*\\{[^}]*?\"text\"\\s*:\\s*\"([^\"]*)\"",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    // turnComplete true-ish
    private static readonly Regex rxTurnComplete = new Regex("(?:turnComplete|turn_complete)\"?\\s*:\\s*(true)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    // Sample rate for model output audio (Gemini Live commonly 24000 Hz)
    private const int MODEL_OUTPUT_RATE = 24000;

    // For marshaling back to main thread
    private readonly Queue<Action> mainThreadQueue = new Queue<Action>();

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private async void Start()
    {
        // Connect WebSocket
        cts = new CancellationTokenSource();

        string url = webSocketUrl.Replace("{API_KEY}", Uri.EscapeDataString(apiKey ?? ""));
        ws = new ClientWebSocket();

        try
        {
            Debug.Log($"🔌 Connecting WS: {url}");
            await ws.ConnectAsync(new Uri(url), cts.Token);
            Debug.Log("✅ WS Connected");

            // Send setup message (choose AUDIO output + transcriptions)
            string setupJson = BuildSetupMessage(modelName, languageCode, voiceName);
            await SendTextAsync(setupJson);

            // Start receive loop
            _ = Task.Run(ReceiveLoop, cts.Token);

            // Start mic
            StartMic();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ WS connect error: {ex.Message}");
        }
    }

    private void Update()
    {
        // // UI/main-thread ops
        // while (mainThreadQueue.Count > 0)
        // {
        //     var a = mainThreadQueue.Dequeue();
        //     try { a?.Invoke(); } catch { }
        // }

        // if (micClip == null) return;

        // // Pull new mic samples
        // int pos = Microphone.GetPosition(microphoneDevice);
        // if (pos < 0) return;

        // if (pos < lastMicSample)
        //     lastMicSample = 0;

        // int available = pos - lastMicSample;
        // if (available > 0)
        // {
        //     float[] samples = new float[available];
        //     micClip.GetData(samples, lastMicSample);
        //     lastMicSample = pos;

        //     // VAD energy
        //     float avg = 0f;
        //     for (int i = 0; i < samples.Length; i++)
        //         avg += Mathf.Abs(samples[i]);
        //     avg /= Mathf.Max(1, samples.Length);

        //     // Convert to PCM16 mono LE and accumulate
        //     AppendFloatToPcm16(samples, micPcmBuffer);

        //     if (useAutoVAD && !awaitingResponse)
        //     {
        //         if (avg > silenceThreshold)
        //         {
        //             isSpeaking = true;
        //             silenceTimer = 0f;
        //         }
        //         else if (isSpeaking)
        //         {
        //             silenceTimer += Time.deltaTime;
        //             // if silence sustained → commit utterance
        //             if (silenceTimer >= silenceDuration)
        //             {
        //                 isSpeaking = false;
        //                 silenceTimer = 0f;
        //                 CommitMicSpeechAndSend();
        //             }
        //         }
        //     }
        // }

        // // Manual PTT (Space): press to start speaking, release to send
        // if (!useAutoVAD && !awaitingResponse)
        // {
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         Debug.Log("🎤 PTT start");
        //         micPcmBuffer.Clear();
        //     }
        //     if (Input.GetKeyUp(KeyCode.Space))
        //     {
        //         Debug.Log("🛑 PTT stop → send");
        //         CommitMicSpeechAndSend();
        //     }
        // }
    }

    private void OnDestroy()
    {
        // try
        // {
        //     cts?.Cancel();
        // }
        // catch { }

        // if (Microphone.IsRecording(microphoneDevice))
        //     Microphone.End(microphoneDevice);

        // if (ws != null && ws.State == WebSocketState.Open)
        // {
        //     try
        //     {
        //         ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None).Wait(200);
        //     }
        //     catch { }
        // }
        // ws?.Dispose();
    }

    // ───────────────────────────── Mic & Encoding ─────────────────────────────

    private void StartMic()
    {
        // if (Microphone.IsRecording(microphoneDevice))
        //     Microphone.End(microphoneDevice);

        // micClip = Microphone.Start(microphoneDevice, true, micClipSeconds, micSampleRate);
        // lastMicSample = 0;
        // Debug.Log($"🎙️ Mic started: {microphoneDevice ?? "Default"} @ {micSampleRate}Hz");
    }

    private static void AppendFloatToPcm16(float[] src, List<byte> dst)
    {
        // mono, float -1..1 → PCM16 LE
        for (int i = 0; i < src.Length; i++)
        {
            float f = Mathf.Clamp(src[i], -1f, 1f);
            short s = (short)Mathf.RoundToInt(f * short.MaxValue);
            dst.Add((byte)(s & 0xFF));
            dst.Add((byte)((s >> 8) & 0xFF));
        }
    }

    private static float[] Pcm16ToFloats(byte[] pcm)
    {
        int n = pcm.Length / 2;
        float[] f = new float[n];
        for (int i = 0; i < n; i++)
        {
            short s = (short)(pcm[2 * i] | (pcm[2 * i + 1] << 8));
            f[i] = s / 32768f;
        }
        return f;
    }

    // ───────────────────────────── Sending ─────────────────────────────

    private async void CommitMicSpeechAndSend()
    {
        if (micPcmBuffer.Count == 0 || ws == null || ws.State != WebSocketState.Open) return;

        try
        {
            string base64 = Convert.ToBase64String(micPcmBuffer.ToArray());
            string json = $@"
{{
  ""realtimeInput"": {{
    ""audio"": {{
      ""data"": ""{base64}"",
      ""mimeType"": ""audio/pcm;rate={micSampleRate}""
    }},
    ""turnComplete"": true
  }}
}}";

            Debug.Log($"➡️ Send mic segment: {micPcmBuffer.Count} bytes PCM16 @ {micSampleRate}Hz");
            await SendTextAsync(json);

            // 준비: 응답 수신 버퍼 초기화
            awaitingResponse = true;
            responsePcm24k.Clear();
            lastTranscript = "";
            UpdateTranscriptLabel("(응답 대기 중…)");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ CommitMicSpeechAndSend error: {ex.Message}");
        }
        finally
        {
            micPcmBuffer.Clear();
        }
    }

    private async Task SendTextAsync(string json)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;

        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
    }

    private string BuildSetupMessage(string model, string langCode, string voice)
    {
        // responseModalities: AUDIO (텍스트도 받고 싶으면 outputAudioTranscription 요청)
        // 일부 환경에서는 필드명이 바뀌거나 schema가 업데이트될 수 있어 유연성 고려
        // voice 설정은 옵션. 빈 값이면 누락.
        string voiceConfig = string.IsNullOrWhiteSpace(voice)
            ? ""
            : $@",""speechConfig"":{{""voiceConfig"":{{""prebuiltVoiceConfig"":{{""voiceName"":""{EscapeJson(voice)}"",""languageCode"":""{EscapeJson(langCode)}""}}}}}}";

        string setup = $@"
{{
  ""setup"": {{
    ""model"": ""{EscapeJson(model)}"",
    ""generationConfig"": {{
      ""responseModalities"": [""AUDIO""],
      ""outputAudioTranscription"": {{}},
      ""inputAudioTranscription"": {{}}
    }}{voiceConfig},
    ""systemInstruction"": {{
      ""text"": ""You are a helpful, concise voice assistant.""
    }}
  }}
}}";
        return setup;
    }

    // ───────────────────────────── Receiving ─────────────────────────────

    private async Task ReceiveLoop()
    {
        var buffer = new byte[8192];

        try
        {
            while (ws != null && ws.State == WebSocketState.Open && !cts.IsCancellationRequested)
            {
                var seg = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult res = await ws.ReceiveAsync(seg, cts.Token);

                if (res.MessageType == WebSocketMessageType.Close)
                {
                    EnqueueMain(() => Debug.Log($"🔌 Server closed WS, {res.CloseStatusDescription}"));
                    break;
                }

                // NOTE: 여기서는 서버가 텍스트 프레임(JSON)으로 준다고 가정
                string msg = Encoding.UTF8.GetString(buffer, 0, res.Count);
                if (logIncomingJson) EnqueueMain(() => Debug.Log($"⬅️ {Truncate(msg, 1000)}"));

                // 1) 오디오 청크 추출
                foreach (Match m in rxAudioBase64.Matches(msg))
                {
                    string b64 = m.Groups[1].Value;
                    if (!string.IsNullOrEmpty(b64))
                    {
                        try
                        {
                            byte[] chunk = Convert.FromBase64String(b64);
                            responsePcm24k.AddRange(chunk);
                        }
                        catch { /* ignore bad chunk */ }
                    }
                }

                // 2) 출력 자막 텍스트 추출
                var tm = rxTranscript.Match(msg);
                if (tm.Success)
                {
                    string txt = UnescapeJsonString(tm.Groups[1].Value);
                    lastTranscript = txt;
                    UpdateTranscriptLabel(txt);
                }

                // 3) 턴 완료 감지
                if (rxTurnComplete.IsMatch(msg))
                {
                    // 메인스레드에서 오디오 재생 처리
                    EnqueueMain(() => OnTurnCompletePlayAudio());
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception ex)
        {
            EnqueueMain(() => Debug.LogError($"❌ ReceiveLoop error: {ex.Message}"));
        }
    }

    private void OnTurnCompletePlayAudio()
    {
        try
        {
            if (responsePcm24k.Count > 0)
            {
                float[] samples = Pcm16ToFloats(responsePcm24k.ToArray());

                // 24kHz 모노 클립 생성
                AudioClip clip = AudioClip.Create("GeminiResponse", samples.Length, 1, MODEL_OUTPUT_RATE, false);
                clip.SetData(samples, 0);
                audioSource.Stop();
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log($"🔊 Play response audio: {samples.Length} samples @ {MODEL_OUTPUT_RATE}Hz");
            }

            if (!string.IsNullOrEmpty(lastTranscript))
            {
                UpdateTranscriptLabel(lastTranscript);
            }
            else
            {
                // 자막이 안 왔으면 간단 표시
                UpdateTranscriptLabel("(음성 응답 재생)");
            }
        }
        finally
        {
            awaitingResponse = false;
            responsePcm24k.Clear();
        }
    }

    // ───────────────────────────── Utils ─────────────────────────────

    private void UpdateTranscriptLabel(string t)
    {
        if (transcriptText == null) return;
        EnqueueMain(() => transcriptText.text = t ?? "");
    }

    private static string EscapeJson(string s)
    {
        return s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";
    }

    private static string UnescapeJsonString(string s)
    {
        // 최소 처리(\" → ", \\ → \)
        return s?.Replace("\\\"", "\"").Replace("\\\\", "\\") ?? "";
    }

    private static string Truncate(string s, int max)
    {
        if (string.IsNullOrEmpty(s) || s.Length <= max) return s;
        return s.Substring(0, max) + "...(trunc)";
    }

    private void EnqueueMain(Action a)
    {
        if (a == null) return;
        lock (mainThreadQueue) mainThreadQueue.Enqueue(a);
    }
}
