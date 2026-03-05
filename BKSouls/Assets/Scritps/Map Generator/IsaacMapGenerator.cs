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
    public IReadOnlyDictionary<Vector2Int, Room> Rooms => rooms;

    // ----------------------------------------------------
    // Room Rect lookup (roomPos -> RectInt)
    // - _floorList는 순서 의존이라 방 프리팹 배치에는 부적합
    // - 방 배치가 clamp의 영향을 받으므로 "실제 배치된 Rect"를 저장해두는 게 안전
    // ----------------------------------------------------
    private Dictionary<Vector2Int, RectInt> _roomRects = new();
    public IReadOnlyDictionary<Vector2Int, RectInt> RoomRects => _roomRects;

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
        _roomRects = new Dictionary<Vector2Int, RectInt>();
    }

    public override void GenerateMap()
    {
        InitializeGrid();

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
            _roomRects.Clear();
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

        // 방 배치 + Rect 기록
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

                float progress = (float)rooms.Count / maxRooms;
                float spawnChance = Mathf.Lerp(0.9f, 0.3f, progress);

                double roll = rng.NextDouble();
                if (roll > spawnChance)
                    continue;

                Room newRoom = new Room(newPos, horizontalSize, verticalSize, RoomType.Normal);
                rooms[newPos] = newRoom;

                rooms[current].Doors.Add(dir);
                rooms[newPos].Doors.Add(-dir);

                // 방 배치 + Rect 기록
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

        // 실제 배치 위치는 clamp 영향을 받음 (그래서 Rect를 저장해야 함)
        gridX = Mathf.Clamp(gridX, 1, gridSize.x - width - 1);
        gridY = Mathf.Clamp(gridY, 1, gridSize.y - height - 1);

        RectInt roomRect = new RectInt(gridX, gridY, width, height);

        // 기존 용도(바닥 영역 기록)
        _floorList.Add(roomRect);

        // roomPos -> rect 매핑 저장(중요)
        _roomRects[roomPos] = roomRect;

        // (선택) Room 클래스에 Rect 필드가 있다면 같이 저장
        // Room에 public RectInt Rect; 가 없다면 아래는 주석 유지
        if (rooms.TryGetValue(roomPos, out var roomRef))
        {
            // roomRef.Rect = roomRect; // Room에 Rect 필드 추가했을 때 사용
        }

        // 방 배치(바닥 타일)
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
        if (rooms.Count == 0) return;

        Vector2Int startPos = Vector2Int.zero;
        if (!rooms.ContainsKey(startPos))
        {
            // 혹시 start가 0,0이 아닐 가능성이 생겼다면 여기서 찾아야 함.
            // 현재 로직은 startPos = Vector2Int.zero 고정이므로 그대로 둠.
            startPos = new List<Vector2Int>(rooms.Keys)[0];
        }

        // 1) 거리 계산 (BFS)
        var dist = ComputeDistancesFrom(startPos);

        // 2) "먼 방 후보들" 중에서 Boss 선택
        //    - topPercent: 상위 몇 %를 "먼 방"으로 볼지
        //    - minDistance: 너무 가까운 건 배제
        Vector2Int bossPos = PickBossRoomByDistance(dist, seed, startPos, topPercent: 0.25f, minDistance: 2);

        // 3) 타입 세팅
        rooms[startPos].Type = RoomType.Start;
        if (rooms.TryGetValue(bossPos, out var bossRoom))
            bossRoom.Type = RoomType.Boss;

        // 4) Special 방 선택 (Start/Boss 제외)
        var rng = new System.Random(HashSeed(seed, 999));

        List<Vector2Int> candidates = new List<Vector2Int>(rooms.Keys);
        candidates.Remove(startPos);
        candidates.Remove(bossPos);

        int count = Mathf.Min(specialRoomCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            int index = rng.Next(0, candidates.Count);
            Vector2Int pos = candidates[index];
            candidates.RemoveAt(index);

            // 혹시 방어적으로 걸러줌
            if (pos == startPos || pos == bossPos) { i--; continue; }

            rooms[pos].Type = RoomType.Special;
        }
    }
    
    private Dictionary<Vector2Int, int> ComputeDistancesFrom(Vector2Int startPos)
    {
        var dist = new Dictionary<Vector2Int, int>(rooms.Count);
        var q = new Queue<Vector2Int>();

        dist[startPos] = 0;
        q.Enqueue(startPos);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            int d = dist[cur];

            // Doors는 "cur에서 연결된 방향들" 목록
            // rooms[cur].Doors에 dir이 있다면, 이웃은 cur + dir
            foreach (var dir in rooms[cur].Doors)
            {
                var nxt = cur + dir;
                if (!rooms.ContainsKey(nxt)) continue;

                if (dist.ContainsKey(nxt)) continue;

                dist[nxt] = d + 1;
                q.Enqueue(nxt);
            }
        }

        // 연결이 끊긴 방이 있으면 dist에 없을 수 있는데,
        // 현재 생성 로직은 연결된 방식이라 보통 모두 포함됨.
        return dist;
    }

    private Vector2Int PickBossRoomByDistance(
        Dictionary<Vector2Int, int> dist,
        int stageSeed,
        Vector2Int startPos,
        float topPercent = 0.30f, // 리프만 쓰면 후보가 줄어드니 약간 늘려도 좋음
        int minDistance = 2
    )
    {
        // 1) 리프(Doors.Count == 1) 중에서만 후보 구성
        var leafList = new List<(Vector2Int pos, int d)>();

        foreach (var kv in dist)
        {
            var pos = kv.Key;
            int d = kv.Value;

            if (pos == startPos) continue;
            if (d < minDistance) continue;

            // 리프 조건: 연결 1개
            if (!rooms.TryGetValue(pos, out var room)) continue;
            if (room.Doors == null || room.Doors.Count != 1) continue;

            leafList.Add((pos, d));
        }

        // 2) 리프 후보가 없으면 "거리 기반 전체 후보"로 fallback
        if (leafList.Count == 0)
        {
            // fallback: start 제외 + minDistance 이상 아무 방
            var anyList = new List<(Vector2Int pos, int d)>();
            foreach (var kv in dist)
            {
                if (kv.Key == startPos) continue;
                if (kv.Value < minDistance) continue;
                anyList.Add((kv.Key, kv.Value));
            }

            if (anyList.Count == 0)
            {
                foreach (var p in rooms.Keys)
                    if (p != startPos) return p;
                return startPos;
            }

            anyList.Sort((a, b) => b.d.CompareTo(a.d));
            int takeAny = Mathf.Clamp(Mathf.CeilToInt(anyList.Count * topPercent), 1, anyList.Count);

            int saltAny = HashSeed(stageSeed, 12345);
            int idxAny = PositiveMod(saltAny, takeAny);
            return anyList[idxAny].pos;
        }

        // 3) 리프 후보를 거리 내림차순 정렬
        leafList.Sort((a, b) => b.d.CompareTo(a.d));

        // 4) 상위 topPercent의 "먼 리프들" 중에서 결정적으로 1개 선택 (최장거리 고정 X)
        int take = Mathf.Clamp(Mathf.CeilToInt(leafList.Count * topPercent), 1, leafList.Count);

        // seed로 인덱스 고정 (결정적)
        int salt = HashSeed(stageSeed, 77777);
        int index = PositiveMod(salt, take);

        return leafList[index].pos;
    }

    private static int PositiveMod(int value, int mod)
    {
        if (mod <= 0) return 0;
        int r = value % mod;
        return r < 0 ? r + mod : r;
    }

    // ----------------------------------------------------
    // ✅ NEW: StartRoom / Room Anchor APIs
    // ----------------------------------------------------

    /// <summary>
    /// Start 방의 월드 중심. (_floorList[0] 의존 제거)
    /// </summary>
    public Vector3 GetStartRoomCenter()
    {
        return GetRoomWorldAnchor(Vector2Int.zero, AnchorMode.Center);
    }

    public enum AnchorMode
    {
        Center,
        BottomLeft
    }

    /// <summary>
    /// roomPos에 대응되는 "실제 배치된 Rect" 기반으로 프리팹 앵커를 계산
    /// </summary>
    public Vector3 GetRoomWorldAnchor(Vector2Int roomPos, AnchorMode mode = AnchorMode.Center)
    {
        if (!_roomRects.TryGetValue(roomPos, out var rect))
        {
            Debug.LogWarning($"[IsaacMapGenerator] GetRoomWorldAnchor failed: rect not found. roomPos={roomPos}");
            // fallback: 논리 좌표로 대충 계산 (권장하지 않음)
            int gridX = (gridSize.x / 2) + (roomPos.x * (horizontalSize + spacing));
            int gridY = (gridSize.y / 2) + (roomPos.y * (verticalSize + spacing));
            return GridToWorld(new Vector2Int(gridX, gridY));
        }

        Vector2Int anchorGrid = mode switch
        {
            AnchorMode.BottomLeft => new Vector2Int(rect.x, rect.y),
            _ => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2),
        };

        return GridToWorld(anchorGrid);
    }

    /// <summary>
    /// 타입/문 등 Room 정보가 필요할 때 쓰기 편한 overload
    /// </summary>
    public Vector3 GetRoomWorldAnchor(Room room, AnchorMode mode = AnchorMode.Center)
    {
        return GetRoomWorldAnchor(room.GridPos, mode);
    }

    // ----------------------------------------------------

    private Vector3 GridToWorld(Vector2Int grid)
    {
        // 타일 생성 방식에 따라 +0.5f 오프셋이 필요할 수도 있음.
        // 지금 프로젝트가 grid.x * cubeSize.x 로 바닥을 깔고 있다면 그대로 두면 됨.
        return new Vector3(grid.x * cubeSize.x, cubeSize.y + 1f, grid.y * cubeSize.z);
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