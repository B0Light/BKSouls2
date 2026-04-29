using BK.Inventory;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

namespace BK
{
    public class TitleScreenManager : MonoBehaviour
    {
        public static TitleScreenManager Instance;

        //  MAIN MENU
        [Header("Main Menu Menus")]
        [SerializeField] GameObject titleScreenMainMenu;
        [SerializeField] GameObject titleScreenLoadMenu;
        [SerializeField] GameObject titleScreenCharacterCreationMenu;

        [Header("Main Menu Buttons")] 
        [SerializeField] Button continueGameButton;
        [SerializeField] Button loadMenuReturnButton;
        [SerializeField] Button mainMenuLoadGameButton;
        [SerializeField] Button mainMenuNewGameButton;
        [SerializeField] Button deleteCharacterPopUpConfirmButton;

        [Header("Main Menu Pop Ups")]
        [SerializeField] GameObject noCharacterSlotsPopUp;
        [SerializeField] Button noCharacterSlotsOkayButton;
        [SerializeField] GameObject deleteCharacterSlotPopUp;

        //  CHARACTER CREATION MENU
        [Header("Character Creation Main Panel Buttons")]
        [SerializeField] Button characterNameButton;
        [SerializeField] Button characterHairButton;
        [SerializeField] Button characterHairColorButton;
        [SerializeField] Button characterSexButton;
        [SerializeField] TextMeshProUGUI characterSexText;
        [SerializeField] Button startGameButton;

        [Header("Character Creation Hair Panel Buttons")]
        [SerializeField] Button[] characterHairButtons;
        [SerializeField] Button[] characterHairColorButtons;

        [Header("Character Creation Secondary Panel Menus")]
        [SerializeField] GameObject characterHairMenu;
        [SerializeField] GameObject characterHairColorMenu;
        [SerializeField] GameObject characterNameMenu;
        [SerializeField] TMP_InputField characterNameInputField;

        [Header("Color Sliders")]
        [SerializeField] Slider redSlider;
        [SerializeField] Slider greenSlider;
        [SerializeField] Slider blueSlider;

        [Header("Hidden Gear")]
        private HeadEquipmentItem hiddenHelmet;

        [Header("Character Slots")]
        public CharacterSlot currentSelectedSlot = CharacterSlot.NO_SLOT;


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Press to Start 
        public void StartNetworkAsHost()
        {
            NetworkManager.Singleton.StartHost();
        }
        
        public void StartNetworkAsClient()
        {
            NetworkManager.Singleton.StartClient();

            WorldSaveGameManager.Instance.LoadLastGame();
        }

        public void AttemptToCreateNewCharacter()
        {
            if (WorldSaveGameManager.Instance.HasFreeCharacterSlot())
            {
                OpenCharacterCreationMenu();
            }
            else
            {
                //  IF THERE ARE NO FREE SLOTS, NOTIFY THE PLAYER
                DisplayNoFreeCharacterSlotsPopUp();
            }
        }

        public void StartNewGame()
        {
            NetworkObject localPlayerObject = NetworkManager.Singleton != null
                ? NetworkManager.Singleton.LocalClient?.PlayerObject
                : null;

            if (localPlayerObject == null)
            {
                Debug.LogError("Cannot start new game because the local network player has not spawned.");
                return;
            }

            PlayerManager player = localPlayerObject.GetComponent<PlayerManager>();
            WorldSaveGameManager.Instance.player = player;

            if (!WorldSaveGameManager.Instance.TryPrepareNewGame())
            {
                DisplayNoFreeCharacterSlotsPopUp();
                return;
            }

            WorldSaveGameManager.Instance.FinishCreatingNewGame();
        }

        public void ContinueLastGame()
        {
            if (!WorldSaveGameManager.Instance.LoadLastGame())
                AttemptToCreateNewCharacter();
        }

        public void OpenLoadGameMenu()
        {
            //  CLOSE MAIN MENU
            titleScreenMainMenu.SetActive(false);

            //  OPEN LOAD MENU
            titleScreenLoadMenu.SetActive(true);

            //  SELECT THE RETURN BUTTON FIRST
            loadMenuReturnButton.Select();
        }

        public void CloseLoadGameMenu()
        {
            //  CLOSE LOAD MENU
            titleScreenLoadMenu.SetActive(false);

            //  OPEN MAIN MENU
            titleScreenMainMenu.SetActive(true);

            //  SELECT THE LOAD BUTTON
            mainMenuLoadGameButton.Select();
        }

        public void ToggleBodyType()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.isMale.Value = !player.playerNetworkManager.isMale.Value;

            if (player.playerNetworkManager.isMale.Value)
            {
                characterSexText.text = "MALE";
            }
            else
            {
                characterSexText.text = "FEMALE";
            }
        }

        public void OpenTitleScreenMainMenu()
        {
            titleScreenMainMenu.SetActive(true);
        }

        public void CloseTitleScreenMainMenu()
        {
            titleScreenMainMenu.SetActive(false);
        }

        public void OpenCharacterCreationMenu()
        {
            CloseTitleScreenMainMenu();

            titleScreenCharacterCreationMenu.SetActive(true);

            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  SETS DEFAULT BODY TYPE
            player.playerBodyManager.ToggleBodyType(true);
        }

        public void CloseCharacterCreationMenu()
        {
            titleScreenCharacterCreationMenu.SetActive(false);

            OpenTitleScreenMainMenu();
        }


        public void OpenChooseHairStyleSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. ENABLE SUB MENU OBJECT (CLASS LIST WITH BUTTONS)
            characterHairMenu.SetActive(true);

            //  3. AUTO SELECT FIRST BUTTON
            if (characterHairButtons.Length > 0)
            {
                characterHairButtons[0].Select();
                characterHairButtons[0].OnSelect(null);
            }

            //  STORE THE HELMET THE PLAYER HAD ON
            if (player.playerInventoryManager.headEquipment != null)
                hiddenHelmet = Instantiate(player.playerInventoryManager.headEquipment);

            //  UNEQUIP THE HELMET AND RELOAD THE GEAR
            player.playerInventoryManager.headEquipment = null;
            player.playerEquipmentManager.EquipArmor();
        }

        public void CloseChooseHairStyleSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. DISABLE SUB MENU OBJECT
            characterHairMenu.SetActive(false);

            //  3. AUTO SELECT "CHOOSE CLASS BUTTON" (SINCE IT WAS THE LAST BUTTON YOU HIT DURING THE MAIN MENU
            characterHairButton.Select();
            characterHairButton.OnSelect(null);

            if (hiddenHelmet != null)
                player.playerInventoryManager.headEquipment = hiddenHelmet;

            player.playerEquipmentManager.EquipArmor();
        }

        public void OpenChooseHairColorSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. ENABLE SUB MENU OBJECT (CLASS LIST WITH BUTTONS)
            characterHairColorMenu.SetActive(true);

            //  3. AUTO SELECT FIRST BUTTON
            if (characterHairColorButtons.Length > 0)
            {
                characterHairColorButtons[0].Select();
                characterHairColorButtons[0].OnSelect(null);
            }

            //  STORE THE HELMET THE PLAYER HAD ON
            if (player.playerInventoryManager.headEquipment != null)
                hiddenHelmet = Instantiate(player.playerInventoryManager.headEquipment);

            //  UNEQUIP THE HELMET AND RELOAD THE GEAR
            player.playerInventoryManager.headEquipment = null;
            player.playerEquipmentManager.EquipArmor();
        }

        public void CloseChooseHairColorSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. DISABLE SUB MENU OBJECT
            characterHairColorMenu.SetActive(false);

            //  3. AUTO SELECT "CHOOSE CLASS BUTTON" (SINCE IT WAS THE LAST BUTTON YOU HIT DURING THE MAIN MENU
            characterHairColorButton.Select();
            characterHairColorButton.OnSelect(null);

            if (hiddenHelmet != null)
                player.playerInventoryManager.headEquipment = hiddenHelmet;

            player.playerEquipmentManager.EquipArmor();
        }

        public void OpenChooseNameSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. DISABLE ALL MAIN MENU BUTTONS (SO YOU CANT ACCIDENTALLY HIT ONE)
            ToggleCharacterCreationScreenMainMenuButtons(false);

            //  2. DISABLE NAME BUTTON GAMEOBJECT, AND REPLACE IT WITH NAME FIELD GAME OBJECT
            characterNameButton.gameObject.SetActive(false);
            characterNameMenu.SetActive(true);

            //  3. SELECT NAME FIELD OBJECT
            characterNameInputField.Select();
        }

        public void CloseChooseNameSubMenu()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            //  1. RE-ENABLE ALL MAIN MENU BUTTONS
            ToggleCharacterCreationScreenMainMenuButtons(true);

            //  2. ENABLE NAME BUTTON GAMEOBJECT, AND DISABLE NAME FIELD GAME OBJECT
            characterNameMenu.SetActive(false);
            characterNameButton.gameObject.SetActive(true);

            //  3. SELECT NAME BUTTON
            characterNameButton.Select();

            player.playerNetworkManager.characterName.Value = characterNameInputField.text;
        }

        private void ToggleCharacterCreationScreenMainMenuButtons(bool status)
        {
            characterNameButton.enabled = status;
            characterHairButton.enabled = status;
            characterHairColorButton.enabled = status;
            characterSexButton.enabled = status;
            startGameButton.enabled = status;
        }

        public void DisplayNoFreeCharacterSlotsPopUp()
        {
            noCharacterSlotsPopUp.SetActive(true);
            noCharacterSlotsOkayButton.Select();
        }

        public void CloseNoFreeCharacterSlotsPopUp()
        {
            noCharacterSlotsPopUp.SetActive(false);
            mainMenuNewGameButton.Select();
        }

        //  CHARACTER SLOTS

        public void SelectCharacterSlot(CharacterSlot characterSlot)
        {
            currentSelectedSlot = characterSlot;
        }

        public void SelectNoSlot()
        {
            currentSelectedSlot = CharacterSlot.NO_SLOT;
        }

        public void AttemptToDeleteCharacterSlot()
        {
            if (currentSelectedSlot != CharacterSlot.NO_SLOT)
            {
                deleteCharacterSlotPopUp.SetActive(true);
                deleteCharacterPopUpConfirmButton.Select();
            }
        }

        public void DeleteCharacterSlot()
        {
            deleteCharacterSlotPopUp.SetActive(false);
            WorldSaveGameManager.Instance.DeleteGame(currentSelectedSlot);

            //  WE DISABLE AND THEN ENABLE THE LOAD MENU, TO REFRESH THE SLOTS (The deleted slots will now become inactive)
            titleScreenLoadMenu.SetActive(false);
            titleScreenLoadMenu.SetActive(true);

            loadMenuReturnButton.Select();
        }
        
        public void DeleteAllCharacterSlot()
        {
            WorldSaveGameManager.Instance.DeleteAllGame();
            
            titleScreenLoadMenu.SetActive(false);
            titleScreenLoadMenu.SetActive(true);
            
            loadMenuReturnButton.Select();
        }

        public void CloseDeleteCharacterPopUp()
        {
            deleteCharacterSlotPopUp.SetActive(false);
            loadMenuReturnButton.Select();
        }

        //  CHARACTER HAIR
        public void SelectHair(int hairID)
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairStyleID.Value = hairID;

            CloseChooseHairStyleSubMenu();
        }

        public void PreviewHair(int hairID)
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairStyleID.Value = hairID;
        }

        public void SelectHairColor()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairColorRed.Value = redSlider.value;
            player.playerNetworkManager.hairColorGreen.Value = greenSlider.value;
            player.playerNetworkManager.hairColorBlue.Value = blueSlider.value;

            CloseChooseHairColorSubMenu();
        }

        public void PreviewHairColor()
        {
            PlayerManager player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>();

            player.playerNetworkManager.hairColorRed.Value = redSlider.value;
            player.playerNetworkManager.hairColorGreen.Value = greenSlider.value;
            player.playerNetworkManager.hairColorBlue.Value = blueSlider.value;
        }

        public void SetRedColorSlider(float redValue)
        {
            redSlider.value = redValue;
        }

        public void SetGreenColorSlider(float greenValue)
        {
            greenSlider.value = greenValue;
        }

        public void SetBlueColorSlider(float blueValue)
        {
            blueSlider.value = blueValue;
        }
    }
}
