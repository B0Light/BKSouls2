using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Dungeon/Room Template")]
    public class RoomTemplateSO : ScriptableObject
    {
        [Header("Basic")]
        public int templateId;
        public RoomType roomType;
        public GameObject roomPrefab;

        [Header("AI Spawner Prefabs")]
        public List<AICharacterSpawner> enemySpawnerPrefabs = new();

        [Header("Doors (optional – overrides RoomManager defaults)")]
        public GameObject entryDoorPrefab;
        public GameObject exitDoorPrefab;

        [Header("Reward")]
        [Tooltip("이 룸 클리어 시 지급될 보상 상자의 기본 등급 (스테이지 깊이와 행운에 따라 상향될 수 있음)")]
        public ItemTier rewardBaseTier = ItemTier.Common;
    }
}