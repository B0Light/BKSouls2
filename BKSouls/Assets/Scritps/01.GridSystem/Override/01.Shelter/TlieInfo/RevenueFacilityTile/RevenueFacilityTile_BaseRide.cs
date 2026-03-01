using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RevenueFacilityTile_BaseRide : RevenueFacilityTile
{
    [SerializeField] private List<Transform> seats = new List<Transform>();
    private List<PathFindingUnit> riders = new List<PathFindingUnit>();
    protected override IEnumerator ProcessQueue()
    {
        isOperating = true;
        
        while (waitingQueue.Count > 0)
        {
            Transform availableSeat = GetAvailableSeat();
            if (availableSeat == null) // 가득 차면 여석이 없음 
            {
                yield return new WaitForSeconds(2f);
                // 출발
                StartCoroutine(OperateAttraction());
                yield break;
            }
            
            // 탑승 
            PathFindingUnit rider = waitingQueue.Peek();
            Processing(rider, availableSeat);
            riders.Add(rider);
            waitingQueue.Dequeue();
            yield return new WaitForSeconds(0.1f);
        }

        // 현재 대기인원이 없지만 누군가 탑승한 경우 
        if (riders.Count > 0)
        {
            // 출발 
            StartCoroutine(OperateAttraction());
        }
        else
        {
            isOperating = false;
        }
    }

    protected override void Processing(PathFindingUnit processor, Transform seat)
    {
        base.Processing(processor, seat);
        processor.transform.parent = seat;
        processor.transform.localPosition = Vector3.zero;
        processor.transform.localRotation = Quaternion.identity;
         
        processor.RideAttraction();
    }
    
    private Transform GetAvailableSeat()
    {
        foreach (Transform seat in seats)
        {
            if (seat.childCount == 0)
            {
                return seat;
            }
        }
        return null;
    }
    
    private void ExitRide(PathFindingUnit rider)
    {
        rider.transform.parent = null; 
        rider.transform.position = exitPoint.position;
        rider.transform.rotation = exitPoint.rotation;
        rider.ExitAttraction();

        rider.GetNextDestination(GetExitRoad());
    }

    protected virtual IEnumerator OperateAttraction()
    {
        Debug.LogWarning("OPERATE ATTRACTION");
        yield return ArriveAttraction();
    }

    protected IEnumerator ArriveAttraction()
    {
        foreach (var rider in riders.ToList())
        {
            ExitRide(rider);
            yield return new WaitForSeconds(1f);
        }
        riders.Clear();
        isOperating = false;
        StartCoroutine(ProcessQueue());
    }
}

