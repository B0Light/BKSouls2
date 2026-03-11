using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder : PathfindingBase<GridCell>
{
    private readonly FixedGridXZ<GridCell> _fixedGrid;
    public List<RectInt> RoomList { get; set; }
    private GridCell _goalNode;

    private Vector2Int _gridSize;
    private GridCell[,] _nodeGrid;
    
    // 이동 방향 정의 (4방향 또는 8방향) - from MapGridPathfinder
    private static readonly Vector2Int[] DirectionsCardinal = 
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };
    
    private static readonly Vector2Int[] DirectionsDiagonal = 
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    public bool AllowDiagonalMovement { get; set; } = false;

    public GridPathfinder(FixedGridXZ<GridCell> fixedGridXZ)
    {
        _fixedGrid = fixedGridXZ;
    }

    public List<GridCell> NavigatePath(Vector2Int start, Vector2Int goal)
    {
        if (_nodeGrid == null)
        {
            _gridSize = new Vector2Int(_fixedGrid.Width, _fixedGrid.Height);
            _nodeGrid = new GridCell[_gridSize.x, _gridSize.y];
            foreach (GridCell obj in _fixedGrid.GetAllGridObjects())
            {
                _nodeGrid[obj.Position.x, obj.Position.y] = obj;
            }
        }

        GridCell startNode = GetNode(start);
        _goalNode = GetNode(goal);
        
        if (!IsValidPosition(start) || !IsValidPosition(goal))
        {
            Debug.LogWarning($"Invalid positions: Start({start}) or Goal({goal})");
            return null;
        }

        ResetNodeCosts();
        startNode.GCost = 0;
        startNode.HCost = GetDistance(startNode, _goalNode);

        List<GridCell> path = FindPath(startNode, _goalNode);
        
        return path;
    }

    private void InitializeNodeGrid()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                if (_fixedGrid != null)
                {
                    _nodeGrid[x, y] = new GridCell(x, y, _fixedGrid.GetGridObject(x, y).CellType);
                }
            }
        }
    }

    private void ResetNodeCosts()
    {
        for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                var node = _nodeGrid[x, y];
                if (node != null)
                {
                    node.GCost = float.MaxValue;
                    node.HCost = 0;
                    node.Parent = null;
                }
            }
        }
    }

    protected override IEnumerable<GridCell> GetNeighbors(GridCell node)
    {
        var directions = AllowDiagonalMovement ? DirectionsDiagonal : DirectionsCardinal;

        foreach (var direction in directions)
        {
            Vector2Int neighborPos = node.Position + direction;
            
            if (IsValidPosition(neighborPos))
            {
                var neighborNode = GetNode(neighborPos);
                if (neighborNode == null) continue;  // Safety check
                
                if (_fixedGrid != null)
                {
                    GridCell currentNeighborGrid = _fixedGrid.GetGridObject(neighborPos.x, neighborPos.y);
                     yield return currentNeighborGrid;
                }
                else
                {
                    Debug.LogWarning("Fixed Grid is null ");
                    yield return neighborNode;
                }
            }
        }
    }
    
    protected override float GetDistance(GridCell a, GridCell b)
    {
        int distX = Mathf.Abs(a.GetEntrancePosition().x - b.GetEntrancePosition().x);
        int distY = Mathf.Abs(a.GetEntrancePosition().y - b.GetEntrancePosition().y);
        
        if (AllowDiagonalMovement)
        {
            int diagonal = Mathf.Min(distX, distY);
            int straight = Mathf.Max(distX, distY) - diagonal;
            return straight + diagonal * 1.414f;
        }
        // use Manhattan distance
        else
        {
            return distX + distY;
        }
    }

    // 셀 타입에 따른 이동 비용 가중치를 적용
    protected override float GetMovementCost(GridCell from, GridCell to)
    {
        float baseCost = GetDistance(from, to);

        // 타일/셀 타입별 가중치. 필요 시 외부 설정으로 분리 가능
        float terrainMultiplier = 1f;
        
        // GridCell의 논리적 CellType 기반 가중치
        switch (to.CellType)
        {
            case CellType.Floor:
            case CellType.Road:
                terrainMultiplier = 0.5f;
                break;
            case CellType.Empty:
                terrainMultiplier = 1f;
                break;
            default:
                terrainMultiplier = 1.0f;
                break;
        }
        

        // 대각 이동 보정: 기본 거리에서 이미 1.414 적용됨. 필요 시 추가 계수 조정 가능
        return baseCost * terrainMultiplier;
    }

    private static Dir ConvertToConnectDirection(Vector2Int direction)
    {
        if (direction == new Vector2Int(0, 1)) return Dir.Down;
        if (direction == new Vector2Int(-1, 0)) return Dir.Right;
        if (direction == new Vector2Int(0, -1)) return Dir.Up;
        if (direction == new Vector2Int(1, 0)) return Dir.Left;
        return Dir.Down;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < _gridSize.x && pos.y >= 0 && pos.y < _gridSize.y;
    }

    private GridCell GetNode(Vector2Int position)
    {
        if (_nodeGrid == null) {
            Debug.LogError("Node grid not initialized. Call NavigatePath or use MapData constructor.");
            return null;
        }
        return _nodeGrid[position.x, position.y];
    }
}
