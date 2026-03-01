using UnityEngine;

public abstract class GridBase<T>
{
    protected readonly int _width;
    protected readonly int _height;
    protected readonly float _cellSize;
    protected readonly Vector3 _originPosition;

    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize;
    public Vector3 OriginPosition => _originPosition;

    protected GridBase(int width, int height, float cellSize, Vector3 originPosition)
    {
        _width = width;
        _height = height;
        _cellSize = cellSize;
        _originPosition = originPosition;
    }

    public abstract T GetGridObject(int x, int z);
    public abstract bool IsValidGridPosition(int x, int z);

    public Vector3 GetWorldPosition(int x, int z) => new Vector3(x, 0, z) * _cellSize + _originPosition;
    public Vector3 GetWorldPosition(Vector2Int pos) => new Vector3(pos.x, 0, pos.y) * _cellSize + _originPosition;

    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
        z = Mathf.FloorToInt((worldPosition - _originPosition).z / _cellSize);
    }
}
