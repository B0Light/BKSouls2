using UnityEngine;

public enum CellType
{
    Empty,
    Floor,
    MainGate,
    Wall,
    Road,
    HQ,
    MajorFacility,
    Resource
}

public class GridCell : IPathNode
{
    // A* Pathfinding을 위한 속성들
    public float GCost { get; set; }
    public float HCost { get; set; }
    public float FCost => GCost + HCost;
    public GridCell Parent { get; set; }
    IPathNode IPathNode.Parent { get => Parent; set => Parent = (GridCell)value; }
    
    // 그리드와 위치 정보
    private readonly int _posX, _posZ;
    public Vector2Int Position { get; }

    // 타일 및 건물 정보
    public CellType CellType { get; set;}
    private PlacedObject _placedObject;
    private Dir _dir;

    public GridCell(int posX, int posZ, CellType cellType)
    {
        _posX = posX;
        _posZ = posZ;
        Position = new Vector2Int(posX, posZ);
        CellType = cellType;
        
        GCost = float.MaxValue;
    }
    
    public void SetPlacedObject(PlacedObject placedObject, Dir dir, CellType cellType)
    {
        _placedObject = placedObject;
        _dir = dir;
        CellType = cellType;
    }

    public void ClearPlacedObject()
    {
        _placedObject = null;
        _dir = Dir.Down;
    }

    public bool CanBuild()
    {
        return _placedObject == null || _placedObject.IsDefault();
    }
    public PlacedObject GetPlacedObject() => _placedObject;
    public Dir GetDirection() => _dir;
    public Dir GetExitDirection() => _placedObject?.GetActualExitDirection() ?? Dir.Down;
    public Vector2Int GetEntrancePosition() => _placedObject != null ? _placedObject.GetEntrance() : new Vector2Int(_posX, _posZ);
    public Vector2Int GetExitPosition() => _placedObject != null ? _placedObject.GetExit() : new Vector2Int(_posX, _posZ);

    public override string ToString()
    {
        return $"GridCell({_posX}, {_posZ}) - {CellType}";
    }
    
    public override bool Equals(object obj)
    {
        return obj is GridCell other && Position.Equals(other.Position);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}