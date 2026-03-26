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
        public GameObject rewardPrefab;
    }
}