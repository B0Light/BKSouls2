using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingUnit : MonoBehaviour
{
    private GridPathfinder _pathfinder;
    [SerializeField] private float rotationSpeed = 5f; // 회전 속도
    [SerializeField] private float moveSpeed = 5f; // 이동 속도

    [SerializeField] private int numOfTarget = 3;
    private PlacedObject _destination;
    
    private readonly Variable<bool> _isMoving = new Variable<bool>(false); // 이동 중인지 확인
    private Animator _animator;
    private readonly int _movementHash = Animator.StringToHash("isMove");
    private readonly int _rideHash = Animator.StringToHash("isRide");
    private readonly int _speedHash = Animator.StringToHash("Speed");
    private ShelterManager _shelterManager;
    private Vector2Int _curGoal;
    private void Awake()
    {
        _pathfinder = new GridPathfinder(BaseGridBuildSystem.Instance.GetGrid());
        _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if(_animator == null) 
            _animator = GetComponentInChildren<Animator>();
        _animator.SetFloat(_speedHash, moveSpeed);
        _isMoving.OnValueChanged += b => _animator?.SetBool(_movementHash, b);
    }

    public void SpawnVisitor(ShelterManager shelterManager)
    {
        _shelterManager = shelterManager;
        Vector2Int startPos = BaseGridBuildSystem.Instance.GetEntrancePos();
        _curGoal = startPos;
        if (!SetRoute(startPos, SelectRandomTarget()))
        {
            GetNextDestination(startPos);
        }
    }
    
    private Vector2Int SelectRandomTarget()
    {
        ShelterGridBuildSystem shelterGridBuildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
        if (!shelterGridBuildSystem) return Vector2Int.zero;
        var attractions = shelterGridBuildSystem.CheckPointList;
        int randomIndex = Random.Range(0, attractions.Count);
        return attractions[randomIndex];
    }

    private bool SetRoute(Vector2Int startPos, Vector2Int goalPos)
    {
        List<Vector3> routePosList = new List<Vector3>();
        List<GridCell> paths = _pathfinder.NavigatePath(startPos, goalPos);
        if (paths != null)
        {
            foreach (var gridRoute in paths)
            {
                _destination = gridRoute.GetPlacedObject();
                routePosList.Add(_destination.GetEntrancePoint());
            }
            StartMoving(routePosList);
            _curGoal = goalPos;
            return true;
        }
        else
        {
            Debug.Log("[PathFindingUnit] NO WAY GOAL : " + goalPos);
            // 선택한 목적지로 가는 경로가 없는 경우 
            return false;
        }
    }
    
    private void StartMoving(List<Vector3> points)
    {
       StartCoroutine(MoveThroughPointsCoroutine(points));
    }
    
    private IEnumerator MoveThroughPointsCoroutine(List<Vector3> points)
    {
        foreach (Vector3 targetPoint in points)
        {
            yield return StartCoroutine(MoveToPointCoroutine(targetPoint));
        }
        TryAttraction();
    }
    
    public IEnumerator MoveToPointCoroutine(Vector3 targetPoint, Quaternion? targetRotation = null)
    {
        yield return StartCoroutine(RotateTowardsPointCoroutine(targetPoint));
        _isMoving.Value = true;
        // Move until very close to the target
        while (Vector3.Distance(transform.position, targetPoint) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
            yield return null; // Wait until next frame
        }

        // Ensure precise positioning
        transform.position = targetPoint;
        
        if (targetRotation.HasValue)
        {
            transform.rotation = targetRotation.Value;
        }

        _isMoving.Value = false;
    }
    
    private IEnumerator RotateTowardsPointCoroutine(Vector3 targetPoint)
    {
        if (targetPoint == transform.position) yield break;
        
        Vector3 direction = (targetPoint - transform.position).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            _isMoving.Value = false;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null; 
        }
        transform.rotation = targetRotation;
    }

    
    private void TryAttraction()
    {
        if (!_destination) return;
        
        IRevenueFacility revenueFacilityTile = _destination.gameObject.GetComponent<IRevenueFacility>();
        
        if(revenueFacilityTile == null) return;

        if (!revenueFacilityTile.AddVisitor(this))
        {
            GetNextDestination(_curGoal);
        }
    }
    
    public void GetNextDestination(Vector2Int curPos)
    {
        Debug.Log("Set NEXT DESTINATION");
        if(numOfTarget > 0)
        {
            numOfTarget--;
            if (!SetRoute(curPos, SelectRandomTarget()))
            {
                GetNextDestination(curPos);
            }
        }
        else
        {
            if (!SetRoute(curPos, Random.Range(0, 1) > 0.5f ? 
                    BaseGridBuildSystem.Instance.GetDungeonPos() : BaseGridBuildSystem.Instance.GetEntrancePos()))
            {
                Destroy(this.gameObject);
            }
        }
    }

    public void RideAttraction()
    {
        _animator.SetBool(_rideHash, true);
        _animator.CrossFade("Ride", 0.2f);
    }

    public void ExitAttraction()
    {
        _animator.SetBool(_rideHash, false);
    }

    public void LeaveShelter()
    {
        _shelterManager.RemoveVisitor(this);
        Destroy(gameObject);
    }
}
