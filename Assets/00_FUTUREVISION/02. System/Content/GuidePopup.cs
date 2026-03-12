/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.10
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FUTUREVISION.WebAR
{
    public class GuidePopup : MonoBehaviour
    {
        [Header("Guide Popup")]
        [SerializeField] protected Image GuidTextImage1;
        [SerializeField] protected Image GuidTextImage2;
        [SerializeField] protected Image GuidTextImage3;
        [SerializeField] protected Image GuidTextImage4;
        [SerializeField] protected Image GuidTextImage5;
        [SerializeField] protected Image GuidTextImage6;
        [Space(10)]
        [SerializeField] private Image handImage;
        protected List<Image> GuideTextImages => new List<Image>
        {
            GuidTextImage1, GuidTextImage2, GuidTextImage3, GuidTextImage4, GuidTextImage5, GuidTextImage6
        };
        [Space(10)]
        public Slider ProgressSlider;

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

        private void Update()
        {
            // 손 알파값 0.5 ~ 1, 1초마다
            if (handImage != null)
            {
                float alpha = Mathf.PingPong(Time.time, 0.5f) + 0.5f; // 0.5 ~ 1
                Color color = handImage.color;
                color.a = alpha;
                handImage.color = color;
            }
        }

        public void ShowGuide(bool newActive)
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
            
            if (newActive == true)
            {
                //WebARManager.Instance.EndGuide();
            }
        }

        public void SetProgress(float progress)
        {
            ProgressSlider.value = progress;
        }
    }
}
