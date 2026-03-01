using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileMappingData", menuName = "Dungeon/Tile Mapping Data")]
public class TileMappingDataSO : ScriptableObject
{
    public List<TileTypeMapping> tileMappings = new List<TileTypeMapping>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        // CellType enum 값들을 가져옴
        var cellTypes = (CellType[])Enum.GetValues(typeof(CellType));

        // 리스트에 없는 CellType 항목을 자동 생성
        foreach (var type in cellTypes)
        {
            if (!tileMappings.Exists(m => m.cellType == type))
            {
                tileMappings.Add(new TileTypeMapping { cellType = type, tileData = null });
            }
        }

        // 만약 enum에서 제거된 타입이 있으면 리스트에서도 제거
        tileMappings.RemoveAll(m => Array.IndexOf(cellTypes, m.cellType) == -1);
    }
#endif
}

[Serializable]
public class TileTypeMapping
{
    public CellType cellType;
    public TileDataSO tileData;
}