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

        public void SaveGameDataToCurrentCharacterData(ref CharacterSaveData currentCharacterData)
        {
            currentCharacterData.sceneIndex = SceneManager.GetActiveScene().buildIndex;

            currentCharacterData.characterName = playerNetworkManager.characterName.Value.ToString();
            currentCharacterData.isMale = playerNetworkManager.isMale.Value;
            currentCharacterData.xPosition = transform.position.x;
            currentCharacterData.yPosition = transform.position.y;
            currentCharacterData.zPosition = transform.position.z;

            //  STATS
            currentCharacterData.currentHealth = playerNetworkManager.currentHealth.Value;
            currentCharacterData.currentStamina = playerNetworkManager.currentStamina.Value;
            currentCharacterData.currentFocusPoints = playerNetworkManager.currentFocusPoints.Value;

            currentCharacterData.vitality = playerNetworkManager.vigor.Value;
            currentCharacterData.endurance = playerNetworkManager.endurance.Value;
            currentCharacterData.mind = playerNetworkManager.mind.Value;
            currentCharacterData.strength = playerNetworkManager.strength.Value;
            currentCharacterData.dexterity = playerNetworkManager.dexterity.Value;
            currentCharacterData.intelligence = playerNetworkManager.intelligence.Value;
            currentCharacterData.faith = playerNetworkManager.faith.Value;

            currentCharacterData.runes = playerStatsManager.runes;

            //  BODY
            currentCharacterData.hairStyleID = playerNetworkManager.hairStyleID.Value;
            currentCharacterData.hairColorRed = playerNetworkManager.hairColorRed.Value;
            currentCharacterData.hairColorGreen = playerNetworkManager.hairColorGreen.Value;
            currentCharacterData.hairColorBlue = playerNetworkManager.hairColorBlue.Value;

            currentCharacterData.currentHealthFlasksRemaining = playerNetworkManager.remainingHealthFlasks.Value;
            currentCharacterData.currentFocusPointsFlaskRemaining = playerNetworkManager.remainingFocusPointsFlasks.Value;

            //  EQUIPMENT
            currentCharacterData.headEquipment = playerNetworkManager.headEquipmentID.Value;
            currentCharacterData.bodyEquipment = playerNetworkManager.bodyEquipmentID.Value;
            currentCharacterData.legEquipment = playerNetworkManager.legEquipmentID.Value;
            currentCharacterData.handEquipment = playerNetworkManager.handEquipmentID.Value;

            currentCharacterData.rightWeaponIndex = playerInventoryManager.rightHandWeaponIndex;
            currentCharacterData.rightWeapon01 = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(playerInventoryManager.weaponsInRightHandSlots[0]); //   THIS SHOULD NEVER BE NULL (it should always default to unarmed)
            currentCharacterData.rightWeapon02 = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(playerInventoryManager.weaponsInRightHandSlots[1]); //   THIS SHOULD NEVER BE NULL (it should always default to unarmed)
            currentCharacterData.rightWeapon03 = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(playerInventoryManager.weaponsInRightHandSlots[2]); //   THIS SHOULD NEVER BE NULL (it should always default to unarmed)

            currentCharacterData.leftWeaponIndex = playerInventoryManager.leftHandWeaponIndex;
            currentCharacterData.leftWeapon01 = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(playerInventoryManager.weaponsInLeftHandSlots[0]); //   THIS SHOULD NEVER BE NULL (it should always default to unarmed)
            currentCharacterData.leftWeapon02 = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(playerInventoryManager.weaponsInLeftHandSlots[1]); //   THIS SHOULD NEVER BE NULL (it should always default to unarmed)
            currentCharacterData.leftWeapon03 = WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(playerInventoryManager.weaponsInLeftHandSlots[2]); //   THIS SHOULD NEVER BE NULL (it should always default to unarmed)

            currentCharacterData.quickSlotIndex = playerInventoryManager.quickSlotItemIndex;
            currentCharacterData.quickSlotItem01 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemsInQuickSlots[0]);
            currentCharacterData.quickSlotItem02 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemsInQuickSlots[1]);
            currentCharacterData.quickSlotItem03 = WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(playerInventoryManager.quickSlotItemsInQuickSlots[2]);

            currentCharacterData.mainProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.mainProjectile);
            currentCharacterData.secondaryProjectile = WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(playerInventoryManager.secondaryProjectile);

            if (playerInventoryManager.currentSpell != null)
                currentCharacterData.currentSpell = playerInventoryManager.currentSpell.itemID;

            //  CLEAR LISTS BEFORE SAVE
            currentCharacterData.weaponsInInventory = new List<SerializableWeapon>();
            currentCharacterData.projectilesInInventory = new List<SerializableRangedProjectile>();
            currentCharacterData.quickSlotItemsInInventory = new List<SerilalizableQuickSlotItem>();
            currentCharacterData.headEquipmentInInventory = new List<int>();
            currentCharacterData.bodyEquipmentInInventory = new List<int>();
            currentCharacterData.handEquipmentInInventory = new List<int>();
            currentCharacterData.legEquipmentInInventory = new List<int>();

            for (int i = 0; i < playerInventoryManager.itemsInInventory.Count; i++)
            {
                if (playerInventoryManager.itemsInInventory[i] == null)
                    continue;

                WeaponItem weaponInInventory = playerInventoryManager.itemsInInventory[i] as WeaponItem;
                HeadEquipmentItem headEquipmentInInventory = playerInventoryManager.itemsInInventory[i] as HeadEquipmentItem;
                BodyEquipmentItem bodyEquipmentInInventory = playerInventoryManager.itemsInInventory[i] as BodyEquipmentItem;
                LegEquipmentItem legEquipmentInInventory = playerInventoryManager.itemsInInventory[i] as LegEquipmentItem;
                HandEquipmentItem handEquipmentInInventory = playerInventoryManager.itemsInInventory[i] as HandEquipmentItem;
                QuickSlotItem quickSlotItemInInventory = playerInventoryManager.itemsInInventory[i] as QuickSlotItem;
                RangedProjectileItem projectileInInventory = playerInventoryManager.itemsInInventory[i] as RangedProjectileItem;

                if (weaponInInventory != null)
                    currentCharacterData.weaponsInInventory.Add(WorldSaveGameManager.instance.GetSerializableWeaponFromWeaponItem(weaponInInventory));

                if (headEquipmentInInventory != null)
                    currentCharacterData.headEquipmentInInventory.Add(headEquipmentInInventory.itemID);

                if (bodyEquipmentInInventory != null)
                    currentCharacterData.bodyEquipmentInInventory.Add(bodyEquipmentInInventory.itemID);

                if (legEquipmentInInventory != null)
                    currentCharacterData.legEquipmentInInventory.Add(legEquipmentInInventory.itemID);

                if (handEquipmentInInventory != null)
                    currentCharacterData.handEquipmentInInventory.Add(handEquipmentInInventory.itemID);

                if (quickSlotItemInInventory != null)
                    currentCharacterData.quickSlotItemsInInventory.Add(WorldSaveGameManager.instance.GetSerializableQuickSlotItemFromQuickSlotItem(quickSlotItemInInventory));

                if (projectileInInventory != null)
                    currentCharacterData.projectilesInInventory.Add(WorldSaveGameManager.instance.GetSerializableRangedProjectileFromRangedProjectileItem(projectileInInventory));
            }
        }

        public void LoadGameDataFromCurrentCharacterData(ref CharacterSaveData currentCharacterData)
        {
            playerNetworkManager.characterName.Value = currentCharacterData.characterName;
            playerNetworkManager.isMale.Value = currentCharacterData.isMale;
            playerBodyManager.ToggleBodyType(currentCharacterData.isMale); //   TOGGLE INCASE THE VALUE IS THE SAME AS DEFAULT (ONVALUECHANGED ONLY WORKS WHEN VALUE IS CHANGED)
            Vector3 myPosition = new Vector3(currentCharacterData.xPosition, currentCharacterData.yPosition, currentCharacterData.zPosition);
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

            if (WorldItemDatabase.Instance.GetHandEquipmentByID(currentCharacterData.handEquipment))
            {
                HandEquipmentItem handEquipment = Instantiate(WorldItemDatabase.Instance.GetHandEquipmentByID(currentCharacterData.handEquipment));
                playerInventoryManager.handEquipment = handEquipment;
            }
            else
            {
                playerInventoryManager.handEquipment = null;
            }

            if (WorldItemDatabase.Instance.GetLegEquipmentByID(currentCharacterData.legEquipment))
            {
                LegEquipmentItem legEquipment = Instantiate(WorldItemDatabase.Instance.GetLegEquipmentByID(currentCharacterData.legEquipment));
                playerInventoryManager.legEquipment = legEquipment;
            }
            else
            {
                playerInventoryManager.legEquipment = null;
            }

            //  WEAPONS
            playerInventoryManager.rightHandWeaponIndex = currentCharacterData.rightWeaponIndex;
            playerInventoryManager.weaponsInRightHandSlots[0] = currentCharacterData.rightWeapon01.GetWeapon();
            playerInventoryManager.weaponsInRightHandSlots[1] = currentCharacterData.rightWeapon02.GetWeapon();
            playerInventoryManager.weaponsInRightHandSlots[2] = currentCharacterData.rightWeapon03.GetWeapon();
            playerInventoryManager.leftHandWeaponIndex = currentCharacterData.leftWeaponIndex;
            playerInventoryManager.weaponsInLeftHandSlots[0] = currentCharacterData.leftWeapon01.GetWeapon();
            playerInventoryManager.weaponsInLeftHandSlots[1] = currentCharacterData.leftWeapon02.GetWeapon();
            playerInventoryManager.weaponsInLeftHandSlots[2] = currentCharacterData.leftWeapon03.GetWeapon();

            //  QUICK SLOT ITEMS
            playerInventoryManager.quickSlotItemIndex = currentCharacterData.quickSlotIndex;
            playerInventoryManager.quickSlotItemsInQuickSlots[0] = currentCharacterData.quickSlotItem01.GetQuickSlotItem();
            playerInventoryManager.quickSlotItemsInQuickSlots[1] = currentCharacterData.quickSlotItem02.GetQuickSlotItem();
            playerInventoryManager.quickSlotItemsInQuickSlots[2] = currentCharacterData.quickSlotItem03.GetQuickSlotItem();
            playerEquipmentManager.LoadQuickSlotEquipment(playerInventoryManager.quickSlotItemsInQuickSlots[playerInventoryManager.quickSlotItemIndex]); // THIS REFRESHES THE HUD

            if (currentCharacterData.rightWeaponIndex >= 0)
            {
                playerInventoryManager.currentRightHandWeapon = playerInventoryManager.weaponsInRightHandSlots[currentCharacterData.rightWeaponIndex];
                playerNetworkManager.currentRightHandWeaponID.Value = playerInventoryManager.weaponsInRightHandSlots[currentCharacterData.rightWeaponIndex].itemID;
            }
            else
            {
                playerNetworkManager.currentRightHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
            }

            playerInventoryManager.leftHandWeaponIndex = currentCharacterData.leftWeaponIndex;

            if (currentCharacterData.leftWeaponIndex >= 0)
            {
                playerInventoryManager.currentLeftHandWeapon = playerInventoryManager.weaponsInLeftHandSlots[currentCharacterData.leftWeaponIndex];
                playerNetworkManager.currentLeftHandWeaponID.Value = playerInventoryManager.weaponsInLeftHandSlots[currentCharacterData.leftWeaponIndex].itemID;
            }
            else
            {
                playerNetworkManager.currentLeftHandWeaponID.Value = WorldItemDatabase.Instance.unarmedWeapon.itemID;
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

            for (int i = 0; i < currentCharacterData.weaponsInInventory.Count; i++)
            {
                WeaponItem weapon = currentCharacterData.weaponsInInventory[i].GetWeapon();
                playerInventoryManager.AddItemToInventory(weapon);
            }

            for (int i = 0; i < currentCharacterData.headEquipmentInInventory.Count; i++)
            {
                EquipmentItem equipment = WorldItemDatabase.Instance.GetHeadEquipmentByID(currentCharacterData.headEquipmentInInventory[i]);
                playerInventoryManager.AddItemToInventory(equipment);
            }

            for (int i = 0; i < currentCharacterData.bodyEquipmentInInventory.Count; i++)
            {
                EquipmentItem equipment = WorldItemDatabase.Instance.GetBodyEquipmentByID(currentCharacterData.bodyEquipmentInInventory[i]);
                playerInventoryManager.AddItemToInventory(equipment);
            }

            for (int i = 0; i < currentCharacterData.legEquipmentInInventory.Count; i++)
            {
                EquipmentItem equipment = WorldItemDatabase.Instance.GetLegEquipmentByID(currentCharacterData.legEquipmentInInventory[i]);
                playerInventoryManager.AddItemToInventory(equipment);
            }

            for (int i = 0; i < currentCharacterData.handEquipmentInInventory.Count; i++)
            {
                EquipmentItem equipment = WorldItemDatabase.Instance.GetHandEquipmentByID(currentCharacterData.handEquipmentInInventory[i]);
                playerInventoryManager.AddItemToInventory(equipment);
            }

            for (int i = 0; i < currentCharacterData.quickSlotItemsInInventory.Count; i++)
            {
                QuickSlotItem quickSlotItem = currentCharacterData.quickSlotItemsInInventory[i].GetQuickSlotItem();
                playerInventoryManager.AddItemToInventory(quickSlotItem);
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
            if (!WorldPlayerInventory.Instance.ReloadItemHelmet(armorItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetRightWeaponInventory().UpdateItemGridSize(currentCharacterData.rightWeaponBoxSize);
            var rightWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.rightWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemHelmet(rightWeaponItem)) Debug.LogError("Reload Error");
            
            WorldPlayerInventory.Instance.GetLeftWeaponInventory().UpdateItemGridSize(currentCharacterData.leftWeaponBoxSize);
            var leftWeaponItem = WorldItemDatabase.Instance.GetItemByID(currentCharacterData.leftWeaponItemCode);
            if (!WorldPlayerInventory.Instance.ReloadItemHelmet(leftWeaponItem)) Debug.LogError("Reload Error");
            
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
            
            playerEquipmentManager.EquipArmor();
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
            playerNetworkManager.OnLegEquipmentChanged(0, playerNetworkManager.legEquipmentID.Value);
            playerNetworkManager.OnHandEquipmentChanged(0, playerNetworkManager.handEquipmentID.Value);

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
