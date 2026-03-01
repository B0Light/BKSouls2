using System.Collections;
using UnityEngine;

public class HUDGridBuildingSelector : MonoBehaviour
{
    [SerializeField] private GameObject selectPrefab;
    [SerializeField] private Transform selectButtonSlot;
    
    public IEnumerator InitBtnSlot(CellType category)
    {
        RefreshSlot();
        yield return StartCoroutine(WaitForDataLoad());

        CategoryBuildHUDManager categoryBuildHUDManager = GridBuildHUDManager.Instance as CategoryBuildHUDManager;
        if(!categoryBuildHUDManager) yield break;
        foreach (var buildObjData in categoryBuildHUDManager.unlockedBuildingByCategory[category])
        {
            GameObject instanceBtnObj = Instantiate(selectPrefab, selectButtonSlot);
            ShopShelfItem_Building btnUnit = instanceBtnObj.GetComponent<ShopShelfItem_Building>();
            btnUnit.Init(buildObjData);
        }
        
        //GameObject instanceExitBtnObj = Instantiate(selectPrefab, selectButtonSlot);
        //HUDGridBuildingUnit exitBtnUnit = instanceExitBtnObj.GetComponent<HUDGridBuildingUnit>();
        //exitBtnUnit.InitExitButton();
    }
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
    }

    public void RefreshSlot()
    {
        for (int i = selectButtonSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(selectButtonSlot.GetChild(i).gameObject);
        }
    }
}
