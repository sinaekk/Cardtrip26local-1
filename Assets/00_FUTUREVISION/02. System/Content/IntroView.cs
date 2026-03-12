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
    public class IntroView : BaseView
    {
        [Header("IntroView / 기존 (호환성 유지)")]
        public Button StartButton;

        [Header("IntroView / STEP1 여행 모드 선택 (신규)")]
        public Button SoloButton;     // 혼자
        public Button FamilyButton;   // 가족
        public Button FriendsButton;  // 친구
        public Button CoupleButton;   // 연인
    }
}
