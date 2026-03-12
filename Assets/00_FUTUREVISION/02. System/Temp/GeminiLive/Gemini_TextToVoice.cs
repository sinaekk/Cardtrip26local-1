using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class Gemini_TextToVoice : MonoBehaviour
{
    [Header("API Key")]
    public string apiKey = "YOUR_API_KEY";

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Config")]
    // Cloud TTS API의 표준 음성 이름 (한국어 - 여성)
    public string languageCode = "ko-KR";
    public string voiceName = "ko-KR-Standard-A"; 
    public string testText = "안녕하세요. 클라우드 TTS 음성 변환 기능을 테스트합니다.";

    // Google Cloud Text-to-Speech API의 공식 엔드포인트
    private const string TTS_API_URL = "https://texttospeech.googleapis.com/v1/text:synthesize";
    
    public UnityEvent OnReceiveError;
    private bool isError = false;

    [ContextMenu("Send Test Text")]
    public void SendTestText()
    {
        _ = GenerateAndPlayTTS(testText);
    }

    public void SendText(string text)
    {
        _ = GenerateAndPlayTTS(text);
    }

    public async Task GenerateAndPlayTTS(string text)
    {
        Debug.Log($"➡️ Requesting TTS for: {text}");

        // API 키를 URL 쿼리로 사용
        string url = $"{TTS_API_URL}?key={apiKey}";

        // === Google Cloud TTS API 요청 Body (주석 제거) ===
        string jsonBody = $@"
{{
  ""input"": {{
    ""text"": ""{text}""
  }},
  ""voice"": {{
    ""languageCode"": ""{languageCode}"",
    ""name"": ""{voiceName}""
  }},
  ""audioConfig"": {{
    ""audioEncoding"": ""LINEAR16"" 
  }}
}}";
        
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log($"📡 Sending request to: {url}");
            var response = await request.SendWebRequestAsync();

            if (response.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ TTS Request Success");
                string responseText = response.downloadHandler.text;
                
                try
                {
                    // Cloud TTS 응답 구조에 맞게 파싱
                    var parsed = JsonUtility.FromJson<CloudTTSResponseWrapper>(responseText);
                    string base64Audio = parsed.audioContent;

                    if (!string.IsNullOrEmpty(base64Audio))
                    {
                        byte[] audioBytes = Convert.FromBase64String(base64Audio);
                        
                        // WavUtility가 WAV 헤더를 올바르게 처리한다고 가정
                        AudioClip clip = WavUtility.ToAudioClip(audioBytes, "CloudTTS"); 
                        
                        if (clip != null)
                        {
                            audioSource.clip = clip;
                            audioSource.Play();
                            Debug.Log("🎧 TTS Audio Playing...");
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ AudioClip 변환 실패. WavUtility 로직 검토 필요.");
                            Debug.Log($"Raw Response: {responseText}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("⚠️ Audio data not found in response");
                        Debug.Log($"Raw Response: {responseText}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("⚠️ Parse error: " + ex.Message + "\nRaw Response: " + responseText);
                    isError = true;
                }
            }
            else
            {
                Debug.LogError($"❌ TTS Request Failed: {request.responseCode} | {request.error}\nRequested URL: {url}\nResponse Text: {request.downloadHandler.text}");
                isError = true;
            }
        }
    }

    private void Update()
    {
        if (isError)
        {
            isError = false;
            OnReceiveError?.Invoke();
        }
    }
    
    // Cloud TTS API 응답에 맞게 역직렬화 클래스
    [Serializable]
    private class CloudTTSResponseWrapper
    {
        public string audioContent; 
    }
}

/// <summary>
/// 16bit PCM WAV 데이터를 Unity AudioClip으로 변환
/// </summary>
public static class WavUtility
{
    public static AudioClip ToAudioClip(byte[] wavFile, string clipName = "GeminiTTS")
    {
        if (wavFile == null || wavFile.Length < 44)
        {
            Debug.LogError("WAV 데이터가 올바르지 않습니다.");
            return null;
        }

        int channels = BitConverter.ToInt16(wavFile, 22);
        int sampleRate = BitConverter.ToInt32(wavFile, 24);
        int bitsPerSample = BitConverter.ToInt16(wavFile, 34);
        if (bitsPerSample != 16)
        {
            Debug.LogWarning($"⚠️ 지원되지 않는 비트 깊이: {bitsPerSample}bit (16bit만 지원)");
            return null;
        }

        // "data" 위치 찾기
        int pos = 12;
        while (!(wavFile[pos] == 'd' && wavFile[pos + 1] == 'a' && wavFile[pos + 2] == 't' && wavFile[pos + 3] == 'a'))
        {
            pos += 4;
            int chunkSize = BitConverter.ToInt32(wavFile, pos);
            pos += 4 + chunkSize;
            if (pos >= wavFile.Length - 8)
            {
                Debug.LogError("⚠️ WAV 파일에서 데이터 섹션을 찾을 수 없음");
                return null;
            }
        }
        pos += 8;

        int sampleCount = (wavFile.Length - pos) / (bitsPerSample / 8);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(wavFile, pos + i * 2);
            samples[i] = sample / 32768f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount / channels, channels, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}

public static class UnityWebRequestAwaiter
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        var operation = request.SendWebRequest();
        operation.completed += _ =>
        {
            tcs.SetResult(request);
        };
        return tcs.Task;
    }
}