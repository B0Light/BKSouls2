using UnityEngine;

public class HUDGridBuildCategorySelector : MonoBehaviour
{
    [SerializeField] private GameObject categoryPrefab;
    [SerializeField] private Transform selectButtonSlot;
    
    public void RefreshBuildingCategory()
    {
        RemoveAllChildren();
        CategoryBuildHUDManager categoryBuildHUDManager = GridBuildHUDManager.Instance as CategoryBuildHUDManager;
        if(!categoryBuildHUDManager) return;
        foreach (var key in categoryBuildHUDManager.unlockedBuildingByCategory.Keys)
        {
            if(key == CellType.HQ) continue;
            GameObject instanceBtnObj = Instantiate(categoryPrefab, selectButtonSlot);
            instanceBtnObj.GetComponent<HUDGridBuildingCategoryUnit>()?.InitButton(key);
        }
    }
    
    private void RemoveAllChildren()
    {
        if (selectButtonSlot == null)
        {
            return;
        }

        for (int i = selectButtonSlot.childCount - 1; i >= 0; i--)
        {
            Destroy(selectButtonSlot.GetChild(i).gameObject);
        }
    }
}
