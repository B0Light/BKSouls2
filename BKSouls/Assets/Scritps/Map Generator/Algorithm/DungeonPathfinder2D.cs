using System;
using System.Collections.Generic;
using UnityEngine;
using bkTools;

public class DungeonPathfinder2D {
    public class Node {
        public Vector2Int Position { get; private set; }
        public Node Previous { get; set; }
        public float Cost { get; set; }

        public Node(Vector2Int position) {
            Position = position;
            Cost = float.PositiveInfinity;
        }
    }

    public struct PathCost {
        public bool traversable;
        public float cost;
    }

    static readonly Vector2Int[] neighbors = {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    Grid2D<Node> grid;
    PriorityQueue<Node, float> queue;
    HashSet<Node> closed;

    public DungeonPathfinder2D(Vector2Int size) {
        grid = new Grid2D<Node>(size, Vector2Int.zero);
        queue = new PriorityQueue<Node, float>();
        closed = new HashSet<Node>();

        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                grid[x, y] = new Node(new Vector2Int(x, y));
            }
        }
    }

    void ResetNodes() {
        var size = grid.Size;
        for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
                var node = grid[x, y];
                node.Previous = null;
                node.Cost = float.PositiveInfinity;
            }
        }
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, Func<Node, Node, PathCost> costFunction) {
        ResetNodes();
        queue.Clear();
        closed.Clear();

        var startNode = grid[start];
        startNode.Cost = 0;
        queue.Enqueue(startNode, 0);

        while (queue.Count > 0) {
            Node current = queue.Dequeue();
            closed.Add(current);

            if (current.Position == end) {
                return ReconstructPath(current);
            }

            foreach (var offset in neighbors) {
                Vector2Int neighborPos = current.Position + offset;

                if (!grid.InBounds(neighborPos)) continue;
                var neighbor = grid[neighborPos];
                if (closed.Contains(neighbor)) continue;

                var pathCost = costFunction(current, neighbor);
                if (!pathCost.traversable) continue;

                float newCost = current.Cost + pathCost.cost;

                if (newCost < neighbor.Cost) {
                    neighbor.Previous = current;
                    neighbor.Cost = newCost;
                    queue.Enqueue(neighbor, newCost);
                }
            }
        }

        return null; // 경로를 찾지 못한 경우
    }

    List<Vector2Int> ReconstructPath(Node endNode) {
        List<Vector2Int> path = new List<Vector2Int>();

        for (Node node = endNode; node != null; node = node.Previous) {
            path.Insert(0, node.Position); // 리스트의 앞에 추가하여 스택처럼 작동
        }

        return path;
    }
}
