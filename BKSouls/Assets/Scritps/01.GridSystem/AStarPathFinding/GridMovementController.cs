using System.Collections.Generic;
using UnityEngine;

public class GridMovementController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public bool allowDiagonalMovement = false;
    public bool smoothMovement = true;
    
    [Header("Debug")]
    public bool showDebugPath = true;
    public Color pathColor = Color.green;

    private List<Vector2Int> _patrolPointList;
    private GridPathfinder _pathfinder;
    private List<GridCell> _currentPath;
    private int _currentPathIndex;
    private bool _isMoving;
    private Vector3 _targetWorldPosition;
    private Vector2Int _currentGridPosition;
    
    // 그리드 좌표와 월드 좌표 변환을 위한 설정
    [HideInInspector] public Vector3 cellSize = Vector3.one;
    [HideInInspector] public Vector3 gridOffset = Vector3.zero;

    public void Initialize(GridPathfinder pathfinder, Vector2Int startPosition)
    {
        _pathfinder = pathfinder;
        _pathfinder.AllowDiagonalMovement = allowDiagonalMovement;
        
        _currentGridPosition = startPosition;
        transform.position = GridToWorldPosition(startPosition);
        _targetWorldPosition = transform.position;
    }

    public void StartPatrol(List<Vector2Int> patrolPointList)
    {
        _patrolPointList = patrolPointList;
        MoveTo(GetNextPatrolPoint());
    }

    private Vector2Int GetNextPatrolPoint()
    {
        if (_patrolPointList == null || _patrolPointList.Count == 0)
        {
            Debug.LogError("Patrol point list is empty or null.");
            return _currentGridPosition; 
        }

        int currentIndex = _patrolPointList.IndexOf(_currentGridPosition);

        if (currentIndex == -1)
        {
            Debug.LogWarning("Current grid position not found in the patrol list. Returning the first point.");
            return _patrolPointList[0];
        }

        int nextIndex = (currentIndex + 1) % _patrolPointList.Count;

        return _patrolPointList[nextIndex];
    }

    private void MoveTo(Vector2Int targetPosition)
    {
        if (_pathfinder == null)
        {
            Debug.LogError("Pathfinder not initialized!");
            return;
        }

        var path = _pathfinder.NavigatePath(_currentGridPosition, targetPosition);
        
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning($"No path found from {_currentGridPosition} to {targetPosition}");
            return;
        }

        _currentPath = path;
        _currentPathIndex = 0;
        _isMoving = false;
        
        StartMovement();
    }

    private void Update()
    {
        if (_isMoving && smoothMovement)
        {
            HandleSmoothMovement();
        }
    }

    private void StartMovement()
    {
        if (_currentPath == null || _currentPathIndex >= _currentPath.Count)
        {
            return;
        }

        var nextGridPos = _currentPath[_currentPathIndex];
        _targetWorldPosition = GridToWorldPosition(nextGridPos.Position);
        
        if (smoothMovement)
        {
            _isMoving = true;
        }
        else
        {
            // 즉시 이동
            transform.position = _targetWorldPosition;
            _currentGridPosition = nextGridPos.Position;
            OnReachedWaypoint();
        }
    }

    private void HandleSmoothMovement()
    {
        transform.position = Vector3.MoveTowards(
            transform.position, 
            _targetWorldPosition, 
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _targetWorldPosition) < 0.01f)
        {
            transform.position = _targetWorldPosition;
            _currentGridPosition = _currentPath[_currentPathIndex].Position;
            _isMoving = false;
            
            OnReachedWaypoint();
        }
    }

    private void OnReachedWaypoint()
    {
        _currentPathIndex++;
        
        if (_currentPathIndex >= _currentPath.Count)
        {
            // 목적지 도달
            OnPathCompleted();
        }
        else
        {
            // 다음 웨이포인트로 이동
            StartMovement();
        }
    }

    private void OnPathCompleted()
    {
        Debug.Log($"Reached destination: {_currentGridPosition}");
        MoveTo(GetNextPatrolPoint());
    }

    private Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * cellSize.x + gridOffset.x,
            gridOffset.y,
            gridPos.y * cellSize.z + gridOffset.z
        );
    }

    // 디버그 드로잉
    private void OnDrawGizmos()
    {
        if (!showDebugPath || _currentPath == null) return;

        Gizmos.color = pathColor;
        
        for (int i = 0; i < _currentPath.Count - 1; i++)
        {
            var current = GridToWorldPosition(_currentPath[i].Position);
            var next = GridToWorldPosition(_currentPath[i + 1].Position);
            
            Gizmos.DrawLine(current, next);
            Gizmos.DrawWireSphere(current, 0.2f);
        }
        
        if (_currentPath.Count > 0)
        {
            var last = GridToWorldPosition(_currentPath[_currentPath.Count-1].Position);
            Gizmos.DrawWireSphere(last, 0.2f);
        }
    }
}