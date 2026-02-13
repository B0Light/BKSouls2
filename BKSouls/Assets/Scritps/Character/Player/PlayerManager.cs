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
        }

        protected override void LateUpdate()
        {
            if (!IsOwner)
                return;

            base.LateUpdate();

            PlayerCamera.instance.HandleAllCameraActions();
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

            //  IF THIS IS THE PLAYER OBJECT OWNED BY THIS CLIENT
            if (IsOwner)
            {
                PlayerCamera.instance.player = this;
                PlayerInputManager.instance.player = this;
                GUIController.Instance.localPlayer = this;
                WorldSaveGameManager.instance.player = this;

                //  UPDATE THE TOTAL AMOUNT OF HEALTH OR STAMINA WHEN THE STAT LINKED TO EITHER CHANGES
                playerNetworkManager.vigor.OnValueChanged += playerNetworkManager.SetNewMaxHealthValue;
                playerNetworkManager.endurance.OnValueChanged += playerNetworkManager.SetNewMaxStaminaValue;
                playerNetworkManager.mind.OnValueChanged += playerNetworkManager.SetNewMaxFocusPointsValue;

                //  UPDATE THE TOTAL AMOUNT OF BUILD UP WE CAN ENDURE BASED ON VITALITY LEVEL
                playerNetworkManager.vigor.OnValueChanged += playerNetworkManager.SetNewMaxBuildUpCapacityValue;
                
                //  UPDATES UI STAT BARS WHEN A STAT CHANGES (HEALTH OR STAMINA)
                playerNetworkManager.currentHealth.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentFocusPoints.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewFocusPointValue;

                //  UPDATE UI BUILD UP BARS WHEN BUILD UP CHANGES
                playerNetworkManager.poisonBuildUp.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewPoisonBuildUpAmount;
                playerNetworkManager.bleedBuildUp.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewBleedBuildUpAmount;
                playerNetworkManager.frostBiteBuildUp.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewFrostBuildUpAmount;

                playerNetworkManager.SetNewMaxHealthValue(0, playerNetworkManager.vigor.Value);
                playerNetworkManager.SetNewMaxStaminaValue(0, playerNetworkManager.endurance.Value);
                playerNetworkManager.SetNewMaxFocusPointsValue(0, playerNetworkManager.mind.Value);
                
                playerInventoryManager.currentQuickSlotIDList.OnItemAdded += playerInventoryManager.OnAddQuickSlotItem;
                playerInventoryManager.currentQuickSlotIDList.OnItemRemoved += playerInventoryManager.OnRemoveQuickSlotItem;
                playerInventoryManager.currentQuickSlotIDList.OnListCleared += playerInventoryManager.OnQuickSlotClear;

                //  RESETS CAMERA ROTATION TO STANDARD WHEN AIMING IS DISABLED
                playerNetworkManager.isAiming.OnValueChanged += playerNetworkManager.OnIsAimingChanged;
            }

            //  ONLY UPDATE FLOATING HP BAR IF THIS CHARACTER IS NOT THE LOCAL PLAYERS CHARACTER (YOU DONT WANNA SEE A HP BAR FLOATING ABOVE YOUR OWN HEAD)
            if (!IsOwner)
                characterNetworkManager.currentHealth.OnValueChanged += characterUIManager.OnHPChanged;

            //  BODY TYPE
            playerNetworkManager.isMale.OnValueChanged += playerNetworkManager.OnIsMaleChanged;
            playerNetworkManager.hairColorRed.OnValueChanged += playerNetworkManager.OnHairColorRedChanged;
            playerNetworkManager.hairColorGreen.OnValueChanged += playerNetworkManager.OnHairColorGreenChanged;
            playerNetworkManager.hairColorBlue.OnValueChanged += playerNetworkManager.OnHairColorBlueChanged;

            //  STATS
            playerNetworkManager.currentHealth.OnValueChanged += playerNetworkManager.OnHpChanged;
            playerNetworkManager.currentFocusPoints.OnValueChanged += playerNetworkManager.OnFocusPointsChanged;
            playerNetworkManager.maxFocusPoints.OnValueChanged += playerNetworkManager.OnMaxFocusPointsChanged;
            playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaRegenTimer;
            
            // STATUS EFFECTS
            playerNetworkManager.isPoisoned.OnValueChanged += playerNetworkManager.OnIsPoisonedChanged;
            playerNetworkManager.isBleeding.OnValueChanged += playerNetworkManager.OnIsBleedingChanged;
            playerNetworkManager.isFrostBitten.OnValueChanged += playerNetworkManager.OnIsFrostBittenChanged;
            playerNetworkManager.isFrozen.OnValueChanged += playerNetworkManager.OnIsFrozenChanged;

            //  LOCK ON
            playerNetworkManager.isLockedOn.OnValueChanged += playerNetworkManager.OnIsLockedOnChanged;
            playerNetworkManager.currentTargetNetworkObjectID.OnValueChanged += playerNetworkManager.OnLockOnTargetIDChange;

            //  BODY
            playerNetworkManager.hairStyleID.OnValueChanged += playerNetworkManager.OnHairStyleIDChanged;

            //  EQUIPMENT
            playerNetworkManager.currentRightHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentRightHandWeaponIDChange;
            playerNetworkManager.currentLeftHandWeaponID.OnValueChanged += playerNetworkManager.OnCurrentLeftHandWeaponIDChange;
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

            //  SPELLS
            playerNetworkManager.isChargingRightSpell.OnValueChanged += playerNetworkManager.OnIsChargingRightSpellChanged;
            playerNetworkManager.isChargingLeftSpell.OnValueChanged += playerNetworkManager.OnIsChargingLeftSpellChanged;

            //  TWO HAND
            playerNetworkManager.isTwoHandingWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingWeaponChanged;
            playerNetworkManager.isTwoHandingRightWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingRightWeaponChanged;
            playerNetworkManager.isTwoHandingLeftWeapon.OnValueChanged += playerNetworkManager.OnIsTwoHandingLeftWeaponChanged;

            //  FLAGS
            playerNetworkManager.isChargingAttack.OnValueChanged += playerNetworkManager.OnIsChargingAttackChanged;

            //  UPON CONNECTING, IF WE ARE THE OWNER OF THIS CHARACTER, BUT WE ARE NOT THE SERVER, RELOAD OUR CHARACTER DATA TO THIS NEWLY INSTANTIATED CHARACTER
            //  WE DONT RUN THIS IF WE ARE THE SERVER, BECAUSE SINCE THEY ARE THE HOST, THEY ARE ALREADY LOADED IN AND DON'T NEED TO RELOAD THEIR DATA
            if (IsOwner && !IsServer)
            {
                LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentGameData);
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
            WorldGameSessionManager.instance.AddPlayerToActivePlayersList(this);

            //  IF WE ARE THE SERVER, WE ARE THE HOST, SO WE DONT NEED TO LOAD PLAYERS TO SYNC THEM
            //  YOU ONLY NEED TO LOAD OTHER PLAYERS GEAR TO SYNC IT IF YOU JOIN A GAME THATS ALREADY BEEN ACTIVE WITHOUT YOU BEING PRESENT
            if (!IsServer && IsOwner)
            {
                foreach (var player in WorldGameSessionManager.instance.players)
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
                for (int i = 0; i < WorldGameSessionManager.instance.players.Count; i++)
                {
                    if (WorldGameSessionManager.instance.players[i].IsHost)
                    {
                        hostPlayer = WorldGameSessionManager.instance.players[i];
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
            WorldGameSessionManager.instance.WaitThenReviveHost();

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

        public void SaveGameDataToCurrentCharacterData(ref SaveGameData currentGameGameData)
        {
            currentGameGameData.sceneIndex = SceneManager.GetActiveScene().buildIndex;

            currentGameGameData.characterName = playerNetworkManager.characterName.Value.ToString();
            currentGameGameData.isMale = playerNetworkManager.isMale.Value;
            currentGameGameData.xPosition = transform.position.x;
            currentGameGameData.yPosition = transform.position.y;
            currentGameGameData.zPosition = transform.position.z;

            //  STATS
            currentGameGameData.currentHealth = playerNetworkManager.currentHealth.Value;
            currentGameGameData.currentStamina = playerNetworkManager.currentStamina.Value;
            currentGameGameData.currentFocusPoints = playerNetworkManager.currentFocusPoints.Value;

            currentGameGameData.vitality = playerNetworkManager.vigor.Value;
            currentGameGameData.endurance = playerNetworkManager.endurance.Value;
            currentGameGameData.mind = playerNetworkManager.mind.Value;
            currentGameGameData.strength = playerNetworkManager.strength.Value;
            currentGameGameData.dexterity = playerNetworkManager.dexterity.Value;
            currentGameGameData.intelligence = playerNetworkManager.intelligence.Value;
            currentGameGameData.faith = playerNetworkManager.faith.Value;

            currentGameGameData.runes = playerStatsManager.runes;

            //  BODY
            currentGameGameData.hairStyleID = playerNetworkManager.hairStyleID.Value;
            currentGameGameData.hairColorRed = playerNetworkManager.hairColorRed.Value;
            currentGameGameData.hairColorGreen = playerNetworkManager.hairColorGreen.Value;
            currentGameGameData.hairColorBlue = playerNetworkManager.hairColorBlue.Value;

            //  EQUIPMENT
            currentGameGameData.rightWeaponItemCode = playerNetworkManager.currentRightHandWeaponID.Value;
            currentGameGameData.leftWeaponItemCode = playerNetworkManager.currentLeftHandWeaponID.Value;
            
            currentGameGameData.helmetItemCode = playerNetworkManager.headEquipmentID.Value;
            currentGameGameData.armorItemCode = playerNetworkManager.bodyEquipmentID.Value;
            currentGameGameData.gauntletItemCode = playerNetworkManager.handEquipmentID.Value;
            currentGameGameData.leggingsItemCode = playerNetworkManager.legEquipmentID.Value;
            
            currentGameGameData.mainProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.mainProjectile);
            currentGameGameData.secondaryProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.secondaryProjectile);

            if (playerInventoryManager.currentSpell != null)
                currentGameGameData.currentSpell = playerInventoryManager.currentSpell.itemID;

            //  CLEAR LISTS BEFORE SAVE
            currentGameGameData.projectilesInInventory = new List<SerializableRangedProjectile>();
            
            
            // Invnetory
            
            currentGameGameData.backpackItems.Clear();
            currentGameGameData.inventoryItems.Clear(); 
            currentGameGameData.safeItems.Clear();
            currentGameGameData.quickSlotConsumableItems.Clear();
            
            foreach (var pair in WorldPlayerInventory.Instance.GetConsumableInventory().GetCurItemDictById())
            {
                currentGameGameData.quickSlotConsumableItems.Add(pair.Key, pair.Value);
            }
         
            foreach (var pair in WorldPlayerInventory.Instance.GetSafeInventory().GetCurItemDictById())
            {
                currentGameGameData.safeItems.Add(pair.Key, pair.Value);
            }
         
            foreach (var pair in WorldPlayerInventory.Instance.GetInventory().GetCurItemDictById())
            {
                currentGameGameData.inventoryItems.Add(pair.Key, pair.Value);
            }
         
            foreach (var pair in WorldPlayerInventory.Instance.GetBackpackInventory().GetCurItemDictById())
            {
                currentGameGameData.backpackItems.Add(pair.Key, pair.Value);
            }
            
            currentGameGameData.lastPlayTime = DateTime.Now.ToString("o");
        }

        public void LoadGameDataFromCurrentCharacterData(ref SaveGameData currentGameData)
        {
            playerNetworkManager.characterName.Value = currentGameData.characterName;
            playerNetworkManager.isMale.Value = currentGameData.isMale;
            playerBodyManager.ToggleBodyType(currentGameData.isMale); //   TOGGLE INCASE THE VALUE IS THE SAME AS DEFAULT (ONVALUECHANGED ONLY WORKS WHEN VALUE IS CHANGED)
            Vector3 myPosition = new Vector3(currentGameData.xPosition, currentGameData.yPosition + 5f, currentGameData.zPosition);
            transform.position = myPosition;

            //  STATS 
            playerNetworkManager.vigor.Value = currentGameData.vitality;
            playerNetworkManager.endurance.Value = currentGameData.endurance;
            playerNetworkManager.mind.Value = currentGameData.mind;
            playerNetworkManager.strength.Value = currentGameData.strength;
            playerNetworkManager.dexterity.Value = currentGameData.dexterity;
            playerNetworkManager.intelligence.Value = currentGameData.intelligence;
            playerNetworkManager.faith.Value = currentGameData.faith;

            playerNetworkManager.maxHealth.Value = playerStatsManager.CalculateHealthBasedOnVitalityLevel(playerNetworkManager.vigor.Value);
            playerNetworkManager.maxStamina.Value = playerStatsManager.CalculateStaminaBasedOnEnduranceLevel(playerNetworkManager.endurance.Value);
            playerNetworkManager.maxFocusPoints.Value = playerStatsManager.CalculateFocusPointsBasedOnMindLevel(playerNetworkManager.mind.Value);
            playerNetworkManager.currentHealth.Value = currentGameData.currentHealth;
            playerNetworkManager.currentStamina.Value = currentGameData.currentStamina;
            playerNetworkManager.currentFocusPoints.Value = currentGameData.currentFocusPoints;
            playerNetworkManager.buildUpCapacity.Value = playerStatsManager.CalculateBuildUpCapacityBasedOnVitalityLevel(playerNetworkManager.vigor.Value);

            playerStatsManager.AddRunes(currentGameData.runes);

            //  BODY
            playerNetworkManager.hairStyleID.Value = currentGameData.hairStyleID;
            playerNetworkManager.hairColorRed.Value = currentGameData.hairColorRed;
            playerNetworkManager.hairColorGreen.Value = currentGameData.hairColorGreen;
            playerNetworkManager.hairColorBlue.Value = currentGameData.hairColorBlue;

            //  EQUIPMENT
            if (WorldItemDatabase.Instance.GetHeadEquipmentByID(currentGameData.helmetItemCode))
            {
                var headEquipment = Instantiate(WorldItemDatabase.Instance.GetHeadEquipmentByID(currentGameData.helmetItemCode));
                playerInventoryManager.headEquipment = headEquipment;
            }
            else
            {
                playerInventoryManager.headEquipment = null;
            }

            if (WorldItemDatabase.Instance.GetBodyEquipmentByID(currentGameData.armorItemCode))
            {
                BodyEquipmentItem bodyEquipment = Instantiate(WorldItemDatabase.Instance.GetBodyEquipmentByID(currentGameData.armorItemCode));
                playerInventoryManager.bodyEquipment = bodyEquipment;
                
                if (bodyEquipment.backpackSize != Vector2Int.zero)
                {
                    WorldPlayerInventory.Instance.GetBackpackInventory().gameObject.SetActive(true);
                    WorldPlayerInventory.Instance.GetBackpackInventory().UpdateItemGridSize(bodyEquipment.backpackSize);
                    foreach (KeyValuePair<int,int> item in currentGameData.backpackItems)
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
            }
            
            if (WorldItemDatabase.Instance.GetHandEquipmentByID(currentGameData.helmetItemCode))
            {
                var handEquipment = Instantiate(WorldItemDatabase.Instance.GetHandEquipmentByID(currentGameData.helmetItemCode));
                playerInventoryManager.handEquipment = handEquipment;
            }
            else
            {
                playerInventoryManager.handEquipment = null;
            }
            
            if (WorldItemDatabase.Instance.GetLegEquipmentByID(currentGameData.leggingsItemCode))
            {
                var legEquipment = Instantiate(WorldItemDatabase.Instance.GetLegEquipmentByID(currentGameData.leggingsItemCode));
                playerInventoryManager.legEquipment = legEquipment;
            }
            else
            {
                playerInventoryManager.legEquipment = null;
            }
            

            if (WorldItemDatabase.Instance.GetSpellByID(currentGameData.currentSpell))
            {
                SpellItem currentSpell = Instantiate(WorldItemDatabase.Instance.GetSpellByID(currentGameData.currentSpell));
                playerNetworkManager.currentSpellID.Value = currentSpell.itemID;
            }
            else
            {
                playerNetworkManager.currentSpellID.Value = -1; // -1 SETS SPELL TO NULL AS ITS NOT A VALID ID
            }

            for (int i = 0; i < currentGameData.projectilesInInventory.Count; i++)
            {
                RangedProjectileItem projectile = currentGameData.projectilesInInventory[i].GetProjectile();
                playerInventoryManager.AddItemToInventory(projectile);
            }
            
            WorldPlayerInventory.Instance.GetHelmetInventory().UpdateItemGridSize(currentGameData.helmetBoxSize);
            var helmetItem = WorldItemDatabase.Instance.GetItemByID(currentGameData.helmetItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemHelmet(helmetItem))
            {
                Debug.LogError($"helmetCode : {currentGameData.helmetItemCode}");
                Debug.LogError($"helmetData : {helmetItem}");
                Debug.LogError("helmet : Reload Error");
            }
            
            WorldPlayerInventory.Instance.GetArmorInventory().UpdateItemGridSize(currentGameData.armorBoxSize);
            var armorItem = WorldItemDatabase.Instance.GetItemByID(currentGameData.armorItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemArmor(armorItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetRightWeaponInventory().UpdateItemGridSize(currentGameData.rightWeaponBoxSize);
            var rightWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentGameData.rightWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemRightWeapon(rightWeaponItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetLeftWeaponInventory().UpdateItemGridSize(currentGameData.leftWeaponBoxSize);
            var leftWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentGameData.leftWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemLeftWeapon(leftWeaponItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetGauntletInventory().UpdateItemGridSize(currentGameData.gauntletBoxSize);
            var gauntletItem = WorldItemDatabase.Instance.GetItemByID(currentGameData.gauntletItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemGauntlet(gauntletItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetLeggingsInventory().UpdateItemGridSize(currentGameData.leggingsBoxSize);
            var leggingsItem = WorldItemDatabase.Instance.GetItemByID(currentGameData.leggingsItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemLeggings(leggingsItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetShareInventory().UpdateItemGridSize(currentGameData.shareBoxSize);
            foreach (KeyValuePair<int,int> item in currentGameData.shareInventoryItems)
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
            
            WorldPlayerInventory.Instance.GetInventory().UpdateItemGridSize(currentGameData.inventoryBoxSize);
            foreach (KeyValuePair<int,int> item in currentGameData.inventoryItems)
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
            WorldPlayerInventory.Instance.GetSafeInventory().UpdateItemGridSize(currentGameData.safeBoxSize);
            foreach (KeyValuePair<int,int> item in currentGameData.safeItems)
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
            // QuickSlot 인벤토리 
            WorldPlayerInventory.Instance.GetConsumableInventory().UpdateItemGridSize(currentGameData.consumableBoxSize);
            foreach (KeyValuePair<int,int> item in currentGameData.quickSlotConsumableItems)
            {
                for (int i = 0; i < item.Value; i++)
                {
                    var itemInfoData = WorldItemDatabase.Instance.GetItemByID(item.Key);
                    if (!WorldPlayerInventory.Instance.ReloadItemQuickSlot(itemInfoData))
                    {
                        Debug.LogError("Reload Error");
                    }
                }
            }
            playerEquipmentManager.LoadMainProjectileEquipment(currentGameData.mainProjectile.GetProjectile());
            playerEquipmentManager.LoadSecondaryProjectileEquipment(currentGameData.secondaryProjectile.GetProjectile());
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

            //  SYNC PROJECTILES
            playerNetworkManager.OnMainProjectileIDChange(0, playerNetworkManager.mainProjectileID.Value);
            playerNetworkManager.OnSecondaryProjectileIDChange(0, playerNetworkManager.secondaryProjectileID.Value);
            playerNetworkManager.OnIsHoldingArrowChanged(false, playerNetworkManager.isHoldingArrow.Value);

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

    }
}
