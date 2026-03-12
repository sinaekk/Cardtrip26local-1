/*
 * 작성자: #AUTHOR#
 * 작성일: #DATE#
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION
{
    [RequireComponent(typeof(Image))]
    public class ImageSwapper : MonoBehaviour
    {
        private Image imageComponent;

        public Sprite Image_korean;
        public Sprite Image_english;
        public Sprite Image_japanese;
        public Sprite Image_chinese;

        private void OnEnable()
        {
            if (imageComponent == null)
            {
                imageComponent = GetComponent<Image>();
            }

            UpdateImage();
        }

        private void UpdateImage()
        {
            //var lang = GlobalManager.Instance.DataModel.Lang;
            //switch (lang)
            //{
            //    case "ko":
            //        imageComponent.sprite = Image_korean;
            //        break;
            //    case "en":
            //        imageComponent.sprite = Image_english;
            //        break;
            //    case "ja":
            //        imageComponent.sprite = Image_japanese;
            //        break;
            //    case "zh":
            //        imageComponent.sprite = Image_chinese;
            //        break;
            //    default:
            //        imageComponent.sprite = Image_korean;
            //        break;
            //}
        }
    }
}
