using BK;
using UnityEngine;

public class Headquarter : RevenueFacilityTile_Shop
{
    public override void UpgradeTile()
    {
        base.UpgradeTile();

        WorldSaveGameManager.Instance.currentCharacterData.shelterLevel = this.level;
        CategoryBuildHUDManager categoryBuildHUDManager = GridBuildHUDManager.Instance as CategoryBuildHUDManager; 
        if(categoryBuildHUDManager)
            categoryBuildHUDManager.UpdateAvailableBuildings();
    }
}
