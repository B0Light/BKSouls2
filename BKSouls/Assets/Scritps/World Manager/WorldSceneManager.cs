using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace BK
{
    public class WorldSceneManager : NetworkBehaviour
    {
        public static WorldSceneManager Instance;

        public SerializedDictionary<string, Vector3> sceneInfos = new SerializedDictionary<string, Vector3>();
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (NetworkManager != null && NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (NetworkManager != null && NetworkManager.SceneManager != null)
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (!IsServer) return;
            
            if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
            {
            }
        }
        
        public void LoadWorldScene(string sceneName)
        {
            if (!IsServer) return;
            
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError($"Invalid path: {sceneName}");
                return;
            }

            NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            GUIController.Instance.localPlayer.LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.Instance.currentCharacterData, GetSpawnPos());
        }
        
        public Vector3 GetSpawnPos()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            return sceneInfos.TryGetValue(sceneName, out Vector3 spawnPos) ? spawnPos : Vector3.zero;
        }
    }
}