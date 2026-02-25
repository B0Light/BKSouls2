using System.Collections.Generic;
using UnityEngine;

public class StageGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject baseRoomPrefab;
    public GameObject doorPrefab;
    public GameObject wallPrefab;

    [Header("Core & Content")]
    public DungeonGraphWfcCore core;
    public RoomContentLibrary contentLibrary;

    public void GenerateStage(StageConfig cfg, bool isBossStage)
    {
        var graph = core.GenerateDoorGraph(cfg, isBossStage);
        if (graph == null)
        {
            Debug.LogError("Stage generation failed.");
            return;
        }

        AssignRoomTypes(graph, cfg, isBossStage);
        SpawnStage(graph, cfg);
    }

    private void AssignRoomTypes(DungeonGraph graph, StageConfig cfg, bool isBossStage)
    {
        // 기본 Combat으로 이미 채워져 있음
        graph.roomTypes[graph.start] = RoomType.Start;
        graph.roomTypes[graph.exit]  = isBossStage ? RoomType.Boss : RoomType.Exit;

        if (isBossStage) return;

        // 메인 경로
        var mainPath = graph.GetShortestPath(graph.start, graph.exit);
        var mainSet = new HashSet<Vector2Int>(mainPath);

        // 상점: 메인 경로 중간쯤(확률)
        if (Random.value < cfg.shopChance && mainPath.Count >= 4)
        {
            var shopPos = mainPath[Mathf.Clamp(mainPath.Count / 2, 1, mainPath.Count - 2)];
            if (graph.roomTypes[shopPos] == RoomType.Combat)
                graph.roomTypes[shopPos] = RoomType.Shop;
        }

        // 엘리트: 메인 경로 후반(확률)
        if (Random.value < cfg.eliteChance && mainPath.Count >= 4)
        {
            var elitePos = mainPath[Mathf.Clamp((mainPath.Count * 3) / 4, 1, mainPath.Count - 2)];
            if (graph.roomTypes[elitePos] == RoomType.Combat)
                graph.roomTypes[elitePos] = RoomType.Elite;
        }

        // leaf에서 보물 배치
        var leaves = graph.GetLeavesExcluding(mainSet);
        Shuffle(leaves);

        int treasureCount = Random.Range(cfg.treasureLeafMin, cfg.treasureLeafMax + 1);
        for (int i = 0; i < leaves.Count && i < treasureCount; i++)
        {
            if (graph.roomTypes[leaves[i]] == RoomType.Combat)
                graph.roomTypes[leaves[i]] = RoomType.Treasure;
        }

        // 나머지 leaf 일부를 이벤트로(확률)
        foreach (var leaf in leaves)
        {
            if (graph.roomTypes[leaf] != RoomType.Combat) continue;
            if (Random.value < cfg.eventChance)
                graph.roomTypes[leaf] = RoomType.Event;
        }
    }

    private void SpawnStage(DungeonGraph graph, StageConfig cfg)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);

        // 방 스폰
        foreach (var cell in graph.AllCells())
        {
            Vector3 pos = new Vector3(cell.x * cfg.cellSize, 0f, cell.y * cfg.cellSize);
            var room = Instantiate(baseRoomPrefab, pos, Quaternion.identity, transform);

            var type = graph.roomTypes[cell];
            if (contentLibrary) contentLibrary.SpawnContent(type, room.transform, cell);
        }

        // 문/벽 스폰
        graph.SpawnDoorsAndWalls(transform, cfg.cellSize, doorPrefab, wallPrefab);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}