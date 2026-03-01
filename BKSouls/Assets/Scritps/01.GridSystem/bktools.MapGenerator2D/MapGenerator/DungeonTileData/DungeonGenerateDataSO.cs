using UnityEngine;

[CreateAssetMenu(menuName = "Dungeon/Editor/GenerateData ")]
public class DungeonGenerateDataSO : ScriptableObject
{
    public string dungeonCode;
    public int seed;
    public Vector2Int gridSize;
    public Vector3 cubeSize;
    [Range(5, 15)] public int roomSize;
    public TileMappingDataSO tileMappingDataSO;
}
