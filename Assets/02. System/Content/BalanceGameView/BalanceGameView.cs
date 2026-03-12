/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.06.01
 *
 * STEP2: 밸런스 게임 뷰.
 * 3~4문항을 순서대로 보여주고, A/B 선택 완료 시
 * OnCompleted(TravelType, SpiritData) 이벤트를 발행한다.
 *
 * 버튼 리스너는 Start()(첫 활성화 시 1회)에서만 연결.
 * 상태 리셋·재시작은 ContentViewModel이 ResetAndStart()로 호출.
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    [Serializable]
    public class BalanceQuestion
    {
        [Tooltip("질문 이미지 Sprite (텍스트 대체용)")]
        public Sprite QuestionSprite;
        public Sprite OptionASprite;
        public Sprite OptionBSprite;
    }

    public class BalanceGameView : BaseView
    {
        [Header("Balance Game View / 질문 데이터")]
        public List<BalanceQuestion> Questions = new List<BalanceQuestion>();

        [Header("Balance Game View / UI")]
        public Image  QuestionImage;
        public Button OptionAButton;
        public Button OptionBButton;
        public Image  OptionAImage;
        public Image  OptionBImage;

        // 완료 이벤트: ContentViewModel에서 1회 연결
        [Header("Balance Game View / Events")]
        public UnityEvent<TravelType, SpiritData> OnCompleted = new UnityEvent<TravelType, SpiritData>();

        private int   _currentIndex = 0;
        private int[] _answers;

        // ── Unity 라이프사이클 ────────────────────────────────────────────
        // Start() : 첫 번째 활성화 시 1회만 실행 → 버튼 리스너 연결
        protected override void Start()
        {
            base.Start();
            OptionAButton.onClick.RemoveAllListeners();
            OptionBButton.onClick.RemoveAllListeners();
            OptionAButton.onClick.AddListener(() => OnAnswer(0));
            OptionBButton.onClick.AddListener(() => OnAnswer(1));
        }

        // Initialize() : base만 호출 — 버튼 리스너 재연결 없음
        public override void Initialize()
        {
            base.Initialize();
        }

        // ── 공개 API ─────────────────────────────────────────────────────
        /// <summary>
        /// ContentViewModel이 SetState(BalanceGame) 진입 시 호출.
        /// 상태를 초기화하고 첫 번째 질문을 표시한다.
        /// </summary>
        public void ResetAndStart()
        {
            _answers      = new int[Questions.Count];
            _currentIndex = 0;
            ShowQuestion(_currentIndex);
        }

        // ── 내부 ─────────────────────────────────────────────────────────
        private void ShowQuestion(int index)
        {
            if (index < 0 || index >= Questions.Count)
            {
                Debug.LogWarning("[BalanceGameView] Question index out of range", this);
                return;
            }
            var q = Questions[index];
            if (QuestionImage != null && q.QuestionSprite != null)
                QuestionImage.sprite = q.QuestionSprite;
            if (OptionAImage  != null && q.OptionASprite  != null)
                OptionAImage.sprite  = q.OptionASprite;
            if (OptionBImage  != null && q.OptionBSprite  != null)
                OptionBImage.sprite  = q.OptionBSprite;
        }

        private void OnAnswer(int choice)
        {
            if (_answers == null || _currentIndex >= Questions.Count) return;

            _answers[_currentIndex] = choice;
            Debug.Log($"[BalanceGame] Q{_currentIndex + 1} → {(choice == 0 ? "A" : "B")}", this);
            _currentIndex++;

            if (_currentIndex < Questions.Count)
                ShowQuestion(_currentIndex);
            else
                Finish();
        }

        private void Finish()
        {
            TravelType type   = TravelTypeResolver.Resolve(_answers);
            SpiritData spirit = SpiritTable.GetByType(type);

            // SessionData 저장
            var session            = GlobalManager.Instance.DataModel.Session;
            session.BalanceAnswers = _answers;
            session.Type           = type;
            session.SpiritKey      = spirit.Key;
            session.SpiritName     = spirit.Name;

            Debug.Log($"[BalanceGame] Finished → Type={type}  Spirit={spirit.Name}", this);
            OnCompleted?.Invoke(type, spirit);
        }
    }
}
