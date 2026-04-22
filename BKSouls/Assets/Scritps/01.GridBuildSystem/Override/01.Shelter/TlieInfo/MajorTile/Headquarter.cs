public class Headquarter : RevenueFacilityTile_Shop
{
    public override void UpgradeTile()
    {
        // 일반 업그레이드 UI를 통한 업그레이드 차단
        // SyncToShelterLevel() 을 통해서만 레벨이 올라간다
    }

    public void SyncToShelterLevel()
    {
        base.UpgradeTile();
    }
}
