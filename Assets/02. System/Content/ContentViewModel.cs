/*
 * 작성자: 김범무
 * 작성일: 2025.05.11
 */

using FUTUREVISION.WebAR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace FUTUREVISION.Content
{
    public enum ContentState
    {
        None,
        Intro,           // STEP1: 여행 모드 선택 (신규 흐름)
        BalanceGame,     // STEP2: 밸런스 게임 → TravelType 산출 (신규)
        // ── 구 흐름 (호환성 유지) ──────────────────────────
        Recommendation,
        Location,
        CardTrip,        // STEP5 ARSpirit 흐름으로 대체 예정
        // ──────────────────────────────────────────────────
        Stamp,
        Reward,
    }

    [Serializable]
    public class ContentData
    {
        public int Mission1Clear = 1;
        public int Mission2Clear = 1;
        public int Mission3Clear = 1;
        public int Mission4Clear = 1;

        // PlayerPrefs에 저장. Json이 안되므로 각각 저장
        // Save
        public void Save()
        {
            PlayerPrefs.SetInt("Mission1Clear", Mission1Clear);
            PlayerPrefs.SetInt("Mission2Clear", Mission2Clear);
            PlayerPrefs.SetInt("Mission3Clear", Mission3Clear);
            PlayerPrefs.SetInt("Mission4Clear", Mission4Clear);
            PlayerPrefs.Save();
        }

        // Load
        public void Load()
        {
            Mission1Clear = PlayerPrefs.GetInt("Mission1Clear", 0);
            Mission2Clear = PlayerPrefs.GetInt("Mission2Clear", 0);
            Mission3Clear = PlayerPrefs.GetInt("Mission3Clear", 0);
            Mission4Clear = PlayerPrefs.GetInt("Mission4Clear", 0);
        }
    }

    public class ContentViewModel : BaseViewModel
    {
        public static string DATA_KEY = "ContentData";

        // 현재 활성 ContentState — 외부에서 읽기 전용
        public ContentState CurrentState { get; private set; } = ContentState.None;

        [Header("Content View Model / 기존 Views")]
        public IntroView IntroView;
        public RecommendationView RecommendationView;
        public LocationView LocationView;
        public CardTripView CardTripView;
        public StampView StampView;
        public RewardView RewardView;

        [Header("Content View Model / New Views")]
        public BalanceGameView BalanceGameView;

        [Header("Content View Model/Data")]
        public int CurrentContentIndex = 0;
        public ContentData Data = new ContentData();
        [Space(10)]
        public int CurrentCourse = 0;
        public int CurrentSpot = 0;
        public List<CourseData> CourseList = new List<CourseData>();

        [Serializable]
        public class CourseData
        {
            public string Name;
            public List<SpotData> SpotList;
        }

        [Serializable]
        public class SpotData
        {
            public string Name;
            public Sprite Image;
            [Space(10)]    
            public double Latitude;
            public double Longitude;
        }

        public override void Initialize()
        {
            Debug.Log("ContentViewModel Initialize", this);
            base.Initialize();

            InitializeIntro();
            InitializeBalanceGame();
            InitializeRecommendation();
            InitializeCardTrip();
            InitilizeStamp();
            InitilizeReward();

            // 데이터 초기화
            var dataModel = GlobalManager.Instance.DataModel;
            CurrentContentIndex = dataModel.Parameters.ContainsKey("contentIndex") ? int.Parse(dataModel.Parameters["contentIndex"]) : 0;
            //Data = GlobalManager.Instance.DataModel.LoadJsonData<ContentData>(DATA_KEY);
            Data.Load();

            // LocationView
            LocationView.Initialize();
            LocationView.LocationPinPrefab.button.onClick.AddListener(() =>
            {
                Debug.Log("LocationPin Button Clicked", this);
                SetState(ContentState.CardTrip);
            });

            // 시작 상태
            StartCoroutine(ReplacementOrigin());
            SetState(ContentState.CardTrip);
        }

        private void Update()
        {
            UpdateCardTrip();
        }

        private void SetState(ContentState newState)
        {
            CurrentState = newState;

            // 모든 View 비활성화
            IntroView.gameObject.SetActive(false);
            RecommendationView.gameObject.SetActive(false);
            LocationView.gameObject.SetActive(false);
            CardTripView.gameObject.SetActive(false);
            StampView.gameObject.SetActive(false);
            RewardView.gameObject.SetActive(false);
            if (BalanceGameView != null) BalanceGameView.gameObject.SetActive(false);

            switch (newState)
            {
                case ContentState.Intro:
                    IntroView.gameObject.SetActive(true);
                    break;
                case ContentState.BalanceGame:
                    if (BalanceGameView != null)
                    {
                        BalanceGameView.gameObject.SetActive(true);
                        OnEnterBalanceGame();
                    }
                    break;
                case ContentState.Recommendation:
                    RecommendationView.gameObject.SetActive(true);
                    SetSpotInfo(0);
                    break;
                case ContentState.Location:
                    LocationView.gameObject.SetActive(true);
                    break;
                case ContentState.CardTrip:
                    CardTripView.gameObject.SetActive(true);
                    StartStage1();
                    break;
                case ContentState.Stamp:
                    StampView.gameObject.SetActive(true);
                    break;
                case ContentState.Reward:
                    RewardView.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private IEnumerator ReplacementOrigin()
        {
            // 이거 순서 안지키면 제대로 안되니 가능한 건들지 말아라.
            var arTracker = WebARManager.Instance.ARTrackerModel;
            if (arTracker.isActiveAndEnabled == false)
            {
                Debug.Log("ReplacementOrigin Enable ARTrackerModel", this);
                arTracker.gameObject.SetActive(true);
                yield return new WaitForSeconds(1.5f);
            }

            if (arTracker.IsPlacement == true)
            {
                Debug.Log("ReplacementOrigin IsPlacement False", this);
                arTracker.ResetPlacement();
                // 프레임 읽어오기 전에 배치하면 이상한데 위치함
                yield return new WaitForSeconds(1f);
            }

            //yield return new WaitUntil(() =>
            //{
            //    return arTracker.Placement.activeSelf;
            //});
            Debug.Log("ReplacementOrigin SetPlacement", this);
            arTracker.SetPlacement();

        }

        #region Intro
        private void InitializeIntro()
        {
            // ── 신규: STEP1 모드 선택 버튼 (null-safe — 프리팹에 버튼이 없으면 무시) ──
            void OnModeSelected(TravelMode mode)
            {
                GlobalManager.Instance.DataModel.Session.Mode = mode;
                Debug.Log($"[Intro] TravelMode selected: {mode}", this);
                StartCoroutine(ReplacementOrigin());
                SetState(ContentState.BalanceGame);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            }

            if (IntroView.SoloButton    != null) IntroView.SoloButton.onClick.AddListener(()    => OnModeSelected(TravelMode.Solo));
            if (IntroView.FamilyButton  != null) IntroView.FamilyButton.onClick.AddListener(()  => OnModeSelected(TravelMode.Family));
            if (IntroView.FriendsButton != null) IntroView.FriendsButton.onClick.AddListener(() => OnModeSelected(TravelMode.Friends));
            if (IntroView.CoupleButton  != null) IntroView.CoupleButton.onClick.AddListener(()  => OnModeSelected(TravelMode.Couple));

            // ── 구 흐름 호환: StartButton이 있으면 기존 Recommendation으로 이동 ──
            if (IntroView.StartButton != null)
            {
                IntroView.StartButton.onClick.AddListener(() =>
                {
                    StartCoroutine(ReplacementOrigin());
                    SetState(ContentState.Recommendation);
                    GlobalManager.Instance.SoundModel.PlayButtonClickSound();
                });
            }
        }
        #endregion
        #region BalanceGame
        private void InitializeBalanceGame()
        {
            if (BalanceGameView == null)
            {
                Debug.LogWarning("[ContentViewModel] BalanceGameView is not assigned.", this);
                return;
            }
            // OnCompleted 리스너 1회 연결 (중복 방지)
            BalanceGameView.OnCompleted.RemoveAllListeners();
            BalanceGameView.OnCompleted.AddListener((type, spirit) =>
            {
                Debug.Log($"[ContentViewModel] BalanceGame done. Type={type} Spirit={spirit.Name}", this);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
                SetState(ContentState.CardTrip);
            });
        }

        private void OnEnterBalanceGame()
        {
            if (BalanceGameView == null) return;
            BalanceGameView.ResetAndStart();
        }
        #endregion
        #region Recommendation
        private void InitializeRecommendation()
        {
            RecommendationView.SetState(ERecommendationState.SelectView);

            // 설문조사 시작
            RecommendationView.StartRecommendationButton.onClick.AddListener(() =>
            {
                RecommendationView.SetState(ERecommendationState.QuestionView);
                RecommendationView.SetQuestion(0);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            RecommendationView.SkipRecommendationButton.onClick.AddListener(() =>
            {
                RecommendationView.SetState(ERecommendationState.ResultView);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });

            // 설문조사 질문 선택
            // TODO: 나중에 질문지, 답변지 데이터로 변경
            RecommendationView.Select1Button.onClick.AddListener(() => OnSelectAnswer(0));
            RecommendationView.Select2Button.onClick.AddListener(() => OnSelectAnswer(1));
            RecommendationView.Select3Button.onClick.AddListener(() => OnSelectAnswer(2));

            // 설문조사 완료 후 추천 페이지
            RecommendationView.StartButton.onClick.AddListener(() =>
            {
                SetState(ContentState.Location);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });

            // 챗봇 응답시 추천 결과 표시
            GlobalManager.Instance.Gemini_Chatbot.OnReceiveChatbot.AddListener(() =>
            {
                string answerText = GlobalManager.Instance.Gemini_Chatbot.LatestResponse;
                Debug.Log("Gemini Chatbot Response: " + answerText, this);

                int recommendedAnswer = 0;
                if (int.TryParse(answerText.Trim(), out recommendedAnswer))
                {
                    CurrentCourse = recommendedAnswer - 1;
                }

                RecommendationView.SetState(ERecommendationState.ResultView);
            });

            // 스팟 버튼
            RecommendationView.ToNextButton.onClick.AddListener(() =>
            {
                if (CurrentSpot < CourseList[CurrentCourse].SpotList.Count - 1)
                {
                    SetSpotInfo(CurrentSpot + 1);
                }
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            RecommendationView.ToBeforeButton.onClick.AddListener(() =>
            {
                if (CurrentSpot > 0)
                {
                    SetSpotInfo(CurrentSpot - 1);
                }
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
        }

        private void SetSpotInfo(int SpotIndex)
        {
            CurrentSpot = SpotIndex;
            var course = CourseList[CurrentCourse];
            var spot = course.SpotList[SpotIndex];

            RecommendationView.SpotNameText.text = spot.Name;
            RecommendationView.SpotImage.sprite = spot.Image;

            // 버튼 상태 업데이트
            RecommendationView.ToBeforeButton.interactable = CurrentSpot > 0;
            RecommendationView.ToNextButton.interactable = CurrentSpot < CourseList[CurrentCourse].SpotList.Count - 1;
        }

        private void OnSelectAnswer(int answer)
        {
            GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            GlobalManager.Instance.DataModel.AnsweredQuestionIndices.Add(answer);

            int nextIndex = RecommendationView.CurrentIndex + 1;
            if (nextIndex < RecommendationView.QuestionList.Count)
            {
                RecommendationView.SetQuestion(nextIndex);
            }
            else
            {
                // 설문조사 완료
                var dataModel = GlobalManager.Instance.DataModel;
                string text = $"질문에 대한 답을 통하여 지정된 코스들 중에서 사용자에게 가장 적합한 코스를 숫자로 답변해줘\n"
                    + $"(EX)1)\n"
                    + $"\n"
                    + $"1) {DataModel.RecommendationList[0]}\n"
                    + $"2) {DataModel.RecommendationList[1]}\n"
                    + $"3) {DataModel.RecommendationList[2]}\n"
                    + $"\n"
                    + $"Q1: {DataModel.QuestionList[0]}\n"
                    + $"A1: {DataModel.AnswerList[0][dataModel.AnsweredQuestionIndices[0]]}\n"
                    + $"Q2: {DataModel.QuestionList[1]}\n"
                    + $"A2: {DataModel.AnswerList[1][dataModel.AnsweredQuestionIndices[1]]}\n"
                    + $"Q3: {DataModel.QuestionList[2]}\n"
                    + $"A3: {DataModel.AnswerList[2][dataModel.AnsweredQuestionIndices[2]]}\n"
                    + $"Q4: {DataModel.QuestionList[3]}\n"
                    + $"A4: {DataModel.AnswerList[3][dataModel.AnsweredQuestionIndices[3]]}\n"
                    + $"\n";

                GlobalManager.Instance.Gemini_Chatbot.SendText(text);
                RecommendationView.SetState(ERecommendationState.WaitingView);
            }
        }
        #endregion
        #region CardTrip
        [Header("Card Trip")]
        public List<Sprite> TreasureSpriteList = new List<Sprite>();
        private bool CardTrip_IsDraged = false;
        private float CardTrip_DragStartHeight = 0f;
        private float CardTrip_DragStartOffsetY = 0f;
        private float CardTrip_CardTargetHeight = 0f;
        private void InitializeCardTrip()
        {
            // 카드 재배치
            CardTripView.ReplaceButton.onClick.AddListener(() =>
            {
                StartCoroutine(ReplacementOrigin());

                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });

            // Stage 1 - 보물상자 클릭
            // 랜덤한 위치에 보물을 숨겨두고 클릭 시 Stage 2로 이동
            CardTripView.TreasureItem.OnClicked.AddListener(() =>
            {
                Debug.Log("TreasureItem Clicked", this);
                StartStage2();
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });

            // Stage 2 - 수호정령 클릭
            // 모든 구슬 클릭 시 Stage 3으로 이동
            foreach (var orbItem in CardTripView.SpritOrbItemList)
            {
                orbItem.OnClicked.AddListener(() =>
                {
                    Debug.Log("SpritOrbItem Clicked", this);
                    orbItem.gameObject.SetActive(false);

                    // 모든 구슬 클릭 시 보상 화면으로 이동
                    bool allClicked = true;
                    foreach (var checkOrbItem in CardTripView.SpritOrbItemList)
                    {
                        if (checkOrbItem.gameObject.activeSelf)
                        {
                            allClicked = false;
                            break;
                        }
                    }
                    if (allClicked)
                    {
                        StartStage3();
                    }

                    GlobalManager.Instance.SoundModel.PlayButtonClickSound();
                });
            }

            // Stage 3 - 용가리 봉인
            CardTrip_DragStartHeight = CardTripView.Card.anchoredPosition.y;

            // 카드를 드래그 하여 용가리 위에 올려놓기
            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            beginDragEntry.callback.AddListener((data) =>
            {
                Debug.Log("CaptureTargetItem Begin Dragged", this);
                PointerEventData pointerData = (PointerEventData)data;
                Vector3 screenPoint = new Vector3(0, pointerData.position.y, 0f);
                CardTripView.Card.anchoredPosition = screenPoint;
                CardTrip_DragStartOffsetY = pointerData.position.y - CardTripView.Card.anchoredPosition.y;
                CardTrip_IsDraged = true;
            });
            CardTripView.CardEventTrigger.triggers.Add(beginDragEntry);
            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            dragEntry.eventID = EventTriggerType.Drag;
            dragEntry.callback.AddListener((data) =>
            {
                Debug.Log("CaptureTargetItem Dragged", this);
                PointerEventData pointerData = (PointerEventData)data;
                Vector3 screenPoint = new Vector3(0, pointerData.position.y, 0f);
                CardTrip_CardTargetHeight = screenPoint.y - CardTrip_DragStartOffsetY;
            });
            CardTripView.CardEventTrigger.triggers.Add(dragEntry);
            // 드래그 종료시 용가리 위에 올려졌는지 확인
            EventTrigger.Entry endDragEntry = new EventTrigger.Entry();
            endDragEntry.eventID = EventTriggerType.EndDrag;
            endDragEntry.callback.AddListener((data) =>
            {
                Debug.Log("CaptureTargetItem End Dragged", this);
                // 레이케이스트로 용가리 위에 올려졌는지 확인
                Ray ray = WebARManager.Instance.ARTrackerModel.ARCamera.cam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    if (hitInfo.collider.gameObject == CardTripView.CaptureTargetItem.gameObject)
                    {
                        // 용가리 위에 올려졌음
                        StartStageClearAnimation();
                    }
                }
                CardTrip_IsDraged = false;
            });
            CardTripView.CardEventTrigger.triggers.Add(endDragEntry);

            // 가이드 화면 터치시 바로 닫기
            CardTripView.Stage1GuideCloseButton.onClick.AddListener(() =>
            {
                CardTripView.SetStage(ECardTripStage.Stage1Play);
                StopCoroutine(guideCoroutine);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            CardTripView.Stage2GuideCloseButton.onClick.AddListener(() =>
            {
                CardTripView.SetStage(ECardTripStage.Stage2Play);
                StopCoroutine(guideCoroutine);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
            CardTripView.Stage3GuideCloseButton.onClick.AddListener(() =>
            {
                CardTripView.SetStage(ECardTripStage.Stage3Play);
                StopCoroutine(guideCoroutine);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
        }

        private void UpdateCardTrip()
        {
            if (CardTrip_IsDraged)
            {
                // 드래그 중일 때 카드의 Y 위치를 목표 위치로 점점 높이기
                Vector3 currentPos = CardTripView.Card.anchoredPosition;
                currentPos.y = Mathf.Lerp(currentPos.y, CardTrip_CardTargetHeight, Time.deltaTime * 10f);
                CardTripView.Card.anchoredPosition = currentPos;
            }
            else
            {
                // 드래그 중이 아닐 때 카드를 원래 위치로 점점 낮추기
                Vector3 currentPos = CardTripView.Card.anchoredPosition;
                currentPos.y = Mathf.Lerp(currentPos.y, CardTrip_DragStartHeight, Time.deltaTime * 10f);
                CardTripView.Card.anchoredPosition = currentPos;
            }
        }

        private Coroutine guideCoroutine;
        private void StartStage1()
        {
            // ── P1-07: SpiritKey 기반 AR 오브젝트 선택 (null-safe) ──────
            string spiritKey = GlobalManager.Instance.DataModel.Session.SpiritKey;
            var arObjectView = WebARManager.Instance.ARViewModel.ARObjectView;
            if (arObjectView != null && !string.IsNullOrEmpty(spiritKey))
            {
                arObjectView.SetCurrentObjectByKey(spiritKey);
                Debug.Log($"[ContentViewModel] AR Spirit selected: {spiritKey}", this);
            }
            // ─────────────────────────────────────────────────────────────

            CardTripView.SetStage(ECardTripStage.Stage1Guide);
            guideCoroutine = StartCoroutine(ARGuideCoroutine(CardTripView.Stage1GuideObject, () =>
            {
                CardTripView.SetStage(ECardTripStage.Stage1Play);
            }));

            // 보물 아이템 설정
            CardTripView.TreasureItem.SpriteRenderer.sprite = TreasureSpriteList[CurrentContentIndex];
            // 보물 위치 랜덤 설정
            float range = 2.5f;
            Vector3 randomPosition = new Vector3(
                UnityEngine.Random.Range(-range, range), 0f, 0f);
            CardTripView.TreasureItem.transform.localPosition = randomPosition;
            CardTripView.TreasureItem.gameObject.SetActive(true);
        }

        private void StartStage2()
        {
            CardTripView.SetStage(ECardTripStage.Stage2Guide);
            guideCoroutine = StartCoroutine(ARGuideCoroutine(CardTripView.Stage2GuideObject, () =>
            {
                CardTripView.SetStage(ECardTripStage.Stage2Play);
            }));

            // 정령구슬 위치 랜덤 설정
            float range = 1.0f;
            foreach (var orbItem in CardTripView.SpritOrbItemList)
            {
                Vector3 randomPosition = new Vector3(
                    UnityEngine.Random.Range(-range, range), 
                    UnityEngine.Random.Range(-range, range), 
                    UnityEngine.Random.Range(-range, range));
                orbItem.transform.localPosition = randomPosition;
                orbItem.gameObject.SetActive(true);
            }
        }

        private void StartStage3()
        {
            CardTripView.SetStage(ECardTripStage.Stage3Guide);
            guideCoroutine = StartCoroutine(ARGuideCoroutine(CardTripView.Stage3GuideObject, () =>
            {
                CardTripView.SetStage(ECardTripStage.Stage3Play);
            }));
        }

        private void StartStageClearAnimation()
        {
            CardTripView.SetStage(ECardTripStage.StageClear, () =>
            {
                StageClear();
            });
        }

        private void StageClear()
        {
            //Data = GlobalManager.Instance.DataModel.LoadJsonData<ContentData>(DATA_KEY);
            // Mission 클리어 처리
            switch (CurrentContentIndex)
            {
                case 0:
                    Data.Mission1Clear = 1;
                    break;
                case 1:
                    Data.Mission2Clear = 1;
                    break;
                case 2:
                    Data.Mission3Clear = 1;
                    break;
                case 3:
                    Data.Mission4Clear = 1;
                    break;
                default:
                    break;
            }
            // 데이터 저장
            //GlobalManager.Instance.DataModel.SaveJsonData<ContentData>(DATA_KEY, Data);
            Data.Save();

            // 스탬프 스테이지로 이동
            SetState(ContentState.Stamp);
            GlobalManager.Instance.SoundModel.PlayMissionSound(true);

            StampView.Mission1Image.gameObject.SetActive(Data.Mission1Clear == 1);
            StampView.Mission2Image.gameObject.SetActive(Data.Mission2Clear == 1);
            StampView.Mission3Image.gameObject.SetActive(Data.Mission3Clear == 1);
            StampView.Mission4Image.gameObject.SetActive(Data.Mission4Clear == 1);

            bool allClear 
                =  (Data.Mission1Clear == 1) 
                && (Data.Mission2Clear == 1) 
                && (Data.Mission3Clear == 1) 
                && (Data.Mission4Clear == 1);
            StampView.CompletePopup.SetActive(allClear);
        }

        private IEnumerator ARGuideCoroutine(GameObject guideObject, Action nextStepAction)
        {
            float waitTime = 5f;
            float elapsedTime = 0f;
            while (elapsedTime < waitTime)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            guideObject.SetActive(false);

            yield return new WaitUntil(() =>
            {
                // 카메라가 어느정도 수평으로 되어있는지 카메라 각도로 확인
                // Up Vector가 카메라의 Up Vector와 유사한지 확인
                return WebARManager.Instance.ARTrackerModel.IsHorizontalWorld();
            });

            //StartCoroutine(ReplacementOrigin());

            nextStepAction?.Invoke();
        }

        [ContextMenu("Clear Save Data")]
        private void ClearSaveData()
        {
            Data = new ContentData();
            GlobalManager.Instance.DataModel.DeleteData(DATA_KEY);
            Debug.Log("ContentViewModel Clear Save Data", this);
        }
        #endregion
        #region Stamp
        private void InitilizeStamp()
        {
            StampView.PopupButton.onClick.AddListener(() =>
            {
                SetState(ContentState.Reward);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });

            StampView.ToRewardButton.onClick.AddListener(() =>
            {
                SetState(ContentState.Reward);
                GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            });
        }
        #endregion
        #region Reward
        private void InitilizeReward()
        {

        }
        #endregion
    }
}
