using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;

public class RevenueFacilityTile : PlacedObject, IRevenueFacility
{
    [Header("Attraction Properties")]
    [SerializeField] protected float cycleTime = 10f; // 한 사이클 시간 (초 단위)
    protected Transform exitPoint; // 출구 위치

    private readonly Queue<PathFindingUnit> _approachingQueue = new Queue<PathFindingUnit>(); // 타일에는 들어 왔으나 아직 줄 서지 않은 인원 
    protected Queue<PathFindingUnit> waitingQueue = new Queue<PathFindingUnit>(); // 대기열
    protected bool isOperating = false; // 놀이기구가 동작 중인지 여부
    
    [SerializeField] protected Transform queueStartPoint;
    protected readonly float queueSpacing = 1.5f;
    [SerializeField] private int maxColumns = 8;
    [SerializeField] private int maxCapacity = 10;
    [SerializeField] protected IncomeEventBalance incomeEventChannel;
    

    protected override void Awake()
    {
        base.Awake();
        exitPoint = FindChildByName(gameObject, "Exit").transform;
    }
    
    public virtual bool AddVisitor(PathFindingUnit visitor)
    {
        if (_approachingQueue.Count + waitingQueue.Count > maxCapacity) return false;
        _approachingQueue.Enqueue(visitor);
        StartCoroutine(EnqueueVisitor(visitor));
        return true;
    }
    
    private IEnumerator EnqueueVisitor(PathFindingUnit visitor)
    {
        int queueCount = _approachingQueue.Count + waitingQueue.Count -1; // index는 0부터 시작 : 첫번쨰 사람 -> 0번 

        int row = queueCount / maxColumns; // 몇 번째 줄인지
        int column = queueCount % maxColumns; // 해당 줄에서 몇 번째 칸인지

        Vector3 targetPosition = queueStartPoint.position 
                                 - queueStartPoint.forward * queueSpacing * column  // 앞뒤 간격
                                 - queueStartPoint.right * queueSpacing * row; // 좌우 간격
        
        yield return StartCoroutine(visitor.MoveToPointCoroutine(targetPosition));
        
        waitingQueue.Enqueue(_approachingQueue.Dequeue());
        if (!isOperating)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    protected virtual IEnumerator ProcessQueue()
    {
        yield return null;
    }

    protected virtual void Processing(PathFindingUnit processor, Transform seat)
    {
        GenerateIncome();
    }

    protected void GenerateIncome()
    {
        // 약간의 랜덤 변동을 주는 수입 계산
        int actualIncome = Mathf.RoundToInt(GetFee() * Random.Range(0.8f, 1.2f));
        
        // 수입 데이터 생성
        IncomeData incomeData = new IncomeData(buildObjData.itemName, actualIncome);
        
        // 이벤트 발행
        if (incomeEventChannel != null)
        {
            incomeEventChannel.RaiseIncomeEvent(incomeData);
        }
    }
}
