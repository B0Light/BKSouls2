using System;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Dungeon/Item Box Database")]
    public class ItemBoxDatabaseSO : ScriptableObject
    {
        [Serializable]
        public struct BoxEntry
        {
            public InteractableItemBox itemBox;

            [Tooltip("상대적 등장 가중치 (0이면 제외)")]
            [Min(0f)] public float weight;
        }

        [SerializeField] private List<BoxEntry> entries = new();

        [Tooltip("스테이지 진행 시 낮은 티어 박스의 가중치 감쇠 강도 (티어 차이 1당 비율)")]
        [Range(0f, 1f)] [SerializeField] private float tierPenaltyStrength = 0.3f;

        /// <param name="stageIndex">현재 스테이지 인덱스 (깊을수록 낮은 티어 박스 확률 감소)</param>
        public GameObject GetPrefab(int stageIndex = 0)
        {
            // 스테이지 깊이에 따른 기대 최소 티어 (3스테이지마다 +1, CalculateRewardTier와 동일 기준)
            int expectedTier = Mathf.Clamp(stageIndex / 3, 0, (int)ItemTier.Mythic);

            float totalWeight = 0f;
            foreach (BoxEntry entry in entries)
            {
                if (entry.itemBox != null && entry.weight > 0f)
                    totalWeight += CalcEffectiveWeight(entry, expectedTier);
            }

            if (totalWeight <= 0f)
            {
                Debug.LogWarning($"[ItemBoxDatabase] 적절한 보상이 없습니다.");
                return null;
            }

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            foreach (BoxEntry entry in entries)
            {
                if (entry.itemBox == null || entry.weight <= 0f) continue;
                cumulative += CalcEffectiveWeight(entry, expectedTier);
                if (roll < cumulative)
                    return entry.itemBox.gameObject;
            }

            Debug.LogWarning($"[ItemBoxDatabase] 적절한 보상이 없습니다.");
            return null;
        }

        private float CalcEffectiveWeight(BoxEntry entry, int expectedTier)
        {
            int tierGap = expectedTier - (int)entry.itemBox.BoxTier;
            if (tierGap <= 0)
                return entry.weight;

            float penalty = Mathf.Clamp01(tierGap * tierPenaltyStrength);
            return entry.weight * (1f - penalty);
        }
    }
}
