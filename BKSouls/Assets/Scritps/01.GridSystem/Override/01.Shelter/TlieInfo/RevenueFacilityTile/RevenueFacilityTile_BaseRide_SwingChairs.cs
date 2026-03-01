using System.Collections;
using UnityEngine;

public class RevenueFacilityTile_BaseRide_SwingChairs : RevenueFacilityTile_BaseRide
{
    [SerializeField] private GameObject rotationObject;
    [SerializeField] private float rotationSpeed = 25f;
    protected override IEnumerator OperateAttraction()
    {
        isOperating = true;
        yield return RotateCoroutine();
        yield return ArriveAttraction();
    }
    
    private IEnumerator RotateCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < cycleTime)
        {
            rotationObject.transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }
    }
}
