using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class RoomInstance : MonoBehaviour
    {
        [Header("Player Spawn Points")]
        [SerializeField] private Transform[] playerSpawnPoints;

        [Header("Enemy Spawn Points")]
        [SerializeField] private Transform[] enemySpawnPoints;

        [Header("Door Anchors")]
        [SerializeField] private Transform entryDoorAnchor;
        [SerializeField] private Transform exitDoorAnchor;

        [Header("Reward Spawn Point")]
        [SerializeField] private Transform rewardSpawnPoint;

        public Transform EntryDoorAnchor => entryDoorAnchor;
        public Transform ExitDoorAnchor => exitDoorAnchor;
        public Transform RewardSpawnPoint => rewardSpawnPoint;

        public IReadOnlyList<Transform> PlayerSpawnPoints => playerSpawnPoints;
        public IReadOnlyList<Transform> EnemySpawnPoints => enemySpawnPoints;

        public Transform GetPlayerSpawnPoint(int index)
        {
            if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
            {
                Debug.LogWarning($"[RoomInstance:{name}] No player spawn points assigned. Using room transform.");
                return transform;
            }

            if (index < 0)
                index = 0;

            return playerSpawnPoints[index % playerSpawnPoints.Length];
        }

        public Transform GetEnemySpawnPoint(int index)
        {
            if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
            {
                Debug.LogWarning($"[RoomInstance:{name}] No enemy spawn points assigned. Using room transform.");
                return transform;
            }

            if (index < 0)
                index = 0;

            return enemySpawnPoints[index % enemySpawnPoints.Length];
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            DrawSpawnPointGizmos(playerSpawnPoints, Color.green, 0.35f);
            DrawSpawnPointGizmos(enemySpawnPoints, Color.red, 0.25f);

            if (entryDoorAnchor != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(entryDoorAnchor.position, Vector3.one * 0.5f);
            }

            if (exitDoorAnchor != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(exitDoorAnchor.position, Vector3.one * 0.5f);
            }

            if (rewardSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(rewardSpawnPoint.position, 0.25f);
            }
        }

        private void DrawSpawnPointGizmos(Transform[] points, Color color, float radius)
        {
            if (points == null)
                return;

            Gizmos.color = color;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null)
                    continue;

                Gizmos.DrawSphere(points[i].position, radius);
#if UNITY_EDITOR
                UnityEditor.Handles.Label(points[i].position + Vector3.up * 0.35f, $"{i}");
#endif
            }
        }
#endif
    }
}