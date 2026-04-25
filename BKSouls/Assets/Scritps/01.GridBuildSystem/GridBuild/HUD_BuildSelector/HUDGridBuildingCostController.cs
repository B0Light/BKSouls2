using System;
using System.Collections;
using BK.Inventory;
using UnityEngine;

public class HUDGridBuildingCostController : MonoBehaviour
{
    [SerializeField] private GameObject costPrefab;
    [SerializeField] private Transform costItemSlot;
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
        while (!BaseGridBuildSystem.Instance)
        {
            yield return null; // 한 프레임 대기
        }
    }
    private void OnEnable()
    {
        StartCoroutine(BindSelectBuilding());
    }

    private IEnumerator BindSelectBuilding()
    {
        yield return StartCoroutine(WaitForDataLoad());
        BaseGridBuildSystem.OnSelectedChanged += Instance_OnSelectedChanged;
        BaseGridBuildSystem.OnObjectPlaced  += Instance_OnSelectedChanged;
    }
    
    private void OnDisable()
    {
        BaseGridBuildSystem.OnSelectedChanged -= Instance_OnSelectedChanged;
        BaseGridBuildSystem.OnObjectPlaced  -= Instance_OnSelectedChanged;
    }

    private void Instance_OnSelectedChanged(BuildObjData objectToPlace)
    {
        BuildObjData item = objectToPlace;

        DeleteAllChildren(costItemSlot);
        if (item == null) return;
        if (costPrefab == null)
        {
            Debug.LogWarning("[HUDGridBuildingCostController] costPrefab is not assigned in the Inspector.");
            return;
        }

        foreach (var costItemPair in item.GetCostItems())
        {
            GameObject spawnedCostItem = Instantiate(costPrefab, costItemSlot);

            spawnedCostItem.GetComponent<ShopCostItem>()?.Init(costItemPair.Key, costItemPair.Value);
        }
    }
    
    private void DeleteAllChildren(Transform parentTransform)
    {
        if(parentTransform == null)
        {
            Debug.LogWarning("[HUDGridBuildingCostController] : No Parent Transform");
            return;
        }
        for (int i = parentTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = parentTransform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
}
