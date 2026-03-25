using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

namespace BK
{
    public class WorldSaveGameManager : Singleton<WorldSaveGameManager>
    {
        public PlayerManager player;

        [Header("SAVE/LOAD")]
        [SerializeField] bool saveGame;
        [SerializeField] bool loadGame;

        [Header("World Scene Index")]
        [SerializeField] private string tutorialSceneIndex = "Scene_World_00_Tutorial";
        [SerializeField] private string holdSceneIndex = "Scene_RoundTableHold";
        public Vector3 holdSceneSpawnPos = new Vector3(0,0,0);

        [Header("Init Position")] 
        [SerializeField] private Vector3 initPosition = new Vector3();

        private SaveFileDataWriter saveFileDataWriter;

        [Header("Current Character Data")]
        public CharacterSlot currentCharacterSlotBeingUsed;
        public CharacterSaveData currentCharacterData;
        private string _saveFileName;
        
        [Header("Character Slots")]
        public List<CharacterSaveData> characterSlots = new List<CharacterSaveData>();
        
        private void Start()
        {
            SetupSaveWriter();
            
            foreach (CharacterSlot slot in Enum.GetValues(typeof(CharacterSlot)))
            {
                characterSlots.Add(null);
            }
            
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
            var slotIndex = (int)characterSlot;
            return $"characterSlot_{slotIndex + 1:00}";
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
                    currentCharacterSlotBeingUsed = slot;
                    currentCharacterData = new CharacterSaveData();
                    NewGame();
                    return;
                }
            }
            TitleScreenManager.Instance.DisplayNoFreeCharacterSlotsPopUp();
        }
        

        private void NewGame()
        {
            player.playerNetworkManager.vigor.Value = 15;
            player.playerNetworkManager.endurance.Value = 10;
            player.playerNetworkManager.mind.Value = 10;

            SaveGame();
            
            currentCharacterData.xPosition = initPosition.x;
            currentCharacterData.yPosition = initPosition.y;
            currentCharacterData.zPosition = initPosition.z;
            
            WorldSceneManager.Instance.LoadWorldScene(tutorialSceneIndex);
            
            
        }

        public void LoadGame()
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
            currentCharacterData = saveFileDataWriter.LoadSaveFile();
            WorldSceneManager.Instance.LoadWorldScene(holdSceneIndex);
        }

        public void LoadHoldScene()
        {
            WorldSceneManager.Instance.LoadWorldScene(holdSceneIndex);
        }
        
        public bool LoadLastGame()
        {
            SetupSaveWriter();
            
            currentCharacterSlotBeingUsed = FindMostRecentlyPlayedSlot();
        
            if (currentCharacterSlotBeingUsed == CharacterSlot.NO_SLOT)
            {
                return false;
            }
            
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
        
            if (saveFileDataWriter.CheckToSeeIfFileExists())
            {
                currentCharacterData = saveFileDataWriter.LoadSaveFile();
                if(NetworkManager.Singleton.IsHost)
                    WorldSceneManager.Instance.LoadWorldScene(holdSceneIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SaveGame()
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
            player.SaveGameDataToCurrentCharacterData(ref currentCharacterData);
            saveFileDataWriter.CreateNewCharacterSaveFile(currentCharacterData);
        }

        public void DeleteGame(CharacterSlot characterSlot)
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);
            saveFileDataWriter.DeleteSaveFile();
            characterSlots[(int)characterSlot] = null;
        }
        
        public void DeleteAllGame()
        {
            foreach (CharacterSlot characterSlot in Enum.GetValues(typeof(CharacterSlot)))
            {
                saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(characterSlot);

                if (saveFileDataWriter.CheckToSeeIfFileExists())
                {
                    DeleteGame(characterSlot);
                }
            }
        }

        private void LoadAllCharacterProfiles()
        {
            SetupSaveWriter();

            foreach (CharacterSlot slot in Enum.GetValues(typeof(CharacterSlot)))
            {
                saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(slot);
            
                // 파일이 존재하는지 먼저 확인
                if (saveFileDataWriter.CheckToSeeIfFileExists())
                {
                    characterSlots[(int)slot] = saveFileDataWriter.LoadSaveFile();
                    //Debug.Log($"슬롯 {i} 로드 성공: {saveFileDataWriter.saveFileName}");
                }
                else
                {
                    characterSlots[(int)slot] = null;
                    //Debug.Log($"슬롯 {i} 파일 없음: {saveFileDataWriter.saveFileName}");
                }
            }
        }
        
        public CharacterSlot FindMostRecentlyPlayedSlot()
        {
            DateTime mostRecentTime = DateTime.MinValue;
            CharacterSlot mostRecentSlot = CharacterSlot.NO_SLOT; // 기본값 설정
            bool foundAnySave = false;
            
            saveFileDataWriter = new SaveFileDataWriter();
            saveFileDataWriter.saveDataDirectoryPath = Application.persistentDataPath;
            
            // 모든 슬롯을 확인
            for (int i = 0; i < characterSlots.Count; i++)
            {
                CharacterSaveData slotData = characterSlots[i];
                CharacterSlot currentSlot = (CharacterSlot)i;
                
                // 슬롯에 저장된 데이터가 있는지 확인
                if (slotData != null && !string.IsNullOrEmpty(slotData.lastPlayTime))
                {
                    // 실제 파일이 존재하는지도 확인
                    saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentSlot);
                    if (saveFileDataWriter.CheckToSeeIfFileExists())
                    {
                        foundAnySave = true;
                        // ISO 8601 형식의 문자열을 DateTime으로 변환
                        DateTime slotDateTime = DateTime.Parse(slotData.lastPlayTime);
                    
                        // 현재까지 발견한 가장 최근 시간보다 더 최근인지 확인
                        if (slotDateTime > mostRecentTime)
                        {
                            mostRecentTime = slotDateTime;
                            mostRecentSlot = currentSlot;
                        }
                        
                        //Debug.Log($"슬롯 {i} 확인됨: {slotData.characterName}, 시간: {slotDateTime}");
                    }
                    else
                    {
                        //Debug.LogWarning($"슬롯 {i}에 데이터가 있지만 파일이 존재하지 않습니다: {saveFileDataWriter.saveFileName}");
                    }
                }
            }
        
            // 저장된 게임을 찾았는지 여부를 로그로 출력
            if (foundAnySave)
            {
                //Debug.Log($"가장 최근 플레이: 슬롯 {(int)mostRecentSlot}, 시간: {mostRecentTime}");
                return mostRecentSlot;
            }
            else
            {
                //Debug.Log("저장된 게임이 없습니다.");
                return CharacterSlot.NO_SLOT; // 저장된 게임이 없으면 NO_SLOT 반환
            }
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