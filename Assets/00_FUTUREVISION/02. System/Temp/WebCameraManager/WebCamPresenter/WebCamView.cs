/*
 * 
 *
 */

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION
{
    public enum EWebCAmFitMode
    {
        None,
        Original,
        Fill,
        Fit,
        MatchHeight,
        MatchWidth,
    }

    public class WebCamView : BaseView
    {
        [Header("WebCamView")]
        [SerializeField] private EWebCAmFitMode fitMode;

        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image maskImage;
        [SerializeField] private RawImage camRawImage;

        private RectTransform rectTransform;

        public EWebCAmFitMode FitMode { get => fitMode; }

        public override void Initialize()
        {
            base.Initialize();

            rectTransform = GetComponent<RectTransform>();
        }

        public virtual void UpdateImageSize(int2 size)
        {
            float widthRatio = rectTransform.rect.width / size.x;
            float heightRatio = rectTransform.rect.height / size.y;

            float ratio = 1.0f;

            switch (FitMode)
            {
                case EWebCAmFitMode.Original:
                    {
                        camRawImage.rectTransform.sizeDelta = new Vector2(size.x, size.y);
                    }
                    break;
                case EWebCAmFitMode.Fill:
                    {
                        // Image가 화면에 꽉 차게 설정
                        ratio = Mathf.Max(widthRatio, heightRatio);
                    }
                    break;
                case EWebCAmFitMode.Fit:
                    {
                        // Image가 화면에 맞게 설정
                        ratio = Mathf.Min(widthRatio, heightRatio);
                    }
                    break;
                case EWebCAmFitMode.MatchHeight:
                    {
                        // Image의 높이를 화면에 맞게 설정
                        ratio = rectTransform.rect.height / size.y;
                    }
                    break;
                case EWebCAmFitMode.MatchWidth:
                    {
                        // Image의 너비를 화면에 맞게 설정
                        ratio = rectTransform.rect.width / size.x;
                    }
                    break;
                default:
                    throw new System.NotImplementedException();
            }
            camRawImage.rectTransform.sizeDelta = new Vector2(size.x * ratio, size.y * ratio);
        }

        public virtual void SetBackgroundImage(Sprite sprite)
        {
            backgroundImage.sprite = sprite;
        }

        public virtual void SetCamTexture(WebCamTexture webCamTexture)
        {
            camRawImage.texture = webCamTexture;
        }
    }

}