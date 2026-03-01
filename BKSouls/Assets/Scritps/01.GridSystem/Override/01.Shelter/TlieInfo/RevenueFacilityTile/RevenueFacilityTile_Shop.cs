using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class RevenueFacilityTile_Shop : RevenueFacilityTile
{
    [SerializeField] private Transform actionPoint;
    private bool _isWorking = false;
    protected override IEnumerator ProcessQueue()
    {
        if(_isWorking) yield break;
        
        while (waitingQueue.Count > 0)
        {
            _isWorking = true;
            PathFindingUnit client = waitingQueue.Dequeue();
            
            yield return client.MoveToPointCoroutine(actionPoint.position, actionPoint.rotation);

            var visitors = waitingQueue.ToArray();
            for (int i = 0; i < visitors.Length; i++)
            {
                Vector3 targetPosition = queueStartPoint.position - queueStartPoint.forward * queueSpacing * i;
                yield return StartCoroutine(visitors[i].MoveToPointCoroutine(targetPosition));
            }
            
            yield return new WaitForSeconds(cycleTime);
            
            yield return StartCoroutine(ExitTrade(client));
            _isWorking = false;
        }
    }
    
    private IEnumerator ExitTrade(PathFindingUnit client)
    {
        GenerateIncome();
        yield return client.MoveToPointCoroutine(exitPoint.position, exitPoint.rotation);
        client.GetNextDestination(GetExitRoad());
    }
}
