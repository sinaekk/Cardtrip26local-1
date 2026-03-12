/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.10
 *
 */

using FUTUREVISION;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FUTUREVISION.WebAR
{
    public class MissionCompletePopup : MonoBehaviour
    {
        [Header("Mission Complete Popup")]
        [SerializeField] protected Image GuidTextImage1;
        [SerializeField] protected Image GuidTextImage2;
        [SerializeField] protected Image GuidTextImage3;
        [SerializeField] protected Image GuidTextImage4;
        [SerializeField] protected Image GuidTextImage5;
        [SerializeField] protected Image GuidTextImage6;

        protected List<Image> GuideTextImages => new List<Image>
        {
            GuidTextImage1, GuidTextImage2, GuidTextImage3, GuidTextImage4, GuidTextImage5, GuidTextImage6
        };

        //[Space(10)]
        //public UnityEvent OnClickGuidePopup;

        public virtual void Initialize()
        {
            //GuideText.Button.onClick.AddListener(() =>
            //{
            //    // 팝업 닫기
            //    gameObject.SetActive(false);

            //    GlobalManager.Instance.SoundModel.PlayButtonClickSound();
            //});
        }

        public void ShowGuide(bool newActive, Action callback = null)
        {
            // 팝업 열기
            gameObject.SetActive(newActive);

            var dataModel = GlobalManager.Instance.DataModel;
            var guideTextImages = GuideTextImages;

            foreach (var image in guideTextImages)
            {
                image.gameObject.SetActive(false);
            }
            //guideTextImages[dataModel.StepIndex].gameObject.SetActive(true);
        }
    }
}
