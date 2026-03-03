using System;
using System.Collections.Generic;
using BK;
using Unity.Netcode;
using UnityEngine;

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

    private IsaacMapGenerator _generator;

    public override void OnNetworkSpawn()
    {
        EnsureGenerator();
        
        if (IsServer)
            HostStartRun();
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

        // Host 로컬 생성
        GenerateLocal(payload);

        // Clients 생성
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

        // ✅ 맵 생성 완료 후: 랜덤 방으로 플레이어 이동
        TryMovePlayerToRandomRoom();

        Debug.Log($"[IsaacMapSync] Local map generated. seed={payload.seed}");
    }

    // -----------------------------
    // Spawn logic
    // -----------------------------

    private void TryMovePlayerToRandomRoom()
    {
        Transform player = ResolvePlayer();
        if (player == null)
        {
            Debug.LogWarning("[IsaacMapSync] Player Transform not found.");
            return;
        }

        Vector3 target = _generator.GetRoomWorldCenter();

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
        
        // 싱글: Player 태그로 찾기
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