using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        public bool IsHoldScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == holdSceneIndex;

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
            if (TryPrepareNewGame())
            {
                FinishCreatingNewGame();
                return;
            }

            TitleScreenManager.Instance.DisplayNoFreeCharacterSlotsPopUp();
        }

        public bool TryPrepareNewGame()
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
                    SetNewGameDefaultStats();
                    return true;
                }
            }

            return false;
        }
        

        private void SetNewGameDefaultStats()
        {
            if (player == null)
            {
                Debug.LogError("Cannot initialize new game stats because WorldSaveGameManager.player is not assigned.");
                return;
            }

            player.playerNetworkManager.vigor.Value        = 15;
            player.playerNetworkManager.endurance.Value    = 15;
            player.playerNetworkManager.mind.Value         = 15;
            player.playerNetworkManager.strength.Value     = 5;
            player.playerNetworkManager.dexterity.Value    = 5;
            player.playerNetworkManager.intelligence.Value = 5;
            player.playerNetworkManager.faith.Value        = 5;

            int healthFlasks = GetDefaultHealthFlaskCharges();
            int focusFlasks  = GetDefaultFocusPointFlaskCharges();
            currentCharacterData.currentHealthFlasksRemaining      = healthFlasks;
            currentCharacterData.currentFocusPointsFlaskRemaining  = focusFlasks;
            player.playerNetworkManager.remainingHealthFlasks.Value      = healthFlasks;
            player.playerNetworkManager.remainingFocusPointsFlasks.Value = focusFlasks;

            EnsureDefaultFlasksInQuickSlots();
        }

        public void FinishCreatingNewGame()
        {
            if (player == null)
            {
                Debug.LogError("Cannot finish creating new game because WorldSaveGameManager.player is not assigned.");
                return;
            }

            currentCharacterData.xPosition = initPosition.x;
            currentCharacterData.yPosition = initPosition.y;
            currentCharacterData.zPosition = initPosition.z;
            ResetFlasksToDefaultCharges();

            SaveGame(false, true);
            WorldSceneManager.Instance.LoadWorldScene(tutorialSceneIndex);
        }

        public void LoadGame()
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);
            currentCharacterData = saveFileDataWriter.LoadSaveFile();
            ClearEquipmentSlotsForShelterLoad(currentCharacterData);
            WorldSceneManager.Instance.LoadWorldScene(holdSceneIndex);
        }

        public void LoadHoldScene()
        {
            ResetStatsForShelterReturn();
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
                ClearEquipmentSlotsForShelterLoad(currentCharacterData);
                if(NetworkManager.Singleton.IsHost)
                    WorldSceneManager.Instance.LoadWorldScene(holdSceneIndex);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ClearEquipmentSlotsForShelterLoad(CharacterSaveData characterData)
        {
            if (characterData == null)
                return;

            characterData.rightMainWeaponItemCode = 0;
            characterData.leftMainWeaponItemCode = 0;
            characterData.rightSubWeaponItemCode = 0;
            characterData.leftSubWeaponItemCode = 0;

            characterData.helmetItemCode = -1;
            characterData.armorItemCode = -1;
            characterData.gauntletItemCode = -1;
            characterData.leggingsItemCode = -1;

            characterData.currentSpell = -1;

            SetFlaskChargesToDefault(characterData);

            characterData.mainProjectile = new SerializableRangedProjectile { itemID = -1 };
            characterData.secondaryProjectile = new SerializableRangedProjectile { itemID = -1 };
        }

        public void SaveGame(bool saveCurrentPlayerPosition = true, bool resetFlasksToDefault = false)
        {
            SetupSaveWriter();
            saveFileDataWriter.saveFileName = DecideCharacterFileNameBasedOnCharacterSlotBeingUsed(currentCharacterSlotBeingUsed);

            Vector3 savedPosition = currentCharacterData != null
                ? new Vector3(currentCharacterData.xPosition, currentCharacterData.yPosition, currentCharacterData.zPosition)
                : Vector3.zero;

            player.SaveGameDataToCurrentCharacterData(ref currentCharacterData);

            if (!saveCurrentPlayerPosition)
            {
                currentCharacterData.xPosition = savedPosition.x;
                currentCharacterData.yPosition = savedPosition.y;
                currentCharacterData.zPosition = savedPosition.z;
            }

            if (IsHoldScene)
                resetFlasksToDefault = true;

            if (resetFlasksToDefault)
            {
                SetFlaskChargesToDefault(currentCharacterData);
                ApplyCurrentFlaskChargesToPlayer();
            }

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

        public void ResetRunes()
        {
            currentCharacterData.runes = 0;

            if (player != null && player.IsOwner)
            {
                player.playerStatsManager.runes = 0;
                GUIController.Instance.playerUIHudManager.ResetRunesUI();
            }
        }

        // ── Pre-dungeon stat snapshot ──────────────────────────────────────────
        public int GetStartingRunesForRun()
        {
            if (currentCharacterData == null)
                return 0;

            int upgradeLevel = Mathf.Clamp(
                currentCharacterData.startingRuneBonusLevel,
                0,
                CharacterSaveData.MaxStartingRuneBonusLevel);

            return upgradeLevel * 1000;
        }

        public void GrantStartingRunesForRun()
        {
            int startingRunes = GetStartingRunesForRun();
            if (startingRunes <= 0 || player == null || !player.IsOwner)
                return;

            player.playerStatsManager.TrackStartingRunesGranted(startingRunes);
            player.playerStatsManager.AddRunes(startingRunes);
        }

        public int GetDefaultHealthFlaskCharges()
        {
            int bonusLevel = currentCharacterData != null
                ? Mathf.Clamp(currentCharacterData.healthFlaskBonusLevel, 0, CharacterSaveData.MaxHealthFlaskBonusLevel)
                : 0;

            return CharacterSaveData.DefaultHealthFlaskCharges + bonusLevel;
        }

        public int GetDefaultFocusPointFlaskCharges()
        {
            int bonusLevel = currentCharacterData != null
                ? Mathf.Clamp(currentCharacterData.focusPointFlaskBonusLevel, 0, CharacterSaveData.MaxFocusPointFlaskBonusLevel)
                : 0;
            return CharacterSaveData.DefaultFocusPointFlaskCharges + bonusLevel;
        }

        public int GetHealthFlaskHealBonus()
        {
            int bonusLevel = currentCharacterData != null
                ? Mathf.Clamp(currentCharacterData.healthFlaskHealBonusLevel, 0, CharacterSaveData.MaxHealthFlaskHealBonusLevel)
                : 0;
            return bonusLevel * CharacterSaveData.HealthFlaskHealBonusPerLevel;
        }

        public int GetFocusPointFlaskHealBonus()
        {
            int bonusLevel = currentCharacterData != null
                ? Mathf.Clamp(currentCharacterData.focusPointFlaskHealBonusLevel, 0, CharacterSaveData.MaxFocusPointFlaskHealBonusLevel)
                : 0;
            return bonusLevel * CharacterSaveData.FocusPointFlaskHealBonusPerLevel;
        }

        public void ResetFlasksToDefaultCharges()
        {
            int healthFlasks = GetDefaultHealthFlaskCharges();
            int focusFlasks = GetDefaultFocusPointFlaskCharges();

            SetFlaskChargesToDefault(currentCharacterData);

            if (player == null || !player.IsOwner)
                return;

            player.playerNetworkManager.remainingHealthFlasks.Value = healthFlasks;
            player.playerNetworkManager.remainingFocusPointsFlasks.Value = focusFlasks;
            GUIController.Instance?.playerUIHudManager?.RefreshQuickSlotCount();
        }

        private void ApplyCurrentFlaskChargesToPlayer()
        {
            if (currentCharacterData == null || player == null || !player.IsOwner)
                return;

            player.playerNetworkManager.remainingHealthFlasks.Value = currentCharacterData.currentHealthFlasksRemaining;
            player.playerNetworkManager.remainingFocusPointsFlasks.Value = currentCharacterData.currentFocusPointsFlaskRemaining;
            GUIController.Instance?.playerUIHudManager?.RefreshQuickSlotCount();
        }

        private void SetFlaskChargesToDefault(CharacterSaveData characterData)
        {
            if (characterData == null)
                return;

            characterData.currentHealthFlasksRemaining = GetDefaultHealthFlaskCharges();
            characterData.currentFocusPointsFlaskRemaining = GetDefaultFocusPointFlaskCharges();
        }

        private void EnsureDefaultFlasksInQuickSlots()
        {
            if (player == null || WorldItemDatabase.Instance == null)
                return;

            PlayerInventoryManager inventory = player.playerInventoryManager;
            if (inventory == null)
                return;

            if (inventory.quickSlotItemsInQuickSlots[0] == null && WorldItemDatabase.Instance.GetDefaultHealthFlask() != null)
                inventory.quickSlotItemsInQuickSlots[0] = Instantiate(WorldItemDatabase.Instance.GetDefaultHealthFlask());

            if (inventory.quickSlotItemsInQuickSlots[1] == null && WorldItemDatabase.Instance.GetDefaultFocusPointFlask() != null)
                inventory.quickSlotItemsInQuickSlots[1] = Instantiate(WorldItemDatabase.Instance.GetDefaultFocusPointFlask());

            if (inventory.quickSlotItemIndex < 0 || inventory.quickSlotItemIndex >= inventory.quickSlotItemsInQuickSlots.Length)
                inventory.quickSlotItemIndex = 0;

            if (player.playerEquipmentManager != null && inventory.quickSlotItemsInQuickSlots[inventory.quickSlotItemIndex] != null)
                player.playerEquipmentManager.LoadQuickSlotEquipment(inventory.quickSlotItemsInQuickSlots[inventory.quickSlotItemIndex]);
        }

        private int _snapVitality, _snapMind, _snapEndurance;
        private int _snapStrength, _snapDexterity, _snapIntelligence, _snapFaith;
        private bool _hasDungeonSnapshot;

        public void SnapshotPreDungeonStats()
        {
            _snapVitality     = currentCharacterData.vitality;
            _snapMind         = currentCharacterData.mind;
            _snapEndurance    = currentCharacterData.endurance;
            _snapStrength     = currentCharacterData.strength;
            _snapDexterity    = currentCharacterData.dexterity;
            _snapIntelligence = currentCharacterData.intelligence;
            _snapFaith        = currentCharacterData.faith;
            _hasDungeonSnapshot = true;
        }

        public void RestorePreDungeonStats()
        {
            if (!_hasDungeonSnapshot) return;

            currentCharacterData.vitality     = _snapVitality;
            currentCharacterData.mind         = _snapMind;
            currentCharacterData.endurance    = _snapEndurance;
            currentCharacterData.strength     = _snapStrength;
            currentCharacterData.dexterity    = _snapDexterity;
            currentCharacterData.intelligence = _snapIntelligence;
            currentCharacterData.faith        = _snapFaith;
            _hasDungeonSnapshot = false;

            // 호스트는 씬 전환 후 LoadGameDataFromCurrentCharacterData가 호출되지 않으므로
            // 런타임 NetworkVariable도 직접 초기화한다.
            if (player != null && player.IsServer && player.IsOwner)
            {
                var nm = player.playerNetworkManager;
                var sm = player.playerStatsManager;
                nm.vigor.Value        = _snapVitality;
                nm.endurance.Value    = _snapEndurance;
                nm.mind.Value         = _snapMind;
                nm.strength.Value     = _snapStrength;
                nm.dexterity.Value    = _snapDexterity;
                nm.intelligence.Value = _snapIntelligence;
                nm.faith.Value        = _snapFaith;
                nm.maxHealth.Value      = sm.CalculateHealthBasedOnVitalityLevel(_snapVitality);
                nm.maxStamina.Value     = sm.CalculateStaminaBasedOnEnduranceLevel(_snapEndurance);
                nm.maxFocusPoints.Value = sm.CalculateFocusPointsBasedOnMindLevel(_snapMind);
                nm.currentHealth.Value  = nm.maxHealth.Value;
                nm.currentStamina.Value = nm.maxStamina.Value;
            }
        }
        // ──────────────────────────────────────────────────────────────────────

        public void ResetStatsForShelterReturn()
        {
            int baseAttributeLevel = CharacterSaveData.DefaultRoguelikeAttributeLevel;

            if (currentCharacterData != null)
            {
                currentCharacterData.vitality     = baseAttributeLevel;
                currentCharacterData.mind         = baseAttributeLevel;
                currentCharacterData.endurance    = baseAttributeLevel;
                currentCharacterData.strength     = baseAttributeLevel;
                currentCharacterData.dexterity    = baseAttributeLevel;
                currentCharacterData.intelligence = baseAttributeLevel;
                currentCharacterData.faith        = baseAttributeLevel;
            }

            _hasDungeonSnapshot = false;

            if (player == null || !player.IsOwner)
                return;

            var nm = player.playerNetworkManager;
            var sm = player.playerStatsManager;

            nm.vigor.Value        = baseAttributeLevel;
            nm.endurance.Value    = baseAttributeLevel;
            nm.mind.Value         = baseAttributeLevel;
            nm.strength.Value     = baseAttributeLevel;
            nm.dexterity.Value    = baseAttributeLevel;
            nm.intelligence.Value = baseAttributeLevel;
            nm.faith.Value        = baseAttributeLevel;
            nm.strengthModifier.Value = 0;

            nm.maxHealth.Value         = sm.CalculateHealthBasedOnVitalityLevel(baseAttributeLevel);
            nm.maxStamina.Value        = sm.CalculateStaminaBasedOnEnduranceLevel(baseAttributeLevel);
            nm.maxFocusPoints.Value    = sm.CalculateFocusPointsBasedOnMindLevel(baseAttributeLevel);
            nm.buildUpCapacity.Value   = sm.CalculateBuildUpCapacityBasedOnVitalityLevel(baseAttributeLevel);
            nm.currentHealth.Value     = nm.maxHealth.Value;
            nm.currentStamina.Value    = nm.maxStamina.Value;
            nm.currentFocusPoints.Value = nm.maxFocusPoints.Value;
            ResetFlasksToDefaultCharges();

            if (currentCharacterData != null)
            {
                currentCharacterData.currentHealth = nm.currentHealth.Value;
                currentCharacterData.currentStamina = nm.currentStamina.Value;
                currentCharacterData.currentFocusPoints = nm.currentFocusPoints.Value;
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

        public void ReturnToOwnServer()
        {
            StartCoroutine(ReturnToOwnServerCoroutine());
        }

        private IEnumerator ReturnToOwnServerCoroutine()
        {
            NetworkManager.Singleton.Shutdown();

            // Shutdown 처리가 완료될 때까지 대기
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            NetworkManager.Singleton.StartHost();

            // 로컬 플레이어가 스폰될 때까지 대기
            float timeout = 10f;
            while (GUIController.Instance.localPlayer == null && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (GUIController.Instance.localPlayer == null)
            {
                Debug.LogError("[ReturnToOwnServer] 호스트 시작 후 플레이어가 스폰되지 않았습니다.");
                yield break;
            }

            LoadHoldScene();
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
