using System;
using System.Collections;
using System.Collections.Generic;
using BK;
using BK.Inventory;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

public class DungeonEnterGUIManager : GUIComponent
{
    [Header("DungeonSelect")] 
    [SerializeField] private RectTransform dungeonSelectSlot;
    [SerializeField] private List<DungeonData> dungeonDataList;
    [SerializeField] private List<GameObject> dungeonSelectButtons;
    
    [SerializeField] private TextMeshProUGUI dungeonName;
    [SerializeField] private TextMeshProUGUI dungeonInfo;
    [SerializeField] private Image dungeonBackgroundImage;
    
    [SerializeField] private Button enterDungeonButton;
    
    [SerializeField] private GameObject available;
    [SerializeField] private GameObject disable;

    private Interactable _interactableObj;

    private string selectedDungeonSceneName;

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            if (NetworkManager.Singleton.SceneManager != null)
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            if (NetworkManager.Singleton.SceneManager != null)
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
        }
    }

    #region GUI

    public override void CloseGUI()
    {
        base.CloseGUI();
        _interactableObj?.ResetInteraction();
    }

    public void InitDungeonEnterHUD(DungeonInfoData dungeonInfoData, Interactable interactable)
    {
        _interactableObj = interactable;
        
        //InputHandlerManager.Instance.SetInputMode(StandaloneInputModule.InputMode.OpenUI);
        
        if(dungeonInfoData.dungeonDataList.Count == 0) return;

        ResetShelf();
        dungeonDataList.AddRange(dungeonInfoData.dungeonDataList);

        for (var i = 0; i < dungeonDataList.Count; i++)
        {
            dungeonSelectButtons[i].SetActive(true);
            var spawnButton = dungeonSelectButtons[i].GetComponent<Button>();
    
            int capturedIndex = i;  
            spawnButton.onClick.AddListener(() => InitEntranceOfDungeon(dungeonDataList[capturedIndex]));
        }

        InitEntranceOfDungeon(dungeonInfoData.dungeonDataList[0]);
    }
    
    private void ResetShelf()
    {
        foreach (GameObject button in dungeonSelectButtons)
        {
            button.SetActive(false);
        }
        dungeonDataList.Clear();
    }

    private void InitEntranceOfDungeon(DungeonData dungeonData)
    {
        dungeonName.text = dungeonData.dungeonName;
        dungeonInfo.text = dungeonData.GetFormattedInfo();
        
        selectedDungeonSceneName = dungeonData.dungeonSceneName;

        available.SetActive(false);
        disable.SetActive(false);
        enterDungeonButton.onClick.RemoveAllListeners();
        
        enterDungeonButton.interactable = true;
        available.SetActive(true);
        enterDungeonButton.onClick.AddListener(EnterDungeonAsHost);
    }

    #endregion

    #region NetworkLobby

    // 멀티가 끝난시점에서 클라이언트 플레이어는 다시 host를 생성해서 게임에 접속해야함 
    public void CreateRoomAsHost()
    {
        CleanupBeforeStartNetwork();

        // Host 시작(서버+로컬클라)
        if (!NetworkManager.Singleton.StartHost())
        {
            Debug.LogError("StartHost failed");
            return;
        }
    }
    
    
    // Host만 누를 수 있는 "던전 입장" 버튼
    public void EnterDungeonAsHost()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("Only host/server can enter dungeon.");
            return;
        }
        
        GUIController.Instance.HandleEscape();
        WorldPlayerInventory.Instance.MoveInventoryToShare();
        // NGO 씬 전환 (전원 동기)
        NetworkManager.Singleton.SceneManager.LoadScene(selectedDungeonSceneName, LoadSceneMode.Single);
    }

    // -------------------------
    // 콜백
    // -------------------------

    private void OnClientConnected(ulong clientId)
    {
        // 로비 UI에서 "접속자 표시"만 갱신하면 됨
        Debug.Log($"Client connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
    }

    private void OnSceneEvent(SceneEvent sceneEvent)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (sceneEvent.SceneEventType == SceneEventType.LoadEventCompleted)
        {
            if (sceneEvent.SceneName == selectedDungeonSceneName)
            {
                // 1) 던전 초기화(시드/웨이브/몬스터 스폰 준비 등)
                // 2) 모든 플레이어를 던전 스폰 포인트로 배치
                DungeonSpawnAllPlayers();
            }
        }
    }

    // -------------------------
    // 던전 진입 후 배치
    // -------------------------

    private void DungeonSpawnAllPlayers()
    {
        foreach (var kv in NetworkManager.Singleton.ConnectedClients)
        {
            var client = kv.Value;
            if (client?.PlayerObject == null) continue;

            var playerGo = client.PlayerObject.gameObject;

            // 예시: clientId별 스폰포인트 배정
            Vector3 spawnPos = GetDungeonSpawnPosition(client.ClientId);
            playerGo.transform.position = spawnPos;
            playerGo.transform.rotation = Quaternion.identity;

            // 필요하면 여기서 서버 권위로 체력/상태 초기화도 적용
        }
    }

    private Vector3 GetDungeonSpawnPosition(ulong clientId)
    {
        return new Vector3((int)clientId * 2f, 0f, 0f);
    }

    // Clean Up 이전 네트워크 기록 제거 
    private void CleanupBeforeStartNetwork()
    {
        // 이미 켜져있던 로컬 Host/Client가 있으면 끄고 시작
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            ShutdownNetworkAndCleanup();
        }

        // 여기서 "싱글용 플레이어/월드 오브젝트" 같은 것 정리 필요하면 수행
    }

    private void ShutdownNetworkAndCleanup()
    {
        if (NetworkManager.Singleton == null) return;
        
        NetworkManager.Singleton.Shutdown();

    }

    #endregion
    

}
