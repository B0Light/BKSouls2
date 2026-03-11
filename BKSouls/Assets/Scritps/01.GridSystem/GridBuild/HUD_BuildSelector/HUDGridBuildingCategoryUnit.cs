using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDGridBuildingCategoryUnit : MonoBehaviour
{
    [SerializeField] private Image buildingIcon;
    [SerializeField] private Button selectButton;
    
    public void InitButton(CellType categoryCode)
    {
        Debug.LogWarning($"Category : {categoryCode}");
        
        gameObject.SetActive(true);
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(Init(categoryCode));
        }
        else
        {
            Debug.Log("GameObject is not ready");
        }
    }
    
    private IEnumerator Init(CellType categoryCode)
    {
        yield return WaitForDataLoad();
        
        selectButton.onClick.RemoveAllListeners();
        
        buildingIcon.sprite = WorldDatabase_Build.Instance.GetCategoryIcon(categoryCode);
        CategoryBuildHUDManager categoryBuildHUDManager = GridBuildHUDManager.Instance as CategoryBuildHUDManager; 
        if(categoryBuildHUDManager)
        {
            selectButton.onClick.AddListener(() => categoryBuildHUDManager.SelectCategory(categoryCode));
        }
    }
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (WorldDatabase_Build.Instance.IsDataLoaded == false)
        {
            yield return null; // 한 프레임 대기
        }
    }
}
