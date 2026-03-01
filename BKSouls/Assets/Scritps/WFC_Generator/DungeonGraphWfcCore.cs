using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonGraphWfcCore : MonoBehaviour
{
    [Header("Defaults")]
    public bool useSeed = false;
    public int seed = 0;

    private System.Random rng;

    private class Variant
    {
        public DoorMask mask;
        public float weight;
        public bool EdgeOpen(Dir4 d) => (mask & DungeonGraph.DirToMask(d)) != 0;
    }

    private class Cell
    {
        public HashSet<int> possible;
        public int? collapsed;
    }

    private List<Variant> variants;
    private Cell[,] wfcGrid;
    private DungeonGraph graph;

    public DungeonGraph GenerateDoorGraph(StageConfig cfg, bool isBossStage)
    {
        rng = useSeed ? new System.Random(seed) : new System.Random(Environment.TickCount);

        // boss stage는 작은 고정 구조를 원하면 여기서 분기해도 됨.
        // 지금은 동일 로직 + 타입 배치에서 Boss로 처리.
        for (int attempt = 0; attempt < cfg.maxAttempts; attempt++)
        {
            BuildVariants(cfg);
            bool ok = TryGenerateOnce(cfg);
            if (!ok) continue;

            // 거리 조건
            int dist = graph.GraphDistance(graph.start, graph.exit);
            if (dist < cfg.minStartExitDistance) continue;

            return graph;
        }

        Debug.LogError("GenerateDoorGraph failed: try lowering minStartExitDistance or increase size.");
        return null;
    }

    private void BuildVariants(StageConfig cfg)
    {
        variants = new List<Variant>();

        AddRotatedVariants(MaskFromOpen(N:true, S:false, E:false, W:false), cfg.wDeadEnd);
        AddRotatedVariants(MaskFromOpen(N:true, S:true,  E:false, W:false), cfg.wStraight);
        AddRotatedVariants(MaskFromOpen(N:true, E:true,  S:false, W:false), cfg.wCorner);
        AddRotatedVariants(MaskFromOpen(N:true, E:true,  W:true,  S:false), cfg.wTJunction);
        AddRotatedVariants(MaskFromOpen(N:true, S:true,  E:true,  W:true),  cfg.wCross);
        AddRotatedVariants(DoorMask.None, cfg.wClosed);
    }

    private static DoorMask MaskFromOpen(bool N, bool S, bool E, bool W)
    {
        DoorMask m = DoorMask.None;
        if (N) m |= DoorMask.North;
        if (S) m |= DoorMask.South;
        if (E) m |= DoorMask.East;
        if (W) m |= DoorMask.West;
        return m;
    }

    private void AddRotatedVariants(DoorMask baseMask, float weight)
    {
        weight = Mathf.Max(0.0001f, weight);

        DoorMask m0 = baseMask;
        DoorMask m90 = RotateMaskCW(m0);
        DoorMask m180 = RotateMaskCW(m90);
        DoorMask m270 = RotateMaskCW(m180);

        var set = new HashSet<DoorMask> { m0, m90, m180, m270 };
        foreach (var m in set)
            variants.Add(new Variant { mask = m, weight = weight });
    }

    private static DoorMask RotateMaskCW(DoorMask m)
    {
        DoorMask r = DoorMask.None;
        if ((m & DoorMask.North) != 0) r |= DoorMask.East;
        if ((m & DoorMask.East)  != 0) r |= DoorMask.South;
        if ((m & DoorMask.South) != 0) r |= DoorMask.West;
        if ((m & DoorMask.West)  != 0) r |= DoorMask.North;
        return r;
    }

    private bool TryGenerateOnce(StageConfig cfg)
    {
        // 1) WFC 성공할 때까지 재시도
        bool wfcOk = false;
        for (int r = 0; r < cfg.wfcMaxRestartsPerAttempt; r++)
        {
            InitWfcGrid(cfg);
            ApplyWfcBorderConstraints(cfg);

            graph = new DungeonGraph(cfg.sizeX, cfg.sizeZ)
            {
                start = new Vector2Int(0, 0),
                exit  = new Vector2Int(cfg.sizeX - 1, cfg.sizeZ - 1),
            };

            if (cfg.forceDoorsOnStartExit)
            {
                ForceAtLeastOneDoor(new Vector2Int(0, 0), cfg.sizeX, cfg.sizeZ);
                ForceAtLeastOneDoor(new Vector2Int(cfg.sizeX - 1, cfg.sizeZ - 1), cfg.sizeX, cfg.sizeZ);
            }

            if (RunWFC(cfg.sizeX, cfg.sizeZ))
            {
                wfcOk = true;
                break;
            }
        }
        if (!wfcOk) return false;

        // 2) WFC 결과 -> doors
        BakeDoorsFromWfc(cfg.sizeX, cfg.sizeZ);
        graph.ClampBordersClosed();

        // 3) 고립 0개 Repair
        RepairFullyConnected(cfg.sizeX, cfg.sizeZ);
        graph.ClampBordersClosed();

        // 4) 루프 제거 
        if (cfg.forceTreeNoLoops)
        {
            MakeSpanningTreeFromStart(cfg.sizeX, cfg.sizeZ);
            graph.ClampBordersClosed();
        }

        return true;
    }

    private void InitWfcGrid(StageConfig cfg)
    {
        wfcGrid = new Cell[cfg.sizeX, cfg.sizeZ];
        var all = Enumerable.Range(0, variants.Count).ToList();

        for (int x = 0; x < cfg.sizeX; x++)
        for (int z = 0; z < cfg.sizeZ; z++)
        {
            wfcGrid[x, z] = new Cell
            {
                possible = new HashSet<int>(all),
                collapsed = null
            };
        }
    }

    private void ApplyWfcBorderConstraints(StageConfig cfg)
    {
        for (int x = 0; x < cfg.sizeX; x++)
        for (int z = 0; z < cfg.sizeZ; z++)
        {
            bool needWestWall = (x == 0);
            bool needEastWall = (x == cfg.sizeX - 1);
            bool needSouthWall = (z == 0);
            bool needNorthWall = (z == cfg.sizeZ - 1);

            var c = wfcGrid[x, z];
            c.possible.RemoveWhere(i =>
            {
                var v = variants[i];
                if (needWestWall  && v.EdgeOpen(Dir4.West))  return true;
                if (needEastWall  && v.EdgeOpen(Dir4.East))  return true;
                if (needSouthWall && v.EdgeOpen(Dir4.South)) return true;
                if (needNorthWall && v.EdgeOpen(Dir4.North)) return true;
                return false;
            });

            if (c.possible.Count == 1) c.collapsed = c.possible.First();
        }
    }

    private void ForceAtLeastOneDoor(Vector2Int p, int sizeX, int sizeZ)
    {
        if (p.x < 0 || p.x >= sizeX || p.y < 0 || p.y >= sizeZ) return;
        var c = wfcGrid[p.x, p.y];
        c.possible.RemoveWhere(i => variants[i].mask == DoorMask.None);
        if (c.possible.Count == 1) c.collapsed = c.possible.First();
    }

    private bool RunWFC(int sizeX, int sizeZ)
    {
        while (true)
        {
            var next = PickLowestEntropyCell(sizeX, sizeZ);
            if (next == null) return true;

            var (x, z) = next.Value;
            var cell = wfcGrid[x, z];
            if (cell.possible.Count == 0) return false;

            int chosen = WeightedChoice(cell.possible);
            cell.collapsed = chosen;
            cell.possible.Clear();
            cell.possible.Add(chosen);

            if (!PropagateFrom(x, z, sizeX, sizeZ)) return false;
        }
    }

    private (int x, int z)? PickLowestEntropyCell(int sizeX, int sizeZ)
    {
        int best = int.MaxValue;
        List<(int x, int z)> cand = new();

        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
        {
            var c = wfcGrid[x, z];
            if (c.collapsed.HasValue) continue;

            int count = c.possible.Count;
            if (count == 0) return (x, z);

            if (count < best)
            {
                best = count;
                cand.Clear();
                cand.Add((x, z));
            }
            else if (count == best)
            {
                cand.Add((x, z));
            }
        }

        if (cand.Count == 0) return null;
        return cand[rng.Next(cand.Count)];
    }

    private int WeightedChoice(HashSet<int> possible)
    {
        float total = 0f;
        foreach (var i in possible) total += variants[i].weight;

        float r = (float)(rng.NextDouble() * total);
        float acc = 0f;

        foreach (var i in possible)
        {
            acc += variants[i].weight;
            if (r <= acc) return i;
        }

        return possible.First();
    }

    private bool PropagateFrom(int sx, int sz, int sizeX, int sizeZ)
    {
        var q = new Queue<(int x, int z)>();
        q.Enqueue((sx, sz));

        while (q.Count > 0)
        {
            var (x, z) = q.Dequeue();

            foreach (var (nx, nz, dir) in NeighCoords(x, z, sizeX, sizeZ))
            {
                if (!ReduceNeighborByConstraint(x, z, nx, nz, dir)) continue;
                if (wfcGrid[nx, nz].possible.Count == 0) return false;
                q.Enqueue((nx, nz));
            }
        }

        return true;
    }

    private IEnumerable<(int nx, int nz, Dir4 dir)> NeighCoords(int x, int z, int sizeX, int sizeZ)
    {
        if (z + 1 < sizeZ) yield return (x, z + 1, Dir4.North);
        if (z - 1 >= 0)    yield return (x, z - 1, Dir4.South);
        if (x + 1 < sizeX) yield return (x + 1, z, Dir4.East);
        if (x - 1 >= 0)    yield return (x - 1, z, Dir4.West);
    }

    private bool ReduceNeighborByConstraint(int x, int z, int nx, int nz, Dir4 dirToNeighbor)
    {
        var a = wfcGrid[x, z];
        var b = wfcGrid[nx, nz];

        int before = b.possible.Count;
        if (before == 0) return false;

        var allowed = new HashSet<int>();

        foreach (int vb in b.possible)
        {
            bool ok = false;
            foreach (int va in a.possible)
            {
                bool edgeA = variants[va].EdgeOpen(dirToNeighbor);
                bool edgeB = variants[vb].EdgeOpen(DungeonGraph.Opp(dirToNeighbor));
                if (edgeA == edgeB)
                {
                    ok = true;
                    break;
                }
            }
            if (ok) allowed.Add(vb);
        }

        if (allowed.Count == before) return false;

        b.possible = allowed;
        if (b.possible.Count == 1 && !b.collapsed.HasValue)
            b.collapsed = b.possible.First();

        return true;
    }

    private void BakeDoorsFromWfc(int sizeX, int sizeZ)
    {
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
        {
            int idx = wfcGrid[x, z].collapsed ?? wfcGrid[x, z].possible.First();
            graph.doors[x, z] = variants[idx].mask;
        }

        // 불일치 안전 처리: 닫기
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
        {
            var p = new Vector2Int(x, z);
            foreach (var (n, dir) in graph.Neighbors(p))
            {
                if (!graph.InBounds(n)) continue;
                bool aOpen = (graph.doors[p.x, p.y] & DungeonGraph.DirToMask(dir)) != 0;
                bool bOpen = (graph.doors[n.x, n.y] & DungeonGraph.DirToMask(DungeonGraph.Opp(dir))) != 0;
                if (aOpen != bOpen)
                {
                    graph.doors[p.x, p.y] &= ~DungeonGraph.DirToMask(dir);
                    graph.doors[n.x, n.y] &= ~DungeonGraph.DirToMask(DungeonGraph.Opp(dir));
                }
            }
        }
    }

    // ---------- Repair (고립 0개) ----------
    private void RepairFullyConnected(int sizeX, int sizeZ)
    {
        var comps = GetComponentsByDoors(sizeX, sizeZ);
        if (comps.Count <= 1) return;

        int mainIndex = FindComponentIndexContaining(comps, graph.start);
        if (mainIndex < 0) mainIndex = 0;

        var main = new HashSet<Vector2Int>(comps[mainIndex]);

        for (int i = 0; i < comps.Count; i++)
        {
            if (i == mainIndex) continue;
            var island = new HashSet<Vector2Int>(comps[i]);

            CarveShortestPathBetweenSets(main, island, sizeX, sizeZ);

            comps = GetComponentsByDoors(sizeX, sizeZ);
            if (comps.Count <= 1) return;

            mainIndex = FindComponentIndexContaining(comps, graph.start);
            if (mainIndex < 0) mainIndex = 0;
            main = new HashSet<Vector2Int>(comps[mainIndex]);
        }
    }

    private List<List<Vector2Int>> GetComponentsByDoors(int sizeX, int sizeZ)
    {
        var visited = new bool[sizeX, sizeZ];
        var comps = new List<List<Vector2Int>>();

        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
        {
            if (visited[x, z]) continue;

            var comp = new List<Vector2Int>();
            var q = new Queue<Vector2Int>();
            var s = new Vector2Int(x, z);

            visited[x, z] = true;
            q.Enqueue(s);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                comp.Add(cur);

                foreach (var (n, dir) in graph.Neighbors(cur))
                {
                    if (!graph.InBounds(n)) continue;
                    if (visited[n.x, n.y]) continue;

                    if (graph.IsEdgeOpen(cur, dir))
                    {
                        visited[n.x, n.y] = true;
                        q.Enqueue(n);
                    }
                }
            }

            comps.Add(comp);
        }

        return comps;
    }

    private int FindComponentIndexContaining(List<List<Vector2Int>> comps, Vector2Int p)
    {
        for (int i = 0; i < comps.Count; i++)
            if (comps[i].Contains(p)) return i;
        return -1;
    }

    private void CarveShortestPathBetweenSets(HashSet<Vector2Int> main, HashSet<Vector2Int> island, int sizeX, int sizeZ)
    {
        var prev = new Dictionary<Vector2Int, (Vector2Int p, Dir4 dirFromPrev)>();
        var q = new Queue<Vector2Int>();
        var visited = new bool[sizeX, sizeZ];

        foreach (var m in main)
        {
            visited[m.x, m.y] = true;
            q.Enqueue(m);
        }

        Vector2Int hit = default;
        bool found = false;

        while (q.Count > 0 && !found)
        {
            var cur = q.Dequeue();

            foreach (var (n, dir) in graph.Neighbors(cur))
            {
                if (!graph.InBounds(n)) continue;
                if (visited[n.x, n.y]) continue;

                visited[n.x, n.y] = true;
                prev[n] = (cur, dir);

                if (island.Contains(n))
                {
                    hit = n;
                    found = true;
                    break;
                }

                q.Enqueue(n);
            }
        }

        if (!found) return;

        var curPos = hit;
        while (!main.Contains(curPos))
        {
            var (p, dirFromPrev) = prev[curPos];
            graph.OpenEdge(p, dirFromPrev);
            curPos = p;
        }
    }

    // ---------- Treeize (루프 제거) ----------
    private void MakeSpanningTreeFromStart(int sizeX, int sizeZ)
    {
        var parent = new Dictionary<Vector2Int, (Vector2Int p, Dir4 dirFromParent)>();
        var visited = new HashSet<Vector2Int>();
        var q = new Queue<Vector2Int>();

        visited.Add(graph.start);
        q.Enqueue(graph.start);

        while (q.Count > 0)
        {
            var cur = q.Dequeue();

            foreach (var (n, dir) in graph.Neighbors(cur))
            {
                if (!graph.InBounds(n)) continue;
                if (!graph.IsEdgeOpen(cur, dir)) continue;
                if (visited.Contains(n)) continue;

                visited.Add(n);
                parent[n] = (cur, dir);
                q.Enqueue(n);
            }
        }

        // 전부 닫고 트리 간선만 다시 연다
        for (int x = 0; x < sizeX; x++)
        for (int z = 0; z < sizeZ; z++)
            graph.doors[x, z] = DoorMask.None;

        foreach (var kv in parent)
        {
            var child = kv.Key;
            var (p, dir) = kv.Value;
            graph.OpenEdge(p, dir);
        }

        // 혹시라도 누락이 있으면 Repair
        RepairFullyConnected(sizeX, sizeZ);
    }
}