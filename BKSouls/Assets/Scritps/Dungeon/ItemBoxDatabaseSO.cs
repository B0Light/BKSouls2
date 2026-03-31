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
            public BoxType boxType;
            public GameObject prefab;

            [Tooltip("상대적 등장 가중치 (0이면 제외)")]
            [Min(0f)] public float weight;
        }

        [SerializeField] private List<BoxEntry> entries = new();

        public GameObject GetPrefab(BoxType type)
        {
            foreach (BoxEntry entry in entries)
            {
                if (entry.boxType == type)
                    return entry.prefab;
            }

            Debug.LogWarning($"[ItemBoxDatabase] '{type}'에 해당하는 프리팹이 없습니다.");
            return null;
        }

        /// <summary>
        /// 가중치 기반으로 랜덤 BoxType을 반환합니다.
        /// weight가 0이거나 prefab이 없는 항목은 제외됩니다.
        /// </summary>
        public BoxType GetRandomBoxType()
        {
            float total = 0f;
            foreach (BoxEntry e in entries)
            {
                if (e.prefab != null && e.weight > 0f)
                    total += e.weight;
            }

            if (total <= 0f)
            {
                Debug.LogWarning("[ItemBoxDatabase] 유효한 항목이 없습니다. 첫 번째 항목으로 대체합니다.");
                return entries.Count > 0 ? entries[0].boxType : BoxType.SupplyBox;
            }

            float rand = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            foreach (BoxEntry e in entries)
            {
                if (e.prefab == null || e.weight <= 0f) continue;

                cumulative += e.weight;
                if (rand <= cumulative)
                    return e.boxType;
            }

            return entries[entries.Count - 1].boxType;
        }
    }
}
