using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine.Events;

public class Gemini_Chatbot : MonoBehaviour
{
    // Gemini REST API의 GenerateContent 엔드포인트
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    private const int MAX_HISTORY = 10; // 기억할 대화 턴의 최대 수

    [Header("API Key")]
    [Tooltip("Google Gemini API 키를 여기에 입력하세요.")]
    public string apiKey = "YAIzaSyChqWJ5UesO2RlGW0trt4iac6cOLCfgwH8";
    private bool isUpdateing = false;
    public UnityEvent OnReceiveChatbot;
    public string LatestResponse = "";
    public UnityEvent OnReceiveError;
    public bool IsError = false;

    [Header("Debug")]
    public string Debug_TestMessage = "안녕하세요. 당신은 어떤 AI인가요?";

    // 🔹 대화 컨텍스트를 저장할 리스트 (REST API는 매번 이 전체를 보냄)
    // JObject 대신 직렬화를 위해 단순 클래스를 사용합니다.
    private List<Content> history = new List<Content>();

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class Content
    {
        public string role;
        public Part[] parts;
    }

    // REST API 요청의 전체 구조
    [System.Serializable]
    public class RequestData
    {
        public Content[] contents;
        public GenerationConfig generationConfig;
    }

    // 추가 설정 (선택 사항)
    [System.Serializable]
    public class GenerationConfig
    {
        public float temperature = 0.9f;
    }

    [System.Serializable]
    public class ResponseCandidate
    {
        public Content content;
    }

    // REST API 응답의 전체 구조
    [System.Serializable]
    public class ResponseData
    {
        public ResponseCandidate[] candidates;
    }


    [ContextMenu("Send Test Message")]
    public void SendTestMessage()
    {
        // WebSocket 방식과 달리 REST API는 setup 단계가 필요 없습니다.
        _ = SendMessageToGemini(Debug_TestMessage);
    }

    private void Update()
    {
        if (isUpdateing)
        {
            isUpdateing = false;
            OnReceiveChatbot?.Invoke();
        }

        if (IsError)
        {
            IsError = false;
            OnReceiveError?.Invoke();
        }
    }

    public void SendText(string text)
    {
        _ = SendMessageToGemini(text);
    }

    /// <summary>
    /// 사용자 메시지를 Gemini에 보내고 컨텍스트를 업데이트합니다.
    /// </summary>
    public async Task SendMessageToGemini(string message)
    {
        // 1. 사용자 메시지를 컨텍스트에 추가
        history.Add(new Content
        {
            role = "user",
            parts = new[] { new Part { text = message } }
        });

        // 2. 컨텍스트가 너무 길어지면 오래된 메시지를 제거
        if (history.Count > MAX_HISTORY)
        {
            // 가장 오래된 메시지부터 제거
            history.RemoveRange(0, history.Count - MAX_HISTORY);
        }

        // 3. 요청 데이터 객체 생성
        RequestData requestData = new RequestData
        {
            contents = history.ToArray(),
            generationConfig = new GenerationConfig()
        };

        string jsonPayload = JsonConvert.SerializeObject(requestData, Formatting.None);
        Debug.Log("➡️ Sending message with history: " + jsonPayload);

        // 4. UnityWebRequest를 사용하여 요청 전송
        using (UnityWebRequest www = new UnityWebRequest(API_URL + "?key=" + apiKey, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();

            // HTTP 헤더 설정
            www.SetRequestHeader("Content-Type", "application/json");

            // 요청 전송 (await를 위해 Task.Yield 사용)
            var operation = www.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            // 5. 응답 처리
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"❌ API Error: {www.error}");
                Debug.LogError($"Response Text: {www.downloadHandler.text}");
                IsError = true;
            }
            else
            {
                HandleResponse(www.downloadHandler.text);
            }
        }
    }

    /// <summary>
    /// 수신된 응답을 파싱하고 컨텍스트에 추가합니다.
    /// </summary>
    private void HandleResponse(string responseJson)
    {
        try
        {
            // JSON 응답 파싱
            ResponseData responseData = JsonConvert.DeserializeObject<ResponseData>(responseJson);

            // 첫 번째 후보 응답을 가져옵니다.
            if (responseData.candidates != null && responseData.candidates.Length > 0)
            {
                Content aiContent = responseData.candidates[0].content;

                // 🔹 AI 응답을 컨텍스트에 추가
                history.Add(aiContent);

                // 🔹 최종 응답 텍스트 추출 및 출력
                string fullResponse = aiContent.parts[0].text;
                Debug.Log($"✅ Full AI response: {fullResponse}");

                // UI 업데이트 등 후속 로직을 여기에 추가합니다.
                // 예: UIManager.Instance.UpdateChatText(fullResponse);

                LatestResponse = fullResponse;
                isUpdateing = true;
            }
            else
            {
                Debug.LogWarning("⚠️ Received a successful response but no candidates found.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("⚠️ Response parsing error: " + ex.Message + "\nRaw Data: " + responseJson);
        }
    }
}