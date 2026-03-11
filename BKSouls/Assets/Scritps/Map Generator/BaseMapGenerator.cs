using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMapGenerator
{
    [Header("기본 설정")]
    public int seed = 0;
    protected Vector2Int gridSize = new Vector2Int(64, 64);
    protected Vector3 cubeSize = new Vector3(2, 2, 2);
    
    [Header("Tile Data Map")]
    protected TileMappingDataSO tileMappingDataSO;
    protected Dictionary<CellType, TileDataSO> tileDataDict;
    
    [Header("상태")]
    [SerializeField] protected bool isMapGenerated = false;
    
    // 공통 그리드 데이터
    protected CellType[,] _grid;
    protected List<RectInt> _floorList;

    protected Transform _slot;

    public BaseMapGenerator(Transform slot, TileMappingDataSO tileMappingData)
    {
        this._slot = slot;
        this.tileMappingDataSO = tileMappingData;
        Init();
    }
    
    public BaseMapGenerator(Transform slot, TileMappingDataSO tileMappingData, Vector2Int gridSize, Vector3 cubeSize)
    {
        this._slot = slot;
        this.tileMappingDataSO = tileMappingData;
        this.gridSize = gridSize;
        this.cubeSize = cubeSize;
        Init();
    }

    private void Init()
    {
        BuildTileDataDictionary();
        InitializeGenerator();
        
        if (seed != 0)
            Random.InitState(seed);
    }
    
    protected virtual void InitializeGenerator() { }
    
    protected virtual void BuildTileDataDictionary()
    {
        tileDataDict = new Dictionary<CellType, TileDataSO>();
        if (tileMappingDataSO == null) return;
        
        foreach (var mapping in tileMappingDataSO.tileMappings)
        {
            if (!tileDataDict.ContainsKey(mapping.cellType) && mapping.tileData != null)
                tileDataDict.Add(mapping.cellType, mapping.tileData);
        }
    }
    
    public abstract void GenerateMap();
    
    protected virtual void InitializeGrid()
    {
        _grid = new CellType[gridSize.x, gridSize.y];
        _floorList = new List<RectInt>();
        
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                _grid[x, y] = CellType.Empty;
            }
        }
    }
    
    protected void BuildWalls()
    {
        // 경계 포함하여 벽 생성
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                if (_grid[x, y] == CellType.Floor || _grid[x, y] == CellType.Road)
                {
                    // 상하좌우 체크 (경계 범위 내에서만)
                    if (x > 0 && _grid[x - 1, y] == CellType.Empty) _grid[x - 1, y] = CellType.Wall;
                    if (x < gridSize.x - 1 && _grid[x + 1, y] == CellType.Empty) _grid[x + 1, y] = CellType.Wall;
                    if (y > 0 && _grid[x, y - 1] == CellType.Empty) _grid[x, y - 1] = CellType.Wall;
                    if (y < gridSize.y - 1 && _grid[x, y + 1] == CellType.Empty) _grid[x, y + 1] = CellType.Wall;
                }
            }
        }
    }
    
    protected virtual void RenderGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                RenderTileAt(x, y);
            }
        }
    }
    
    protected virtual void RenderTileAt(int x, int y)
    {
        if (!TryGetTileData(x, y, out TileDataSO tileData)) return;
        
        Vector3 spawnPos = new Vector3(x * cubeSize.x, 0, y * cubeSize.z);
        tileData.SpawnTile(spawnPos, cubeSize, _slot);
    }
    
    protected virtual bool TryGetTileData(int x, int y, out TileDataSO tileData)
    {
        return tileDataDict.TryGetValue(_grid[x, y], out tileData) && tileData != null;
    }
    
    protected virtual void OnMapGenerationComplete()
    {
        isMapGenerated = true;
        Debug.Log($"{GetType().Name}: 맵 생성 완료");
    }
    
    public virtual void ClearMap()
    {
        if (_slot != null)
        {
            // slot 하위의 모든 자식 오브젝트 제거
            ClearChildrenRecursively(_slot);
        }
        
        // 그리드 초기화
        if (_grid != null)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    _grid[x, y] = CellType.Empty;
                }
            }
        }
        
        // 바닥 리스트 초기화
        if (_floorList != null)
        {
            _floorList.Clear();
        }
        
        isMapGenerated = false;
        Debug.Log($"{GetType().Name}: 맵 제거 완료");
    }
    
    private void ClearChildrenRecursively(Transform parent)
    {
        if (parent == null) return;
        
        // 런타임에서는 즉시 제거를 위해 다른 방법 사용
        if (Application.isPlaying)
        {
            // 런타임에서는 모든 자식을 리스트에 담고 한번에 제거
            var childrenToDestroy = new List<GameObject>();
            for (int i = 0; i < parent.childCount; i++)
            {
                childrenToDestroy.Add(parent.GetChild(i).gameObject);
            }
            
            foreach (var child in childrenToDestroy)
            {
                if (child != null)
                {
                    child.SetActive(false); // 즉시 비활성화
                    UnityEngine.Object.Destroy(child);
                }
            }
        }
        else
        {
            // 에디터에서는 기존 방식 사용
            while (parent.childCount > 0)
            {
                Transform child = parent.GetChild(0);
                if (child != null)
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }
    
    protected virtual void CreateStraightPath(Vector2Int startPos, Vector2Int endPos)
    {
        Vector2Int direction = new Vector2Int(
            endPos.x > startPos.x ? 1 : endPos.x < startPos.x ? -1 : 0,
            endPos.y > startPos.y ? 1 : endPos.y < startPos.y ? -1 : 0
        );
        
        Vector2Int current = startPos;
        
        while (current != endPos)
        {
            if (current.x >= 0 && current.x < gridSize.x && 
                current.y >= 0 && current.y < gridSize.y)
            {
                if (_grid[current.x, current.y] == CellType.Empty)
                {
                    _grid[current.x, current.y] = CellType.Road;
                }
            }
            
            // X축 먼저 이동
            if (current.x != endPos.x)
            {
                current.x += direction.x;
            }
            // Y축 이동
            else if (current.y != endPos.y)
            {
                current.y += direction.y;
            }
        }
        _grid[startPos.x, startPos.y] = CellType.MainGate;
        _grid[endPos.x, endPos.y] = CellType.MainGate;
    }
}
