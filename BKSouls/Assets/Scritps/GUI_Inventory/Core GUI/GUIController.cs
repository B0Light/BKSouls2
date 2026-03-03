using System;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BK
{
    public class GUIController : Singleton<GUIController>
    {
        [Header("MouseCursor")]
        [SerializeField] private Texture2D customCursor; // 사용할 커서 이미지
        private readonly Vector2 _hotspot = Vector2.zero; // 커서 중심 위치
        
        [HideInInspector] public PlayerManager localPlayer;
        
        public SettingGUIManager settingGUIManager;
    
        [HideInInspector] public PlayerUIHudManager playerUIHudManager;
        [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;
        [HideInInspector] public PlayerUICharacterMenuManager playerUICharacterMenuManager;
        [HideInInspector] public PlayerUISiteOfGraceManager playerUISiteOfGraceManager;
        [HideInInspector] public PlayerUITeleportLocationManager playerUITeleportLocationManager;
        [HideInInspector] public PlayerUILoadingScreenManager playerUILoadingScreenManager;
        [HideInInspector] public PlayerUILevelUpManager playerUILevelUpManager;
        [HideInInspector] public InventoryGUIManager inventoryGUIManager;
        [HideInInspector] public ItemShopUIManager itemShopUIManager;
        [HideInInspector] public DungeonEnterGUIManager dungeonEnterGUIManager;
        //[HideInInspector] public DialogueGUIManager dialogueGUIManager;
        [HideInInspector] public UI_InteractionCountDown interactionCountDown;
        
        //[HideInInspector] public MapGUIManager mapGUIManager;
        //[HideInInspector] public PerkGUIManager perkGUIManager;
        [SerializeField] private CanvasGroup cashCanvasGroup;

        [Header("LOG")]
        public bool menuWindowIsOpen = false; 
        public bool popUpWindowIsOpen = false;
        public GUIComponent currentOpenGUI;
        
        // INPUT 
        PlayerControls playerControls;

        private CanvasGroup _canvasGroup;
        private bool _activeMainHud = true;

        protected override void Awake()
        {
            base.Awake();
            playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
            playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
            playerUICharacterMenuManager = GetComponentInChildren<PlayerUICharacterMenuManager>();
            playerUISiteOfGraceManager = GetComponentInChildren<PlayerUISiteOfGraceManager>();
            playerUITeleportLocationManager = GetComponentInChildren<PlayerUITeleportLocationManager>();
            playerUILoadingScreenManager = GetComponentInChildren<PlayerUILoadingScreenManager>();
            playerUILevelUpManager = GetComponentInChildren<PlayerUILevelUpManager>();
            
            inventoryGUIManager = GetComponentInChildren<InventoryGUIManager>();
            itemShopUIManager = GetComponentInChildren<ItemShopUIManager>();
            dungeonEnterGUIManager = GetComponentInChildren<DungeonEnterGUIManager>();
            //dialogueGUIManager = GetComponentInChildren<DialogueGUIManager>();
            interactionCountDown = GetComponentInChildren<UI_InteractionCountDown>();
            //mapGUIManager = GetComponentInChildren<MapGUIManager>();
            //perkGUIManager = GetComponentInChildren<PerkGUIManager>();

            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            if (customCursor != null)
            {
                Cursor.SetCursor(customCursor, _hotspot, CursorMode.Auto);
            }
        }
        
        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.UI.CloseTab.performed += HandleEscape;
            }

            playerControls.Enable();
        }

        private void OnDisable()
        {
            playerControls.Disable();
        }

        private void HandleEscape(InputAction.CallbackContext context)
        {
            if(_activeMainHud == false) return;
            if (!TryCloseActiveUI())
            {
                OpenPauseMenu();
            }
        }

        private bool TryCloseActiveUI()
        {
            if (currentOpenGUI == null)
                return false;

            CloseGUI();
            return true;
        }

        private void OpenPauseMenu()
        {
            OpenGUI(settingGUIManager);
            //settingGUIManager.OpenDisplaySetter();
        }

        public void HandleEscape()
        {
            if(_activeMainHud == false) return;
            if (!TryCloseActiveUI())
            {
                OpenPauseMenu();
            }
        }
        
        public void HandleTab()
        {
            if(_activeMainHud == false) return;
            switch (currentOpenGUI)
            {
                case InventoryGUIManager:
                case null:
                    ToggleInventory();
                    break;
                default:
                    TryCloseActiveUI();
                    break;
            }
        }

        public void HandleNextGUI()
        {
            if(currentOpenGUI == null) return;

            currentOpenGUI.SelectNextGUI();
        }

        #region Control GUI

        private void OpenGUI(GUIComponent newGUI)
        {
            if (currentOpenGUI == newGUI)
                return; // 이미 열려있는 경우 아무것도 안 함

            CloseCurrentGUI();

            currentOpenGUI = newGUI;
            currentOpenGUI?.OpenGUI();
        }

        public void CloseGUI()
        {
            CloseCurrentGUI();
            currentOpenGUI = null;

            cashCanvasGroup.alpha = 0;
        }

        private void CloseCurrentGUI() => currentOpenGUI?.CloseGUI();

        #endregion
        
        public void OpenShop(List<Item> items, Interactable interactableObject, bool isMaster = false)
        {
            OpenGUI(itemShopUIManager);
            itemShopUIManager.OpenShop(items, interactableObject, isMaster);
        }

        private void ToggleInventory()
        {
            if (inventoryGUIManager.IsOpen)
                TryCloseActiveUI();
            else
                OpenInventory();
        }

        private void OpenInventory()
        {
            OpenGUI(inventoryGUIManager);
        }

        public void OpenInteractableBox(int width, int height, List<int> itemIdList, Interactable interactable)
        {
            OpenGUI(inventoryGUIManager);
            inventoryGUIManager.OpenInteractionInventory(false, width, height, itemIdList, interactable);
        }
        
        public void OpenShareBox(int width, int height, List<int> itemIdList, Interactable interactable)
        {
            OpenGUI(inventoryGUIManager);
            inventoryGUIManager.OpenInteractionInventory(true, width, height, itemIdList, interactable);
        }


        public void OpenDungeonEntrance(DungeonInfoData dungeonInfoData, Interactable interactable)
        {
            OpenGUI(dungeonEnterGUIManager);
            dungeonEnterGUIManager.InitDungeonEnterHUD(dungeonInfoData, interactable);
        }


        public void OpenForge(Interactable interactable)
        {
            OpenGUI(inventoryGUIManager);
            inventoryGUIManager.OpenForge(interactable);
        }

        /*
        public void OpenDialogue(string npcName, UnityAction closeAction)
        {
            OpenGUI(dialogueGUIManager);
            dialogueGUIManager.InitDialogue(npcName, closeAction);
        }
        */
        public void OpenShredder(int width, int height, List<int> itemIdList, Interactable interactable)
        {
            OpenGUI(inventoryGUIManager);
            inventoryGUIManager.OpenInteractionShredder(width, height, itemIdList, interactable);
            
            //cashCanvasGroup.alpha = 1;
        }
        /*
        public void OpenPerkManager(PlayerManager player, Interactable interactable)
        {
            OpenGUI(perkGUIManager);
            perkGUIManager.OpenPerkManager(player, interactable);
        }
        */
        /*
        public void OpenMap()
        {
            OpenGUI(mapGUIManager);
        }
        */
        public void WaitToInteraction(float time, Action action)
        {
            interactionCountDown.SetInteractionTime(time);
            interactionCountDown.Interaction(action);
        }

        public void ToggleMainGUI(bool value)
        {
            _canvasGroup.alpha = value ? 1 : 0;
            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
            _activeMainHud = value;
        }
    }
}
