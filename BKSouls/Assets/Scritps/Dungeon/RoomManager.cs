using System.Collections;
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

        public IReadOnlyList<RoomTemplateSO> RoomTemplates => roomTemplates;

        [Header("NavMesh")]
        [SerializeField] private DungeonNavMeshBuilder navMeshBuilder;

        [Header("Item Box Database")]
        [SerializeField] private ItemBoxDatabaseSO itemBoxDatabase;

        [Header("Site of Grace")]
        [SerializeField] private GameObject siteOfGracePrefab;

        [Header("Runtime Prefabs")]
        [SerializeField] private GameObject entryDoorPrefab;
        [SerializeField] private GameObject exitDoorPrefab;

        [Header("Enemy Count")]
        [SerializeField] private int minBattleEnemyCount = 2;
        [SerializeField] private int maxBattleEnemyCount = 4;
        [SerializeField] private int minEliteEnemyCount = 3;
        [SerializeField] private int maxEliteEnemyCount = 6;
        [SerializeField] private int minBossEnemyCount  = 1;
        [SerializeField] private int maxBossEnemyCount  = 1;

        [Header("Room Clear Rune Rewards")]
        [SerializeField] private int battleRoomRuneReward = 100;
        [SerializeField] private int eliteRoomRuneReward  = 300;
        [SerializeField] private int bossRoomRuneReward   = 800;

        private RoomInstance currentRoomInstance;
        private RoomPlan currentPlan;
        private RoomTemplateSO currentTemplate;

        private readonly List<AICharacterManager> aliveEnemies = new();
        private readonly List<AICharacterManager> spawnedEnemies = new();
        private readonly List<AICharacterSpawner> runtimeSpawners = new();

        private readonly List<NetworkObject> currentRewardObjects = new();
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
                navMeshBuilder?.Clear();
                DestroyRoomGeometry();
                return;
            }

            LoadRoomGeometryLocalByIndex(newValue);

            // 클라이언트는 AI를 직접 실행하지 않으므로 fire-and-forget으로 재빌드한다.
            navMeshBuilder?.Rebuild();
        }

        public void LoadRoom(RoomPlan plan, RoomTemplateSO template)
        {
            if (!IsServer)
                return;

            StartCoroutine(CoLoadRoom(plan, template));
        }

        private IEnumerator CoLoadRoom(RoomPlan plan, RoomTemplateSO template)
        {
            if (plan == null)
            {
                Debug.LogError("[RoomManager] LoadRoom failed: plan is null.");
                yield break;
            }

            if (template == null)
            {
                Debug.LogError("[RoomManager] LoadRoom failed: template is null.");
                yield break;
            }

            int templateIndex = roomTemplates.IndexOf(template);
            if (templateIndex < 0)
            {
                Debug.LogError($"[RoomManager] Template '{template.name}' is not registered in roomTemplates.");
                yield break;
            }

            currentPlan = plan;
            currentTemplate = template;

            SetState(RoomState.Loading);

            ShowLoadingScreenClientRpc();

            CleanupCurrentRoomNetworkObjectsOnly();
            LoadRoomGeometryServer(template, templateIndex);

            SpawnDoors();
            WarpAllPlayersToRoom();
            PrepareRoom();

            // ── NavMesh 재빌드 ──────────────────────────────────────
            // 지오메트리 로드 후 NavMesh를 서버에서 빌드한다.
            // NavMesh Agent(적 AI)는 이 완료 후에 스폰해야 경로 탐색 오류가 없다.
            if (navMeshBuilder != null)
            {
                bool navMeshReady = false;
                navMeshBuilder.Rebuild(() => navMeshReady = true);
                yield return new WaitUntil(() => navMeshReady);
            }
            else
            {
                Debug.LogWarning("[RoomManager] DungeonNavMeshBuilder가 할당되지 않았습니다. NavMesh 없이 진행합니다.");
                yield return null;
            }
            // ────────────────────────────────────────────────────────

            HideLoadingScreenClientRpc();

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

        // 씬 전환 직전 호출 — 모든 네트워크 오브젝트(적/보상/문/낙하 아이템)를 정리한다
        public void CleanupForSceneTransition()
        {
            if (!IsServer)
                return;

            CleanupCurrentRoomNetworkObjectsOnly();
            DestroyRoomGeometry();
        }

        private void CleanupCurrentRoomNetworkObjectsOnly()
        {
            DespawnAllEnemies();
            DestroyRuntimeSpawners();
            DespawnReward();
            DespawnDoors();
            DespawnAllDroppedItems();
        }

        private void DespawnAllDroppedItems()
        {
            PickUpItemInteractable[] droppedItems = FindObjectsByType<PickUpItemInteractable>(FindObjectsSortMode.None);
            foreach (PickUpItemInteractable item in droppedItems)
            {
                NetworkObject netObj = item.NetworkObject;
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
                else if (item != null)
                    Destroy(item.gameObject);
            }
        }

        private void DespawnAllEnemies()
        {
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                AICharacterManager enemy = spawnedEnemies[i];
                if (enemy == null)
                    continue;

                NetworkObject netObj = enemy.NetworkObject;
                if (netObj != null && netObj.IsSpawned)
                    netObj.Despawn(true);
                else
                    Destroy(enemy.gameObject);
            }

            spawnedEnemies.Clear();
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
            foreach (NetworkObject netObj in currentRewardObjects)
            {
                if (netObj == null) continue;

                if (netObj.IsSpawned)
                    netObj.Despawn(true);
                else
                    Destroy(netObj.gameObject);
            }

            currentRewardObjects.Clear();
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

            // 지오메트리 제거 전 NavMesh 데이터도 함께 클리어한다.
            navMeshBuilder?.Clear();

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

            GameObject resolvedEntryPrefab = currentTemplate?.entryDoorPrefab != null
                ? currentTemplate.entryDoorPrefab
                : entryDoorPrefab;

            GameObject resolvedExitPrefab = currentTemplate?.exitDoorPrefab != null
                ? currentTemplate.exitDoorPrefab
                : exitDoorPrefab;

            if (currentRoomInstance.EntryDoorAnchor != null && resolvedEntryPrefab != null)
            {
                currentEntryDoor = SpawnSingleDoor(resolvedEntryPrefab, currentRoomInstance.EntryDoorAnchor, "EntryDoor");
                if (currentEntryDoor != null)
                    currentEntryDoor.SetDoorRoleServer(RoguelikeDoorRole.Entry);
            }

            if (currentRoomInstance.ExitDoorAnchor != null && resolvedExitPrefab != null)
            {
                currentExitDoor = SpawnSingleDoor(resolvedExitPrefab, currentRoomInstance.ExitDoorAnchor, "ExitDoor");
                if (currentExitDoor != null)
                    currentExitDoor.SetDoorRoleServer(RoguelikeDoorRole.Exit);
            }
        }

        private RoguelikeRoomDoor SpawnSingleDoor(GameObject prefab, Transform anchor, string debugName)
        {
            Transform parent = networkRuntimeRoot != null ? networkRuntimeRoot : null;

            GameObject doorObj = Instantiate(
                prefab,
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
        private void ShowLoadingScreenClientRpc()
        {
            if (PlayerUILoadingScreenManager.Instance != null)
                PlayerUILoadingScreenManager.Instance.ActivateLoadingScreen();
        }

        [ClientRpc]
        private void HideLoadingScreenClientRpc()
        {
            if (PlayerUILoadingScreenManager.Instance != null)
                PlayerUILoadingScreenManager.Instance.DeactivateLoadingScreen();
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

            // Start 방은 기본 아이템 상자 지급, 나머지 비전투 방은 Site of Grace
            bool isStartRoom = currentPlan != null && currentPlan.roomType == RoomType.Start;
            SpawnRewardIfNeeded(isSiteOfGrace: !isStartRoom);
        }

        private void StartCombatRoom()
        {
            SetState(RoomState.Combat);

            int enemyCount = GetEnemyCountByRoomType(currentPlan.roomType);
            SpawnEnemies(enemyCount);
        }

        private int GetEnemyCountByRoomType(RoomType roomType)
        {
            System.Random rng = new(currentPlan.seed ^ (int)roomType);

            switch (roomType)
            {
                case RoomType.Battle:
                    return rng.Next(minBattleEnemyCount, maxBattleEnemyCount + 1);
                case RoomType.Elite:
                    return rng.Next(minEliteEnemyCount, maxEliteEnemyCount + 1);
                case RoomType.Boss:
                    return rng.Next(minBossEnemyCount, maxBossEnemyCount + 1);
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

            if (!spawnedEnemies.Contains(aiCharacter))
                spawnedEnemies.Add(aiCharacter);

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

            int runeReward = GetRuneRewardByRoomType(currentPlan.roomType);
            if (runeReward > 0)
                AwardRoomRuneRewardClientRpc(runeReward);

            SpawnRewardIfNeeded();

            if (currentExitDoor != null)
                currentExitDoor.SetDoorOpenServer(true);

            SetState(RoomState.Cleared);
        }

        private int GetRuneRewardByRoomType(RoomType roomType)
        {
            switch (roomType)
            {
                case RoomType.Battle: return battleRoomRuneReward;
                case RoomType.Elite:  return eliteRoomRuneReward;
                case RoomType.Boss:   return bossRoomRuneReward;
                default:              return 0;
            }
        }

        [ClientRpc]
        private void AwardRoomRuneRewardClientRpc(int amount)
        {
            PlayerManager localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject?.GetComponent<PlayerManager>();
            if (localPlayer == null) return;
            localPlayer.playerStatsManager.AddRunes(amount);
        }

        private void SpawnRewardIfNeeded(bool isSiteOfGrace = false)
        {
            if (!IsServer)
                return;

            if (currentRewardObjects.Count > 0)
                return;

            if (currentTemplate == null)
                return;

            if (currentRoomInstance == null)
                return;

            Transform[] spawnPoints = currentRoomInstance.GetRewardSpawnPoints();
            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("[RoomManager] 보상 스폰 포인트가 없습니다.");
                return;
            }

            Transform parent = networkRuntimeRoot != null ? networkRuntimeRoot : null;

            // 템플릿에 지정된 커스텀 인터랙터블이 있으면 우선 사용
            if (currentTemplate.rewardInteractablePrefabs != null && currentTemplate.rewardInteractablePrefabs.Count > 0)
            {
                SpawnTemplateRewardInteractable(spawnPoints, parent);
                return;
            }

            if (isSiteOfGrace)
            {
                if (siteOfGracePrefab == null)
                {
                    Debug.LogWarning("[RoomManager] SiteOfGracePrefab이 할당되지 않아 생성하지 않습니다.");
                    return;
                }

                GameObject graceObj = Instantiate(
                    siteOfGracePrefab,
                    spawnPoints[0].position,
                    spawnPoints[0].rotation,
                    parent);

                NetworkObject graceNetObj = graceObj.GetComponent<NetworkObject>();
                if (graceNetObj == null)
                {
                    Debug.LogError("[RoomManager] SiteOfGrace prefab must contain NetworkObject.");
                    Destroy(graceObj);
                    return;
                }

                graceNetObj.Spawn(true);
                currentRewardObjects.Add(graceNetObj);

                Debug.Log("[RoomManager] Site of Grace 스폰 (적 없는 방).");
                return;
            }

            if (itemBoxDatabase == null)
            {
                Debug.LogWarning("[RoomManager] ItemBoxDatabase가 할당되지 않아 보상 상자를 생성하지 않습니다.");
                return;
            }

            int stageIndex = RunManager.Instance != null ? RunManager.Instance.CurrentRoomIndex : 0;

            bool isStartRoom = currentPlan != null && currentPlan.roomType == RoomType.Start;

            GameObject boxPrefab;
            ItemTier finalTier;
            if (isStartRoom && itemBoxDatabase.HasStartRoomEntries)
            {
                boxPrefab = itemBoxDatabase.GetStartRoomPrefab();
                finalTier = currentTemplate.rewardBaseTier;
            }
            else
            {
                // Start 방(첫 번째 방)은 운 보정 없이 기본 등급 고정
                finalTier = isStartRoom
                    ? currentTemplate.rewardBaseTier
                    : CalculateRewardTier(currentTemplate.rewardBaseTier, stageIndex);
                boxPrefab = itemBoxDatabase.GetPrefab(stageIndex);
            }

            BoxType[] validTypes = System.Array.FindAll(
                (BoxType[])System.Enum.GetValues(typeof(BoxType)),
                t => t != BoxType.None);
            BoxType boxType = validTypes[Random.Range(0, validTypes.Length)];

            if (boxPrefab == null)
            {
                Debug.LogWarning($"[RoomManager] 적절한 보상이 없습니다 : 보상 스킵.");
                return;
            }

            GameObject rewardObj = Instantiate(
                boxPrefab,
                spawnPoints[0].position,
                spawnPoints[0].rotation,
                parent);

            // Spawn 전에 Setup 호출 → Start()보다 먼저 실행됨
            InteractableItemBox itemBox = rewardObj.GetComponent<InteractableItemBox>();
            itemBox?.Setup(boxType, finalTier);

            NetworkObject netObj = rewardObj.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("[RoomManager] Reward prefab must contain NetworkObject.");
                Destroy(rewardObj);
                return;
            }

            netObj.Spawn(true);
            currentRewardObjects.Add(netObj);

            Debug.Log($"[RoomManager] 보상 상자 스폰: type={boxType}, tier={finalTier} (stage={stageIndex}, baseTier={currentTemplate.rewardBaseTier})");
        }

        private void SpawnTemplateRewardInteractable(Transform[] spawnPoints, Transform parent)
        {
            List<GameObject> prefabs = currentTemplate.rewardInteractablePrefabs;
            int stageIndex = RunManager.Instance != null ? RunManager.Instance.CurrentRoomIndex : 0;

            for (int i = 0; i < prefabs.Count; i++)
            {
                GameObject prefab = prefabs[i];
                if (prefab == null) continue;

                // 스폰 포인트가 부족하면 마지막 포인트를 재사용
                Transform spawnPoint = spawnPoints[Mathf.Min(i, spawnPoints.Length - 1)];

                GameObject obj = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, parent);

                // ItemBox 계열이면 기존 Setup 로직 적용
                if (obj.TryGetComponent<InteractableItemBox>(out InteractableItemBox itemBox))
                {
                    ItemTier finalTier = CalculateRewardTier(currentTemplate.rewardBaseTier, stageIndex);
                    BoxType[] validTypes = System.Array.FindAll(
                        (BoxType[])System.Enum.GetValues(typeof(BoxType)),
                        t => t != BoxType.None);
                    BoxType boxType = validTypes[Random.Range(0, validTypes.Length)];
                    itemBox.Setup(boxType, finalTier);
                }

                NetworkObject netObj = obj.GetComponent<NetworkObject>();
                if (netObj == null)
                {
                    Debug.LogError($"[RoomManager] rewardInteractablePrefabs[{i}] ({prefab.name})에 NetworkObject가 없습니다.");
                    Destroy(obj);
                    continue;
                }

                netObj.Spawn(true);
                currentRewardObjects.Add(netObj);

                Debug.Log($"[RoomManager] Template 인터랙터블 스폰 [{i}]: {prefab.name} at {spawnPoint.name}");
            }
        }

        /// <summary>
        /// 기본 등급 + 스테이지 깊이 보너스 + 행운 요소로 최종 티어를 결정합니다.
        /// - 스테이지 3개마다 +1 등급
        /// - 10% 확률로 +2, 30% 확률로 +1 (행운)
        /// </summary>
        private static ItemTier CalculateRewardTier(ItemTier baseTier, int stageIndex)
        {
            int tier = (int)baseTier;

            // 스테이지 깊이 보너스
            tier += stageIndex / 3;

            // 행운 보너스
            float luck = Random.value;
            if (luck < 0.10f)       tier += 2;
            else if (luck < 0.40f)  tier += 1;

            return (ItemTier)Mathf.Clamp(tier, 0, (int)ItemTier.Mythic);
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