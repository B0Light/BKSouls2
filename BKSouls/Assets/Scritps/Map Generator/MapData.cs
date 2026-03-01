using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 맵 생성 결과를 저장하는 데이터 클래스
/// </summary>
[System.Serializable]
public class MapData
{
    /// <summary>
    /// 생성된 그리드 데이터
    /// </summary>
    public CellType[,] grid;
    
    /// <summary>
    /// 바닥 타일들의 위치 정보
    /// </summary>
    public List<RectInt> floorList;
    
    /// <summary>
    /// 그리드 크기
    /// </summary>
    public Vector2Int gridSize;
    
    /// <summary>
    /// 맵이 생성되었는지 여부
    /// </summary>
    public bool isGenerated;
    
    /// <summary>
    /// 생성된 방의 개수
    /// </summary>
    public int roomCount;
    
    /// <summary>
    /// 생성된 복도의 개수
    /// </summary>
    public int corridorCount;
    
    /// <summary>
    /// 맵 생성에 사용된 시드 값
    /// </summary>
    public int seed;
    
    /// <summary>
    /// 맵 생성 시간
    /// </summary>
    public System.DateTime generationTime;
    
    public MapData()
    {
        generationTime = System.DateTime.Now;
    }
    
    /// <summary>
    /// 특정 위치의 셀 타입을 가져옵니다.
    /// </summary>
    public CellType GetCellType(int x, int y)
    {
        if (grid == null || x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            return CellType.Empty;
        
        return grid[x, y];
    }
    
    /// <summary>
    /// 특정 위치의 셀 타입을 설정합니다.
    /// </summary>
    public void SetCellType(int x, int y, CellType cellType)
    {
        if (grid == null || x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            return;
        
        grid[x, y] = cellType;
    }
    
    /// <summary>
    /// 맵 정보를 로그로 출력합니다.
    /// </summary>
    public void LogMapInfo()
    {
        Debug.Log($"맵 생성 완료 - 크기: {gridSize}, 방 개수: {roomCount}, 복도 개수: {corridorCount}, 생성 시간: {generationTime}");
    }
}
