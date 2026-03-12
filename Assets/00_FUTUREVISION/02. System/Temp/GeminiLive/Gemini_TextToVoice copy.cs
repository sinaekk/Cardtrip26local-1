using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.WebSockets;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

public class Gemini_TextToVoice_copy : MonoBehaviour
{
    private const string WS_URL =
        "wss://generativelanguage.googleapis.com/ws/google.ai.generativelanguage.v1beta.GenerativeService.BidiGenerateContent";

    private const string MODEL = "models/gemini-2.5-flash-preview-native-audio-dialog";
    private const int DEFAULT_RATE = 24000; // Gemini 오디오 출력은 기본적으로 24kHz

    [Header("API Key")]
    public string apiKey = "YOUR_API_KEY";

    [Header("Audio")]
    public AudioSource audioSource;

    private ClientWebSocket ws;
    private CancellationTokenSource cts;
    private bool setupComplete = false;

    // 🔹 누적 오디오 샘플
    private List<float> accumulatedSamples = new List<float>();

    public UnityEvent OnReceiveError;
    public bool IsError = false;
    public UnityEvent OnReceiveVoiceToText;
    private bool playbackReady = false;

    [Header("Debug")]
    public string Debug_TestText = "안녕하세요. 제미니 음성 변환 기능을 테스트합니다.";

    [ContextMenu("Send Test Text")]
    public void SendTestText()
    {
        if (setupComplete)
            _ = SendText(Debug_TestText);
        else
            Debug.LogWarning("Setup not complete yet.");
    }

    async void OnEnable()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();
        ws.Options.SetRequestHeader("x-goog-api-key", apiKey);

        try
        {
            await ws.ConnectAsync(new Uri(WS_URL), cts.Token);
            Debug.Log("✅ Connected to Gemini Live");

            await SendSetup();
            _ = Task.Run(ReceiveLoop);

            //await Task.Delay(1000);
            //await SendText(Debug_TestText);
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ Connect error: " + ex.Message);
        }
    }

    void Update()
    {
        // turnComplete 이벤트가 오면 누적된 오디오 합쳐서 재생
        if (playbackReady && accumulatedSamples.Count > 0)
        {
            float[] finalSamples = accumulatedSamples.ToArray();
            accumulatedSamples.Clear();
            playbackReady = false;

            var clip = AudioClip.Create("GeminiFullAudio", finalSamples.Length, 1, DEFAULT_RATE, false);
            bool ok = clip.SetData(finalSamples, 0);
            Debug.Log($"🎧 Full AudioClip created: len={clip.length:F2}s, samples={clip.samples}, freq={clip.frequency}, SetData={ok}");

            audioSource.clip = clip;
            audioSource.Play();

            OnReceiveVoiceToText?.Invoke();
        }

        // 에러 발생시
        if (IsError)
        {
            IsError = false;
            OnReceiveError?.Invoke();
        }
    }

    private async Task SendSetup()
    {
        string json = $@"
        {{
          ""setup"": {{
            ""model"": ""{MODEL}"",
            ""generationConfig"": {{
              ""responseModalities"": [""AUDIO""]
            }},
            ""realtimeInputConfig"": {{}}
          }}
        }}";
        Debug.Log("➡️ Sending setup: " + json);
        await SendRaw(json);
    }

    public async Task SendText(string text)
    {
        string json = $@"
        {{
          ""realtimeInput"": {{
            ""text"": ""{text}""
          }}
        }}";
        Debug.Log("➡️ Sending text: " + text);
        await SendRaw(json);
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[32768];
        try
        {
            while (ws.State == WebSocketState.Open)
            {
                var sb = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                string msg = sb.ToString();
                //Debug.Log("⬅️ Received message: " + msg);

                if (msg.Contains("\"setupComplete\""))
                {
                    setupComplete = true;
                    Debug.Log("✅ Setup complete");
                }

                if (msg.Contains("\"inlineData\""))
                {
                    string mimeType;
                    string b64 = ExtractAudioBase64(msg, out mimeType);

                    if (!string.IsNullOrEmpty(b64))
                    {
                        try
                        {
                            byte[] audioData = Convert.FromBase64String(b64);

                            if (mimeType.Contains("pcm"))
                            {
                                float[] samples = PCM16ToFloat(audioData);
                                accumulatedSamples.AddRange(samples);
                                //Debug.Log($"➕ Added {samples.Length} PCM samples (total={accumulatedSamples.Count})");
                            }
                            else if (mimeType.Contains("wav"))
                            {
                                WAV_TextToVoice wav = new WAV_TextToVoice(audioData);
                                accumulatedSamples.AddRange(wav.LeftChannel);
                                //Debug.Log($"➕ Added {wav.SampleCount} WAV samples (total={accumulatedSamples.Count})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("Base64 decode error: " + ex.Message);
                            IsError = true;
                        }
                    }
                }

                // 🔹 응답이 끝났다고 알리는 신호 처리
                if (msg.Contains("\"turnComplete\"") || msg.Contains("\"final\""))
                {
                    playbackReady = true;
                    Debug.Log("✅ Turn complete → will assemble full audio for playback");
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log($"🔌 Closed by server: {result.CloseStatus} {result.CloseStatusDescription}");
                    IsError = true;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("⚠️ ReceiveLoop error: " + ex.Message);
            IsError = true;
        }
    }

    // ===== Utilities =====
    private async Task SendRaw(string json)
    {
        if (ws == null || ws.State != WebSocketState.Open) return;
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
    }

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

    private string ExtractAudioBase64(string msg, out string mimeType)
    {
        mimeType = "audio/pcm";
        try
        {
            var root = JObject.Parse(msg);
            var inlineData = root["serverContent"]?["modelTurn"]?["parts"]?[0]?["inlineData"];
            if (inlineData != null)
            {
                var mt = inlineData["mimeType"]?.ToString();
                if (!string.IsNullOrEmpty(mt))
                    mimeType = mt;

                var data = inlineData["data"]?.ToString();
                if (!string.IsNullOrEmpty(data))
                    return data;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("⚠️ ExtractAudioBase64 parse error: " + ex.Message);
        }

        return null;
    }

    private async void OnDisable()
    {
        if (ws != null)
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            ws.Dispose();
        }
    }
}

/// <summary>
/// 간단한 WAV 파일 파서 (16bit PCM 전용)
/// </summary>
public class WAV_TextToVoice
{
    public float[] LeftChannel { get; private set; }
    public int ChannelCount { get; private set; }
    public int SampleCount { get; private set; }
    public int Frequency { get; private set; }

    public WAV_TextToVoice(byte[] wav)
    {
        ChannelCount = BitConverter.ToInt16(wav, 22);
        Frequency = BitConverter.ToInt32(wav, 24);

        int pos = 12;
        while (!(wav[pos] == 100 && wav[pos + 1] == 97 &&
                 wav[pos + 2] == 116 && wav[pos + 3] == 97)) pos += 4;
        pos += 8;

        SampleCount = (wav.Length - pos) / 2;
        LeftChannel = new float[SampleCount];

        int i = 0;
        while (pos < wav.Length)
        {
            short sample = BitConverter.ToInt16(wav, pos);
            LeftChannel[i] = sample / 32768f;
            pos += 2;
            i++;
        }
    }
}
