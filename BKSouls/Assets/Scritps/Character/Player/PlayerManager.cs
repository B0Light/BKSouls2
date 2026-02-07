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

                //  UPDATES UI STAT BARS WHEN A STAT CHANGES (HEALTH OR STAMINA)
                playerNetworkManager.currentHealth.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentFocusPoints.OnValueChanged += GUIController.Instance.playerUIHudManager.SetNewFocusPointValue;
                playerNetworkManager.currentStamina.OnValueChanged += playerStatsManager.ResetStaminaRegenTimer;

                playerNetworkManager.SetNewMaxHealthValue(0, playerNetworkManager.vigor.Value);
                playerNetworkManager.SetNewMaxStaminaValue(0, playerNetworkManager.endurance.Value);
                playerNetworkManager.SetNewMaxFocusPointsValue(0, playerNetworkManager.mind.Value);

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
                LoadGameDataFromCurrentCharacterData(ref WorldSaveGameManager.instance.currentCharacterData);
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

                //  UPDATES UI STAT BARS WHEN A STAT CHANGES (HEALTH OR STAMINA)
                playerNetworkManager.currentHealth.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewHealthValue;
                playerNetworkManager.currentStamina.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewStaminaValue;
                playerNetworkManager.currentFocusPoints.OnValueChanged -= GUIController.Instance.playerUIHudManager.SetNewFocusPointValue;
                playerNetworkManager.currentStamina.OnValueChanged -= playerStatsManager.ResetStaminaRegenTimer;

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

            //  BODY
            currentGameData.hairStyleID = playerNetworkManager.hairStyleID.Value;
            currentGameData.hairColorRed = playerNetworkManager.hairColorRed.Value;
            currentGameData.hairColorGreen = playerNetworkManager.hairColorGreen.Value;
            currentGameData.hairColorBlue = playerNetworkManager.hairColorBlue.Value;

            currentGameData.currentHealthFlasksRemaining = playerNetworkManager.remainingHealthFlasks.Value;
            currentGameData.currentFocusPointsFlaskRemaining = playerNetworkManager.remainingFocusPointsFlasks.Value;

            //  EQUIPMENT
            currentGameData.headEquipment = playerNetworkManager.headEquipmentID.Value;
            currentGameData.bodyEquipment = playerNetworkManager.bodyEquipmentID.Value;
            
            currentGameData.mainProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.mainProjectile);
            currentGameData.secondaryProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.secondaryProjectile);

            if (playerInventoryManager.currentSpell != null)
                currentGameData.currentSpell = playerInventoryManager.currentSpell.itemID;

            //  CLEAR LISTS BEFORE SAVE
            currentGameData.projectilesInInventory = new List<SerializableRangedProjectile>();
            
            
            // Invnetory
            
            currentGameData.backpackItems.Clear();
            currentGameData.inventoryItems.Clear(); 
            currentGameData.safeItems.Clear();
            currentGameData.quickSlotConsumableItems.Clear();
            
            foreach (var pair in WorldPlayerInventory.Instance.GetConsumableInventory().GetCurItemDictById())
            {
                currentGameData.quickSlotConsumableItems.Add(pair.Key, pair.Value);
            }
         
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
            
            // 장비 
            currentGameData.rightWeaponItemCode = playerNetworkManager.currentRightHandWeaponID.Value;
            currentGameData.leftWeaponItemCode = playerNetworkManager.currentLeftHandWeaponID.Value;
            currentGameData.helmetItemCode = playerNetworkManager.headEquipmentID.Value;
            currentGameData.armorItemCode = playerNetworkManager.bodyEquipmentID.Value;
         
            // 저장공간 저장
            //currentGameData.rightWeaponBoxSize = WorldPlayerInventory.Instance.GetRightWeaponInventory().GetCurItemGridSize();
            //currentGameData.leftWeaponBoxSize = WorldPlayerInventory.Instance.GetLeftWeaponInventory().GetCurItemGridSize();
            //currentGameData.helmetBoxSize = WorldPlayerInventory.Instance.GetHelmetInventory().GetCurItemGridSize(); 
            //currentGameData.armorBoxSize = WorldPlayerInventory.Instance.GetArmorInventory().GetCurItemGridSize();
            //currentGameData.consumableBoxSize = WorldPlayerInventory.Instance.GetConsumableInventory().GetCurItemGridSize();
            //currentGameData.backpackSize = WorldPlayerInventory.Instance.GetBackpackInventory().GetCurItemGridSize();
            //currentGameData.inventoryBoxSize = WorldPlayerInventory.Instance.GetInventory().GetCurItemGridSize();
            //currentGameData.safeBoxSize = WorldPlayerInventory.Instance.GetSafeInventory().GetCurItemGridSize();
        }

        public void LoadGameDataFromCurrentCharacterData(ref CharacterSaveData currentCharacterData)
        {
            playerNetworkManager.characterName.Value = currentCharacterData.characterName;
            playerNetworkManager.isMale.Value = currentCharacterData.isMale;
            playerBodyManager.ToggleBodyType(currentCharacterData.isMale); //   TOGGLE INCASE THE VALUE IS THE SAME AS DEFAULT (ONVALUECHANGED ONLY WORKS WHEN VALUE IS CHANGED)
            Vector3 myPosition = new Vector3(currentCharacterData.xPosition, currentCharacterData.yPosition + 5f, currentCharacterData.zPosition);
            transform.position = myPosition;

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
            playerNetworkManager.currentHealth.Value = currentCharacterData.currentHealth;
            playerNetworkManager.currentStamina.Value = currentCharacterData.currentStamina;
            playerNetworkManager.currentFocusPoints.Value = currentCharacterData.currentFocusPoints;
            playerStatsManager.AddRunes(currentCharacterData.runes);

            playerNetworkManager.remainingHealthFlasks.Value = currentCharacterData.currentHealthFlasksRemaining;
            playerNetworkManager.remainingFocusPointsFlasks.Value = currentCharacterData.currentFocusPointsFlaskRemaining;

            //  BODY
            playerNetworkManager.hairStyleID.Value = currentCharacterData.hairStyleID;
            playerNetworkManager.hairColorRed.Value = currentCharacterData.hairColorRed;
            playerNetworkManager.hairColorGreen.Value = currentCharacterData.hairColorGreen;
            playerNetworkManager.hairColorBlue.Value = currentCharacterData.hairColorBlue;

            //  EQUIPMENT
            if (WorldItemDatabase.Instance.GetHeadEquipmentByID(currentCharacterData.headEquipment))
            {
                HeadEquipmentItem headEquipment = Instantiate(WorldItemDatabase.Instance.GetHeadEquipmentByID(currentCharacterData.headEquipment));
                playerInventoryManager.headEquipment = headEquipment;
            }
            else
            {
                playerInventoryManager.headEquipment = null;
            }

            if (WorldItemDatabase.Instance.GetBodyEquipmentByID(currentCharacterData.bodyEquipment))
            {
                BodyEquipmentItem bodyEquipment = Instantiate(WorldItemDatabase.Instance.GetBodyEquipmentByID(currentCharacterData.bodyEquipment));
                playerInventoryManager.bodyEquipment = bodyEquipment;
            }
            else
            {
                playerInventoryManager.bodyEquipment = null;
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
            
            WorldPlayerInventory.Instance.GetHelmetInventory().UpdateItemGridSize(currentCharacterData.helmetBoxSize);
            var helmetItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.helmetItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemHelmet(helmetItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetArmorInventory().UpdateItemGridSize(currentCharacterData.armorBoxSize);
            var armorItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.armorItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemArmor(armorItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetRightWeaponInventory().UpdateItemGridSize(currentCharacterData.rightWeaponBoxSize);
            var rightWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.rightWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemRightWeapon(rightWeaponItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetLeftWeaponInventory().UpdateItemGridSize(currentCharacterData.leftWeaponBoxSize);
            var leftWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.leftWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemLeftWeapon(leftWeaponItem)) Debug.LogError("Reload Error");
            
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

            if (currentCharacterData.backpackSize != Vector2Int.zero)
            {
                WorldPlayerInventory.Instance.GetBackpackInventory().gameObject.SetActive(true);
                WorldPlayerInventory.Instance.GetBackpackInventory().UpdateItemGridSize(currentCharacterData.backpackSize);
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
            // QuickSlot 인벤토리 
            WorldPlayerInventory.Instance.GetConsumableInventory().UpdateItemGridSize(currentCharacterData.consumableBoxSize);
            foreach (KeyValuePair<int,int> item in currentCharacterData.quickSlotConsumableItems)
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
            playerEquipmentManager.LoadMainProjectileEquipment(currentCharacterData.mainProjectile.GetProjectile());
            playerEquipmentManager.LoadSecondaryProjectileEquipment(currentCharacterData.secondaryProjectile.GetProjectile());
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
