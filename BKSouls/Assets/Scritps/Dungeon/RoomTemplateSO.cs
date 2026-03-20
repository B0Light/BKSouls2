using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BK/Roguelike/Room Template")]
public class RoomTemplateSO : ScriptableObject
{
    [Header("Basic")]
    public int templateId;
    public RoomType roomType;
    public GameObject roomPrefab;

    [Header("Spawn Info")]
    public List<GameObject> enemyPrefabs = new();
    public GameObject rewardPrefab;
}