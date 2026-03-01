using System;
using System.Collections;
using System.Collections.Generic;
using BK;
using BK.Inventory;
using UnityEngine;

public class CategoryBuildHUDManager : GridBuildHUDManager
{
    public HUDGridBuildCategorySelector gridBuildCategorySelector;
    public HUDGridBuildingSelector gridBuildingSelector;
    
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup buildingSelectionCanvasGroup;

    [SerializeField] private List<CellType> activeTileType;
    public SerializableDictionary<CellType, HashSet<BuildObjData>> unlockedBuildingByCategory;
    
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(Init());
    }

    protected override void Start()
    {
        base.Start();
        ToggleBuildSelectionHUD(false);
    }

    private IEnumerator Init()
    {
        yield return WaitForDataLoad();
        
        InitBuildingCategory();
        UpdateAvailableBuildings();
    }
    
    private IEnumerator WaitForDataLoad()
    {
        // 데이터가 로드될 때까지 대기
        while (!WorldDatabase_Build.Instance.IsDataLoaded)
        {
            yield return null; // 한 프레임 대기
        }
    }
    
    private void InitBuildingCategory()
    {
        foreach (CellType tileCategory in activeTileType)
        {
            if(unlockedBuildingByCategory.ContainsKey(tileCategory) == false)
                unlockedBuildingByCategory.Add(tileCategory, new HashSet<BuildObjData>());
        }
    }
    
    public void UpdateAvailableBuildings()
    {
        int curShelterLevel = WorldSaveGameManager.Instance.currentCharacterData.shelterLevel;

        foreach (var buildObjData in WorldDatabase_Build.Instance.GetBuildingsUpToTierReadOnly((ItemTier)curShelterLevel))
        {
            UpdateCategory(buildObjData);
        }
        gridBuildCategorySelector.RefreshBuildingCategory();
    }
    
    private void UpdateCategory(BuildObjData buildObjData)
    {
        CellType tileCategory = buildObjData.GetCellType(); 
        
        // 추가 할 수 없는 건물 계열 (터렛류)
        if(unlockedBuildingByCategory.ContainsKey(tileCategory) == false) return;
        
        unlockedBuildingByCategory[tileCategory].Add(buildObjData);
    }
    
    public void ToggleBuildSelector()
    {
        bool isOpen = buildingSelectionCanvasGroup.interactable;

        gridBuildCategorySelector.RefreshBuildingCategory();
        ToggleBuildSelectionHUD(!isOpen);
    }
    
    public void ToggleBuildSelectionHUD(bool isActive)
    {
        buildingSelectionCanvasGroup.alpha = isActive ? 1f : 0f;
        buildingSelectionCanvasGroup.blocksRaycasts = isActive;
        buildingSelectionCanvasGroup.interactable = isActive;
    }
    
    public void SelectCategory(CellType id)
    {
        BaseGridBuildSystem.Instance.SelectToBuild(null);
        StartCoroutine(gridBuildingSelector.InitBtnSlot(id));
    }

    public void RefreshCategory()
    {
        BaseGridBuildSystem.Instance.SelectToBuild(null);
        gridBuildingSelector.RefreshSlot();
    }

    public override void ExitBuildHUD()
    {
        base.ExitBuildHUD();
        SaveGridData(); 
    }

    private void SaveGridData()
    {
        WorldSaveGameManager.Instance.currentCharacterData.buildings.Clear();
        ShelterGridBuildSystem shelterGridBuildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
        if(!shelterGridBuildSystem) return;
        foreach (var building in shelterGridBuildSystem.SaveBuildingDataList)
        {
            if (building != null)
            {
                WorldSaveGameManager.Instance.currentCharacterData.buildings.Add(building);
            }
        }
        WorldSaveGameManager.Instance.SaveGame();
    }
}
