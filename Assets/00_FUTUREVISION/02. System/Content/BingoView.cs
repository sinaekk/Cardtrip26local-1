/*
 * 작성자: 김범무
 * 작성일: 2025.05.11
 */

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public class BingoView : MonoBehaviour
    {
        [SerializeField] protected GameObject BingoPanel;
        [SerializeField] protected Button OpenBingoButton;
        public Button EndGameButton;
        public Button ReplaceButton;

        [Space(10)]
        [SerializeField] protected Image Bingo1Image;
        [SerializeField] protected Image Bingo2Image;
        [SerializeField] protected Image Bingo3Image;
        [SerializeField] protected Image Bingo4Image;
        [SerializeField] protected Image Bingo5Image;
        [SerializeField] protected Image Bingo6Image;
        protected List<Image> BingoImages => new List<Image>
        {
            Bingo1Image,
            Bingo2Image,
            Bingo3Image,
            Bingo4Image,
            Bingo5Image,
            Bingo6Image
        };

        [Space(10)]
        [SerializeField] protected Button Bingo1;
        [SerializeField] protected Button Bingo2;
        [SerializeField] protected Button Bingo3;
        [SerializeField] protected Button Bingo4;
        [SerializeField] protected Button Bingo5;
        [SerializeField] protected Button Bingo6;
        protected List<Button> BingoButtons => new List<Button>
        {
            Bingo1,
            Bingo2,
            Bingo3,
            Bingo4,
            Bingo5,
            Bingo6
        };
        

        [Space(10)]
        [SerializeField] protected Image BingoHighlight1;
        [SerializeField] protected Image BingoHighlight2;
        [SerializeField] protected Image BingoHighlight3;
        [SerializeField] protected Image BingoHighlight4;
        [SerializeField] protected Image BingoHighlight5;
        [SerializeField] protected Image BingoHighlight6;
        protected List<Image> BingoHighlights => new List<Image>
        {
            BingoHighlight1,
            BingoHighlight2,
            BingoHighlight3,
            BingoHighlight4,
            BingoHighlight5,
            BingoHighlight6
        };

        private Coroutine highlightCoroutine;

        public virtual void Initialize()
        {
            OpenBingoButton.onClick.AddListener(OpenBingo);
            // CloseBingoButton.onClick.AddListener(CloseBingo);

            foreach (var button in BingoButtons)
            {
                button.onClick.AddListener(() =>
                {
                    CloseBingo();
                });
            }

            BingoPanel.SetActive(false);
        }

        public void OpenBingo()
        {
            BingoPanel.SetActive(true);

            UpdateBingoState();
            highlightCoroutine = StartCoroutine(HighlightBingoButton());

            GlobalManager.Instance.SoundModel.PlayButtonClickSound();
        }

        public void CloseBingo()
        {
            BingoPanel.SetActive(false);

            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }

            GlobalManager.Instance.SoundModel.PlayButtonClickSound();
        }

        protected void UpdateBingoState()
        {
            var dataModel = GlobalManager.Instance.DataModel;

            //Bingo1Image.gameObject.SetActive(dataModel.ClearState.ClearState1);
            //Bingo2Image.gameObject.SetActive(dataModel.ClearState.ClearState2);
            //Bingo3Image.gameObject.SetActive(dataModel.ClearState.ClearState3);
            //Bingo4Image.gameObject.SetActive(dataModel.ClearState.ClearState4);
            //Bingo5Image.gameObject.SetActive(dataModel.ClearState.ClearState5);
            //Bingo6Image.gameObject.SetActive(dataModel.ClearState.ClearState6);

            //// 빙고만 연 경우 버튼 비활성화
            //if (dataModel.IsOpenBingo)
            //{
            //    for (int buttonIndex = 0; buttonIndex < BingoButtons.Count; buttonIndex++)
            //    {
            //        BingoButtons[buttonIndex].interactable = false;
            //        BingoHighlights[buttonIndex].gameObject.SetActive(false);
            //    }
            //}
            //else
            //{
            //    for (int buttonIndex = 0; buttonIndex < BingoButtons.Count; buttonIndex++)
            //    {
            //        BingoButtons[buttonIndex].interactable = buttonIndex == dataModel.StepIndex;
            //        BingoHighlights[buttonIndex].gameObject.SetActive(buttonIndex == dataModel.StepIndex);
            //    }
            //}
        }

        private IEnumerator HighlightBingoButton()
        {
            var dataModel = GlobalManager.Instance.DataModel;
            //var currentHighlight = BingoHighlights[dataModel.StepIndex];

            //currentHighlight.color = Color.white;

            float spendTime = 0.0f;
            while (true)
            {
                spendTime += Time.deltaTime;

                float weight = Mathf.Sin(spendTime * 2 * Mathf.PI / 1.0f);
                weight = (weight + 1.0f) / 2.0f; // Normalize to [0, 1]

                var colorA = Color.white;
                colorA.a = 0.3f;
                var colorB = Color.white;
                colorB.a = 0.8f;
                //currentHighlight.color = Color.Lerp(colorA, colorB, weight);
                yield return null;
            }
        }
    }
}
