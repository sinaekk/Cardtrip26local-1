/*
 * 작성자: Kim Bummoo
 * 작성일: 2025.06.01
 *
 * 정령 데이터 클래스.
 * TravelType → SpiritData 1:1 매핑 (fallback 하드코딩 포함).
 * 추후 Google Sheets CSV로 교체 가능.
 */

using System.Collections.Generic;

namespace FUTUREVISION
{
    [System.Serializable]
    public class SpiritData
    {
        public string     Key;          // "festival_spirit"
        public TravelType Type;
        public string     Name;         // "꽃축제 정령"
        public string     Description;
        public string     ArPrefabKey;  // Addressable 키 (AR 오브젝트 프리팹명)
    }

    /// <summary>
    /// 정령 테이블 – 하드코딩 fallback.
    /// Sheets 연동 시 이 값을 덮어쓴다.
    /// </summary>
    public static class SpiritTable
    {
        public static readonly List<SpiritData> All = new List<SpiritData>
        {
            new SpiritData
            {
                Key         = "festival_spirit",
                Type        = TravelType.FestivalExplorer,
                Name        = "꽃축제 정령",
                Description = "활기차고 탐험을 좋아하는 정령",
                ArPrefabKey = "Spirit_Festival",
            },
            new SpiritData
            {
                Key         = "sensory_spirit",
                Type        = TravelType.SensoryRecorder,
                Name        = "감성 기록 정령",
                Description = "섬세하고 아름다운 순간을 담는 정령",
                ArPrefabKey = "Spirit_Sensory",
            },
            new SpiritData
            {
                Key         = "food_spirit",
                Type        = TravelType.FoodCollector,
                Name        = "맛집 수집 정령",
                Description = "맛있는 것을 찾아다니는 식도락 정령",
                ArPrefabKey = "Spirit_Food",
            },
            new SpiritData
            {
                Key         = "healing_spirit",
                Type        = TravelType.HealingWalker,
                Name        = "힐링 산책 정령",
                Description = "자연 속에서 쉬며 힐링하는 정령",
                ArPrefabKey = "Spirit_Healing",
            },
        };

        public static SpiritData GetByType(TravelType type)
        {
            foreach (var s in All)
            {
                if (s.Type == type) return s;
            }
            return All[0]; // fallback: 첫 번째
        }

        public static SpiritData GetByKey(string key)
        {
            foreach (var s in All)
            {
                if (s.Key == key) return s;
            }
            return All[0];
        }
    }
}
