using System;
using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

namespace BK
{
    public class PlayerManager : CharacterManager
    {
        [HideInInspector] public PlayerAnimatorManager playerAnimatorManager;
        [HideInInspector] public PlayerLocomotionManager playerLocomotionManager;
        [HideInInspector] public PlayerNetworkManager playerNetworkManager;
        [HideInInspector] public PlayerStatsManager playerStatsManager;
        [HideInInspector] public PlayerInventoryManager playerInventoryManager;
        [HideInInspector] public PlayerEquipmentManager playerEquipmentManager;
        [HideInInspector] public PlayerCombatManager playerCombatManager;
        [HideInInspector] public PlayerInteractionManager playerInteractionManager;
        [HideInInspector] public PlayerEffectsManager playerEffectsManager;
        [HideInInspector] public PlayerBodyManager playerBodyManager;
        
        [Header("Spawn Protection")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float probeUp = 3f;
        [SerializeField] private float probeDown = 80f;
        [SerializeField] private float groundOffset = 0.05f;
        [SerializeField] private int maxFramesToWait = 180; // 약 3초(60fps) 정도

        protected override void Awake()
        {
            base.Awake();

            //  DO MORE STUFF, ONLY FOR THE PLAYER

            playerLocomotionManager = GetComponent<PlayerLocomotionManager>();
            playerAnimatorManager = GetComponent<PlayerAnimatorManager>();
            playerNetworkManager = GetComponent<PlayerNetworkManager>();
            playerStatsManager = GetComponent<PlayerStatsManager>();
            playerInventoryManager = GetComponent<PlayerInventoryManager>();
            playerEquipmentManager = GetComponent<PlayerEquipmentManager>();
            playerCombatManager = GetComponent<PlayerCombatManager>();
            playerInteractionManager = GetComponent<PlayerInteractionManager>();
            playerEffectsManager = GetComponent<PlayerEffectsManager>();
            playerBodyManager = GetComponent<PlayerBodyManager>();
        }

        protected override void Update()
        {
            base.Update();

            //  IF WE DO NOT OWN THIS GAMEOBJECT, WE DO NOT CONTROL OR EDIT IT
            if (!IsOwner)
                return;

            //  HANDLE MOVEMENT
            playerLocomotionManager.HandleAllMovement();

            //  REGEN STAMINA
            playerStatsManager.RegenerateStamina();

            playerStatsManager.HandleShelterRegen();
        }

        protected override void LateUpdate()
        {
            if (!IsOwner)
                return;

            base.LateUpdate();

            PlayerCamera.Instance.HandleAllCameraActions();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            
            if (IsOwner)
            {
                PlayerCamera.Instance.player = this;
                PlayerInputManager.Instance.player = this;
                GUIController.Instance.localPlayer = this;
                WorldSaveGameManager.Instance.player = this;
                
                playerNetworkManager.vigor.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
                playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;
                playerNetworkManager.mind.OnValueChanged += playerNetworkManager.SetNewMaxFocusPointsValue;
                
                playerNetworkManager.vigor.OnValueChanged += playerNetworkManager.SetNewMaxBuildUpCapacityValue;
                
                playerNetworkManager.currentHealth.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentFocusPoints.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewFocusPointValue;
                
                playerNetworkManager.poisonBuildUp.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewPoisonBuildUpAmount;
                playerNetworkManager.bleedBuildUp.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewBleedBuildUpAmount;
                playerNetworkManager.frostBiteBuildUp.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewFrostBuildUpAmount;

                playerNetworkManager.SetNewMaxHealthValue(0, playerNetworkManager.vigor.Value);
                playerNetworkManager.SetNewMaxStaminaValue(0, playerNetworkManager.endurance.Value);
                playerNetworkManager.SetNewMaxFocusPointsValue(0, playerNetworkManager.mind.Value);
                
                playerNetworkManager.isAiming.OnValueChanged += playerNetworkManager.OnIsAimingChanged;
            }
            
            if (!IsOwner)
                characterNetworkManager.currentHealth.OnValueChanged += characterUIManager.OnHPChanged;
            
            playerNetworkManager.isMale.OnValueChanged += playerNetworkManager.OnIsMaleChanged;
            playerNetworkManager.hairColorRed.OnValueChanged += playerNetworkManager.OnHairColorRedChanged;
            playerNetworkManager.hairColorGreen.OnValueChanged += playerNetworkManager.OnHairColorGreenChanged;
            playerNetworkManager.hairColorBlue.OnValueChanged += playerNetworkManager.OnHairColorBlueChanged;
            
            playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.OnHpChanged;
            playerNetworkManager.currentFocusPoints.OnValueChanged += playerNetworkManager.OnFocusPointsChanged;
            playerNetworkManager.maxFocusPoints.OnValueChanged += playerNetworkManager.OnMaxFocusPointsChanged;
            playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaRegenTimer;
            
            playerNetworkManager.isPoisoned.OnValueChanged += playerNetworkManager.OnIsPoisonedChanged;
            playerNetworkManager.isBleeding.OnValueChanged += playerNetworkManager.OnIsBleedingChanged;
            playerNetworkManager.isFrostBitten.OnValueChanged += playerNetworkManager.OnIsFrostBittenChanged;
            playerNetworkManager.isFrozen.OnValueChanged += playerNetworkManager.OnIsFrozenChanged;
            
            playerNetworkManager.isLockedOn.OnValueChanged += playerNetworkManager.OnIsLockedOnChanged;
            playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged += playerNetworkManager.OnLockOnTargetIDChange;
            
            playerNetworkManager.hairStyleID.OnValueChanged += playerNetworkManager.OnHairStyleIDChanged;
            
            playerNetworkManager.currentRightHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightHandWeaponIDChange;
            playerNetworkManager.currentLeftHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftHandWeaponIDChange;
            playerNetworkManager.currentRightSubWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightSubWeaponIDChange;
            playerNetworkManager.currentLeftSubWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftSubWeaponIDChange;
            
            playerNetworkManager.currentWeaponBeingUsed.OnValueChanged += playerNetworkManager.OnCurrentWeaponBeingUsedIDChange;
            playerNetworkManager.currentQuickSlotItemID.OnValueChanged += playerNetworkManager.OnCurrentQuickSlotItemIDChange;
            playerNetworkManager.isChugging.OnValueChanged += playerNetworkManager.OnIsChuggingChanged;
            playerNetworkManager.currentSpellID.OnValueChanged += playerNetworkManager.OnCurrentSpellIDChange;
            playerNetworkManager.isBlocking.OnValueChanged += playerNetworkManager.OnIsBlockingChanged;
            playerNetworkManager.headEquipmentID.OnValueChanged += playerNetworkManager.OnHeadEquipmentChanged;
            playerNetworkManager.bodyEquipmentID.OnValueChanged += playerNetworkManager.OnBodyEquipmentChanged;
            playerNetworkManager.legEquipmentID.OnValueChanged += playerNetworkManager.OnLegEquipmentChanged;
            
            playerNetworkManager.handEquipmentID.OnValueChanged += playerNetworkManager.OnHandEquipmentChanged;
            playerNetworkManager.mainProjectileID.OnValueChanged += playerNetworkManager.OnMainProjectileIDChange;
            playerNetworkManager.secondaryProjectileID.OnValueChanged += playerNetworkManager.OnSecondaryProjectileIDChange;
            playerNetworkManager.isHoldingArrow.OnValueChanged += playerNetworkManager.OnIsHoldingArrowChanged;
            
            playerNetworkManager.isChargingRightSpell.OnValueChanged += playerNetworkManager.OnIsChargingRightSpellChanged;
            playerNetworkManager.isChargingLeftSpell.OnValueChanged += playerNetworkManager.OnIsChargingLeftSpellChanged;
            
            playerNetworkManager.isTwoHandingWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingWeaponChanged;
            playerNetworkManager.isTwoHandingRightWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingRightWeaponChanged;
            playerNetworkManager.isTwoHandingLeftWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingLeftWeaponChanged;
            
            playerNetworkManager.isChargingAttack.OnValueChanged += playerNetworkManager.OnIsChargingAttackChanged;
            
            if (IsOwner && !IsServer)
            {
                LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.Instance.currentCharacterData, new Vector3());
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

            //  IF THIS IS THE PLAYER OBJECT OWNED BY THIS CLIENT
            if (IsOwner)
            {
                //  UPDATE THE TOTAL AMOUNT OF HEALTH OR STAMINA WHEN THE STAT LINKED TO EITHER CHANGES
                playerNetworkManager.vigor.OnValueChanged -= playerNetworkManager.SetNewMaxHealthValue;
                playerNetworkManager.endurance.OnValueChanged -= playerNetworkManager.SetNewMaxStaminaValue;
                playerNetworkManager.mind.OnValueChanged -= playerNetworkManager.SetNewMaxFocusPointsValue;
                
                //  UPDATE THE TOTAL AMOUNT OF BUILD UP WE CAN ENDURE BASED ON VITALITY LEVEL
                playerNetworkManager.vigor.OnValueChanged -= playerNetworkManager.SetNewMaxBuildUpCapacityValue;

                //  UPDATES UI STAT BARS WHEN A STAT CHANGES (HEALTH OR STAMINA)
                playerNetworkManager.currentHealth.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentFocusPoints.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewFocusPointValue;
                
                //  UPDATE UI BUILD UP BARS WHEN BUILD UP CHANGES
                playerNetworkManager.poisonBuildUp.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewPoisonBuildUpAmount;
                playerNetworkManager.bleedBuildUp.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewBleedBuildUpAmount;
                playerNetworkManager.frostBiteBuildUp.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewFrostBuildUpAmount;
                
                //  RESETS CAMERA ROTATION TO STANDARD WHEN AIMING IS DISABLED
                playerNetworkManager.isAiming.OnValueChanged -= playerNetworkManager.OnIsAimingChanged;
            }

            if (!IsOwner)
                characterNetworkManager.currentHealth.OnValueChanged -= characterUIManager.OnHPChanged;

            //  BODY TYPE
            playerNetworkManager.isMale.OnValueChanged -= playerNetworkManager.OnIsMaleChanged;

            //  STATS
            playerNetworkManager.currentHealth.OnValueChanged -= playerNetworkManager.OnHpChanged;
            playerNetworkManager.currentFocusPoints.OnValueChanged -= playerNetworkManager.OnFocusPointsChanged;
            playerNetworkManager.maxFocusPoints.OnValueChanged -= playerNetworkManager.OnMaxFocusPointsChanged;
            playerNetworkManager.currentStamina.OnValueChanged -= playerStatsManager.ResetStaminaRegenTimer;
            
            // STATUS EFFECTS
            playerNetworkManager.isPoisoned.OnValueChanged -= playerNetworkManager.OnIsPoisonedChanged;
            playerNetworkManager.isBleeding.OnValueChanged -= playerNetworkManager.OnIsBleedingChanged;
            playerNetworkManager.isFrostBitten.OnValueChanged -= playerNetworkManager.OnIsFrostBittenChanged;
            playerNetworkManager.isFrozen.OnValueChanged -= playerNetworkManager.OnIsFrozenChanged;


            //  LOCK ON
            playerNetworkManager.isLockedOn.OnValueChanged -= playerNetworkManager.OnIsLockedOnChanged;
            playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged -= playerNetworkManager.OnLockOnTargetIDChange;

            //  BODY
            playerNetworkManager.hairStyleID.OnValueChanged -= playerNetworkManager.OnHairStyleIDChanged;
            playerNetworkManager.hairColorRed.OnValueChanged -= playerNetworkManager.OnHairColorRedChanged;
            playerNetworkManager.hairColorGreen.OnValueChanged -= playerNetworkManager.OnHairColorGreenChanged;
            playerNetworkManager.hairColorBlue.OnValueChanged -= playerNetworkManager.OnHairColorBlueChanged;

            //  EQUIPMENT
            playerNetworkManager.currentRightHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentRightHandWeaponIDChange;
            playerNetworkManager.currentLeftHandWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentLeftHandWeaponIDChange;
            playerNetworkManager.currentRightSubWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentRightSubWeaponIDChange;
            playerNetworkManager.currentLeftSubWeaponID.OnValueChanged -= playerNetworkManager.OnCurrentLeftSubWeaponIDChange;
            
            playerNetworkManager.currentWeaponBeingUsed.OnValueChanged -= playerNetworkManager.OnCurrentWeaponBeingUsedIDChange;
            playerNetworkManager.currentQuickSlotItemID.OnValueChanged -= playerNetworkManager.OnCurrentQuickSlotItemIDChange;
            playerNetworkManager.isChugging.OnValueChanged -= playerNetworkManager.OnIsChuggingChanged;
            playerNetworkManager.currentSpellID.OnValueChanged -= playerNetworkManager.OnCurrentSpellIDChange;
            playerNetworkManager.headEquipmentID.OnValueChanged -= playerNetworkManager.OnHeadEquipmentChanged;
            playerNetworkManager.bodyEquipmentID.OnValueChanged -= playerNetworkManager.OnBodyEquipmentChanged;
            playerNetworkManager.legEquipmentID.OnValueChanged -= playerNetworkManager.OnLegEquipmentChanged;
            playerNetworkManager.handEquipmentID.OnValueChanged -= playerNetworkManager.OnHandEquipmentChanged;
            
            playerNetworkManager.mainProjectileID.OnValueChanged -= playerNetworkManager.OnMainProjectileIDChange;
            playerNetworkManager.secondaryProjectileID.OnValueChanged -= playerNetworkManager.OnSecondaryProjectileIDChange;
            playerNetworkManager.isHoldingArrow.OnValueChanged -= playerNetworkManager.OnIsHoldingArrowChanged;

            //  SPELLS
            playerNetworkManager.isChargingRightSpell.OnValueChanged -= playerNetworkManager.OnIsChargingRightSpellChanged;
            playerNetworkManager.isChargingLeftSpell.OnValueChanged -= playerNetworkManager.OnIsChargingLeftSpellChanged;

            //  TWO HAND
            playerNetworkManager.isTwoHandingWeapon.OnValueChanged -= playerNetworkManager.OnIsTwoHandingWeaponChanged;
            playerNetworkManager.isTwoHandingRightWeapon.OnValueChanged -= playerNetworkManager.OnIsTwoHandingRightWeaponChanged;
            playerNetworkManager.isTwoHandingLeftWeapon.OnValueChanged -= playerNetworkManager.OnIsTwoHandingLeftWeaponChanged;

            //  FLAGS
            playerNetworkManager.isChargingAttack.OnValueChanged -= playerNetworkManager.OnIsChargingAttackChanged;
        }

        private void OnClientConnectedCallback(ulong clientID)
        {
            WorldGameSessionManager.Instance.AddPlayerToActivePlayersList(this);

            //  IF WE ARE THE SERVER, WE ARE THE HOST, SO WE DONT NEED TO LOAD PLAYERS TO SYNC THEM
            //  YOU ONLY NEED TO LOAD OTHER PLAYERS GEAR TO SYNC IT IF YOU JOIN A GAME THATS ALREADY BEEN ACTIVE WITHOUT YOU BEING PRESENT
            if (!IsServer && IsOwner)
            {
                foreach (var player in WorldGameSessionManager.Instance.players)
                {
                    if (player != this)
                    {
                        player.LoadOtherPlayerCharacterWhenJoiningServer();
                    }
                }

                StartCoroutine(EmergeAtMostRecentSiteOfGrace());
            }
        }

        private IEnumerator EmergeAtMostRecentSiteOfGrace()
        {
            PlayerManager hostPlayer = null;

            while (hostPlayer == null)
            {
                for (int i = 0; i < WorldGameSessionManager.Instance.players.Count; i++)
                {
                    if (WorldGameSessionManager.Instance.players[i].IsHost)
                    {
                        hostPlayer = WorldGameSessionManager.Instance.players[i];
                    }
                }

                yield return null;
            }

            WorldObjectManager.instance.sitesOfGrace[hostPlayer.playerNetworkManager.lastSiteOfGraceUsed.Value].TeleportToSiteOfGrace();
        }

        public override IEnumerator ProcessDeathEvent(bool manuallySelectDeathAnimation = false)
        {
            if (IsOwner)
                GUIController.Instance.playerUIPopUpManager.SendYouDiedPopUp();

            //  TODO KICK NON HOST PLAYERS FROM GAME IF HOST DIES
            WorldGameSessionManager.Instance.WaitThenReviveHost();

            return base.ProcessDeathEvent(manuallySelectDeathAnimation);
        }

        public override void ReviveCharacter()
        {
            base.ReviveCharacter();

            if (IsOwner)
            {
                isDead.Value = false;
                playerNetworkManager.currentHealth.Value = playerNetworkManager.maxHealth.Value;
                playerNetworkManager.currentStamina.Value = playerNetworkManager.maxStamina.Value;
                //  RESTORE FOCUS POINTS

                //  PLAY REBIRTH EFFECTS
                playerAnimatorManager.PlayTargetActionAnimation("Empty", false);
            }
        }

        public void SaveGameDataToCurrentCharacterData(ref CharacterSaveData currentGameData)
        {
            currentGameData.sceneIndex = SceneManager.GetActiveScene().buildIndex;

            currentGameData.characterName = playerNetworkManager.characterName.Value.ToString();
            currentGameData.isMale = playerNetworkManager.isMale.Value;
            currentGameData.xPosition = transform.position.x;
            currentGameData.yPosition = transform.position.y;
            currentGameData.zPosition = transform.position.z;

            //  STATS
            currentGameData.currentHealth = playerNetworkManager.currentHealth.Value;
            currentGameData.currentStamina = playerNetworkManager.currentStamina.Value;
            currentGameData.currentFocusPoints = playerNetworkManager.currentFocusPoints.Value;

            currentGameData.vitality = playerNetworkManager.vigor.Value;
            currentGameData.endurance = playerNetworkManager.endurance.Value;
            currentGameData.mind = playerNetworkManager.mind.Value;
            currentGameData.strength = playerNetworkManager.strength.Value;
            currentGameData.dexterity = playerNetworkManager.dexterity.Value;
            currentGameData.intelligence = playerNetworkManager.intelligence.Value;
            currentGameData.faith = playerNetworkManager.faith.Value;

            currentGameData.runes = playerStatsManager.runes;
            currentGameData.balance = WorldPlayerInventory.Instance.balance.Value;

            //  BODY
            currentGameData.hairStyleID = playerNetworkManager.hairStyleID.Value;
            currentGameData.hairColorRed = playerNetworkManager.hairColorRed.Value;
            currentGameData.hairColorGreen = playerNetworkManager.hairColorGreen.Value;
            currentGameData.hairColorBlue = playerNetworkManager.hairColorBlue.Value;

            currentGameData.currentHealthFlasksRemaining = playerNetworkManager.remainingHealthFlasks.Value;
            currentGameData.currentFocusPointsFlaskRemaining = playerNetworkManager.remainingFocusPointsFlasks.Value;

            //  EQUIPMENT
            currentGameData.rightMainWeaponItemCode = playerNetworkManager.currentRightHandWeaponID.Value;
            currentGameData.leftMainWeaponItemCode = playerNetworkManager.currentLeftHandWeaponID.Value;
            currentGameData.rightSubWeaponItemCode = playerNetworkManager.currentRightSubWeaponID.Value;
            currentGameData.leftSubWeaponItemCode = playerNetworkManager.currentLeftSubWeaponID.Value;
            
            currentGameData.helmetItemCode = playerNetworkManager.headEquipmentID.Value;
            currentGameData.armorItemCode = playerNetworkManager.bodyEquipmentID.Value;
            currentGameData.gauntletItemCode = playerNetworkManager.handEquipmentID.Value;
            currentGameData.leggingsItemCode = playerNetworkManager.legEquipmentID.Value;

            currentGameData.quickSlotItemIDs[0] = playerInventoryManager.quickSlotItemsInQuickSlots[0]?.itemID ?? -1;
            currentGameData.quickSlotItemIDs[1] = playerInventoryManager.quickSlotItemsInQuickSlots[1]?.itemID ?? -1;
            currentGameData.quickSlotItemIDs[2] = playerInventoryManager.quickSlotItemsInQuickSlots[2]?.itemID ?? -1;
            
            currentGameData.mainProjectile = WorldSaveGameManager.Instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.mainProjectile);
            currentGameData.secondaryProjectile = WorldSaveGameManager.Instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.secondaryProjectile);

            if (playerInventoryManager.currentSpell != null)
                currentGameData.currentSpell = playerInventoryManager.currentSpell.itemID;

            //  CLEAR LISTS BEFORE SAVE
            currentGameData.projectilesInInventory = new List<SerializableRangedProjectile>();
            
            // Invnetory
            
            currentGameData.backpackItems.Clear();
            currentGameData.inventoryItems.Clear(); 
            currentGameData.safeItems.Clear();
            
            foreach (var pair in WorldPlayerInventory.Instance.GetSafeInventory().GetCurItemDictById())
            {
                currentGameData.safeItems.Add(pair.Key, pair.Value);
            }
         
            foreach (var pair in WorldPlayerInventory.Instance.GetInventory().GetCurItemDictById())
            {
                currentGameData.inventoryItems.Add(pair.Key, pair.Value);
            }
         
            foreach (var pair in WorldPlayerInventory.Instance.GetBackpackInventory().GetCurItemDictById())
            {
                currentGameData.backpackItems.Add(pair.Key, pair.Value);
            }

            currentGameData.buildings.Clear();
            var buildSystem = BaseGridBuildSystem.Instance as ShelterGridBuildSystem;
            if (buildSystem != null)
            {
                foreach (var building in buildSystem.SaveBuildingDataList)
                {
                    currentGameData.buildings.Add(building);
                }
                Debug.Log($"Saved {currentGameData.buildings.Count} buildings.");
            }
        }

        public void LoadGameDataFromCurrentCharacterData(ref CharacterSaveData currentCharacterData, Vector3 position)
        {
            playerNetworkManager.characterName.Value = currentCharacterData.characterName;
            playerNetworkManager.isMale.Value = currentCharacterData.isMale;
            playerBodyManager.ToggleBodyType(currentCharacterData.isMale);
            
            StartSpawnProtectionSnap(position + Vector3.up);
            //  STATS 
            playerNetworkManager.vigor.Value = currentCharacterData.vitality;
            playerNetworkManager.endurance.Value = currentCharacterData.endurance;
            playerNetworkManager.mind.Value = currentCharacterData.mind;
            playerNetworkManager.strength.Value = currentCharacterData.strength;
            playerNetworkManager.dexterity.Value = currentCharacterData.dexterity;
            playerNetworkManager.intelligence.Value = currentCharacterData.intelligence;
            playerNetworkManager.faith.Value = currentCharacterData.faith;

            playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnVitalityLevel(playerNetworkManager.vigor.Value);
            playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(playerNetworkManager.endurance.Value);
            playerNetworkManager.maxFocusPoints.Value = playerStatsManager.CalculateFocusPointsBasedOnMindLevel(playerNetworkManager.mind.Value);
            playerNetworkManager.currentHealth.Value = playerNetworkManager.maxHealth.Value;
            playerNetworkManager.currentStamina.Value = currentCharacterData.currentStamina;
            playerNetworkManager.currentFocusPoints.Value = currentCharacterData.currentFocusPoints;
            playerNetworkManager.buildUpCapacity.Value = playerStatsManager.CalculateBuildUpCapacityBasedOnVitalityLevel(playerNetworkManager.vigor.Value);

            playerStatsManager.AddRunes(currentCharacterData.runes);
            WorldPlayerInventory.Instance.balance.Value = currentCharacterData.balance;

            playerNetworkManager.remainingHealthFlasks.Value = currentCharacterData.currentHealthFlasksRemaining;
            playerNetworkManager.remainingFocusPointsFlasks.Value = currentCharacterData.currentFocusPointsFlaskRemaining;

            //  BODY
            playerNetworkManager.hairStyleID.Value = currentCharacterData.hairStyleID;
            playerNetworkManager.hairColorRed.Value = currentCharacterData.hairColorRed;
            playerNetworkManager.hairColorGreen.Value = currentCharacterData.hairColorGreen;
            playerNetworkManager.hairColorBlue.Value = currentCharacterData.hairColorBlue;

            //  EQUIPMENT
            if (WorldItemDatabase.Instance.GetHeadEquipmentByID(currentCharacterData.helmetItemCode))
            {
                var headEquipment = Instantiate(WorldItemDatabase.Instance.GetHeadEquipmentByID(currentCharacterData.helmetItemCode));
                playerInventoryManager.headEquipment = headEquipment;
            }
            else
            {
                playerInventoryManager.headEquipment = null;
            }

            if (WorldItemDatabase.Instance.GetBodyEquipmentByID(currentCharacterData.armorItemCode))
            {
                BodyEquipmentItem bodyEquipment = Instantiate(WorldItemDatabase.Instance.GetBodyEquipmentByID(currentCharacterData.armorItemCode));
                playerInventoryManager.bodyEquipment = bodyEquipment;
                
                if (bodyEquipment.backpackSize != Vector2Int.zero)
                {
                    WorldPlayerInventory.Instance.GetBackpackInventory().gameObject.SetActive(true);
                    WorldPlayerInventory.Instance.GetBackpackInventory().UpdateItemGridSize(bodyEquipment.backpackSize);
                    foreach (KeyValuePair<int,int> item in currentCharacterData.backpackItems)
                    {
                        for (int i = 0; i < item.Value; i++)
                        {
                            var itemInfoData = WorldItemDatabase.Instance.GetItemByID(item.Key);
                            if (!WorldPlayerInventory.Instance.ReloadItemBackpack(itemInfoData))
                            {
                                Debug.LogWarning("Reload Error");
                            }
                        }
                    }
                }
                else
                {
                    WorldPlayerInventory.Instance.GetBackpackInventory().gameObject.SetActive(false);
                }
                
            }
            else
            {
                playerInventoryManager.bodyEquipment = null;
                WorldPlayerInventory.Instance.GetBackpackInventory().gameObject.SetActive(false);
            }
            
            if (WorldItemDatabase.Instance.GetHandEquipmentByID(currentCharacterData.helmetItemCode))
            {
                var handEquipment = Instantiate(WorldItemDatabase.Instance.GetHandEquipmentByID(currentCharacterData.helmetItemCode));
                playerInventoryManager.handEquipment = handEquipment;
            }
            else
            {
                playerInventoryManager.handEquipment = null;
            }
            
            if (WorldItemDatabase.Instance.GetLegEquipmentByID(currentCharacterData.leggingsItemCode))
            {
                var legEquipment = Instantiate(WorldItemDatabase.Instance.GetLegEquipmentByID(currentCharacterData.leggingsItemCode));
                playerInventoryManager.legEquipment = legEquipment;
            }
            else
            {
                playerInventoryManager.legEquipment = null;
            }
            

            if (WorldItemDatabase.Instance.GetSpellByID(currentCharacterData.currentSpell))
            {
                SpellItem currentSpell = Instantiate(WorldItemDatabase.Instance.GetSpellByID(currentCharacterData.currentSpell));
                playerNetworkManager.currentSpellID.Value = currentSpell.itemID;
            }
            else
            {
                playerNetworkManager.currentSpellID.Value = -1; // -1 SETS SPELL TO NULL AS ITS NOT A VALID ID
            }

            for (int i = 0; i < currentCharacterData.projectilesInInventory.Count; i++)
            {
                RangedProjectileItem projectile = currentCharacterData.projectilesInInventory[i].GetProjectile();
                playerInventoryManager.AddItemToInventory(projectile);
            }
            
            if(currentCharacterData.quickSlotItemIDs[0] != -1)
                playerInventoryManager.quickSlotItemsInQuickSlots[0] = Instantiate(WorldItemDatabase.Instance.GetQuickSlotItemByID(currentCharacterData.quickSlotItemIDs[0]));
            if(currentCharacterData.quickSlotItemIDs[1] != -1)
                playerInventoryManager.quickSlotItemsInQuickSlots[1] = Instantiate(WorldItemDatabase.Instance.GetQuickSlotItemByID(currentCharacterData.quickSlotItemIDs[1]));
            if(currentCharacterData.quickSlotItemIDs[2] != -1)    
                playerInventoryManager.quickSlotItemsInQuickSlots[2] = Instantiate(WorldItemDatabase.Instance.GetQuickSlotItemByID(currentCharacterData.quickSlotItemIDs[2]));
            
            playerEquipmentManager.LoadQuickSlotEquipment(playerInventoryManager.quickSlotItemsInQuickSlots[0]);

            
            // ARMOR
            WorldPlayerInventory.Instance.GetHelmetInventory().UpdateItemGridSize(currentCharacterData.helmetBoxSize);
            var helmetItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.helmetItemCode);
            if (helmetItem && !WorldPlayerInventory.Instance.ReloadItemHelmet(helmetItem)) Debug.LogError($"Reload Error : Helmet - {helmetItem.itemID}");
            
            WorldPlayerInventory.Instance.GetArmorInventory().UpdateItemGridSize(currentCharacterData.armorBoxSize);
            var armorItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.armorItemCode);
            if (armorItem && !WorldPlayerInventory.Instance.ReloadItemArmor(armorItem)) Debug.LogError($"Reload Error : Armor - {armorItem.itemID}");
            
            WorldPlayerInventory.Instance.GetGauntletInventory().UpdateItemGridSize(currentCharacterData.gauntletBoxSize);
            var gauntletItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.gauntletItemCode);
            if (gauntletItem && !WorldPlayerInventory.Instance.ReloadItemGauntlet(gauntletItem)) Debug.LogError($"Reload Error : Gauntlet - {gauntletItem.itemID}");
            
            WorldPlayerInventory.Instance.GetLeggingsInventory().UpdateItemGridSize(currentCharacterData.leggingsBoxSize);
            var leggingsItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.leggingsItemCode);
            if (leggingsItem && !WorldPlayerInventory.Instance.ReloadItemLeggings(leggingsItem)) Debug.LogError($"Reload Error : Leggings - {leggingsItem.itemID}");
            
            
            // WEAPON
            WorldPlayerInventory.Instance.GetRightWeaponInventory().UpdateItemGridSize(currentCharacterData.rightWeaponBoxSize);
            var rightMainWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.rightMainWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemRightMainWeapon(rightMainWeaponItem)) Debug.LogError($"Reload Error : Right Main - {rightMainWeaponItem.itemID}");
            
            WorldPlayerInventory.Instance.GetLeftWeaponInventory().UpdateItemGridSize(currentCharacterData.leftWeaponBoxSize);
            var leftMainWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.leftMainWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemLeftMainWeapon(leftMainWeaponItem)) Debug.LogError($"Reload Error : Left Main - {leftMainWeaponItem.itemID}");
            
            WorldPlayerInventory.Instance.GetRightSubWeaponInventory().UpdateItemGridSize(currentCharacterData.rightWeaponBoxSize);
            var rightSubWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.rightSubWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemRightSubWeapon(rightSubWeaponItem)) Debug.LogError($"Reload Error : Right Sub - {rightSubWeaponItem.itemID}");
            
            WorldPlayerInventory.Instance.GetLeftSubWeaponInventory().UpdateItemGridSize(currentCharacterData.leftWeaponBoxSize);
            var leftSubWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.leftSubWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemLeftSubWeapon(leftSubWeaponItem)) Debug.LogError($"Reload Error : Left Sub - {leftSubWeaponItem.itemID}");
            
            WorldPlayerInventory.Instance.GetShareInventory().UpdateItemGridSize(currentCharacterData.shareBoxSize);
            foreach (KeyValuePair<int,int> item in currentCharacterData.shareInventoryItems)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    var itemInfoData = WorldItemDatabase.Instance.GetItemByID(item.Key);
                    if (!WorldPlayerInventory.Instance.ReloadItemShareBox(itemInfoData))
                    {
                        Debug.LogWarning("Reload Error");
                    }
                }
            }
            
            WorldPlayerInventory.Instance.GetInventory().UpdateItemGridSize(currentCharacterData.inventoryBoxSize);
            foreach (KeyValuePair<int,int> item in currentCharacterData.inventoryItems)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    var itemInfoData = WorldItemDatabase.Instance.GetItemByID(item.Key);
                    if (!WorldPlayerInventory.Instance.ReloadItemInventory(itemInfoData))
                    {
                        Debug.LogWarning("Reload Error");
                    }
                }
            }
            
            // 금고 인벤토리 
            WorldPlayerInventory.Instance.GetSafeInventory().UpdateItemGridSize(currentCharacterData.safeBoxSize);
            foreach (KeyValuePair<int,int> item in currentCharacterData.safeItems)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    var itemInfoData = WorldItemDatabase.Instance.GetItemByID(item.Key);
                    if (!WorldPlayerInventory.Instance.ReloadItemSafe(itemInfoData))
                    {
                        Debug.LogWarning("Reload Error");
                    }
                }
            }
            
            playerEquipmentManager.LoadMainProjectileEquipment(currentCharacterData.mainProjectile.GetProjectile());
            playerEquipmentManager.LoadSecondaryProjectileEquipment(currentCharacterData.secondaryProjectile.GetProjectile());
            
            // 마지막 플레이 타임 저장 
            currentCharacterData.lastPlayTime = DateTime.Now.ToString("o");
        }

        public void LoadOtherPlayerCharacterWhenJoiningServer()
        {
            //  SYNC BODY TYPE
            playerNetworkManager.OnIsMaleChanged(false, playerNetworkManager.isMale.Value);
            playerNetworkManager.OnHairStyleIDChanged(0, playerNetworkManager.hairStyleID.Value);
            playerNetworkManager.OnHairColorRedChanged(0, playerNetworkManager.hairColorRed.Value);
            playerNetworkManager.OnHairColorGreenChanged(0, playerNetworkManager.hairColorGreen.Value);
            playerNetworkManager.OnHairColorBlueChanged(0, playerNetworkManager.hairColorBlue.Value);

            //  SYNC WEAPONS
            playerNetworkManager.OnCurrentRightHandWeaponIDChange(0, playerNetworkManager.currentRightHandWeaponID.Value);
            playerNetworkManager.OnCurrentLeftHandWeaponIDChange(0, playerNetworkManager.currentLeftHandWeaponID.Value);
            playerNetworkManager.OnCurrentSpellIDChange(0, playerNetworkManager.currentSpellID.Value);

            //  SYNC ARMOR
            playerNetworkManager.OnHeadEquipmentChanged(0, playerNetworkManager.headEquipmentID.Value);
            playerNetworkManager.OnBodyEquipmentChanged(0, playerNetworkManager.bodyEquipmentID.Value);
            playerNetworkManager.OnHandEquipmentChanged(0, playerNetworkManager.handEquipmentID.Value);
            playerNetworkManager.OnLegEquipmentChanged(0, playerNetworkManager.legEquipmentID.Value);

            //  SYNC PROJECTILES
            playerNetworkManager.OnMainProjectileIDChange(0, playerNetworkManager.mainProjectileID.Value);
            playerNetworkManager.OnSecondaryProjectileIDChange(0, playerNetworkManager.secondaryProjectileID.Value);
            playerNetworkManager.OnIsHoldingArrowChanged(false, playerNetworkManager.isHoldingArrow.Value);
            
            playerNetworkManager.OnCurrentSpellIDChange(0, playerNetworkManager.currentSpellID.Value);

            //  SYNC TWO HAND STATUS
            playerNetworkManager.OnIsTwoHandingRightWeaponChanged(false, playerNetworkManager.isTwoHandingRightWeapon.Value);
            playerNetworkManager.OnIsTwoHandingLeftWeaponChanged(false, playerNetworkManager.isTwoHandingLeftWeapon.Value);
            
            //  SYNC STATUS EFFECTS
            playerNetworkManager.OnIsPoisonedChanged(false, playerNetworkManager.isPoisoned.Value);


            //  SYNC BLOCK STATUS
            playerNetworkManager.OnIsBlockingChanged(false, playerNetworkManager.isBlocking.Value);

            //  ARMOR

            //  LOCK ON
            if (playerNetworkManager.isLockedOn.Value)
            {
                playerNetworkManager.OnLockOnTargetIDChange(0, playerNetworkManager.currentTargetNetworkObjectID.Value);
            }
        }

        // SpawnProtect
        public void StartSpawnProtectionSnap(Vector3 desiredPos)
        {
            StopCoroutine(nameof(CoSpawnProtectSnap));
            StartCoroutine(CoSpawnProtectSnap(desiredPos));
        }

        private IEnumerator CoSpawnProtectSnap(Vector3 desiredPos)
        {
            // 1) 스폰 보호 ON
            if (playerLocomotionManager != null)
                playerLocomotionManager.isSpawnProtected = true;

            // 2) 컨트롤러 잠깐 OFF (텔레포트/스냅 시 충돌 계산 꼬임 방지)
            if (characterController != null)
                characterController.enabled = false;

            // 3) 일단 위로 띄워서 “바닥이 생기는 순간”을 기다리며 스냅
            Vector3 probeStart = desiredPos + Vector3.up * probeUp;
            transform.SetPositionAndRotation(probeStart, Quaternion.identity);

            // 콜라이더/지형 생성 타이밍 확보
            yield return null;

            bool grounded = false;
            Vector3 snappedPos = desiredPos;

            for (int i = 0; i < maxFramesToWait; i++)
            {
                // 매 프레임 바닥을 찾는다
                if (Physics.Raycast(probeStart, Vector3.down, out RaycastHit hit, probeUp + probeDown, groundMask, QueryTriggerInteraction.Ignore))
                {
                    snappedPos = hit.point + Vector3.up * groundOffset;
                    grounded = true;
                    break;
                }

                // 바닥이 없으면 계속 “원하는 위치 주변”에 고정(추락 방지)
                transform.SetPositionAndRotation(probeStart, Quaternion.identity);
                yield return null;
            }

            // 4) 최종 위치 세팅
            transform.SetPositionAndRotation(grounded ? snappedPos : desiredPos, Quaternion.identity);

            // 5) 컨트롤러 ON
            if (characterController != null)
                characterController.enabled = true;

            // 6) 낙하 속도 리셋 (중요)
            if (playerLocomotionManager != null)
            {
                playerLocomotionManager.ResetSpawnProtect();
            }
        }
    }
}
