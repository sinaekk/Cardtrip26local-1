using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ModelInfo
{
    public string name;
    public string displayName;
    public List<string> supportedGenerationMethods;
}

[System.Serializable]
public class ModelListResponse
{
    public List<ModelInfo> models;
}

public class GeminiListModels : MonoBehaviour
{
    [Header("API Key")]
    public string apiKey = "YOUR_API_KEY";

    private const string LIST_MODELS_URL = "https://generativelanguage.googleapis.com/v1beta/models";

    public ModelListResponse modelListResponse;

    void Start()
    {
        StartCoroutine(GetModels());
    }

    private IEnumerator GetModels()
    {
        UnityWebRequest req = UnityWebRequest.Get(LIST_MODELS_URL);
        req.SetRequestHeader("x-goog-api-key", apiKey);

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ ListModels error: " + req.error);
        }
        else
        {
            Debug.Log("✅ Models response: " + req.downloadHandler.text);

            // JSON 파싱
            var resp = JsonUtility.FromJson<ModelListResponse>(req.downloadHandler.text);
            if (resp != null && resp.models != null)
            {
                foreach (var model in resp.models)
                {
                    string methods = model.supportedGenerationMethods != null
                        ? string.Join(",", model.supportedGenerationMethods)
                        : "";
                    Debug.Log($"📌 {model.name} ({model.displayName}) -> {methods}");
                }
            }

            modelListResponse = resp;
        }
    }
}
