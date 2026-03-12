/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.06.01
 *
 * STEP1~7 전체 세션 상태를 담는 데이터 구조체.
 * GlobalManager.Instance.DataModel.Session 으로 접근.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public enum TravelMode
    {
        Solo,       // 혼자
        Family,     // 가족
        Friends,    // 친구
        Couple,     // 연인
    }

    public enum TravelType
    {
        None,
        FestivalExplorer,   // 축제 탐험가
        SensoryRecorder,    // 감성 기록가
        FoodCollector,      // 맛집 수집가
        HealingWalker,      // 힐링 산책가
    }

    [Serializable]
    public class CourseCard
    {
        public string       CourseId;
        public string       Name;
        public string       Description;
        public List<string> TypeTags   = new List<string>(); // "FestivalExplorer" 등
        public List<string> ModeTags   = new List<string>(); // "Family","Couple" 등
        public List<string> Keywords   = new List<string>(); // "봄꽃","포토존" 등
        public string       AiHint;
    }

    [Serializable]
    public class SessionData
    {
        // ── STEP 1 ──────────────────────────────────────
        public TravelMode Mode = TravelMode.Solo;

        // ── STEP 2 ──────────────────────────────────────
        // BalanceAnswers[i] : 0=A선택 / 1=B선택
        public int[]      BalanceAnswers = new int[4];
        public TravelType Type           = TravelType.None;
        public string     SpiritKey      = "";   // "festival_spirit" 등
        public string     SpiritName     = "";   // "꽃축제 정령"

        // ── STEP 3 (팀 모드 전용) ─────────────────────
        public string ConcessionAnswer = "";
        public string RoleA            = "";
        public string RoleB            = "";
        public string TeamPromise      = "";

        // ── STEP 4 ──────────────────────────────────────
        public List<CourseCard> RecommendedCourses = new List<CourseCard>();
        public CourseCard       SelectedCourse     = null;
        public string           CourseReasonText   = "";

        // ── STEP 5 ──────────────────────────────────────
        public bool       IsARComplete = false;
        public List<bool> Stamps       = new List<bool> { false, false, false, false };

        // ── STEP 6 ──────────────────────────────────────
        public string       LabelText     = "";
        public List<string> LabelKeywords = new List<string>();

        // ── 유틸리티 ─────────────────────────────────
        public bool IsTeamMode => Mode != TravelMode.Solo;

        public void Reset()
        {
            Mode               = TravelMode.Solo;
            BalanceAnswers     = new int[4];
            Type               = TravelType.None;
            SpiritKey          = "";
            SpiritName         = "";
            ConcessionAnswer   = "";
            RoleA              = "";
            RoleB              = "";
            TeamPromise        = "";
            RecommendedCourses = new List<CourseCard>();
            SelectedCourse     = null;
            CourseReasonText   = "";
            IsARComplete       = false;
            Stamps             = new List<bool> { false, false, false, false };
            LabelText          = "";
            LabelKeywords      = new List<string>();
        }
    }
}
