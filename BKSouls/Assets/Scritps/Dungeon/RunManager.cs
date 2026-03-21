using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace BK
{
    public class RunManager : NetworkBehaviour
    {
        public static RunManager Instance { get; private set; }

        [SerializeField] private List<RoomTemplateSO> allRoomTemplates = new();
        [SerializeField] private RoomManager roomManager;

        private readonly List<RoomPlan> floorPlan = new();
        private int currentRoomIndex = -1;
        private int runSeed;
        private bool hasStartedRun;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Debug.Log($"[RunManager] OnNetworkSpawn | IsServer:{IsServer} IsHost:{IsHost}");

            if (!IsServer)
                return;

            StartCoroutine(CoWaitAndStartRun());
        }

        private IEnumerator CoWaitAndStartRun()
        {
            yield return null;

            while (roomManager == null || !roomManager.IsSpawned)
            {
                yield return null;
            }

            if (hasStartedRun)
                yield break;

            hasStartedRun = true;
            StartRun();
        }

        public void StartRun()
        {
            Debug.Log("[RunManager] StartRun called");

            if (!IsServer)
                return;

            runSeed = Random.Range(int.MinValue, int.MaxValue);
            GenerateFloorPlan(runSeed);
            currentRoomIndex = -1;

            Debug.Log($"[RunManager] floorPlan.Count = {floorPlan.Count}");

            LoadNextRoom();
        }

        private void GenerateFloorPlan(int seed)
        {
            floorPlan.Clear();

            System.Random rng = new System.Random(seed);

            AddRoom(RoomType.Start, rng);
            AddRoom(RoomType.Battle, rng);
            AddRoom(RoomType.Battle, rng);
            AddRoom(RoomType.Elite, rng);
            AddRoom(RoomType.Boss, rng);
        }

        private void AddRoom(RoomType type, System.Random rng)
        {
            List<int> validIndices = new();

            for (int i = 0; i < allRoomTemplates.Count; i++)
            {
                if (allRoomTemplates[i] != null && allRoomTemplates[i].roomType == type)
                    validIndices.Add(i);
            }

            if (validIndices.Count == 0)
            {
                Debug.LogError($"[RunManager] No RoomTemplateSO found for type: {type}");
                return;
            }

            int selected = validIndices[rng.Next(validIndices.Count)];
            int roomSeed = rng.Next();

            floorPlan.Add(new RoomPlan(type, selected, roomSeed));
        }

        public void LoadNextRoom()
        {
            Debug.Log($"[RunManager] LoadNextRoom | IsServer:{IsServer}");

            if (!IsServer)
                return;

            currentRoomIndex++;

            if (currentRoomIndex >= floorPlan.Count)
            {
                Debug.Log("[RunManager] Run Finished.");
                return;
            }

            RoomPlan plan = floorPlan[currentRoomIndex];
            RoomTemplateSO template = allRoomTemplates[plan.templateIndex];

            Debug.Log($"[RunManager] Load room {currentRoomIndex} / {plan.roomType} / {template.name}");

            roomManager.LoadRoom(plan, template);
        }

        public void RequestNextRoomFromRoomManager()
        {
            if (!IsServer)
                return;

            LoadNextRoom();
        }
    }
}