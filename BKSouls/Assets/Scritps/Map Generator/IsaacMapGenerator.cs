using System;
using System.Collections.Generic;
using UnityEngine;

public class IsaacMapGenerator : BaseMapGenerator
{
    [Header("맵 설정")]
    public int maxRooms = 15;
    public int specialRoomCount = 3;

    [Header("방 크기 (고정)")]
    public int horizontalSize = 11;
    public int verticalSize = 11;

    [Header("Spacing(복도 공간)")]
    public int spacing = 3; // 방 사이 간격(복도 공간)

    [Header("FailSafe")]
    public int minRoomsToAccept = 5;
    public int maxTries = 12;

    private Dictionary<Vector2Int, Room> rooms = new();

    public IsaacMapGenerator(Transform slot, TileMappingDataSO tileMappingData) : base(slot, tileMappingData) { }
    public IsaacMapGenerator(Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize)
        : base(slot, tileMappingData, gridSize, cubeSize) { }

    public IsaacMapGenerator(
        Transform slot,
        TileMappingDataSO tileMappingData,
        Vector2Int gridSize,
        Vector3 cubeSize,
        int seed,
        int maxRooms,
        int specialRoomCount,
        int horizontalSize,
        int verticalSize,
        int spacing
    ) : base(slot, tileMappingData, gridSize, cubeSize)
    {
        this.seed = seed;
        this.maxRooms = maxRooms;
        this.specialRoomCount = specialRoomCount;
        this.horizontalSize = horizontalSize;
        this.verticalSize = verticalSize;
        this.spacing = spacing;
    }

    protected override void InitializeGenerator()
    {
        rooms = new Dictionary<Vector2Int, Room>();
    }

    public override void GenerateMap()
    {
        InitializeGrid();

        // 결정적 생성: seed 기반으로 여러 번 시도하되, 시도마다 파생 seed 사용
        bool ok = TryGenerateRoomsDeterministic();

        if (!ok)
        {
            Debug.LogWarning($"[IsaacMapGenerator] FailSafe: 최소 방({minRoomsToAccept}) 달성 실패. 그래도 현재 결과로 진행합니다.");
        }

        PlaceSpecialRoomsDeterministic();
        BuildWalls();
        RenderGrid();

        OnMapGenerationComplete();
    }

    private bool TryGenerateRoomsDeterministic()
    {
        for (int attempt = 0; attempt < maxTries; attempt++)
        {
            rooms.Clear();
            InitializeGrid(); // 시도마다 그리드도 초기화(결과 누적 방지)

            int effectiveSeed = HashSeed(seed, attempt);
            var rng = new System.Random(effectiveSeed);

            GenerateRoomsWithRng(rng);

            if (rooms.Count >= minRoomsToAccept)
                return true;
        }

        return false;
    }

    private void GenerateRoomsWithRng(System.Random rng)
    {
        Vector2Int startPos = Vector2Int.zero;
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(startPos);

        rooms[startPos] = new Room(startPos, horizontalSize, verticalSize, RoomType.Start);
        PlaceRoomOnGrid(startPos, horizontalSize, verticalSize);

        while (frontier.Count > 0 && rooms.Count < maxRooms)
        {
            Vector2Int current = frontier.Dequeue();

            List<Vector2Int> directions = GetDirections();
            ShuffleList(directions, rng);

            foreach (var dir in directions)
            {
                Vector2Int newPos = current + dir;

                if (rooms.ContainsKey(newPos))
                    continue;

                if (IsOutOfGrid(newPos, horizontalSize, verticalSize, spacing))
                    continue;

                // 방 개수에 따른 가변 확률(결정적으로)
                float progress = (float)rooms.Count / maxRooms;
                float spawnChance = Mathf.Lerp(0.9f, 0.3f, progress);
                double roll = rng.NextDouble();
                if (roll > spawnChance)
                    continue;

                Room newRoom = new Room(newPos, horizontalSize, verticalSize, RoomType.Normal);
                rooms[newPos] = newRoom;

                rooms[current].Doors.Add(dir);
                rooms[newPos].Doors.Add(-dir);

                PlaceRoomOnGrid(newPos, horizontalSize, verticalSize);
                frontier.Enqueue(newPos);

                if (rooms.Count >= maxRooms)
                    break;
            }
        }
    }

    private bool IsOutOfGrid(Vector2Int roomGridPos, int width, int height, int spacing)
    {
        int gridX = (gridSize.x / 2) + (roomGridPos.x * (width + spacing));
        int gridY = (gridSize.y / 2) + (roomGridPos.y * (height + spacing));

        return gridX < 1 || gridX + width >= gridSize.x - 1 ||
               gridY < 1 || gridY + height >= gridSize.y - 1;
    }

    private void PlaceRoomOnGrid(Vector2Int roomPos, int width, int height)
    {
        int gridX = (gridSize.x / 2) + (roomPos.x * (width + spacing));
        int gridY = (gridSize.y / 2) + (roomPos.y * (height + spacing));

        gridX = Mathf.Clamp(gridX, 1, gridSize.x - width - 1);
        gridY = Mathf.Clamp(gridY, 1, gridSize.y - height - 1);

        RectInt roomRect = new RectInt(gridX, gridY, width, height);
        _floorList.Add(roomRect);

        // 방 배치
        for (int x = gridX; x < gridX + width; x++)
        {
            for (int y = gridY; y < gridY + height; y++)
            {
                if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
                {
                    _grid[x, y] = CellType.Floor;
                }
            }
        }

        // 복도 생성(문 방향)
        if (rooms.TryGetValue(roomPos, out var room))
        {
            foreach (var door in room.Doors)
                CreateCorridor(gridX, gridY, width, height, door, spacing);
        }
    }

    private void CreateCorridor(int roomX, int roomY, int width, int height, Vector2Int direction, int spacing)
    {
        int corridorLength = spacing - 1;

        int startX, startY;

        if (direction.x > 0) { startX = roomX + width; startY = roomY + height / 2; }
        else if (direction.x < 0) { startX = roomX - 1; startY = roomY + height / 2; }
        else if (direction.y > 0) { startX = roomX + width / 2; startY = roomY + height; }
        else { startX = roomX + width / 2; startY = roomY - 1; }

        int endX = startX + direction.x * corridorLength;
        int endY = startY + direction.y * corridorLength;

        CreateStraightPath(new Vector2Int(startX, startY), new Vector2Int(endX, endY));
    }

    private void PlaceSpecialRoomsDeterministic()
    {
        // 특수방 선택도 동일 rng로 처리해야 맵이 완전 동일해짐
        var rng = new System.Random(HashSeed(seed, 999)); // stage seed에서 파생

        List<Vector2Int> candidates = new List<Vector2Int>(rooms.Keys);
        candidates.Remove(Vector2Int.zero); // 시작방 제외

        int count = Mathf.Min(specialRoomCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int index = rng.Next(0, candidates.Count);
            Vector2Int pos = candidates[index];
            candidates.RemoveAt(index);

            rooms[pos].Type = RoomType.Special;
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

    private static List<Vector2Int> GetDirections()
    {
        return new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
    }

    private static void ShuffleList<T>(List<T> list, System.Random rng)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}