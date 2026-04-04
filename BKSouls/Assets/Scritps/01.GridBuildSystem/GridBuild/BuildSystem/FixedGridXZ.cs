using System;
using UnityEngine;

public class FixedGridXZ<GridCell> : GridBase<GridCell>
{
    private readonly GridCell[,] _gridArray;
    
    public FixedGridXZ(int width, int height, float cellSize, Vector3 originPosition,
        Func<int, int, GridCell> createGridObject) : base(width, height, cellSize, originPosition)
    {
        _gridArray = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                _gridArray[x, z] = createGridObject(x, z);
            }
        }
    }

    public GridCell[,] GetAllGridObjects() => _gridArray;
    
    public override GridCell GetGridObject(int x, int z) => IsValidGridPosition(x, z) ? _gridArray[x, z] : default;
    
    public override bool IsValidGridPosition(int x, int z) => x >= 0 && z >= 0 && x < _width && z < _height;
}
