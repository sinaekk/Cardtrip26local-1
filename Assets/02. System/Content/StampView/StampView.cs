/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.05.13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FUTUREVISION.Content
{
    public class StampView : BaseView
    {
        [Header("Stamp View")]
        public Image Mission1Image;
        public Image Mission2Image;
        public Image Mission3Image;
        public Image Mission4Image;
        [Space]
        public GameObject CompletePopup;
        public Button ToRewardButton;
        public Button PopupButton;
    }
}
