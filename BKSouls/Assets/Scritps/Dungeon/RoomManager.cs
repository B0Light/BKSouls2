using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace BK
{
    public class RoomManager : NetworkBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [Header("Scene Roots")]
        [SerializeField] private Transform roomRoot;
        [SerializeField] private Transform networkRuntimeRoot;

        [Header("Room Template Database")]
        [SerializeField] private List<RoomTemplateSO> roomTemplates = new();

        [Header("Runtime Prefabs")]
        [SerializeField] private GameObject doorPrefab;

        [Header("Enemy Count")]
        [SerializeField] private int baseBattleEnemyCount = 3;
        [SerializeField] private int eliteEnemyCount = 5;
        [SerializeField] private int bossEnemyCount = 1;

        private RoomInstance currentRoomInstance;
        private RoomPlan currentPlan;
        private RoomTemplateSO currentTemplate;

        private readonly List<AICharacterManager> aliveEnemies = new();
        private readonly List<AICharacterSpawner> runtimeSpawners = new();

        private NetworkObject currentRewardObject;
        private RoguelikeRoomDoor currentEntryDoor;
        private RoguelikeRoomDoor currentExitDoor;

        private readonly NetworkVariable<int> currentRoomState = new((int)RoomState.None);
        private readonly NetworkVariable<int> currentRoomTemplateIndex = new(-1);

        public RoomState CurrentState => (RoomState)currentRoomState.Value;
        public RoomInstance CurrentRoomInstance => currentRoomInstance;
        public RoomPlan CurrentPlan => currentPlan;
        public RoomTemplateSO CurrentTemplate => currentTemplate;

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

            currentRoomTemplateIndex.OnValueChanged += OnRoomTemplateIndexChanged;

            // 늦게 들어온 클라이언트 대응
            if (!IsServer && currentRoomTemplateIndex.Value >= 0)
            {
                LoadRoomGeometryLocalByIndex(currentRoomTemplateIndex.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            currentRoomTemplateIndex.OnValueChanged -= OnRoomTemplateIndexChanged;

            if (Instance == this)
                Instance = null;

            base.OnNetworkDespawn();
        }

        private void OnRoomTemplateIndexChanged(int previousValue, int newValue)
        {
            if (IsServer)
                return;

            if (newValue < 0)
            {
                DestroyRoomGeometry();
                return;
            }

            LoadRoomGeometryLocalByIndex(newValue);
        }

        public void LoadRoom(RoomPlan plan, RoomTemplateSO template)
        {
            if (!IsServer)
                return;

            if (plan == null)
            {
                Debug.LogError("[RoomManager] LoadRoom failed: plan is null.");
                return;
            }

            if (template == null)
            {
                Debug.LogError("[RoomManager] LoadRoom failed: template is null.");
                return;
            }

            int templateIndex = roomTemplates.IndexOf(template);
            if (templateIndex < 0)
            {
                Debug.LogError($"[RoomManager] Template '{template.name}' is not registered in roomTemplates.");
                return;
            }

            currentPlan = plan;
            currentTemplate = template;

            SetState(RoomState.Loading);

            CleanupCurrentRoomNetworkObjectsOnly();
            LoadRoomGeometryServer(template, templateIndex);

            SpawnDoors();
            WarpAllPlayersToRoom();
            PrepareRoom();

            switch (plan.roomType)
            {
                case RoomType.Start:
                case RoomType.Event:
                case RoomType.Shop:
                case RoomType.Rest:
                    ClearNonCombatRoom();
                    break;

                case RoomType.Battle:
                case RoomType.Elite:
                case RoomType.Boss:
                    StartCombatRoom();
                    break;

                default:
                    Debug.LogWarning($"[RoomManager] Unhandled RoomType: {plan.roomType}");
                    ClearNonCombatRoom();
                    break;
            }
        }

        private void SetState(RoomState state)
        {
            if (!IsServer)
                return;

            currentRoomState.Value = (int)state;
        }

        private void CleanupCurrentRoomNetworkObjectsOnly()
        {
            DespawnAllEnemies();
            DestroyRuntimeSpawners();
            DespawnReward();
            DespawnDoors();
        }

        private void DespawnAllEnemies()
        {
            for (int i = aliveEnemies.Count - 1; i >= 0; i--)
            {
                AICharacterManager enemy = aliveEnemies[i];
                if (enemy == null)
                    continue;

                NetworkObject netObj = enemy.NetworkObject;
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
                else
                    Destroy(enemy.gameObject);
            }

            aliveEnemies.Clear();
        }

        private void DestroyRuntimeSpawners()
        {
            for (int i = runtimeSpawners.Count - 1; i >= 0; i--)
            {
                if (runtimeSpawners[i] != null)
                    Destroy(runtimeSpawners[i].gameObject);
            }

            runtimeSpawners.Clear();
        }

        private void DespawnReward()
        {
            if (currentRewardObject == null)
                return;

            if (currentRewardObject.IsSpawned)
                currentRewardObject.Despawn(true);
            else
                Destroy(currentRewardObject.gameObject);

            currentRewardObject = null;
        }

        private void DespawnDoors()
        {
            if (currentEntryDoor != null)
            {
                NetworkObject entryNetObj = currentEntryDoor.NetworkObject;
                if (entryNetObj != null && entryNetObj.IsSpawned)
                    entryNetObj.Despawn(true);
                else
                    Destroy(currentEntryDoor.gameObject);

                currentEntryDoor = null;
            }

            if (currentExitDoor != null)
            {
                NetworkObject exitNetObj = currentExitDoor.NetworkObject;
                if (exitNetObj != null && exitNetObj.IsSpawned)
                    exitNetObj.Despawn(true);
                else
                    Destroy(currentExitDoor.gameObject);

                currentExitDoor = null;
            }
        }

        private void LoadRoomGeometryServer(RoomTemplateSO template, int templateIndex)
        {
            DestroyRoomGeometry();
            SpawnRoomGeometryLocal(template);

            currentRoomTemplateIndex.Value = templateIndex;
        }

        private void LoadRoomGeometryLocalByIndex(int templateIndex)
        {
            if (templateIndex < 0 || templateIndex >= roomTemplates.Count)
            {
                Debug.LogError($"[RoomManager] Invalid template index: {templateIndex}");
                return;
            }

            RoomTemplateSO template = roomTemplates[templateIndex];
            if (template == null)
            {
                Debug.LogError($"[RoomManager] Room template at index {templateIndex} is null.");
                return;
            }

            DestroyRoomGeometry();
            SpawnRoomGeometryLocal(template);
        }

        private void DestroyRoomGeometry()
        {
            if (currentRoomInstance == null)
                return;

            Destroy(currentRoomInstance.gameObject);
            currentRoomInstance = null;
        }

        private void SpawnRoomGeometryLocal(RoomTemplateSO template)
        {
            if (template == null)
            {
                Debug.LogError("[RoomManager] SpawnRoomGeometryLocal failed: template is null.");
                return;
            }

            if (template.roomPrefab == null)
            {
                Debug.LogError("[RoomManager] SpawnRoomGeometryLocal failed: roomPrefab is null.");
                return;
            }

            Transform parent = roomRoot != null ? roomRoot : transform;
            GameObject roomObj = Instantiate(template.roomPrefab, parent);

            currentRoomInstance = roomObj.GetComponent<RoomInstance>();
            if (currentRoomInstance == null)
            {
                Debug.LogError($"[RoomManager] Room prefab '{template.roomPrefab.name}' must contain RoomInstance.");
            }
        }

        private void SpawnDoors()
        {
            if (!IsServer)
                return;

            if (currentRoomInstance == null)
            {
                Debug.LogError("[RoomManager] SpawnDoors failed: currentRoomInstance is null.");
                return;
            }

            if (doorPrefab == null)
            {
                Debug.LogWarning("[RoomManager] doorPrefab is null. Doors will not be spawned.");
                return;
            }

            if (currentRoomInstance.EntryDoorAnchor != null)
            {
                currentEntryDoor = SpawnSingleDoor(currentRoomInstance.EntryDoorAnchor, "EntryDoor");
                if (currentEntryDoor != null)
                    currentEntryDoor.SetDoorRoleServer(RoguelikeDoorRole.Entry);
            }

            if (currentRoomInstance.ExitDoorAnchor != null)
            {
                currentExitDoor = SpawnSingleDoor(currentRoomInstance.ExitDoorAnchor, "ExitDoor");
                if (currentExitDoor != null)
                    currentExitDoor.SetDoorRoleServer(RoguelikeDoorRole.Exit);
            }
        }

        private RoguelikeRoomDoor SpawnSingleDoor(Transform anchor, string debugName)
        {
            Transform parent = networkRuntimeRoot != null ? networkRuntimeRoot : null;

            GameObject doorObj = Instantiate(
                doorPrefab,
                anchor.position,
                anchor.rotation,
                parent);

            doorObj.name = debugName;

            NetworkObject netObj = doorObj.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError($"[RoomManager] {debugName} prefab must contain NetworkObject.");
                Destroy(doorObj);
                return null;
            }

            RoguelikeRoomDoor door = doorObj.GetComponent<RoguelikeRoomDoor>();
            if (door == null)
            {
                Debug.LogError($"[RoomManager] {debugName} prefab must contain RoguelikeRoomDoor.");
                Destroy(doorObj);
                return null;
            }

            netObj.Spawn(true);
            return door;
        }

        private void WarpAllPlayersToRoom()
        {
            if (!IsServer) return;

            if (currentRoomInstance == null)
            {
                Debug.LogError("[RoomManager] WarpAllPlayersToRoom failed: currentRoomInstance is null.");
                return;
            }

            var clients = NetworkManager.Singleton.ConnectedClientsList;

            for (int i = 0; i < clients.Count; i++)
            {
                NetworkClient client = clients[i];
                if (client?.PlayerObject == null) continue;

                Transform spawnPoint = currentRoomInstance.GetPlayerSpawnPoint(i);
                if (spawnPoint == null)
                {
                    Debug.LogWarning($"[RoomManager] Missing spawn point for player index {i}.");
                    continue;
                }

                // NetworkTransform이 있으면 Teleport로 처리 (클라이언트 권한도 서버에서 덮어쓸 수 있음)

                // 클라이언트 본인에게 RPC로 물리/CC 처리 포함한 이동 명령
                WarpPlayerClientRpc(
                    spawnPoint.position,
                    spawnPoint.rotation,
                    new ClientRpcParams
                    {
                        Send = new ClientRpcSendParams
                        {
                            TargetClientIds = new[] { client.ClientId }
                        }
                    }
                );
            }
        }

        [ClientRpc]
        private void WarpPlayerClientRpc(Vector3 position, Quaternion rotation, ClientRpcParams _ = default)
        {
            NetworkObject localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (localPlayer == null) return;

            // NetworkTransform Teleport는 서버에서 이미 처리됨
            // 클라이언트에서는 CC/Rigidbody만 처리
            CharacterController cc = localPlayer.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            localPlayer.transform.SetPositionAndRotation(position, rotation);

            Rigidbody rb = localPlayer.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (cc != null) cc.enabled = true;
        }

        private void ApplyWarp(NetworkObject playerNetObj, Vector3 position, Quaternion rotation)
        {
            CharacterController cc = playerNetObj.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerNetObj.transform.SetPositionAndRotation(position, rotation);

            Rigidbody rb = playerNetObj.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (cc != null) cc.enabled = true;
        }
        
        private void PrepareRoom()
        {
            if (!IsServer)
                return;

            if (currentEntryDoor != null)
            {
                currentEntryDoor.SetDoorLockedServer(false);
                currentEntryDoor.SetDoorOpenServer(true);
            }

            if (currentExitDoor != null)
            {
                currentExitDoor.SetDoorLockedServer(false);
                currentExitDoor.SetDoorOpenServer(false);
            }
        }

        private void ClearNonCombatRoom()
        {
            SetState(RoomState.Cleared);

            if (currentExitDoor != null)
                currentExitDoor.SetDoorOpenServer(true);

            SpawnRewardIfNeeded();
        }

        private void StartCombatRoom()
        {
            SetState(RoomState.Combat);

            int enemyCount = GetEnemyCountByRoomType(currentPlan.roomType);
            SpawnEnemies(enemyCount);
        }

        private int GetEnemyCountByRoomType(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Battle:
                    return baseBattleEnemyCount;
                case RoomType.Elite:
                    return eliteEnemyCount;
                case RoomType.Boss:
                    return bossEnemyCount;
                default:
                    return 0;
            }
        }

        private void SpawnEnemies(int count)
        {
            if (!IsServer)
                return;

            if (currentRoomInstance == null)
            {
                Debug.LogError("[RoomManager] SpawnEnemies failed: currentRoomInstance is null.");
                ClearRoom();
                return;
            }

            if (currentTemplate == null)
            {
                Debug.LogError("[RoomManager] SpawnEnemies failed: currentTemplate is null.");
                ClearRoom();
                return;
            }

            if (currentTemplate.enemySpawnerPrefabs == null || currentTemplate.enemySpawnerPrefabs.Count == 0)
            {
                Debug.LogWarning("[RoomManager] No enemy spawner prefabs configured. Room will clear automatically.");
                ClearRoom();
                return;
            }

            System.Random rng = new System.Random(currentPlan.seed);

            for (int i = 0; i < count; i++)
            {
                AICharacterSpawner spawnerPrefab =
                    currentTemplate.enemySpawnerPrefabs[rng.Next(currentTemplate.enemySpawnerPrefabs.Count)];

                if (spawnerPrefab == null)
                    continue;

                Transform spawnPoint = currentRoomInstance.GetEnemySpawnPoint(i);
                if (spawnPoint == null)
                {
                    Debug.LogWarning($"[RoomManager] Missing enemy spawn point for enemy index {i}.");
                    continue;
                }

                Transform parent = networkRuntimeRoot != null ? networkRuntimeRoot : null;

                AICharacterSpawner runtimeSpawner = Instantiate(
                    spawnerPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation,
                    parent);

                runtimeSpawners.Add(runtimeSpawner);

                AICharacterManager aiCharacter = runtimeSpawner.AttemptToSpawnCharacter();
                if (aiCharacter == null)
                    continue;

                RegisterSpawnedAI(aiCharacter);
            }

            if (aliveEnemies.Count == 0)
                ClearRoom();
        }

        private void RegisterSpawnedAI(AICharacterManager aiCharacter)
        {
            if (aiCharacter == null)
                return;

            if (!aliveEnemies.Contains(aiCharacter))
                aliveEnemies.Add(aiCharacter);

            RoguelikeAIReporter reporter = aiCharacter.GetComponent<RoguelikeAIReporter>();
            if (reporter == null)
                reporter = aiCharacter.gameObject.AddComponent<RoguelikeAIReporter>();

            reporter.Initialize(this, aiCharacter);
        }

        public void NotifyEnemyDead(AICharacterManager enemy)
        {
            if (!IsServer)
                return;

            if (enemy == null)
                return;

            bool removed = aliveEnemies.Remove(enemy);
            if (!removed)
                return;

            if (aliveEnemies.Count == 0)
                ClearRoom();
        }

        private void ClearRoom()
        {
            if (!IsServer)
                return;

            SetState(RoomState.Reward);

            SpawnRewardIfNeeded();

            if (currentExitDoor != null)
                currentExitDoor.SetDoorOpenServer(true);

            SetState(RoomState.Cleared);
        }

        private void SpawnRewardIfNeeded()
        {
            if (!IsServer)
                return;

            if (currentRewardObject != null)
                return;

            if (currentTemplate == null)
                return;

            if (currentTemplate.rewardPrefab == null)
                return;

            if (currentRoomInstance == null || currentRoomInstance.RewardSpawnPoint == null)
                return;

            Transform parent = networkRuntimeRoot != null ? networkRuntimeRoot : null;

            GameObject rewardObj = Instantiate(
                currentTemplate.rewardPrefab,
                currentRoomInstance.RewardSpawnPoint.position,
                currentRoomInstance.RewardSpawnPoint.rotation,
                parent);

            NetworkObject netObj = rewardObj.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("[RoomManager] Reward prefab must contain NetworkObject.");
                Destroy(rewardObj);
                return;
            }

            netObj.Spawn(true);
            currentRewardObject = netObj;
        }

        public void TryMoveNextRoomServer()
        {
            if (!IsServer)
                return;

            if (CurrentState != RoomState.Cleared)
                return;

            SetState(RoomState.Transition);

            if (RunManager.Instance == null)
            {
                Debug.LogError("[RoomManager] RunManager.Instance is null.");
                return;
            }

            RunManager.Instance.RequestNextRoomFromRoomManager();
        }

        public bool IsCurrentRoomCleared()
        {
            return CurrentState == RoomState.Cleared;
        }

        public IReadOnlyList<AICharacterManager> GetAliveEnemies()
        {
            return aliveEnemies;
        }

        public RoguelikeRoomDoor GetEntryDoor()
        {
            return currentEntryDoor;
        }

        public RoguelikeRoomDoor GetExitDoor()
        {
            return currentExitDoor;
        }
    }
}