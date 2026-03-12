// using System;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;
// using System.IO;
// using System.Linq;
// using System.Collections.Generic;
// using UnityEngine.Networking;
// using UnityEngine.Events;
// using System.Collections.Concurrent;

// public class Gemini_VoiceToText : MonoBehaviour
// {
//     private const string API_URL =
//         "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent";

//     private const int MIC_RATE = 16000;
//     private const int CHUNK_SAMPLES = 1600;

//     [Header("API Key")]
//     public string apiKey = "YOUR_API_KEY";

//     private AudioClip micClip;
//     private int lastSample = 0;
//     public bool IsReadyToSpend = false;

//     public UnityEvent OnSendVoiceToText;
//     public UnityEvent OnReceiveVoiceToText;
//     private bool IsUpdating = false;
//     public UnityEvent OnReceiveError;
//     private bool IsError = false;

//     [Header("Voice Activity Detection")]
//     public float silenceThreshold = 0.01f;   // 볼륨 임계값
//     public float silenceDuration = 0.8f;     // 침묵 시간(초)
//     private float silenceTimer = 0f;
//     private bool isSpeaking = false;
//     public bool IsSpeaking => isSpeaking;

//     private List<byte> audioBuffer = new List<byte>();

//     [Header("Pre Buffer")]
//     public float preBufferSeconds = 0.5f; // 발화 시작 전에 붙일 대기 버퍼 (초)
//     private Queue<byte> preBuffer = new Queue<byte>();
//     private int PreBufferBytes => (int)(MIC_RATE * preBufferSeconds) * 2; // 16bit PCM

//     [Header("Debug")]
//     public float LastMicAverage = 0f;
//     public string LatestTranscript = "";
//     public bool PlayRecordedAudio = false;

//     void OnEnable()
//     {
//         // micClip = Microphone.Start(null, true, 5, MIC_RATE);
//         lastSample = 0;
//     }

//     void Update()
//     {
//         // if (IsReadyToSpend)
//         // {
//         //     DetectAndBufferSpeech();
//         // }
//         // else
//         // {
//         //     lastSample = Microphone.GetPosition(null);
//         // }

//         // if (IsUpdating)
//         // {
//         //     IsUpdating = false;
//         //     OnReceiveVoiceToText.Invoke();
//         // }

//         // if (IsError)
//         // {
//         //     IsError = false;
//         //     OnReceiveError.Invoke();
//         // }
//     }

//     private void OnDisable()
//     {
//         // if (Microphone.IsRecording(null))
//         // {
//         //     Microphone.End(null);
//         // }

//         // Queue와 List 초기화
//         preBuffer.Clear();
//         audioBuffer.Clear();
//     }

//     private void DetectAndBufferSpeech()
//     {
//         // int pos = Microphone.GetPosition(null);
//         // if (pos < lastSample) lastSample = 0;

//         // int diff = pos - lastSample;
//         // while (diff >= CHUNK_SAMPLES)
//         // {
//         //     float[] samples = new float[CHUNK_SAMPLES];
//         //     micClip.GetData(samples, lastSample);
//         //     LastMicAverage = samples.Average(s => Mathf.Abs(s));

//         //     byte[] pcm = FloatToPCM16(samples);

//         //     // 🔹 항상 preBuffer에 쌓아두기 (슬라이딩 윈도우)
//         //     foreach (var b in pcm)
//         //     {
//         //         preBuffer.Enqueue(b);
//         //         if (preBuffer.Count > PreBufferBytes)
//         //             preBuffer.Dequeue();
//         //     }

//         //     if (LastMicAverage > silenceThreshold)
//         //     {
//         //         if (!isSpeaking)
//         //         {
//         //             // 발화 시작: preBuffer를 audioBuffer에 합쳐서 앞부분 보존
//         //             audioBuffer.AddRange(preBuffer);
//         //         }

//         //         isSpeaking = true;
//         //         silenceTimer = 0f;
//         //         audioBuffer.AddRange(pcm);
//         //     }
//         //     else
//         //     {
//         //         if (isSpeaking)
//         //         {
//         //             silenceTimer += (float)CHUNK_SAMPLES / MIC_RATE;
//         //             if (silenceTimer > silenceDuration)
//         //             {
//         //                 CommitSpeech();
//         //                 isSpeaking = false;
//         //                 silenceTimer = 0f;
//         //                 audioBuffer.Clear();
//         //             }
//         //         }
//         //     }

//         //     lastSample += CHUNK_SAMPLES;
//         //     diff -= CHUNK_SAMPLES;
//         // }
//     }

//     private void CommitSpeech()
//     {
//         // if (audioBuffer.Count == 0) return;

//         // // PCM16 → WAV → Base64
//         // byte[] wavData = EncodeAsWav(audioBuffer.ToArray(), MIC_RATE, 1);
//         // string base64 = Convert.ToBase64String(wavData);

//         // _ = SendToGemini(base64);
//         // OnSendVoiceToText.Invoke();

//         // Debug.Log($"➡️ Sent speech segment ({audioBuffer.Count} bytes PCM, {wavData.Length} bytes WAV)");

//         // if (PlayRecordedAudio)
//         // {
//         //     var clip = AudioClip.Create("Recorded", wavData.Length / 2, 1, MIC_RATE, false);
//         //     float[] floatData = new float[wavData.Length / 2];
//         //     for (int i = 0; i < floatData.Length; i++)
//         //     {
//         //         short val = BitConverter.ToInt16(wavData, i * 2);
//         //         floatData[i] = val / (float)short.MaxValue;
//         //     }
//         //     clip.SetData(floatData, 0);
//         //     AudioSource.PlayClipAtPoint(clip, Vector3.zero);
//         // }
//     }

//     private async Task SendToGemini(string base64)
//     {
//         string json = $@"
// {{
//   ""contents"": [
//     {{
//       ""role"": ""user"",
//       ""parts"": [
//         {{
//           ""text"": ""Generate a transcript of the speech. Transcribe only the human speech and english from the following audio. Ignore background noises, music, or non-speech sounds.""
//         }},
//         {{
//           ""inlineData"": {{
//             ""mimeType"": ""audio/wav"",
//             ""data"": ""{base64}""
//           }}
//         }}
//       ]
//     }}
//   ]
// }}";

//         using (UnityWebRequest req = new UnityWebRequest(API_URL, "POST"))
//         {
//             byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
//             req.uploadHandler = new UploadHandlerRaw(bodyRaw);
//             req.downloadHandler = new DownloadHandlerBuffer();
//             req.SetRequestHeader("Content-Type", "application/json");
//             req.SetRequestHeader("x-goog-api-key", apiKey);

//             var asyncOp = req.SendWebRequest();
//             while (!asyncOp.isDone) await Task.Yield();

//             if (req.result != UnityWebRequest.Result.Success)
//             {
//                 Debug.LogError("❌ Gemini API error: " + req.error + " " + req.downloadHandler.text);
//                 IsError = true;
//             }
//             else
//             {
//                 Debug.Log("✅ Gemini API response: " + req.downloadHandler.text);
//                 LatestTranscript = ExtractText(req.downloadHandler.text);
//                 IsUpdating = true;
//                 Debug.Log("📝 Transcript: " + LatestTranscript);
//             }
//         }
//     }

//     // ===== Utilities =====

//     private byte[] FloatToPCM16(float[] samples)
//     {
//         using (MemoryStream ms = new MemoryStream())
//         {
//             foreach (var f in samples)
//             {
//                 short val = (short)(Mathf.Clamp(f, -1f, 1f) * short.MaxValue);
//                 ms.Write(BitConverter.GetBytes(val), 0, 2);
//             }
//             return ms.ToArray();
//         }
//     }

//     // PCM16 → WAV (RIFF 헤더 추가)
//     private byte[] EncodeAsWav(byte[] pcmData, int sampleRate, int channels)
//     {
//         using (MemoryStream ms = new MemoryStream())
//         using (BinaryWriter writer = new BinaryWriter(ms))
//         {
//             int byteRate = sampleRate * channels * 2;

//             // RIFF 헤더
//             writer.Write(Encoding.ASCII.GetBytes("RIFF"));
//             writer.Write(36 + pcmData.Length);
//             writer.Write(Encoding.ASCII.GetBytes("WAVE"));

//             // fmt 서브청크
//             writer.Write(Encoding.ASCII.GetBytes("fmt "));
//             writer.Write(16);
//             writer.Write((short)1); // PCM
//             writer.Write((short)channels);
//             writer.Write(sampleRate);
//             writer.Write(byteRate);
//             writer.Write((short)(channels * 2));
//             writer.Write((short)16);

//             // data 서브청크
//             writer.Write(Encoding.ASCII.GetBytes("data"));
//             writer.Write(pcmData.Length);
//             writer.Write(pcmData);

//             return ms.ToArray();
//         }
//     }

//     private string ExtractText(string json)
//     {
//         // 간단 파서 (정식으론 JSON 파서 사용 권장)
//         int idx = json.IndexOf("\"text\":");
//         if (idx < 0) return null;
//         idx = json.IndexOf("\"", idx + 7) + 1;
//         int end = json.IndexOf("\"", idx);
//         if (end < 0) return null;
//         return json.Substring(idx, end - idx);
//     }
// }
