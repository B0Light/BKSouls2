using UnityEngine;
using System.Collections.Generic;
using BK;
using BK.Inventory;

public class ShelterGridBuildSystem : BaseGridBuildSystem
{
    [Header("Base Tile Code")] 
    [SerializeField] private int emptyTile = 0;
    [SerializeField] private int entranceTile = 1;
    [SerializeField] private int headquarterTile = 2;
    [SerializeField] private int roadTile = 3;
    [SerializeField] private int dungeonEntranceTile = 90;
    
    private readonly int _gridWidth = 7;
    private readonly int _gridHeight = 9;
    private readonly int _cellSize = 5;
    
    [Space(10)] 
    private Vector2Int _entrancePos = new Vector2Int(3, 0);
    private Vector2Int _headquarterPos = new Vector2Int(4, 1);
    private Vector2Int _dungeonEntrancePos = new Vector2Int(3, 8);
    
    public List<Vector2Int> CheckPointList { get; private set; }
    
    public List<SaveBuildingData> SaveBuildingDataList { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        _fixedGrid = new FixedGridXZ<GridCell>(
            _gridWidth,
            _gridHeight,
            _cellSize,
            transform.position,
            (x, z) => new GridCell(x, z, CellType.Empty)
        );
    }

    private void Start()
    {
        CheckPointList = new List<Vector2Int>();
        LoadDefaultTiles();
        LoadDefaultObject();
        LoadSaveBuildingData();
    }
    
    #region LoadData
    
    private void LoadDefaultTiles()
    {
        for (int i = 0; i < _gridWidth; i++)
        {
            for (int j = 0; j < _gridHeight; j++)
            {
                SetDefaultTile(i,j);
            }
        }
        ObjectToPlace = null;
    }
    
    private void SetDefaultTile(int x, int y)
    {
        ObjectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(emptyTile);
        if(ObjectToPlace == null) return;
        PlaceTile(x,y, Dir.Down);
        ObjectToPlace = null;
    }
    
    private void LoadDefaultObject()
    {
        LoadEntrance();
        LoadDefaultRoad();
        LoadHeadquarter();
        LoadDefaultDungeonEntrance();
    }

    private void LoadEntrance()
    {
        ObjectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(entranceTile);
        if(ObjectToPlace == null) return;
        var placedObject = PlaceTile(_entrancePos.x,_entrancePos.y,Dir.Down, 0,true);
        //CheckPointList.Add(placedObject.GetEntrance());
        ObjectToPlace = null;
    }
    
    private void LoadHeadquarter()
    {
        ObjectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(headquarterTile);
        if(ObjectToPlace == null) return;
        var placedObject = PlaceTile(_headquarterPos.x,_headquarterPos.y,Dir.Left,
            WorldSaveGameManager.Instance.currentCharacterData.shelterLevel,true);
        CheckPointList.Add(placedObject.GetEntrance());
        ObjectToPlace = null;
    }
    
    private void LoadDefaultRoad()
    {
        ObjectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(roadTile);
        if(ObjectToPlace == null) return;
        for(int i = _entrancePos.y; i < _dungeonEntrancePos.y; i++)
            PlaceTile(_entrancePos.x,i,Dir.Down, 0);
        ObjectToPlace = null;
    }
    
    private void LoadDefaultDungeonEntrance()
    {
        ObjectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(dungeonEntranceTile);
        if(ObjectToPlace == null) return;
        var placedObject = PlaceTile(_dungeonEntrancePos.x,_dungeonEntrancePos.y,Dir.Down, 0,true);
        CheckPointList.Add(placedObject.GetEntrance());
        ObjectToPlace = null;
    }
    
    private void LoadSaveBuildingData()
    {
        SaveBuildingDataList = new List<SaveBuildingData>();
        foreach (var saveData in WorldSaveGameManager.Instance.currentCharacterData.buildings)
        {
            int sX = saveData.x;
            int sZ = saveData.y;
            ObjectToPlace = WorldDatabase_Build.Instance.GetBuildingByID(saveData.code);
            if(ObjectToPlace == null) continue;
            Dir dir = ObjectToPlace.GetCellType() == CellType.Road ? Dir.Down : (Dir)saveData.dir;
            PlaceTile(sX,sZ, dir, saveData.level);
        }
        ObjectToPlace = null;
    }

    #endregion

    public override PlacedObject PlaceTile(int x, int z, Dir dir, int level = 0, bool isIrremovable = false)
    {
        PlacedObject placedObject = base.PlaceTile(x, z, dir, level, isIrremovable);
        if(ObjectToPlace.itemID >= 100)
        {
            SaveBuildingDataList.Add(new SaveBuildingData(x, z, ObjectToPlace.itemID, (int)dir, level));
        }
        if(ObjectToPlace.itemID >= 300)
        {
            CheckPointList.Add(placedObject.GetEntrance());
        }
        
        return placedObject;
    }

    protected override PlacedObject BuildTile(int x, int z, Dir dir, int level = 0, bool isIrremovable = false)
    {
        PlacedObject placedObject = base.BuildTile(x, z, dir, level, isIrremovable);
        IsUpdateSurroundingRoad(placedObject);
        return placedObject;
    }

    public override void RemoveTile(PlacedObject placedObject)
    {
        if (placedObject != null && placedObject.Irremovable == false)
        {
            int itemCode = placedObject.GetBuildObjData().itemID;
            // 저장 데이터에서 삭제
            if (SaveBuildingDataList.Remove(new SaveBuildingData(placedObject.GetOriginPos().x, placedObject.GetOriginPos().y,
                    itemCode, (int)placedObject.GetDir(), placedObject.GetLevel())))
            {
                if (placedObject.GetLevel() > 0)
                {
                    WorldPlayerInventory.Instance.balance.Value += Mathf.RoundToInt(placedObject.GetTotalUpgradeCost() * 0.5f);
                }
            }
            
            if(itemCode >= 300)
                CheckPointList.Remove(placedObject.GetEntrance());
            
            List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

            placedObject.DestroySelf();
            foreach (Vector2Int gridPosition in gridPositionList)
            {
                ClearObjectAtGridPosition(gridPosition);
            }

            foreach (Vector2Int gridPosition in gridPositionList)
            {
                UpdateSurroundingRoads(gridPosition);
                SetDefaultTile(gridPosition.x,gridPosition.y);
            }
        }
    }
    
    private void IsUpdateSurroundingRoad(PlacedObject placedObject)
    {
        CellType curTileType = ObjectToPlace.GetCellType();
        switch (curTileType)
        {
            case CellType.Road:
                UpdateSurroundingRoads(placedObject.GetEntrance());
                break;
            case CellType.MajorFacility:
            case CellType.HQ: 
                UpdateSurroundingRoads(placedObject.GetExit());
                UpdateSurroundingRoads(placedObject.GetEntrance());
                break;
        }
    }
    
    private void UpdateSurroundingRoads(Vector2Int position)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 상
            new Vector2Int(0, -1),  // 하
            new Vector2Int(-1, 0),  // 좌
            new Vector2Int(1, 0)    // 우
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = position + dir;
            var neighborObject = _fixedGrid.GetGridObject(neighborPos.x, neighborPos.y)?.GetPlacedObject();

            if (neighborObject is RoadTile neighborRoad)
            {
                neighborRoad.UpdateConnections(); // 주변 도로의 연결 상태 업데이트
            }
        }
    }
    
    #region UpgradeTile

    public override bool TryUpgrade(PlacedObject placedObject)
    {
        BuildObjData buildObjData = placedObject.GetBuildObjData();

        if (placedObject.GetLevel() >= buildObjData.maxLevel)
        {
            Debug.LogWarning("최고 레벨 ");
            return false;
        }

        if (WorldPlayerInventory.Instance.TrySpend(placedObject.GetUpgradeCost()))
        {
            UpgradeTile(placedObject);
            return true;
        }
        // 실패시 처리 
        Debug.LogWarning("[비용 부족] 비용 : " + placedObject.GetUpgradeCost());
        return false;
    }
    
    private void UpgradeTile(PlacedObject placedObject)
    {
        Vector2Int originPos = placedObject.GetOriginPos();
        int itemCode = placedObject.GetBuildObjData().itemID;
        int direction = (int)placedObject.GetDir();
        int previousLevel = placedObject.GetLevel();

        // 업그레이드 실행
        placedObject.UpgradeTile();

        var oldData = new SaveBuildingData(originPos.x, originPos.y, itemCode, direction, previousLevel);
        var newData = new SaveBuildingData(originPos.x, originPos.y, itemCode, direction, previousLevel + 1);

        if (SaveBuildingDataList.Remove(oldData))
        {
            SaveBuildingDataList.Add(newData);
        }
        else
        {
            Debug.LogWarning("기존 저장 데이터가 제거되지 않았습니다.");
        }
    }

    #endregion

    public override Vector2Int GetEntrancePos() => _entrancePos;
    
    public override Vector2Int GetDungeonPos() => _dungeonEntrancePos;
}
