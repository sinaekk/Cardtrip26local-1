using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json.Linq;

[RequireComponent(typeof(AudioSource))]
public class GeminiLive : MonoBehaviour
{
    // =========================
    // 🔹 설정 (Configuration)
    // =========================
    [Header("API Key")]
    [Tooltip("Google AI Studio에서 발급받은 API 키를 입력하세요.")]
    public string apiKey = "YOUR_API_KEY"; // ◀️ 여기에 API 키를 반드시 입력하세요!

    [Header("Audio")]
    [Tooltip("Gemini의 음성 응답을 재생할 AudioSource 컴포넌트입니다.")]
    public AudioSource audioSource;

    // Gemini API 및 오디오 관련 상수
    private const string MODEL = "models/gemini-2.5-flash-preview-native-audio-dialog";
    private const string WS_URL = "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent";
    private const int SAMPLE_RATE_IN = 16000;
    private const int SAMPLE_RATE_OUT = 24000;

    // =========================
    // 🔹 내부 변수 (Internal State)
    // =========================
    private ClientWebSocket ws;
    private CancellationTokenSource cts;
    private Task receiveLoopTask;

    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    private AudioClip micClip;
    private string micDevice;
    private bool isRecording = false;
    private int lastMicSamplePos = 0;
    private Coroutine streamMicrophoneCoroutine;

    // =========================
    // 🔹 Unity 생명주기 (Lifecycle Methods)
    // =========================
    async void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        await ConnectToGemini();
    }

    void Update()
    {
        while (mainThreadActions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    private async void OnDestroy()
    {
        Debug.Log("스크립트가 파괴되어 연결을 종료합니다.");
        await StopMicAndDisconnect();
    }

    // =========================
    // 🔹 마이크 제어 (Microphone Control)
    // =========================
    [ContextMenu("Start Mic")]
    public void StartMic()
    {
        // if (isRecording)
        // {
        //     Debug.LogWarning("이미 녹음이 진행 중입니다.");
        //     return;
        // }

        // if (Microphone.devices.Length == 0)
        // {
        //     Debug.LogError("사용 가능한 마이크가 없습니다!");
        //     return;
        // }

        // micDevice = Microphone.devices[0];
        // micClip = Microphone.Start(micDevice, true, 1, SAMPLE_RATE_IN);
        // isRecording = true;
        // lastMicSamplePos = 0;

        // streamMicrophoneCoroutine = StartCoroutine(StreamMicrophone());

        // Debug.Log($"🎙️ '{micDevice}' 마이크 녹음을 시작합니다.");
    }

    [ContextMenu("Stop Mic")]
    public void StopMic()
    {
        // if (!isRecording) return;
        // Microphone.End(micDevice);
        // isRecording = false;
        // micClip = null;
        // if (streamMicrophoneCoroutine != null)
        // {
        //     StopCoroutine(streamMicrophoneCoroutine);
        //     streamMicrophoneCoroutine = null;
        // }
        // Debug.Log("🛑 마이크 녹음을 중지합니다.");
    }

    // =========================
    // 🔹 핵심 로직 (Core Logic)
    // =========================
    private async Task ConnectToGemini()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY")
        {
            Debug.LogError("API 키가 설정되지 않았습니다! Inspector 창에서 API 키를 입력해주세요.");
            return;
        }

        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();
        ws.Options.SetRequestHeader("x-goog-api-key", apiKey);

        Debug.Log("Gemini 서버에 연결을 시도합니다...");

        try
        {
            await ws.ConnectAsync(new Uri(WS_URL), cts.Token);
            if (ws.State == WebSocketState.Open)
            {
                Debug.Log("✅ Gemini 서버에 성공적으로 연결되었습니다!");
                receiveLoopTask = Task.Run(ReceiveLoop);
                await InitSession();
            }
            else
            {
                Debug.LogError($"연결 실패. 상태: {ws.State}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ 연결 중 오류 발생: {ex.Message}");
        }
    }

    private async Task InitSession()
    {
        string json = $@"
        {{
          ""setup"": {{
            ""model"": ""{MODEL}"",
            ""generationConfig"": {{
              ""responseModalities"": [""AUDIO""]
            }}
          }}
        }}";
        Debug.Log("➡️ 세션 초기화 요청: " + json);
        await SendRaw(json);
    }

    // 🔹 새롭게 추가된 디버그용 텍스트 전송 메서드
    [ContextMenu("Send Test Text")]
    public async Task SendDebugText()
    {
        string testText = "안녕하세요, 제미나이. 테스트를 위해 텍스트를 보냅니다.";
        string json = $@"
        {{
          ""realtimeInput"": {{
            ""text"": ""{testText}""
          }}
        }}";

        Debug.Log("➡️ 디버그 텍스트 전송: " + testText);
        await SendRaw(json);
    }

    private System.Collections.IEnumerator StreamMicrophone()
    {
        // while (isRecording)
        // {
        //     int currentPos = Microphone.GetPosition(micDevice);
        //     if (currentPos != lastMicSamplePos)
        //     {
        //         int samplesToRead = (micClip.samples + currentPos - lastMicSamplePos) % micClip.samples;
        //         if (samplesToRead > 0)
        //         {
        //             float[] sampleData = new float[samplesToRead];
        //             micClip.GetData(sampleData, lastMicSamplePos);
        //             byte[] pcmData = FloatToPCM16(sampleData);
        //             _ = SendAudio(pcmData);
        //             lastMicSamplePos = currentPos;
        //         }
        //     }
        //     yield return new WaitForSeconds(0.05f);
        // }
        yield return null;
    }

    private async Task SendAudio(byte[] audioBytes)
    {
        if (ws?.State != WebSocketState.Open || audioBytes.Length == 0) return;

        var request = new JObject
        {
            ["realtimeInput"] = new JObject
            {
                ["audio"] = new JObject { ["data"] = Convert.ToBase64String(audioBytes) }
            }
        };
        await SendRaw(request.ToString());
    }

    // =========================
    // 🔹 네트워크 통신 (Networking)
    // =========================
    private async Task ReceiveLoop()
    {
        var buffer = new ArraySegment<byte>(new byte[8192]);
        try
        {
            while (ws.State == WebSocketState.Open && !cts.IsCancellationRequested)
            {
                var result = await ws.ReceiveAsync(buffer, cts.Token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    string closeReason = $"🔌 서버가 연결을 종료했습니다. 상태: {result.CloseStatus}, 설명: {result.CloseStatusDescription}";
                    mainThreadActions.Enqueue(() => Debug.LogWarning(closeReason));
                    break;
                }
                if (result.Count > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(buffer.Array, buffer.Offset, result.Count);
                        var message = Encoding.UTF8.GetString(ms.ToArray());
                        mainThreadActions.Enqueue(() => HandleMessage(message));
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            mainThreadActions.Enqueue(() => Debug.LogError($"⚠️ 수신 루프 오류: {ex.Message}"));
        }
        finally
        {
            mainThreadActions.Enqueue(() => Debug.Log("수신 루프가 종료되었습니다."));
        }
    }

    private void HandleMessage(string msg)
    {
        try
        {
            var root = JObject.Parse(msg);
            var audioPart = root["serverContent"]?["modelTurn"]?["parts"]?[0]?["inlineData"];
            if (audioPart != null)
            {
                string b64 = audioPart["data"]?.ToString();
                if (!string.IsNullOrEmpty(b64))
                {
                    byte[] audioData = Convert.FromBase64String(b64);
                    float[] samples = PCM16ToFloat(audioData);
                    var clip = AudioClip.Create("GeminiAudio", samples.Length, 1, SAMPLE_RATE_OUT, false);
                    clip.SetData(samples, 0);
                    audioSource.PlayOneShot(clip);
                    Debug.Log($"🎧 Gemini 오디오 재생 ({clip.length:F2}s)");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"⚠️ 메시지 처리 중 오류: {ex.Message}");
        }
    }

    private Task SendRaw(string json)
    {
        if (ws?.State != WebSocketState.Open) return Task.CompletedTask;
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        return ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
    }

    private async Task StopMicAndDisconnect()
    {
        StopMic();
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
        if (ws != null)
        {
            if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
            {
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client is disconnecting", CancellationToken.None);
            }
            ws.Dispose();
            ws = null;
        }
        if (receiveLoopTask != null && !receiveLoopTask.IsCompleted)
        {
            await receiveLoopTask;
        }
    }

    // =========================
    // 🔹 오디오 유틸리티 (Audio Utilities)
    // =========================
    private float[] PCM16ToFloat(byte[] bytes)
    {
        int count = bytes.Length / 2;
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            short val = BitConverter.ToInt16(bytes, i * 2);
            samples[i] = val / 32768f;
        }
        return samples;
    }

    private byte[] FloatToPCM16(float[] samples)
    {
        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; i++)
        {
            short val = (short)(Mathf.Clamp(samples[i], -1f, 1f) * 32767);
            var sampleBytes = BitConverter.GetBytes(val);
            bytes[i * 2] = sampleBytes[0];
            bytes[i * 2 + 1] = sampleBytes[1];
        }
        return bytes;
    }
}