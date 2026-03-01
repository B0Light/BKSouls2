using System;
using System.Collections.Generic;
using bkTools;

public interface IPathNode
{
    float GCost { get; set; }
    float HCost { get; set; }
    float FCost { get; }
    IPathNode Parent { get; set; }
}

public abstract class PathfindingBase<TNode>
    where TNode : class, IPathNode
{
    protected abstract IEnumerable<TNode> GetNeighbors(TNode node);
    protected abstract float GetDistance(TNode a, TNode b);
    
    // 실제 G 비용을 계산할 때 사용할 이동 비용 훅
    // 기본은 격자 간 거리 사용. 하위 클래스에서 셀 타입/지형 가중치를 반영하도록 재정의 가능
    protected virtual float GetMovementCost(TNode from, TNode to)
    {
        return GetDistance(from, to);
    }

    protected List<TNode> FindPath(TNode startNode, TNode goalNode)
    {
        PriorityQueue<TNode, float> pq = new PriorityQueue<TNode, float>();
        HashSet<TNode> closedList = new HashSet<TNode>();

        pq.Enqueue(startNode, 0);

        while (pq.Count > 0)
        {
            TNode currentNode = pq.Dequeue();
            closedList.Add(currentNode);
            if (currentNode.Equals(goalNode))
            {
                return RetracePath(startNode, goalNode);
            }
            
            foreach (TNode neighbor in GetNeighbors(currentNode))
            {
                if (closedList.Contains(neighbor)) continue;

                float tentativeGCost = currentNode.GCost + GetMovementCost(currentNode, neighbor);

                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = GetDistance(neighbor, goalNode);
                    neighbor.Parent = currentNode;
                    
                    pq.Enqueue(neighbor, tentativeGCost);
                }
            }
        }

        return null; // 경로를 찾지 못한 경우
    }

    private List<TNode> RetracePath(TNode startNode, TNode goalNode)
    {
        List<TNode> path = new List<TNode>();
        TNode currentNode = goalNode;

        while (!currentNode.Equals(startNode))
        {
            path.Add(currentNode);
            currentNode = (TNode)currentNode.Parent;
        }
        path.Add(startNode);
        path.Reverse();
        return path;
    }
}
