using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace BK
{
    public class PlayerUIManager1 : Singleton<PlayerUIManager1>
    {
        [HideInInspector] public PlayerManager localPlayer;

        [Header("NETWORK JOIN")]
        [SerializeField] bool startGameAsClient;

        [HideInInspector] public PlayerUIHudManager playerUIHudManager;
        [HideInInspector] public PlayerUIPopUpManager playerUIPopUpManager;
        [HideInInspector] public PlayerUICharacterMenuManager playerUICharacterMenuManager;
        [HideInInspector] public PlayerUISiteOfGraceManager playerUISiteOfGraceManager;

        [HideInInspector] public PlayerUILoadingScreenManager playerUILoadingScreenManager;
        [HideInInspector] public PlayerUILevelUpManager playerUILevelUpManager;

        [Header("UI Flags")]
        public bool menuWindowIsOpen = false;       // INVENTORY SCREEN, EQUIPMENT MENU, BLACKSMITH MENU ECT
        public bool popUpWindowIsOpen = false;      // ITEM PICK UP, DIALOGUE POP UP ECT

        protected override void Awake()
        {
            base.Awake();
            //playerUIHudManager = GetComponentInChildren<PlayerUIHudManager>();
            playerUIPopUpManager = GetComponentInChildren<PlayerUIPopUpManager>();
            playerUICharacterMenuManager = GetComponentInChildren<PlayerUICharacterMenuManager>();
            playerUISiteOfGraceManager = GetComponentInChildren<PlayerUISiteOfGraceManager>();

            playerUILoadingScreenManager = GetComponentInChildren<PlayerUILoadingScreenManager>();
            playerUILevelUpManager = GetComponentInChildren<PlayerUILevelUpManager>();
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (startGameAsClient)
            {
                startGameAsClient = false;
                //  WE MUST FIRST SHUT DOWN, BECAUSE WE HAVE STARTED AS A HOST DURING THE TITLE SCREEN
                NetworkManager.Singleton.Shutdown();
                //  WE THEN RESTART, AS A CLIENT
                NetworkManager.Singleton.StartClient();
            }
        }

        public void CloseAllMenuWindows()
        {
            playerUICharacterMenuManager.CloseMenuAfterFixedFrame();
            playerUISiteOfGraceManager.CloseMenuAfterFixedFrame();

            playerUILevelUpManager.CloseMenuAfterFixedFrame();
        }
    }
}
