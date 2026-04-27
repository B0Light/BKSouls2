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
        [Tooltip("이 룸 클리어 시 생성될 인터랙터블 보상 프리팹 목록 (ItemBox / SiteOfGrace / Shop 등 Interactable 구현체). " +
                 "각 항목은 RoomInstance의 rewardSpawnPoints[i]에 대응합니다. " +
                 "비워두면 RoomManager의 ItemBoxDatabase 기반 기본 보상 로직이 사용됩니다.")]
        public List<GameObject> rewardInteractablePrefabs = new();

        [Tooltip("이 룸 클리어 시 지급될 보상 상자의 기본 등급 (스테이지 깊이와 행운에 따라 상향될 수 있음)")]
        public ItemTier rewardBaseTier = ItemTier.Common;

        [Header("Sub Reward")]
        [Tooltip("전투 시작 시점에 생성되는 보조 보상 프리팹 목록입니다. 주로 재료 파밍 상자에 사용합니다.")]
        [HideInInspector] public List<GameObject> subRewardInteractablePrefabs = new();

        [Tooltip("Sub reward가 ItemBox 계열일 때 적용할 기본 티어입니다.")]
        public ItemTier subRewardBaseTier = ItemTier.Common;
    }
}
