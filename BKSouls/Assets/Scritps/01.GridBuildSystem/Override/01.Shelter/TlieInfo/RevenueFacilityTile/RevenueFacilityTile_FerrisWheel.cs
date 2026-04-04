using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevenueFacilityTile_FerrisWheel : RevenueFacilityTile
{
    [SerializeField] private List<Transform> seats = new List<Transform>();
    private Dictionary<Transform, bool> seatOccupied = new Dictionary<Transform, bool>();

    private void Start()
    {
        // 모든 좌석을 비어 있다고 초기화
        foreach (var seat in seats)
        {
            seatOccupied[seat] = false;
        }
    }
    
    protected override IEnumerator ProcessQueue()
    {
        while (waitingQueue.Count > 0)
        {
            PathFindingUnit rider = waitingQueue.Dequeue();
            Transform assignedSeat = null;

            yield return StartCoroutine(WaitForLowestAvailableSeat());
            assignedSeat = GetLowestAvailableSeat();

            if (assignedSeat == null)
            {
                Debug.LogWarning("모든 좌석이 찼습니다.");
                yield break;
            }

            Processing(rider, assignedSeat);
        }
    }
        
    protected override void Processing(PathFindingUnit processor, Transform seat)
    {
        base.Processing(processor, seat);
        seatOccupied[seat] = true;

        processor.transform.parent = seat; 
        processor.transform.localPosition = Vector3.zero; 
        processor.transform.localRotation = Quaternion.identity;
         
        processor.RideAttraction();
        StartCoroutine(StayOnRide(processor, seat));
    }
    
    private IEnumerator StayOnRide(PathFindingUnit rider, Transform seat)
    {
        yield return new WaitForSeconds(36f); // 한 바퀴 회전

        ExitRide(rider, seat);
    }

    private void ExitRide(PathFindingUnit rider, Transform seat)
    {
        rider.transform.parent = null; 
        rider.transform.position = exitPoint.position;
        rider.transform.rotation = exitPoint.rotation;
        rider.ExitAttraction();
        seatOccupied[seat] = false; 

        rider.GetNextDestination(GetExitRoad());

        // 다음 대기자 처리
        if (waitingQueue.Count > 0)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    private IEnumerator WaitForLowestAvailableSeat()
    {
        Transform seat = null;

        // 2️⃣ 가장 낮은 빈 좌석이 생길 때까지 반복
        while ((seat = GetLowestAvailableSeat()) == null || seatOccupied[seat])
        {
            yield return null; // 다음 프레임까지 대기
        }
    }
    
    private Transform GetLowestAvailableSeat()
    {
        Transform lowestSeat = null;
        float lowestY = float.MaxValue;

        foreach (var seat in seats)
        {
            if (seat.position.y < lowestY)
            {
                lowestY = seat.position.y;
                lowestSeat = seat;
            }
        }

        return lowestSeat;
    }
}
