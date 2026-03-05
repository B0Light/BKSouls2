using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnRoomInstInfo", menuName = "Dungeon/RoomInst/SpawnRoomInstInfo")]
public class SpawnRoomInstInfo : ScriptableObject
{
    [Header("Room Prefabs")]
    public List<NetworkObject> normalRoomPrefabs;
    public List<NetworkObject> specialRoomPrefabs;
    public NetworkObject startRoomPrefab;
    public NetworkObject bossRoomPrefab;
}
