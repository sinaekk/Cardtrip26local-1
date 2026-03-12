/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.13
 */

using FUTUREVISION.WebAR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    [Serializable]
    public class QuizData
    {
        public GameObject quizObject;
        public Button[] answerButtons;
        public int correctAnswerIndex;
    }

    public class QuizView : MonoBehaviour
    {
        [SerializeField] protected List<QuizData> QuizDataList;
        private int currentQuizIndex = 0;
        [Space(10)]
        [SerializeField] protected GameObject OXPanel;
        [SerializeField] protected GameObject OMark;
        [SerializeField] protected GameObject XMark;

        public virtual void Initialize()
        {
            foreach (var quizData in QuizDataList)
            {
                quizData.quizObject.SetActive(false);
                for (int i = 0; i < quizData.answerButtons.Length; i++)
                {
                    int num = i + 1; // Capture the current index
                    quizData.answerButtons[i].onClick.AddListener(() => OnAnswerButtonClicked(quizData, num));
                }
            }

            //
            //ShowQuiz(GlobalManager.Instance.DataModel.StepIndex);
            OXPanel.SetActive(false);
        }

        public void ShowQuiz(int quizIndex)
        {
            currentQuizIndex = quizIndex;

            if (quizIndex < 0 || quizIndex >= QuizDataList.Count)
            {
                Debug.LogWarning("Quiz index out of range");
                return;
            }
            foreach (var quizData in QuizDataList)
            {
                quizData.quizObject.SetActive(false);
            }
            QuizDataList[quizIndex].quizObject.SetActive(true);
        }

        private void OnAnswerButtonClicked(QuizData quizData, int answerNum)
        {
            bool isCorrect = answerNum == quizData.correctAnswerIndex;
            OMark.SetActive(isCorrect);
            XMark.SetActive(!isCorrect);


            if (isCorrect)
            {
                //GlobalManager.Instance.DataModel.SetCorrectState(currentQuizIndex, true);
            }

            StartCoroutine(ShowQuizPanel(1.5f, isCorrect));
            GlobalManager.Instance.SoundModel.PlayMissionSound(isCorrect);
        }

        public IEnumerator ShowQuizPanel(float delay, bool isCorrect)
        {
            //OXPanel.SetActive(true);
            yield return new WaitForSeconds(delay);
            //OXPanel.SetActive(false);

            //if (isCorrect)
            //{
            //    WebARManager.Instance.ContentViewModel.SetContentState(ContentState.Finding);
            //}
        }
    }
}
