using System;
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

        private readonly Dictionary<string, Vector3> sceneInfoDic = new Dictionary<string, Vector3>();
        [SerializeField] private List<SceneInfo> sceneInfos = new List<SceneInfo>();
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            SetSceneInfo();
        }

        private void SetSceneInfo()
        {
            foreach (var scene in sceneInfos)
            {
                sceneInfoDic.Add(scene.sceneName, scene.spawnPos);
            }
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

            GUIController.HideCursor();
            NetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            GUIController.Instance.localPlayer.LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.Instance.currentCharacterData, GetSpawnPos(sceneName));
        }
        
        public Vector3 GetSpawnPos(string sceneName)
        {
            return sceneInfoDic.TryGetValue(sceneName, out Vector3 spawnPos) ? spawnPos : Vector3.zero;
        }
    }

    [Serializable]
    public class SceneInfo
    {
        public string sceneName;
        public Vector3 spawnPos;
    }
}