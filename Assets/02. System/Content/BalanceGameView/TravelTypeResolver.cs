/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.06.01
 *
 * 밸런스 게임 답변 배열 → TravelType 산출 룰.
 * BalanceAnswers[i] : 0 = A 선택 / 1 = B 선택
 *
 * 각 질문의 A/B 선택이 어떤 TravelType 점수에 해당하는지를
 * ScoreRules 배열로 관리한다.
 * 추후 Google Sheets 연동 시 ScoreRules를 외부 데이터로 교체한다.
 */

using System.Collections.Generic;
using UnityEngine;

namespace FUTUREVISION
{
    public static class TravelTypeResolver
    {
        // 질문당 A/B 선택이 가산하는 TravelType
        // ScoreRule[i] = (A 선택 시 가산 타입, B 선택 시 가산 타입)
        private static readonly (TravelType a, TravelType b)[] ScoreRules =
        {
            // Q1: 여행에서 더 끌리는 것은?
            // A: 북적이는 축제 현장  →  FestivalExplorer
            // B: 조용한 골목 카페    →  SensoryRecorder
            (TravelType.FestivalExplorer, TravelType.SensoryRecorder),

            // Q2: 여행 중 꼭 하고 싶은 것은?
            // A: 맛집 탐방           →  FoodCollector
            // B: 자연 속 산책        →  HealingWalker
            (TravelType.FoodCollector, TravelType.HealingWalker),

            // Q3: 여행 사진 스타일은?
            // A: 맛있는 음식/소품 클로즈업  →  FoodCollector
            // B: 감성적인 풍경 사진         →  SensoryRecorder
            (TravelType.FoodCollector, TravelType.SensoryRecorder),

            // Q4: 여행 마지막 날 하고 싶은 것은?
            // A: 남은 축제/이벤트 즐기기    →  FestivalExplorer
            // B: 느긋하게 공원 산책         →  HealingWalker
            (TravelType.FestivalExplorer, TravelType.HealingWalker),
        };

        /// <summary>
        /// answers 배열(0=A / 1=B)로 TravelType 을 산출한다.
        /// 동점 시 첫 번째 질문 답변 우선.
        /// </summary>
        public static TravelType Resolve(int[] answers)
        {
            var scores = new Dictionary<TravelType, int>
            {
                { TravelType.FestivalExplorer, 0 },
                { TravelType.SensoryRecorder,  0 },
                { TravelType.FoodCollector,    0 },
                { TravelType.HealingWalker,    0 },
            };

            int count = Mathf.Min(answers.Length, ScoreRules.Length);
            for (int i = 0; i < count; i++)
            {
                TravelType scored = answers[i] == 0 ? ScoreRules[i].a : ScoreRules[i].b;
                scores[scored]++;
            }

            // 최고 점수 타입 산출 (동점 시 첫 번째 질문 답 우선)
            TravelType result = TravelType.None;
            int best = -1;
            foreach (var kv in scores)
            {
                if (kv.Value > best)
                {
                    best   = kv.Value;
                    result = kv.Key;
                }
            }

            // 동점 tiebreak: 첫 번째 질문의 답변 타입을 우선
            if (answers.Length > 0)
            {
                TravelType firstAnswer = answers[0] == 0 ? ScoreRules[0].a : ScoreRules[0].b;
                int firstScore = scores[firstAnswer];
                if (firstScore == best) result = firstAnswer;
            }

            Debug.Log($"[TravelTypeResolver] Result={result}  scores={ScoreRules.Length}q");
            return result;
        }
    }
}
