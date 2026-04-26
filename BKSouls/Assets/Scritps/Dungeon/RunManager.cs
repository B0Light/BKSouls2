using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using Unity.Netcode;
using UnityEngine;

namespace BK
{
    public class RunManager : NetworkBehaviour
    {
        public static RunManager Instance { get; private set; }

        [SerializeField] private RoomManager roomManager;
        [SerializeField] private List<RoomType> floorSequence = new()
        {
            RoomType.Start,
            RoomType.Battle,
            RoomType.Battle,
            RoomType.Elite,
            RoomType.Boss
        };

        private readonly List<RoomPlan> floorPlan = new();
        private int currentRoomIndex = -1;
        public int CurrentRoomIndex => currentRoomIndex;
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

            foreach (RoomType type in floorSequence)
                AddRoom(type, rng);
        }

        private void AddRoom(RoomType type, System.Random rng)
        {
            List<int> validIndices = new();

            for (int i = 0; i < roomManager.RoomTemplates.Count; i++)
            {
                if (roomManager.RoomTemplates[i] != null && roomManager.RoomTemplates[i].roomType == type)
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
                RestoreAllPlayersClientRpc();
                ResetLevelUpUIClientRpc();
                ApplyClearRuneRewardClientRpc();
                if (roomManager != null)
                    roomManager.CleanupForSceneTransition();
                WorldSaveGameManager.Instance.LoadHoldScene();
                return;
            }

            RoomPlan plan = floorPlan[currentRoomIndex];
            RoomTemplateSO template = roomManager.RoomTemplates[plan.templateIndex];

            Debug.Log($"[RunManager] Load room {currentRoomIndex} / {plan.roomType} / {template.name}");

            roomManager.LoadRoom(plan, template);
        }

        public void RequestNextRoomFromRoomManager()
        {
            if (!IsServer)
                return;

            LoadNextRoom();
        }

        [ClientRpc]
        private void ApplyClearRuneRewardClientRpc()
        {
            PlayerManager localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject?.GetComponent<PlayerManager>();
            if (localPlayer == null) return;

            int balanceGain = localPlayer.playerStatsManager.runes;
            if (balanceGain > 0)
                WorldPlayerInventory.Instance.balance.Value += balanceGain;

            WorldSaveGameManager.Instance.ResetRunes();
            WorldSaveGameManager.Instance.currentCharacterData.balance = WorldPlayerInventory.Instance.balance.Value;
            WorldPlayerInventory.Instance.ClearEquipmentSlots();
            WorldPlayerInventory.Instance.SaveInventoryAndBackpackToCurrentCharacterData();
        }

        [ClientRpc]
        private void ResetLevelUpUIClientRpc()
        {
            PlayerUILevelUpManager levelUpManager = GUIController.Instance.playerUILevelUpManager;
            if (levelUpManager != null)
                levelUpManager.ResetSliders();
        }

        [ClientRpc]
        private void RestoreAllPlayersClientRpc()
        {
            PlayerManager localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject?.GetComponent<PlayerManager>();
            if (localPlayer == null)
                return;

            CharacterNetworkManager net = localPlayer.characterNetworkManager;
            net.currentHealth.Value = net.maxHealth.Value;
            net.currentStamina.Value = net.maxStamina.Value;
            net.currentFocusPoints.Value = net.maxFocusPoints.Value;

            Debug.Log("[RunManager] 셸터 귀환: 체력/기력/마나 전체 회복.");
        }
    }
}
