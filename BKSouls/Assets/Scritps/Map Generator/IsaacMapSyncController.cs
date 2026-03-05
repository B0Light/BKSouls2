using System;
using System.Collections.Generic;
using BK;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IsaacMapSyncController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tileSlot;
    [SerializeField] private TileMappingDataSO tileMappingData;

    [Header("Generator Base Settings")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(64, 64);
    [SerializeField] private Vector3 cubeSize = new Vector3(2f, 2f, 2f);

    [Header("Isaac Style Settings (Host decides)")]
    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int specialRoomCount = 3;
    [SerializeField] private int horizontalSize = 11;
    [SerializeField] private int verticalSize = 11;
    [SerializeField] private int spacing = 3;

    [Header("Run State (Host)")]
    [SerializeField] private int runSeed = 0;
    [SerializeField] private int stageIndex = 0;

    [Header("Spawn After Generate")]
    [SerializeField] private float spawnHeightOffset = 1.0f;
    [SerializeField] private float groundCheckRayHeight = 20f;
    [SerializeField] private LayerMask groundMask = ~0; // 필요하면 Ground 레이어로 제한
    
    private IsaacMapGenPayload _pendingPayload;
    private bool _hasPendingPayload;

    [Header("Room Prefabs")] 
    [SerializeField] private SpawnRoomInstInfo spawnRoomInstInfo;
    private readonly List<NetworkObject> _spawnedRoomObjects = new();

    private IsaacMapGenerator _generator;

    public override void OnNetworkSpawn()
    {
        EnsureGenerator();
        
        if (NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        
        if (IsServer)
            HostStartRun();
    }
    
    public override void OnNetworkDespawn()
    {
        if (NetworkManager != null && NetworkManager.SceneManager != null)
            NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
    }
    
    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (!IsServer) return;

        // 모든 클라이언트 로드 완료 시점
        if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            // 이 이벤트는 "이번 씬 로드 요청에 대한 모든 클라 완료"를 의미
            if (_hasPendingPayload)
            {
                // 1) 서버 타일 맵 생성
                GenerateLocal(_pendingPayload);

                // 2) 서버 방 프리팹 스폰
                SpawnRoomPrefabsServer(_pendingPayload);

                // 3) 클라에게도 타일 생성 지시(이미 로드 완료된 상태라 순서 안전)
                GenerateStageRpc(_pendingPayload);

                _hasPendingPayload = false;
            }
        }
    }

    // -----------------------------
    // Host controls (context menu)
    // -----------------------------

    [ContextMenu("Host: Start Run (Init Seed)")]
    public void HostStartRun()
    {
        if (!IsServer) { Debug.LogWarning("[IsaacMapSync] HostStartRun: Host(서버)에서만 호출하세요."); return; }

        if (runSeed == 0) runSeed = Environment.TickCount;
        stageIndex = 0;
        HostGenerateStageAndSync();
    }

    [ContextMenu("Host: Next Stage")]
    public void HostNextStage()
    {
        if (!IsServer) { Debug.LogWarning("[IsaacMapSync] HostNextStage: Host(서버)에서만 호출하세요."); return; }

        stageIndex++;
        HostGenerateStageAndSync();
    }

    // -----------------------------
    // Core
    // -----------------------------

    private void HostGenerateStageAndSync()
    {
        EnsureGenerator();
        if (_generator == null) return;

        int stageSeed = HashSeed(runSeed, stageIndex);
        var payload = BuildPayload(stageSeed);
        
        _pendingPayload = payload;
        _hasPendingPayload = true;

        // Host 로컬 맵 생성
        GenerateLocal(payload);

        // 방 프리팹 스폰 (Host만)
        SpawnRoomPrefabsServer(payload);

        // Clients 맵 생성
        GenerateStageRpc(payload);

        Debug.Log($"[IsaacMapSync] Generated & synced. runSeed={runSeed}, stageIndex={stageIndex}, stageSeed={stageSeed}");
    }

    private IsaacMapGenPayload BuildPayload(int stageSeed)
    {
        return new IsaacMapGenPayload
        {
            seed = stageSeed,
            gridSize = gridSize,
            cubeSize = cubeSize,
            maxRooms = maxRooms,
            specialRoomCount = specialRoomCount,
            horizontalSize = horizontalSize,
            verticalSize = verticalSize,
            spacing = spacing
        };
    }

    private void EnsureGenerator()
    {
        if (_generator != null) return;

        if (tileSlot == null || tileMappingData == null)
        {
            Debug.LogError("[IsaacMapSync] tileSlot 또는 tileMappingData가 비어있습니다.");
            return;
        }

        _generator = new IsaacMapGenerator(
            tileSlot,
            tileMappingData,
            gridSize,
            cubeSize,
            seed: 0,
            maxRooms: maxRooms,
            specialRoomCount: specialRoomCount,
            horizontalSize: horizontalSize,
            verticalSize: verticalSize,
            spacing: spacing
        );
    }

    private void GenerateLocal(IsaacMapGenPayload payload)
    {
        EnsureGenerator();
        if (_generator == null) return;

        _generator.ClearMap();

        _generator.seed = payload.seed;
        _generator.maxRooms = payload.maxRooms;
        _generator.specialRoomCount = payload.specialRoomCount;
        _generator.horizontalSize = payload.horizontalSize;
        _generator.verticalSize = payload.verticalSize;
        _generator.spacing = payload.spacing;

        _generator.GenerateMap();

        // 맵 생성 완료 후: 시작 방으로 플레이어 이동
        TryMovePlayerToStartRoom();

        Debug.Log($"[IsaacMapSync] Local map generated. seed={payload.seed}");
        foreach (var kv in _generator.Rooms)
        {
            Debug.Log($"Room Info : [{kv.Key}] : {kv.Value.Type}");
        }
    }
    
    private void SpawnRoomPrefabsServer(IsaacMapGenPayload payload)
    {
        if (!IsServer) return;

        // 기존 스폰 제거(스테이지 넘어갈 때)
        foreach (var no in _spawnedRoomObjects)
            if (no != null && no.IsSpawned) no.Despawn(true);
        _spawnedRoomObjects.Clear();

        foreach (var kv in _generator.Rooms) // Rooms 접근자 만들어두면 좋음
        {
            Vector2Int roomPos = kv.Key;
            Room room = kv.Value;

            NetworkObject prefab = PickRoomPrefab(payload.seed, room);
            Vector3 anchor = _generator.GetRoomWorldAnchor(roomPos);

            var inst = Instantiate(prefab, anchor, Quaternion.identity);
        
            // room 식별자를 네트워크로 전달하고 싶으면 RoomInstance 컴포넌트에 세팅
            var roomInst = inst.GetComponent<RoomInstance>();
            if (roomInst != null)
                roomInst.SetRoomId(roomPos, room.Type, payload.seed); // 서버에서만 세팅

            inst.Spawn(true);
            _spawnedRoomObjects.Add(inst);
        }
    }
    
    private NetworkObject PickRoomPrefab(int stageSeed, Room room)
    {
        // 1) 타입별 풀 선택
        if (room.Type == RoomType.Start)
        {
            if (spawnRoomInstInfo.startRoomPrefab == null)
            {
                Debug.LogError("[IsaacMapSync] startRoomPrefab is null.");
                return null;
            }
            return spawnRoomInstInfo.startRoomPrefab;
        }
        
        if (room.Type == RoomType.Boss)
        {
            if (spawnRoomInstInfo.bossRoomPrefab == null)
            {
                Debug.LogError("[IsaacMapSync] startRoomPrefab is null.");
                return null;
            }
            return spawnRoomInstInfo.bossRoomPrefab;
        }

        List<NetworkObject> pool = room.Type == RoomType.Special ? spawnRoomInstInfo.specialRoomPrefabs : spawnRoomInstInfo.normalRoomPrefabs;

        if (pool == null || pool.Count == 0)
        {
            Debug.LogError($"[IsaacMapSync] Room prefab pool is empty. type={room.Type}");
            return null;
        }

        // 2) seed + roomPos + type로 결정적 인덱스 생성
        //    (stageSeed가 같고 roomPos/type가 같으면 항상 같은 프리팹 선택)
        int salt = HashRoomSalt(room.GridPos, room.Type);
        int h = HashSeed(stageSeed, salt);

        // 음수 방지
        int index = PositiveMod(h, pool.Count);
        return pool[index];
    }

    

    // -----------------------------
    // Spawn logic
    // -----------------------------

    private void TryMovePlayerToStartRoom()
    {
        Transform player = ResolvePlayer();
        if (player == null)
        {
            Debug.LogWarning("[IsaacMapSync] Player Transform not found.");
            return;
        }

        Vector3 target = _generator.GetStartRoomCenter();

        // 바닥 찾기(위에서 아래로 레이캐스트)
        Vector3 rayOrigin = target + Vector3.up * groundCheckRayHeight;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckRayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
        {
            target = hit.point;
        }

        target += Vector3.up * spawnHeightOffset;

        // CharacterController/NavMeshAgent가 있으면 안전하게 껐다 켜는 게 좋음
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null) agent.enabled = false;

        player.SetPositionAndRotation(target, Quaternion.identity);

        if (agent != null) agent.enabled = true;
        if (cc != null) cc.enabled = true;
    }

    private Transform ResolvePlayer()
    {
        var go = GUIController.Instance.localPlayer.gameObject;
        return go != null ? go.transform : null;
    }

    // -----------------------------
    // RPC
    // -----------------------------

    [Rpc(SendTo.NotServer)]
    private void GenerateStageRpc(IsaacMapGenPayload payload)
    {
        GenerateLocal(payload);
    }

    // -----------------------------
    // Utils
    // -----------------------------
    
    private static int PositiveMod(int value, int mod)
    {
        if (mod <= 0) return 0;
        int r = value % mod;
        return r < 0 ? r + mod : r;
    }
    
    private static int HashRoomSalt(Vector2Int roomPos, RoomType roomType)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + roomPos.x;
            h = h * 31 + roomPos.y;
            h = h * 31 + (int)roomType;
            return h;
        }
    }

    private static int HashSeed(int baseSeed, int salt)
    {
        unchecked
        {
            int h = 17;
            h = h * 31 + baseSeed;
            h = h * 31 + salt;
            return h;
        }
    }
}