using Unity.Netcode;
using UnityEngine;

/// <summary>
/// "맵 생성 동기화"만 담당.
/// - Host가 seed/payload 결정
/// - Clients는 받은 payload로 로컬 생성
/// </summary>
public class IsaacMapSyncController : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Transform tileSlot;
    [SerializeField] private TileMappingDataSO tileMappingData;

    [Header("Default Config (Host가 결정)")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(64, 64);
    [SerializeField] private Vector3 cubeSize = new Vector3(2, 2, 2);

    [SerializeField] private int maxRooms = 15;
    [SerializeField] private int specialRoomCount = 3;
    [SerializeField] private int horizontalSize = 11;
    [SerializeField] private int verticalSize = 11;
    [SerializeField] private int spacing = 3;

    private IsaacMapGenerator _generator;

    public override void OnNetworkSpawn()
    {
        // 로컬 생성기 준비(호스트/클라 둘 다)
        EnsureGenerator();
    }

    [ContextMenu("Host: Generate & Sync")]
    public void HostGenerateAndSync()
    {
        if (!IsServer)
        {
            Debug.LogWarning("HostGenerateAndSync는 Host(서버)에서만 호출하세요.");
            return;
        }

        // ✅ Host가 이번 스테이지 seed 결정
        int seed = System.Environment.TickCount; // 테스트용. 실제론 runSeed/roomIndex 기반 추천

        var payload = new IsaacMapGenPayload
        {
            seed = seed,
            gridSize = gridSize,
            cubeSize = cubeSize,
            maxRooms = maxRooms,
            specialRoomCount = specialRoomCount,
            horizontalSize = horizontalSize,
            verticalSize = verticalSize,
            spacing = spacing
        };

        // Host도 로컬 생성
        GenerateLocal(payload);

        // Clients에게 전송(Host 포함 송신 필요 없음, 이미 로컬 생성했으니까 Clients만 보내도 됨)
        GenerateMapRpc(payload);
    }

    private void EnsureGenerator()
    {
        if (_generator != null) return;

        if (tileSlot == null || tileMappingData == null)
        {
            Debug.LogError("tileSlot / tileMappingData가 비어있습니다.");
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

        // 이전 맵 제거
        _generator.ClearMap();

        // payload 적용
        _generator.seed = payload.seed;
        // grid/cube는 생성기 생성자에서 세팅됐지만, 혹시 변경될 수 있으니 새로 만들고 싶으면
        // generator를 새로 생성하는 쪽이 안전(여기선 grid/cube는 고정이라 가정)

        _generator.maxRooms = payload.maxRooms;
        _generator.specialRoomCount = payload.specialRoomCount;
        _generator.horizontalSize = payload.horizontalSize;
        _generator.verticalSize = payload.verticalSize;
        _generator.spacing = payload.spacing;

        _generator.GenerateMap();
        Debug.Log($"[IsaacMapSync] Local map generated. seed={payload.seed}");
    }

    [Rpc(SendTo.NotServer)]
    private void GenerateMapRpc(IsaacMapGenPayload payload)
    {
        // Host는 이미 생성했으니, 여기서는 클라만 실행됨(Clients)
        if (IsServer) return;

        GenerateLocal(payload);
    }
}