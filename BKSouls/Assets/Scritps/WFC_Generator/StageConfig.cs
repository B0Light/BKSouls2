using UnityEngine;

[System.Serializable]
public class StageConfig
{
    [Header("Grid")]
    public int sizeX = 16;
    public int sizeZ = 16;
    public float cellSize = 10f;

    [Header("Path constraint")]
    public int minStartExitDistance = 18;
    public int maxAttempts = 30;               // 스테이지 생성 전체 재시도

    [Header("Graph shape")]
    public bool forceTreeNoLoops = true;       // 로그라이크: 기본 true
    public bool forceDoorsOnStartExit = true;

    [Header("WFC")]
    public int wfcMaxRestartsPerAttempt = 10;

    [Header("WFC Pattern Weights (Roguelike 추천)")]
    [Range(0.0f, 10f)] public float wDeadEnd = 2.8f;
    [Range(0.0f, 10f)] public float wStraight = 2.2f;
    [Range(0.0f, 10f)] public float wCorner = 2.0f;
    [Range(0.0f, 10f)] public float wTJunction = 0.9f;
    [Range(0.0f, 10f)] public float wCross = 0.15f;   // 루프 유발 → 낮게
    [Range(0.0f, 10f)] public float wClosed = 0.05f;  // 막방(가능하면 매우 낮게)

    [Header("Room rolls")]
    [Range(0,1)] public float shopChance = 0.35f;
    [Range(0,1)] public float eliteChance = 0.30f;
    [Range(0,1)] public float eventChance = 0.25f;

    [Header("Leaves")]
    public int treasureLeafMin = 1;
    public int treasureLeafMax = 3;
}