using System;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;

public class BaseGridBuildSystem : MonoBehaviour
{ 
    public static BaseGridBuildSystem Instance { get; private set; }
    public BuildObjData ObjectToPlace { get; set; }

    protected FixedGridXZ<GridCell> _fixedGrid;
    
    public static event Action<BuildObjData> OnSelectedChanged;
    public static event Action<BuildObjData> OnObjectPlaced;

    protected virtual void Awake()
    {
        Instance = this;
    }

    #region Place & Remove Tile

    public virtual PlacedObject PlaceTile(int x, int z, Dir dir, int level = 0, bool isIrremovable = false)
    {
        var gridPositionList = ObjectToPlace.GetGridPositionList(new Vector2Int(x, z), dir);

        if(!CanBuildAtPos(gridPositionList)) return null;
        
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
            gridObject.GetPlacedObject()?.DestroySelf();
            gridObject.ClearPlacedObject();
        }
        
        return BuildTile(x, z, dir, level, isIrremovable);
    }
    
    protected virtual PlacedObject BuildTile(int x, int z, Dir dir, int level = 0, bool isIrremovable = false)
    {
        Vector2Int rotationOffset = ObjectToPlace.GetRotationOffset(dir);
        Vector3 placedObjectWorldPosition = _fixedGrid.GetWorldPosition(x, z) +
                                            new Vector3(rotationOffset.x, 0, rotationOffset.y) * _fixedGrid.CellSize;

        PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(x, z), dir, ObjectToPlace, level, isIrremovable);

        var gridPositionList = ObjectToPlace.GetGridPositionList(new Vector2Int(x, z), dir);
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            SetObjectOnGrid(gridPosition, placedObject, dir);
        }

        OnObjectPlaced?.Invoke(ObjectToPlace);
        return placedObject;
    }
    
    public virtual bool CanBuildAtPos(List<Vector2Int> gridPositionList)
    {
        foreach (Vector2Int gridPosition in gridPositionList)
        {
            var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
            if (gridObject == null || !gridObject.CanBuild())
            {
                return false;
            }
        }
        return true;
    }

    public virtual void RemoveTile(PlacedObject placedObject)
    {
        if (placedObject != null && placedObject.Irremovable == false)
        {
           List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

            placedObject.DestroySelf();
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                ClearObjectAtGridPosition(gridPosition);
            }
        }
    }
    
    // Grid 상에 배치 
    private void SetObjectOnGrid(Vector2Int position, PlacedObject placedObject, Dir dir)
    {
        var gridObject = _fixedGrid.GetGridObject(position.x, position.y);
        gridObject?.SetPlacedObject(placedObject, dir, ObjectToPlace.GetCellType()); // BuildObjData 저장
    }

    // Grid 상에 제거
    protected void ClearObjectAtGridPosition(Vector2Int gridPosition)
    {
        var gridObject = _fixedGrid.GetGridObject(gridPosition.x, gridPosition.y);
        if (gridObject != null)
        {
            gridObject.ClearPlacedObject();
        }
    }

    #endregion
    
    #region UpgradeTile

    public virtual bool TryUpgrade(PlacedObject placedObject)
    {
        return true;
    }

    #endregion
    
    public void SelectToBuild(BuildObjData buildData)
    {
        ObjectToPlace = buildData;
        OnSelectedChanged?.Invoke(ObjectToPlace);
        
        Debug.Log($"Select : {buildData?.name}");
    }
    
    public FixedGridXZ<GridCell> GetGrid() => _fixedGrid;
    
    public virtual Vector2Int GetEntrancePos() => Vector2Int.zero;
    
    public virtual Vector2Int GetDungeonPos() => Vector2Int.zero;
}