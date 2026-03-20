using System.Collections.Generic;
using UnityEngine;

public class RoomInstance : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] playerSpawnPoints;
    [SerializeField] private Transform[] enemySpawnPoints;

    [Header("Doors")]
    [SerializeField] private NetworkRoomDoor entryDoor;
    [SerializeField] private NetworkRoomDoor exitDoor;

    [Header("Reward Point")]
    [SerializeField] private Transform rewardSpawnPoint;

    public IReadOnlyList<Transform> PlayerSpawnPoints => playerSpawnPoints;
    public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;

    public NetworkRoomDoor EntryDoor => entryDoor;
    public NetworkRoomDoor ExitDoor => exitDoor;
    public Transform RewardSpawnPoint => rewardSpawnPoint;

    public Transform GetPlayerSpawnPoint(int playerIndex)
    {
        if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
            return transform;

        return playerSpawnPoints[playerIndex % playerSpawnPoints.Length];
    }

    public Transform GetEnemySpawnPoint(int enemyIndex)
    {
        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            return transform;

        return enemySpawnPoints[enemyIndex % enemySpawnPoints.Length];
    }
}