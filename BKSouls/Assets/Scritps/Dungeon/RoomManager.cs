using System.Collections.Generic;
using BK;
using Unity.Netcode;
using UnityEngine;

public class RoomManager : NetworkBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("Current Room")]
    [SerializeField] private Transform roomRoot;

    [Header("Enemy")]
    [SerializeField] private int baseBattleEnemyCount = 3;
    [SerializeField] private int eliteEnemyCount = 5;

    private RoomInstance currentRoomInstance;
    private RoomPlan currentPlan;
    private RoomTemplateSO currentTemplate;

    private readonly List<AICharacterManager> aliveEnemies = new();
    private NetworkObject currentRewardObject;

    private readonly NetworkVariable<int> currentRoomState = new((int)RoomState.None);
    public RoomState CurrentState => (RoomState)currentRoomState.Value;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadRoom(RoomPlan plan, RoomTemplateSO template)
    {
        if (!IsServer) return;

        currentPlan = plan;
        currentTemplate = template;

        SetState(RoomState.Loading);

        CleanupCurrentRoom();
        SpawnRoomGeometry(template);
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
        }
    }

    private void SetState(RoomState state)
    {
        if (!IsServer) return;
        currentRoomState.Value = (int)state;
    }

    private void CleanupCurrentRoom()
    {
        // 적 정리
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] != null && aliveEnemies[i].NetworkObject != null && aliveEnemies[i].NetworkObject.IsSpawned)
            {
                aliveEnemies[i].NetworkObject.Despawn(true);
            }
        }
        aliveEnemies.Clear();

        // 보상 정리
        if (currentRewardObject != null && currentRewardObject.IsSpawned)
        {
            currentRewardObject.Despawn(true);
            currentRewardObject = null;
        }

        // 방 지형 정리
        if (currentRoomInstance != null)
        {
            Destroy(currentRoomInstance.gameObject);
            currentRoomInstance = null;
        }
    }

    private void SpawnRoomGeometry(RoomTemplateSO template)
    {
        if (template == null || template.roomPrefab == null)
        {
            Debug.LogError("[RoomManager] Invalid room template or room prefab.");
            return;
        }

        GameObject roomObj = Instantiate(template.roomPrefab, roomRoot);
        currentRoomInstance = roomObj.GetComponent<RoomInstance>();

        if (currentRoomInstance == null)
        {
            Debug.LogError("[RoomManager] Room prefab must contain RoomInstance component.");
        }
    }

    private void WarpAllPlayersToRoom()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;

        for (int i = 0; i < clients.Count; i++)
        {
            NetworkClient client = clients[i];
            if (client.PlayerObject == null) continue;

            Transform spawnPoint = currentRoomInstance.GetPlayerSpawnPoint(i);
            client.PlayerObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);

            // Rigidbody 사용 시 추가 초기화 필요할 수 있음
            Rigidbody rb = client.PlayerObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void PrepareRoom()
    {
        if (currentRoomInstance.EntryDoor != null)
            currentRoomInstance.EntryDoor.SetDoorStateServer(true);

        if (currentRoomInstance.ExitDoor != null)
            currentRoomInstance.ExitDoor.SetDoorStateServer(false);
    }

    private void ClearNonCombatRoom()
    {
        SetState(RoomState.Cleared);

        if (currentRoomInstance.ExitDoor != null)
            currentRoomInstance.ExitDoor.SetDoorStateServer(true);

        SpawnRewardIfNeeded();
    }

    private void StartCombatRoom()
    {
        SetState(RoomState.Combat);

        int enemyCount = GetEnemyCountByRoomType(currentPlan.roomType);
        SpawnEnemies();
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
                return 1;
            default:
                return 0;
        }
    }

    private void SpawnEnemies()
    {
        
    }

    public void NotifyEnemyDead(AICharacterManager enemy)
    {
        if (!IsServer) return;

        if (aliveEnemies.Remove(enemy))
        {
            if (aliveEnemies.Count == 0)
            {
                ClearRoom();
            }
        }
    }

    private void ClearRoom()
    {
        if (!IsServer) return;

        SetState(RoomState.Reward);

        SpawnRewardIfNeeded();

        if (currentRoomInstance.ExitDoor != null)
            currentRoomInstance.ExitDoor.SetDoorStateServer(true);

        SetState(RoomState.Cleared);
    }

    private void SpawnRewardIfNeeded()
    {
        if (currentTemplate.rewardPrefab == null || currentRoomInstance.RewardSpawnPoint == null)
            return;

        GameObject rewardObj = Instantiate(
            currentTemplate.rewardPrefab,
            currentRoomInstance.RewardSpawnPoint.position,
            currentRoomInstance.RewardSpawnPoint.rotation);

        NetworkObject netObj = rewardObj.GetComponent<NetworkObject>();
        if (netObj == null)
        {
            Debug.LogError("[RoomManager] Reward prefab needs NetworkObject.");
            Destroy(rewardObj);
            return;
        }

        netObj.Spawn(true);
        currentRewardObject = netObj;
    }

    public void TryMoveNextRoomServer()
    {
        if (!IsServer) return;
        if (CurrentState != RoomState.Cleared) return;

        SetState(RoomState.Transition);
        RunManager.Instance.RequestNextRoomFromRoomManager();
    }
}