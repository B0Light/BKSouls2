using UnityEngine;

public class RoomContentLibrary : MonoBehaviour
{
    [Header("Optional markers / prefabs")]
    public GameObject startMarker;
    public GameObject exitPortal;
    public GameObject bossPortal;

    public GameObject shopPrefab;
    public GameObject eliteMarker;
    public GameObject eventPrefab;
    public GameObject treasurePrefab;
    
    public GameObject combatSpawnerPrefab;
    public GameObject eliteSpawnerPrefab;

    public void SpawnContent(RoomType type, Transform roomRoot, Vector2Int cell)
    {
        // roomRoot 아래에 스폰
        switch (type)
        {
            case RoomType.Start:
                Spawn(startMarker, roomRoot);
                break;

            case RoomType.Exit:
                Spawn(exitPortal, roomRoot);
                break;

            case RoomType.Boss:
                Spawn(bossPortal, roomRoot);
                break;

            case RoomType.Shop:
                Spawn(shopPrefab, roomRoot);
                break;

            case RoomType.Elite:
                // 엘리트 전투 스포너(또는 마커)
                if (!Spawn(eliteSpawnerPrefab, roomRoot))
                    Spawn(eliteMarker, roomRoot);
                break;

            case RoomType.Event:
                Spawn(eventPrefab, roomRoot);
                break;

            case RoomType.Treasure:
                Spawn(treasurePrefab, roomRoot);
                break;

            case RoomType.Combat:
            default:
                Spawn(combatSpawnerPrefab, roomRoot);
                break;
        }
    }

    private bool Spawn(GameObject prefab, Transform parent)
    {
        if (!prefab) return false;
        Instantiate(prefab, parent.position, parent.rotation, parent);
        return true;
    }
}