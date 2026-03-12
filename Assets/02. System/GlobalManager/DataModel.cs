using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace FUTUREVISION
{
    public class DataModel : BaseModel
    {
        [Header("Data Model")]
        [Header("Data Model/Parameter")]
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        [Header("Data Model/Data")]
        public string mode;

        [Header("Data Model/Session")]
        public SessionData Session = new SessionData();

        [Header("Data Model/Reference")]
        public List<AssetReference> ObjectReferences = new List<AssetReference>();

        public List<int> AnsweredQuestionIndices = new List<int>();

        static public List<string> QuestionList = new List<string>()
        {
            "지금 여행 간다면, 어떤 분위기가 좋아?",
            "친구와 함께 간다면 어떤 곳이 끌려?",
            "가장 기대되는 여행 순간은 언제야?",
            "이 중 하나를 고른다면?",
        };
        static public List<List<string>> AnswerList = new List<List<string>>()
        {
            new List<string>()
            {
                "자연을 느낄 수 있는 조용한 곳",
                "놀거리, 체험거리가 많은 곳",
                "고즈넉하고 옛 감성이 느껴지는 곳"
            },
            new List<string>()
            {
                "호수나 숲이 있는 산책 코스",
                "놀이기구나 체험존",
                "민속촌이나 테마파크"
            },
            new List<string>()
            {
                "풍경 사진 찍기",
                "체험 활동",
                "조용히 둘러보며 사색하기"
            },
            new List<string>()
            {
                "힐링 산책존",
                "놀이기구나 체험존",
                "유서 깊은 건물과 마을"
            },
        };
        static public List<string> RecommendationList = new List<string>()
        {
            "환경(힐링/자연): 용담호수, 경안천, 기흥호수공원, 백암산, 석성산, 청년김대건길, 광교산, 고기리 계곡, 사계절산책로(처인구)",
            "문화(체험/놀이/즐김): 한국민속촌, 에버랜드, 와우정사, 농촌테마파크, 곤충테마파크, 보정동 카페거리",
            "역사(유산/인문/감성): 처인성, 백암고택, 남사에담촌, 용인중앙시장, 심곡서원, 조선백자박물관, 김대건기념관, 심곡 서원 고택군"
        };

        public override void Initialize()
        {
            // Parameter
            InitializeParameters();

            // Data
            string mode = Parameters.ContainsKey("mode") ? Parameters["mode"] : "";

            // Reference
            // 최초 실행시 어드레서블 오브젝트 로드
            foreach (var objRef in ObjectReferences)
            {
                objRef.LoadAssetAsync<GameObject>();
            }
        }

        private void InitializeParameters()
        {
            // URL 파라미터에서 값 가져오기
            var url = Application.absoluteURL;

            if (url.Contains("?"))
            {
                var param = url.Split('?')[1];  // ? 뒤의 파라미터 부분만 가져오기
                var paramList = param.Split('&');
                Parameters = new Dictionary<string, string>();

                foreach (var p in paramList)
                {
                    var keyValue = p.Split('=');
                    if (keyValue.Length == 2)
                    {
                        // URL 디코딩 적용 (특수문자 및 한글 처리)
                        string key = WWW.UnEscapeURL(keyValue[0]);
                        string value = WWW.UnEscapeURL(keyValue[1]);
                        Parameters[key] = value;
                    }
                }
            }
        }
        #region Data Management
        public void SaveJsonData<T>(string key, T data)
        {
            string jsonData = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(key, jsonData);
            PlayerPrefs.Save();
        }

        public T LoadJsonData<T>(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string jsonData = PlayerPrefs.GetString(key);
                return JsonConvert.DeserializeObject<T>(jsonData);
            }
            // Simplify 'default' expression to remove IDE0034 warning
            return default;
        }

        public void DeleteData(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }
        #endregion
    }
}
