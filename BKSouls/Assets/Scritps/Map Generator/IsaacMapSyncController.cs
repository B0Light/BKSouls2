using System;
using Unity.Netcode;
using UnityEngine;

/// - Host가 runSeed/stageIndex 기반으로 stageSeed를 파생
/// - payload(Seed + 파라미터)만 전송 → 각 클라가 로컬로 동일 맵 생성

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

    private IsaacMapGenerator _generator;

    public override void OnNetworkSpawn()
    {
        EnsureGenerator();
    }

    // -----------------------------
    // Public Host Controls (Context Menu)
    // -----------------------------

    [ContextMenu("Host: Start Run (Init Seed)")]
    public void HostStartRun()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[IsaacMapSync] HostStartRun: Host(서버)에서만 호출하세요.");
            return;
        }

        if (runSeed == 0)
            runSeed = Environment.TickCount;

        stageIndex = 0;
        HostGenerateStageAndSync();
    }

    [ContextMenu("Host: Regenerate Current Stage")]
    public void HostRegenerateCurrentStage()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[IsaacMapSync] HostRegenerateCurrentStage: Host(서버)에서만 호출하세요.");
            return;
        }

        HostGenerateStageAndSync();
    }

    [ContextMenu("Host: Next Stage")]
    public void HostNextStage()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[IsaacMapSync] HostNextStage: Host(서버)에서만 호출하세요.");
            return;
        }

        stageIndex++;
        HostGenerateStageAndSync();
    }

    [ContextMenu("Host: Set New RunSeed (Random)")]
    public void HostSetNewRunSeed()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[IsaacMapSync] HostSetNewRunSeed: Host(서버)에서만 호출하세요.");
            return;
        }

        runSeed = Environment.TickCount;
        stageIndex = 0;
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

        // 생성기 인스턴스(설정값은 GenerateLocal에서 payload로 매번 갱신)
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

        // 이전 맵 제거
        _generator.ClearMap();

        // payload 적용
        _generator.seed = payload.seed;
        _generator.maxRooms = payload.maxRooms;
        _generator.specialRoomCount = payload.specialRoomCount;
        _generator.horizontalSize = payload.horizontalSize;
        _generator.verticalSize = payload.verticalSize;
        _generator.spacing = payload.spacing;

        // gridSize / cubeSize를 런타임에 바꿀 계획이 있다면,
        // BaseMapGenerator가 이를 변경 가능하게 설계되어 있어야 함.
        // (일단은 고정값 가정)

        _generator.GenerateMap();

        Debug.Log($"[IsaacMapSync] Local map generated. seed={payload.seed}");
    }

    // -----------------------------
    // RPC
    // -----------------------------

    // Host 제외한 모든 클라이언트에서 실행
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