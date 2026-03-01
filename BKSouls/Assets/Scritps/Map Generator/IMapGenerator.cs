using UnityEngine;

/// <summary>
/// 맵 생성기의 공통 인터페이스
/// </summary>
public interface IMapGenerator
{
    /// <summary>
    /// 맵을 생성합니다.
    /// </summary>
    void GenerateMap();
    
    /// <summary>
    /// 맵 생성이 완료되었는지 확인합니다.
    /// </summary>
    bool IsMapGenerated { get; }
    
    /// <summary>
    /// 생성된 맵 데이터를 가져옵니다.
    /// </summary>
    MapData GetMapData();
}
