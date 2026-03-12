/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.13
 */

using System;
using System.Collections;
using System.Collections.Generic;
using FUTUREVISION.WebAR;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public enum ERecommendationState
    {
        None,
        SelectView,
        QuestionView,
        WaitingView,
        ResultView,
    }

    public class RecommendationView : BaseView
    {
        [Header("Recommendation View")]
        [Header("Recommendation View/선택화면")]
        public GameObject SelectView;
        public Button StartRecommendationButton;
        public Button SkipRecommendationButton;

        [Header("Recommendation View/설문조사")]
        public GameObject QuestionView;
        public Image TextImage;
        public Button Select1Button;
        public Button Select2Button;
        public Button Select3Button;
        [Space]
        public int CurrentIndex = 0;
        [Serializable]
        public class Question
        {
            public Sprite TextSprite;
            public Sprite Select1Sprit;
            public Sprite Select2Sprit;
            public Sprite Select3Sprit;
        }
        public List<Question> QuestionList;
        
        [Header("Recommendation View/대기 페이지")]
        public GameObject WaitingView;


        [Header("Recommendation View/추천 페이지")]
        public GameObject ResultView;
        [Space(10)]
        public Image SpotImage;
        public TextMeshProUGUI SpotNameText;
        [Space(10)]
        public Button ToNextButton;
        public Button ToBeforeButton;
        [Space(10)]
        public Button StartButton;

        [Header("Temp")]
        public GameObject TEMP_A_Course;

        public void SetQuestion(int index)
        {
            if (index < 0 || index >= QuestionList.Count)
                return;
            CurrentIndex = index;
            var question = QuestionList[index];
            TextImage.sprite = question.TextSprite;
            Select1Button.image.sprite = question.Select1Sprit;
            Select2Button.image.sprite = question.Select2Sprit;
            Select3Button.image.sprite = question.Select3Sprit;
        }

        public void SetState(ERecommendationState state)
        {
            SelectView.SetActive(false);
            QuestionView.SetActive(false);
            WaitingView.SetActive(false);
            ResultView.SetActive(false);
            TEMP_A_Course.SetActive(false);
            switch (state)
            {
                case ERecommendationState.None:
                    break;
                case ERecommendationState.SelectView:
                    SelectView.SetActive(true);
                    break;
                case ERecommendationState.QuestionView:
                    QuestionView.SetActive(true);
                    break;
                case ERecommendationState.WaitingView:
                    WaitingView.SetActive(true);
                    break;
                case ERecommendationState.ResultView:
                    ResultView.SetActive(true);
                    if (WebARManager.Instance.ContentViewModel.CurrentCourse == 0)
                    {
                        TEMP_A_Course.SetActive(true);
                        // var animator = TEMP_A_Course.GetComponent<Animation>();
                        // animator.Play(animator.clip.name);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
