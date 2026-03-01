using UnityEngine;

public enum DungeonResource
{
    Herb,
    Ore,
    Chest,
    Trap,
    Turret,
}

public class DungeonResourceTile : RevenueFacilityTile
{
    [SerializeField] private DungeonResource resourceType;
    public DungeonResource DungeonResource => resourceType;
    public GridItem resourceItem;
    private int _itemCountQueue = 4;

    public override bool AddVisitor(PathFindingUnit visitor)
    {
        if (_itemCountQueue <= 0) return false;
        _itemCountQueue -= 1;
        if (_itemCountQueue == 0) ResetTile();
        return base.AddVisitor(visitor);
    }
    
    
    // 해당 타일의 모든 아이템이 소모되면 호출 
    private void ResetTile()
    {
        BaseGridBuildSystem.Instance.RemoveTile(this);
    }
}
