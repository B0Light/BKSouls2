using System;
using UnityEngine;

namespace BK
{
    public class WorldSaveGameManager : MonoBehaviour
    {
        public static WorldSaveGameManager instance;

        public PlayerManager player;

        [Header("SAVE/LOAD")]
        [SerializeField] bool saveGame;
        [SerializeField] bool loadGame;

        [Header("World Scene Index")]
        [SerializeField] int worldSceneIndex = 1;

        private SaveFileDataWriter saveFileDataWriter;

        [Header("Current Character Data")]
        public CharacterSlot currentCharacterSlotBeingUsed;
        public CharacterSaveData currentCharacterData;

        [Header("Character Slots")]
        // 10개의 슬롯을 배열로 관리 (인스펙터에서도 확인 가능)
        public CharacterSaveData[] allCharacterSlots = new CharacterSaveData[11]; 

        private void Awake()
        {
            if (instance == null) { instance = this; }
            else { Destroy(gameObject); }
        }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            SetupSaveWriter();
            LoadAllCharacterProfiles();
        }

        // 공통 Writer 설정 메서드
        private void SetupSaveWriter()
        {
            if (saveFileDataWriter == null)
            {
                saveFileDataWriter = new SaveFileDataWriter();
                saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            }
        }

        private void Update()
        {
            if (saveGame) { saveGame = false; SaveGame(); }
            if (loadGame) { loadGame = false; LoadGame(); }
        }

        // 파일 이름 결정 로직을 수동 Switch에서 자동 문자열 생성으로 변경
        public string DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(CharacterSlot characterSlot)
        {
            if (characterSlot == CharacterSlot.NO_SLOT) return "";
            // Enum 이름을 기반으로 파일명 생성 (예: "CharacterSlot_01")
            return characterSlot.ToString();
        }

        public bool HasFreeCharacterSlot()
        {
            SetupSaveWriter();
            foreach (CharacterSlot slot in Enum.GetValues(typeof(CharacterSlot)))
            {
                if (slot == CharacterSlot.NO_SLOT) continue;
                saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(slot);
                if (!saveFileDataWriter.CheckToSeeIfFileExists()) return true;
            }
            return false;
        }

        public void AttemptToCreateNewGame()
        {
            SetupSaveWriter();
            foreach (CharacterSlot slot in Enum.GetValues(typeof(CharacterSlot)))
            {
                if (slot == CharacterSlot.NO_SLOT) continue;
                saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(slot);

                if (!saveFileDataWriter.CheckToSeeIfFileExists())
                {
                    StartNewGameOnSlot(slot);
                    return;
                }
            }
            TitleScreenManager.Instance.DisplayNoFreeCharacterSlotsPopUp();
        }

        private void StartNewGameOnSlot(CharacterSlot slot)
        {
            currentCharacterSlotBeingUsed = slot;
            currentCharacterData = new CharacterSaveData();
            NewGame();
        }

        private void NewGame()
        {
            player.playerNetworkManager.vigor.Value = 15;
            player.playerNetworkManager.endurance.Value = 10;
            player.playerNetworkManager.mind.Value = 10;

            SaveGame();
            WorldSceneManager.instance.LoadWorldScene(worldSceneIndex);
        }

        public void LoadGame()
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
            currentCharacterData = saveFileDataWriter.LoadSaveFile();
            WorldSceneManager.instance.LoadWorldScene(worldSceneIndex);
        }

        public void SaveGame()
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
            player.SaveGameDataToCurrentCharacterData(ref currentCharacterData);
            saveFileDataWriter.CreateNewCharacterSaveFile(currentCharacterData);
            
            // 저장 후 배열 데이터도 갱신 (선택 사항)
            allCharacterSlots[(int)currentCharacterSlotBeingUsed] = currentCharacterData;
        }

        public void DeleteGame(CharacterSlot characterSlot)
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
            saveFileDataWriter.DeleteSaveFile();
            allCharacterSlots[(int)characterSlot] = null;
        }

        private void LoadAllCharacterProfiles()
        {
            SetupSaveWriter();
            foreach (CharacterSlot slot in Enum.GetValues(typeof(CharacterSlot)))
            {
                if (slot == CharacterSlot.NO_SLOT) continue;
                saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(slot);
                // 배열 인덱스에 맞춰 데이터 로드
                allCharacterSlots[(int)slot] = saveFileDataWriter.LoadSaveFile();
            }
        }

        public int GetWorldSceneIndex()
        {
            return worldSceneIndex;
        }

        public SerializableWeapon GetSerializableWeaponFromWeaponItem(WeaponItem weapon)
        {
            SerializableWeapon serializedWeapon = new SerializableWeapon();

            //  GET WEAPON I.D
            serializedWeapon.itemID = weapon.itemID;
            
            //  GET ASH OF WAR I.D IF ONE IS PRESENT (THERE SHOULD ALWAYS BE ONE BY DEFAULT)
            if (weapon.ashOfWarAction != null)
            {
                serializedWeapon.ashOfWarID = weapon.ashOfWarAction.itemID;
            }
            else
            {
                //  WE USE AN INVALID ID IF THERE IS NO ASH OF WAR, SO THE VALUE WILL BE NULL IF IT TRIES TO SEARCH FOR ONE USING THE I.D
                serializedWeapon.ashOfWarID = -1;
            }

            return serializedWeapon;
        }

        public SerializableRangedProjectile GetSerializableRangedProjectileFromRangedProjectileItem(RangedProjectileItem projectile)
        {
            SerializableRangedProjectile serializedProjectile = new SerializableRangedProjectile();

            if (projectile != null)
            {
                //  GET WEAPON I.D
                serializedProjectile.itemID = projectile.itemID;
                serializedProjectile.itemAmount = projectile.currentAmmoAmount;
            }
            else
            {
                serializedProjectile.itemID = -1;
            }

            return serializedProjectile;
        }

        public SerializableFlask GetSerializableFlaskFromFlaskItem(FlaskItem flask)
        {
            SerializableFlask serializedFlask = new SerializableFlask();

            if (flask != null)
            {
                serializedFlask.itemID = flask.itemID;
            }
            else
            {
                serializedFlask.itemID = -1;
            }

            return serializedFlask;
        }

        public SerilalizableQuickSlotItem GetSerializableQuickSlotItemFromQuickSlotItem(QuickSlotItem quickSlotItem)
        {
            SerilalizableQuickSlotItem serializedQuickSlotItem = new SerilalizableQuickSlotItem();

            if (quickSlotItem != null)
            {
                //  GET WEAPON I.D
                serializedQuickSlotItem.itemID = quickSlotItem.itemID;
                serializedQuickSlotItem.itemAmount = quickSlotItem.itemAmount;
            }
            else
            {
                serializedQuickSlotItem.itemID = -1;
            }

            return serializedQuickSlotItem;
        }
    }
}