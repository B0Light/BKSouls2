public class RevenueFacilityTile_Exit : RevenueFacilityTile
{
    public override bool AddVisitor(PathFindingUnit visitor)
    {
        GenerateIncome();
        visitor.LeaveShelter();
        return true;
    }
}
