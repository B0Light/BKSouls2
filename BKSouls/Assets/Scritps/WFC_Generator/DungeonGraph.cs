using System;
using System.Collections.Generic;
using UnityEngine;

public enum Dir4 { North, South, East, West }

[Flags]
public enum DoorMask
{
    None  = 0,
    North = 1 << 0,
    South = 1 << 1,
    East  = 1 << 2,
    West  = 1 << 3,
}

public enum RoomType { Start, Combat, Elite, Shop, Event, Treasure, Exit, Boss }

public class DungeonGraph
{
    public readonly int sizeX;
    public readonly int sizeZ;

    public DoorMask[,] doors; // [x,z]
    public Dictionary<Vector2Int, RoomType> roomTypes = new();

    public Vector2Int start;
    public Vector2Int exit;

    public DungeonGraph(int sizeX, int sizeZ)
    {
        this.sizeX = sizeX;
        this.sizeZ = sizeZ;
        doors = new DoorMask[sizeX, sizeZ];

        // default room types = Combat
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
            roomTypes[new Vector2Int(x, z)] = RoomType.Combat;
    }

    public IEnumerable<Vector2Int> AllCells()
    {
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
            yield return new Vector2Int(x, z);
    }

    public bool InBounds(Vector2Int p) => p.x >= 0 && p.x < sizeX && p.y >= 0 && p.y < sizeZ;

    public Vector2Int Neighbor(Vector2Int p, Dir4 dir) => dir switch
    {
        Dir4.North => new Vector2Int(p.x, p.y + 1),
        Dir4.South => new Vector2Int(p.x, p.y - 1),
        Dir4.East  => new Vector2Int(p.x + 1, p.y),
        Dir4.West  => new Vector2Int(p.x - 1, p.y),
        _ => p
    };

    public IEnumerable<(Vector2Int n, Dir4 dir)> Neighbors(Vector2Int p)
    {
        yield return (Neighbor(p, Dir4.North), Dir4.North);
        yield return (Neighbor(p, Dir4.South), Dir4.South);
        yield return (Neighbor(p, Dir4.East),  Dir4.East);
        yield return (Neighbor(p, Dir4.West),  Dir4.West);
    }

    public static DoorMask DirToMask(Dir4 d) => d switch
    {
        Dir4.North => DoorMask.North,
        Dir4.South => DoorMask.South,
        Dir4.East  => DoorMask.East,
        Dir4.West  => DoorMask.West,
        _ => DoorMask.None
    };

    public static Dir4 Opp(Dir4 d) => d switch
    {
        Dir4.North => Dir4.South,
        Dir4.South => Dir4.North,
        Dir4.East  => Dir4.West,
        Dir4.West  => Dir4.East,
        _ => Dir4.North
    };

    public void ClampBordersClosed()
    {
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
        {
            if (x == 0)         doors[x, z] &= ~DoorMask.West;
            if (x == sizeX - 1) doors[x, z] &= ~DoorMask.East;
            if (z == 0)         doors[x, z] &= ~DoorMask.South;
            if (z == sizeZ - 1) doors[x, z] &= ~DoorMask.North;
        }
    }

    public void OpenEdge(Vector2Int a, Dir4 dir)
    {
        var b = Neighbor(a, dir);
        if (!InBounds(a) || !InBounds(b)) return;

        doors[a.x, a.y] |= DirToMask(dir);
        doors[b.x, b.y] |= DirToMask(Opp(dir));
    }

    public bool IsEdgeOpen(Vector2Int a, Dir4 dir)
    {
        var b = Neighbor(a, dir);
        if (!InBounds(a) || !InBounds(b)) return false;

        var ma = DirToMask(dir);
        var mb = DirToMask(Opp(dir));
        return (doors[a.x, a.y] & ma) != 0 && (doors[b.x, b.y] & mb) != 0;
    }

    public int GraphDistance(Vector2Int s, Vector2Int g)
    {
        if (!InBounds(s) || !InBounds(g)) return -1;

        var dist = new int[sizeX, sizeZ];
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
            dist[x, z] = -1;

        var q = new Queue<Vector2Int>();
        dist[s.x, s.y] = 0;
        q.Enqueue(s);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == g) return dist[cur.x, cur.y];

            foreach (var (n, dir) in Neighbors(cur))
            {
                if (!InBounds(n)) continue;
                if (dist[n.x, n.y] != -1) continue;

                if (IsEdgeOpen(cur, dir))
                {
                    dist[n.x, n.y] = dist[cur.x, cur.y] + 1;
                    q.Enqueue(n);
                }
            }
        }

        return -1;
    }

    public List<Vector2Int> GetShortestPath(Vector2Int s, Vector2Int g)
    {
        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var dist = new int[sizeX, sizeZ];
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
            dist[x, z] = -1;

        var q = new Queue<Vector2Int>();
        dist[s.x, s.y] = 0;
        q.Enqueue(s);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            if (cur == g) break;

            foreach (var (n, dir) in Neighbors(cur))
            {
                if (!InBounds(n)) continue;
                if (dist[n.x, n.y] != -1) continue;

                if (IsEdgeOpen(cur, dir))
                {
                    dist[n.x, n.y] = dist[cur.x, cur.y] + 1;
                    prev[n] = cur;
                    q.Enqueue(n);
                }
            }
        }

        if (dist[g.x, g.y] == -1) return new List<Vector2Int>();

        var path = new List<Vector2Int>();
        var p = g;
        path.Add(p);
        while (p != s)
        {
            p = prev[p];
            path.Add(p);
        }
        path.Reverse();
        return path;
    }

    public List<Vector2Int> GetLeaves()
    {
        var leaves = new List<Vector2Int>();
        foreach (var c in AllCells())
        {
            int deg = 0;
            foreach (var (n, dir) in Neighbors(c))
                if (InBounds(n) && IsEdgeOpen(c, dir)) deg++;

            if (deg == 1) leaves.Add(c);
        }
        return leaves;
    }

    public List<Vector2Int> GetLeavesExcluding(HashSet<Vector2Int> exclude)
    {
        var leaves = GetLeaves();
        leaves.RemoveAll(p => exclude.Contains(p));
        // 랜덤성이 필요하면 호출부에서 섞어도 됨
        return leaves;
    }

    // 문/벽 스폰 (B 방식)
    public void SpawnDoorsAndWalls(Transform parent, float cellSize, GameObject doorPrefab, GameObject wallPrefab)
    {
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
        {
            var p = new Vector2Int(x, z);

            // 중복 방지: East, North만
            if (x + 1 < sizeX) SpawnEdge(parent, p, Dir4.East, cellSize, doorPrefab, wallPrefab);
            if (z + 1 < sizeZ) SpawnEdge(parent, p, Dir4.North, cellSize, doorPrefab, wallPrefab);

            // 외곽은 벽
            if (x == 0)         SpawnOuterWall(parent, p, Dir4.West, cellSize, wallPrefab);
            if (z == 0)         SpawnOuterWall(parent, p, Dir4.South, cellSize, wallPrefab);
            if (x == sizeX - 1) SpawnOuterWall(parent, p, Dir4.East, cellSize, wallPrefab);
            if (z == sizeZ - 1) SpawnOuterWall(parent, p, Dir4.North, cellSize, wallPrefab);
        }
    }

    private void SpawnEdge(Transform parent, Vector2Int cell, Dir4 dir, float cellSize, GameObject doorPrefab, GameObject wallPrefab)
    {
        bool open = IsEdgeOpen(cell, dir);
        var prefab = open ? doorPrefab : wallPrefab;
        if (!prefab) return;

        Vector3 basePos = new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
        Vector3 offset = dir switch
        {
            Dir4.East  => new Vector3(+cellSize * 0.5f, 0, 0),
            Dir4.West  => new Vector3(-cellSize * 0.5f, 0, 0),
            Dir4.North => new Vector3(0, 0, +cellSize * 0.5f),
            Dir4.South => new Vector3(0, 0, -cellSize * 0.5f),
            _ => Vector3.zero
        };

        Quaternion rot = (dir == Dir4.East || dir == Dir4.West)
            ? Quaternion.Euler(0, 90, 0)
            : Quaternion.identity;

        UnityEngine.Object.Instantiate(prefab, basePos + offset, rot, parent);
    }

    private void SpawnOuterWall(Transform parent, Vector2Int cell, Dir4 dir, float cellSize, GameObject wallPrefab)
    {
        if (!wallPrefab) return;

        Vector3 basePos = new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);
        Vector3 offset = dir switch
        {
            Dir4.East  => new Vector3(+cellSize * 0.5f, 0, 0),
            Dir4.West  => new Vector3(-cellSize * 0.5f, 0, 0),
            Dir4.North => new Vector3(0, 0, +cellSize * 0.5f),
            Dir4.South => new Vector3(0, 0, -cellSize * 0.5f),
            _ => Vector3.zero
        };

        Quaternion rot = (dir == Dir4.East || dir == Dir4.West)
            ? Quaternion.Euler(0, 90, 0)
            : Quaternion.identity;

        UnityEngine.Object.Instantiate(wallPrefab, basePos + offset, rot, parent);
    }
}