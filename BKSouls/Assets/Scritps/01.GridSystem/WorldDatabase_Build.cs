using System.Collections.Generic;
using System.Linq;
using BK.Inventory;
using UnityEngine;

public class WorldDatabase_Build : Singleton<WorldDatabase_Build>
{
    public bool IsDataLoaded { get; private set; }
    
    [SerializeField] private List<BuildObjData> allBuildObjDataList = new List<BuildObjData>();
    [SerializeField] private List<CategoryIconData> defaultCategoryIcon = new List<CategoryIconData>();
    [SerializeField] private Sprite defaultIcon;
    private readonly Dictionary<ItemTier, List<BuildObjData>> _buildObjByLevel = new Dictionary<ItemTier, List<BuildObjData>>();
    
    protected override void Awake()
    {
        base.Awake();
        IsDataLoaded = false;
        ClassifyData();
        IsDataLoaded = true;
    }

    private void ClassifyData()
    {
        foreach (var buildObj in allBuildObjDataList)
        {
            var tier = buildObj.itemTier;
        
            if (!_buildObjByLevel.TryGetValue(tier, out List<BuildObjData> buildObjList))
            {
                buildObjList = new List<BuildObjData>();
                _buildObjByLevel[tier] = buildObjList;
            }
        
            buildObjList.Add(buildObj);
        }
    }
    
    public BuildObjData GetBuildingByID(int id) => 
        allBuildObjDataList.FirstOrDefault(buildObjData => buildObjData.itemID == id);

    public Sprite GetCategoryIcon(CellType id)
    {
        foreach (var categoryData in defaultCategoryIcon)
        {
            if (categoryData.cellType == id)
                return categoryData.cellIcon;
        }

        return defaultIcon;
    }
    
    public IReadOnlyList<BuildObjData> GetBuildingsByTierReadOnly(ItemTier tier)
    {
        return _buildObjByLevel.TryGetValue(tier, out List<BuildObjData> buildObjList) 
            ? buildObjList.AsReadOnly() 
            : new List<BuildObjData>().AsReadOnly();
    }
    
    public IReadOnlyList<BuildObjData> GetBuildingsUpToTierReadOnly(ItemTier maxTier)
    {
        List<BuildObjData> result = new List<BuildObjData>();
        
        foreach (var kvp in _buildObjByLevel)
        {
            if (kvp.Key <= maxTier)
            {
                result.AddRange(kvp.Value);
            }
        }
        
        return result.AsReadOnly();
    }
}