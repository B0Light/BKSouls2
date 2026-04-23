using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEngine.Serialization;

namespace BK
{
    public class PlayerInputManager : Singleton<PlayerInputManager>
    {
        //  INPUT CONTROLS
        private PlayerControls playerControls;

        //  LOCAL PLAYER
        public PlayerManager player;

        [Header("Camera Movement Input")]
        [SerializeField] Vector2 camera_Input;
        public float cameraVertical_Input;
        public float cameraHorizontal_Input;

        [Header("Lock On Input")]
        [SerializeField] bool lockOn_Input;
        [SerializeField] bool lockOn_Left_Input;
        [SerializeField] bool lockOn_Right_Input;
        private Coroutine lockOnCoroutine;

        [Header("Player Movement Input")]
        [SerializeField] Vector2 movementInput;
        public float vertical_Input;
        public float horizontal_Input;
        public float moveAmount;

        [Header("Player Action Input")]
        [SerializeField] bool dodge_Input = false;
        [SerializeField] bool sprint_Input = false;
        [SerializeField] bool jump_Input = false;
        [SerializeField] bool switch_Right_Weapon_Input = false;
        [SerializeField] bool switch_Left_Weapon_Input = false;
        [SerializeField] bool switch_Quick_Slot_Item_Input = false;
        [SerializeField] bool interaction_Input = false;
        [SerializeField] bool use_Item_Input = false;

        [Header("Bumper Inputs")]
        [SerializeField] bool RB_Input = false;
        [SerializeField] bool hold_RB_Input = false;
        [SerializeField] bool LB_Input = false;
        [SerializeField] bool hold_LB_Input = false;

        [Header("Trigger Inputs")]
        [SerializeField] bool RT_Input = false;
        [SerializeField] bool Hold_RT_Input = false;
        [SerializeField] bool LT_Input = false;

        [Header("Two Hand Inputs")]
        [SerializeField] bool two_Hand_Input = false;
        [SerializeField] bool two_Hand_Right_Weapon_Input = false;
        [SerializeField] bool two_Hand_Left_Weapon_Input = false;

        [Header("QUED INPUTS")]
        [SerializeField] private bool input_Que_Is_Active = false;
        [SerializeField] float default_Que_Input_Time = 0.35f;
        [SerializeField] float que_Input_Timer = 0;
        [SerializeField] bool que_RB_Input = false;
        [SerializeField] bool que_RT_Input = false;

        [Header("UI INPUTS")]
        private Vector2 _mousePos;
        [SerializeField] bool openCharacterMenuInput = false;
        [SerializeField] bool closeMenuInput = false;
        

        private void Start()
        {
            //  WHEN THE SCENE CHANGES, RUN THIS LOGIC
            SceneManager.activeSceneChanged += OnSceneChange;

            Instance.enabled = false;

            if (playerControls != null)
            {
                playerControls.Disable();
            }
        }

        private void OnSceneChange(Scene oldScene, Scene newScene)
        {
            //  IF WE ARE LOADING INTO OUR WORLD SCENE, ENABLE OUR PLAYERS CONTROLS
            if (newScene.buildIndex == 0)
            {
                Instance.enabled = false;

                if (playerControls != null)
                {
                    playerControls.Disable();
                }
            }
            else
            {
                Instance.enabled = true;

                if (playerControls != null)
                {
                    playerControls.Enable();
                }
            }
        }

        // GridBuild 등 외부 UI 사용 중 플레이어 액션 입력 차단/복구
        public void DisablePlayerActions()
        {
            if (playerControls == null) return;
            playerControls.PlayerActions.Disable();
            playerControls.PlayerMovement.Disable();
        }

        public void EnablePlayerActions()
        {
            if (playerControls == null) return;
            playerControls.PlayerActions.Enable();
            playerControls.PlayerMovement.Enable();
        }

        private void OnEnable()
        {
            if (playerControls == null)
            {
                playerControls = new PlayerControls();

                playerControls.PlayerMovement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
                playerControls.PlayerCamera.Movement.performed += i => camera_Input = i.ReadValue<Vector2>();

                //  ACTIONS
                playerControls.PlayerActions.Dodge.performed += i => dodge_Input = true;
                playerControls.PlayerActions.Jump.performed += i => jump_Input = true;
                playerControls.PlayerActions.SwitchRightWeapon.performed += i => switch_Right_Weapon_Input = true;
                playerControls.PlayerActions.SwitchLeftWeapon.performed += i => switch_Left_Weapon_Input = true;
                playerControls.PlayerActions.SwitchQuickSlotItem.performed += i => switch_Quick_Slot_Item_Input = true;
                playerControls.PlayerActions.Interact.performed += i => interaction_Input = true;
                playerControls.PlayerActions.X.performed += i => use_Item_Input = true;

                //  BUMPERS
                playerControls.PlayerActions.RB.performed += i => RB_Input = true;
                playerControls.PlayerActions.HoldRB.performed += i => hold_RB_Input = true;
                playerControls.PlayerActions.HoldRB.canceled += i => hold_RB_Input = false;

                playerControls.PlayerActions.LB.performed += i => LB_Input = true;
                playerControls.PlayerActions.LB.canceled += i => player.playerNetworkManager.isBlocking.Value = false;
                playerControls.PlayerActions.LB.canceled += i => player.playerNetworkManager.isAiming.Value = false;
                playerControls.PlayerActions.HoldLB.performed += i => hold_LB_Input = true;
                playerControls.PlayerActions.HoldLB.canceled += i => hold_LB_Input = false;

                //  TRIGGERS
                playerControls.PlayerActions.RT.performed += i => RT_Input = true;
                playerControls.PlayerActions.HoldRT.performed += i => Hold_RT_Input = true;
                playerControls.PlayerActions.HoldRT.canceled += i => Hold_RT_Input = false;
                playerControls.PlayerActions.LT.performed += i => LT_Input = true;

                //  TWO HAND
                playerControls.PlayerActions.TwoHandWeapon.performed += i => two_Hand_Input = true;
                playerControls.PlayerActions.TwoHandWeapon.canceled += i => two_Hand_Input = false;
                playerControls.PlayerActions.TwoHandRightWeapon.performed += i => two_Hand_Right_Weapon_Input = true;
                playerControls.PlayerActions.TwoHandRightWeapon.canceled += i => two_Hand_Right_Weapon_Input = false;
                playerControls.PlayerActions.TwoHandLeftWeapon.performed += i => two_Hand_Left_Weapon_Input = true;
                playerControls.PlayerActions.TwoHandLeftWeapon.canceled += i => two_Hand_Left_Weapon_Input = false;

                //  LOCK ON
                playerControls.PlayerActions.LockOn.performed += i => lockOn_Input = true;
                playerControls.PlayerActions.SeekLeftLockOnTarget.performed += i => lockOn_Left_Input = true;
                playerControls.PlayerActions.SeekRightLockOnTarget.performed += i => lockOn_Right_Input = true;

                //  HOLDING THE INPUT, SETS THE BOOL TO TRUE
                playerControls.PlayerActions.Sprint.performed += i => sprint_Input = true;
                //  RELEASING THE INPUT, SETS THE BOOL TO FALSE
                playerControls.PlayerActions.Sprint.canceled += i => sprint_Input = false;

                //  QUED INPUTS
                playerControls.PlayerActions.QueRB.performed += i => QueInput(ref que_RB_Input);
                playerControls.PlayerActions.QueRT.performed += i => QueInput(ref que_RT_Input);

                //  UI INPUTS
                playerControls.PlayerActions.Dodge.performed += i => closeMenuInput = true;
                playerControls.PlayerActions.OpenCharacterMenu.performed += i => openCharacterMenuInput = true;
            }

            playerControls.Enable();
        }

        private void OnDestroy()
        {
            //  IF WE DESTROY THIS OBJECT, UNSUBSCRIBE FROM THIS EVENT
            SceneManager.activeSceneChanged -= OnSceneChange;
        }

        //  IF WE MINIMIZE OR LOWER THE WINDOW, STOP ADJUSTING INPUTS
        private void OnApplicationFocus(bool focus)
        {
            if (enabled)
            {
                if (focus)
                {
                    playerControls.Enable();
                }
                else
                {
                    playerControls.Disable();
                }
            }
        }

        private void Update()
        {
            if(GUIController.Instance.currentOpenGUI == null)
                HandleAllInputs();
        }

        private void HandleAllInputs()
        {
            HandleUseItemInput();
            HandleTwoHandInput();
            HandleLockOnInput();
            HandleLockOnSwitchTargetInput();
            HandlePlayerMovementInput();
            HandleCameraMovementInput();
            HandleDodgeInput();
            HandleSprintInput();
            HandleJumpInput();
            HandleRBInput();
            HandleHoldRBInput();
            HandleLBInput();
            HandleHoldLBInput();
            HandleRTInput();
            HandleChargeRTInput();
            HandleLTInput();
            HandleSwitchRightWeaponInput();
            HandleSwitchLeftWeaponInput();
            HandleSwitchQuickSlotItemInput();
            HandleQuedInputs();
            HandleInteractionInput();
            HandleCloseUIInput();
            HandleOpenCharacterMenuInput();
        }

        //  USE ITEM
        private void HandleUseItemInput()
        {
            if (use_Item_Input)
            {
                use_Item_Input = false;

                if (GUIController.Instance.menuWindowIsOpen)
                    return;

                if (player.playerInventoryManager.currentQuickSlotItem != null)
                {
                    player.playerInventoryManager.currentQuickSlotItem.AttemptToUseItem(player);

                    //  SEND SERVER RPC SO OUR PLAYER PERFORMS ITEM ACTION ON OTHER CLIENTS GAME WINDOWS
                    player.playerNetworkManager.NotifyServerOfQuickSlotItemActionServerRpc
                        (NetworkManager.Singleton.LocalClientId, player.playerInventoryManager.currentQuickSlotItem.itemID);
                }
            }
        }

        //  TWO HAND
        private void HandleTwoHandInput()
        {
            if (!two_Hand_Input)
                return;

            if (two_Hand_Right_Weapon_Input)
            {
                //  IF WE ARE USING THE TWO HAND INPUT AND PRESSING THE RIGHT TWO HAND BUTTON WE WANT TO STOP THE REGULAR RB INPUT (OR ELSE WE WOULD ATTACK)
                RB_Input = false;
                two_Hand_Right_Weapon_Input = false;
                player.playerNetworkManager.isBlocking.Value = false;

                if (player.playerNetworkManager.isTwoHandingWeapon.Value)
                {
                    //  IF WE ARE TWO HANDING A WEAPON ALREADY, CHANGE THE IS TWOHANDING BOOL TO FALSE WHICH TRIGGERS AN "ONVALUECHANGED" FUNCTION,
                    //  WHICH UN-TWOHANDS CURRENT WEAPON
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                    return;
                }
                else
                {
                    //  IF WE ARE NOT ALREADY TWO HANGING, CHANGE THE RIGHT TWO HAND BOOL TO TRUE, WHICH TRIGGERS AN ONVALUECHANGED FUNCTION
                    //  THIS FUNCTION TWO HANDS THE RIGHT WEAPON
                    player.playerNetworkManager.isTwoHandingRightWeapon.Value = true;
                    return;
                }
            }
            else if (two_Hand_Left_Weapon_Input)
            {
                //  IF WE ARE USING THE TWO HAND INPUT AND PRESSING THE RIGHT TWO HAND BUTTON WE WANT TO STOP THE REGULAR RB INPUT (OR ELSE WE WOULD ATTACK)
                LB_Input = false;
                two_Hand_Left_Weapon_Input = false;
                player.playerNetworkManager.isBlocking.Value = false;

                if (player.playerNetworkManager.isTwoHandingWeapon.Value)
                {
                    //  IF WE ARE TWO HANDING A WEAPON ALREADY, CHANGE THE IS TWOHANDING BOOL TO FALSE WHICH TRIGGERS AN "ONVALUECHANGED" FUNCTION,
                    //  WHICH UN-TWOHANDS CURRENT WEAPON
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                    return;
                }
                else
                {
                    //  IF WE ARE NOT ALREADY TWO HANGING, CHANGE THE RIGHT TWO HAND BOOL TO TRUE, WHICH TRIGGERS AN ONVALUECHANGED FUNCTION
                    //  THIS FUNCTION TWO HANDS THE RIGHT WEAPON
                    player.playerNetworkManager.isTwoHandingLeftWeapon.Value = true;
                    return;
                }
            }
        }

        //  LOCK ON
        private void HandleLockOnInput()
        {
            //  CHECK FOR DEAD TARGET
            if (player.playerNetworkManager.isLockedOn.Value)
            {
                if (player.playerCombatManager.currentTarget == null)
                    return;

                //  ONLY SEARCH FOR A NEW TARGET WHEN THE CURRENT ONE DIES
                if (player.playerCombatManager.currentTarget.isDead.Value)
                {
                    player.playerNetworkManager.isLockedOn.Value = false;

                    if (lockOnCoroutine != null)
                        StopCoroutine(lockOnCoroutine);

                    lockOnCoroutine = StartCoroutine(PlayerCamera.Instance.WaitThenFindNewTarget());
                }
            }


            if (lockOn_Input && player.playerNetworkManager.isLockedOn.Value)
            {
                lockOn_Input = false;
                PlayerCamera.Instance.ClearLockOnTargets();
                player.playerNetworkManager.isLockedOn.Value = false;
                //  DISABLE LOCK ON
                return;
            }

            if (lockOn_Input && !player.playerNetworkManager.isLockedOn.Value)
            {
                lockOn_Input = false;

                //  활을 장착 중이면 록온 불가
                WeaponItem rightWeapon = player.playerInventoryManager.currentRightHandWeapon;
                if (rightWeapon != null && rightWeapon.weaponClass == WeaponClass.Bow)
                    return;

                PlayerCamera.Instance.HandleLocatingLockOnTargets();

                if (PlayerCamera.Instance.nearestLockOnTarget != null)
                {
                    player.playerCombatManager.SetTarget(PlayerCamera.Instance.nearestLockOnTarget);
                    player.playerNetworkManager.isLockedOn.Value = true;
                }
            }
        }

        private void HandleLockOnSwitchTargetInput()
        {
            if (lockOn_Left_Input)
            {
                lockOn_Left_Input = false;

                if (player.playerNetworkManager.isLockedOn.Value)
                {
                    PlayerCamera.Instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.Instance.leftLockOnTarget != null)
                    {
                        player.playerCombatManager.SetTarget(PlayerCamera.Instance.leftLockOnTarget);
                    }
                }
            }

            if (lockOn_Right_Input)
            {
                lockOn_Right_Input = false;

                if (player.playerNetworkManager.isLockedOn.Value)
                {
                    PlayerCamera.Instance.HandleLocatingLockOnTargets();

                    if (PlayerCamera.Instance.rightLockOnTarget != null)
                    {
                        player.playerCombatManager.SetTarget(PlayerCamera.Instance.rightLockOnTarget);
                    }
                }
            }
        }

        //  MOVEMENT
        private void HandlePlayerMovementInput()
        {
            vertical_Input = movementInput.y;
            horizontal_Input = movementInput.x;

            //  RETURNS THE ABSOLUTE NUMBER, (Meaning number without the negative sign, so its always positive)
            moveAmount = Mathf.Clamp01(Mathf.Abs(vertical_Input) + Mathf.Abs(horizontal_Input));

            //  WE CLAMP THE VALUES, SO THEY ARE 0, 0.5 OR 1 (OPTIONAL)
            if (moveAmount <= 0.5 && moveAmount > 0)
            {
                moveAmount = 0.5f;
            }
            else if (moveAmount > 0.5 && moveAmount <= 1)
            {
                moveAmount = 1;
            }

            // WHY DO WE PASS 0 ON THE HORIZONTAL? BECAUSE WE ONLY WANT NON-STRAFING MOVEMENT
            // WE USE THE HORIZONTAL WHEN WE ARE STRAFING OR LOCKED ON

            if (player == null)
                return;

            if (moveAmount != 0)
            {
                player.playerNetworkManager.isMoving.Value = true;
            }
            else
            {
                player.playerNetworkManager.isMoving.Value = false;
            }

            if (!player.playerLocomotionManager.canRun)
            {
                if (moveAmount > 0.5f)
                    moveAmount = 0.5f;

                if (vertical_Input > 0.5f)
                    vertical_Input = 0.5f;

                if (horizontal_Input > 0.5f)
                    horizontal_Input = 0.5f;
            }

            if (player.playerNetworkManager.isLockedOn.Value && !player.playerNetworkManager.isSprinting.Value)
            {
                //  IF WE ARE LOCKED ON PASS THE HORIZONTAL MOVEMENT AS WELL
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontal_Input, vertical_Input, player.playerNetworkManager.isSprinting.Value);
                return;
            }

            if (player.playerNetworkManager.isAiming.Value)
            {
                //  IF WE ARE LOCKED ON PASS THE HORIZONTAL MOVEMENT AS WELL
                player.playerAnimatorManager.UpdateAnimatorMovementParameters(horizontal_Input, vertical_Input, player.playerNetworkManager.isSprinting.Value);
                return;
            }

            //  IF WE ARE NOT LOCKED ON, ONLY USE THE MOVE AMOUNT
            player.playerAnimatorManager.UpdateAnimatorMovementParameters(0, moveAmount, player.playerNetworkManager.isSprinting.Value);
        }

        private void HandleCameraMovementInput()
        {
            cameraVertical_Input = camera_Input.y;
            cameraHorizontal_Input = camera_Input.x;
        }

        //  ACTION
        private void HandleDodgeInput()
        {
            if (dodge_Input)
            {
                dodge_Input = false;

                //  DO NOTHING IF MENU OR UI WINDOW IS OPEN
                if (GUIController.Instance.menuWindowIsOpen)
                    return;

                // 활 시위 중 구르기 불가
                if (player.playerNetworkManager.isHoldingArrow.Value)
                    return;

                player.playerLocomotionManager.AttemptToPerformDodge();
            }
        }

        private void HandleSprintInput()
        {
            if (sprint_Input)
            {
                player.playerLocomotionManager.HandleSprinting();
            }
            else
            {
                player.playerNetworkManager.isSprinting.Value = false;
            }
        }

        private void HandleJumpInput()
        {
            if (jump_Input)
            {
                jump_Input = false;

                //  IF WE HAVE A UI WINDOW OPEN, SIMPLY RETURN WITHOUT DOING ANYTHING
                if (GUIController.Instance.menuWindowIsOpen)
                    return;

                // 활 시위 중 점프 불가
                if (player.playerNetworkManager.isHoldingArrow.Value)
                    return;

                //  ATTEMPT TO PERFORM JUMP
                player.playerLocomotionManager.AttemptToPerformJump();
            }
        }

        private void HandleRBInput()
        {
            if (two_Hand_Input)
                return;

            if (RB_Input)
            {
                RB_Input = false;

                //  TODO: IF WE HAVE A UI WINDOW OPEN, RETURN AND DO NOTHING

                player.playerNetworkManager.SetCharacterActionHand(true);

                //  TODO: IF WE ARE TWO HANDING THE WEAPON, USE THE TWO HANDED ACTION

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RB_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleHoldRBInput()
        {
            if (hold_RB_Input)
            {
                player.playerNetworkManager.isChargingRightSpell.Value = true;
                player.playerNetworkManager.isHoldingArrow.Value = true;
            }
            else
            {
                player.playerNetworkManager.isChargingRightSpell.Value = false;
                player.playerNetworkManager.isHoldingArrow.Value = false;
            }
        }

        private void HandleLBInput()
        {
            if (two_Hand_Input)
                return;

            if (LB_Input)
            {
                LB_Input = false;

                //  TODO: IF WE HAVE A UI WINDOW OPEN, RETURN AND DO NOTHING

                player.playerNetworkManager.SetCharacterActionHand(false);

                //  TODO: IF WE ARE TWO HANDING THE WEAPON, USE THE TWO HANDED ACTION
                if (player.playerNetworkManager.isTwoHandingRightWeapon.Value)
                {
                    player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_LB_Action, player.playerInventoryManager.currentRightHandWeapon);
                }
                else
                {
                    player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentLeftHandWeapon.oh_LB_Action, player.playerInventoryManager.currentLeftHandWeapon);
                }
            }
        }

        private void HandleHoldLBInput()
        {
            if (hold_LB_Input)
            {
                player.playerNetworkManager.isChargingLeftSpell.Value = true;
            }
            else
            {
                player.playerNetworkManager.isChargingLeftSpell.Value = false;
            }
        }

        private void HandleRTInput()
        {
            if (RT_Input)
            {
                RT_Input = false;

                //  TODO: IF WE HAVE A UI WINDOW OPEN, RETURN AND DO NOTHING

                player.playerNetworkManager.SetCharacterActionHand(true);

                //  TODO: IF WE ARE TWO HANDING THE WEAPON, USE THE TWO HANDED ACTION

                player.playerCombatManager.PerformWeaponBasedAction(player.playerInventoryManager.currentRightHandWeapon.oh_RT_Action, player.playerInventoryManager.currentRightHandWeapon);
            }
        }

        private void HandleChargeRTInput()
        {
            //  WE ONLY WANT TO CHECK FOR A CHARGE IF WE ARE IN AN ACTION THAT REQUIRES IT (Attacking)
            if (player.isPerformingAction)
            {
                if (player.playerNetworkManager.isUsingRightHand.Value)
                {
                    player.playerNetworkManager.isChargingAttack.Value = Hold_RT_Input;
                }
            }
        }

        private void HandleLTInput()
        {
            if (LT_Input)
            {
                LT_Input = false;

                WeaponItem weaponPerformingAshOfWar = player.playerCombatManager.SelectWeaponToPerformAshOfWar();

                weaponPerformingAshOfWar?.ashOfWarAction?.AttemptToPerformAction(player);
            }
        }
        
        private void HandleSwitchRightWeaponInput()
        {
            if (switch_Right_Weapon_Input)
            {
                switch_Right_Weapon_Input = false;

                if (GUIController.Instance.menuWindowIsOpen)
                    return;

                if (player.isPerformingAction)
                    return;

                // 활 시위 중 무기 교체 불가
                if (player.playerNetworkManager.isHoldingArrow.Value)
                    return;

                if (player.playerCombatManager.isUsingItem)
                    return;

                player.playerEquipmentManager.SwitchRightWeapon();
            }
        }

        private void HandleSwitchLeftWeaponInput()
        {
            if (switch_Left_Weapon_Input)
            {
                switch_Left_Weapon_Input = false;

                if (GUIController.Instance.menuWindowIsOpen)
                    return;

                if (player.isPerformingAction)
                    return;

                // 활 시위 중 무기 교체 불가
                if (player.playerNetworkManager.isHoldingArrow.Value)
                    return;

                if (player.playerCombatManager.isUsingItem)
                    return;

                player.playerEquipmentManager.SwitchLeftWeapon();
            }
        }

        private void HandleSwitchQuickSlotItemInput()
        {
            if (switch_Quick_Slot_Item_Input)
            {
                switch_Quick_Slot_Item_Input = false;

                if (GUIController.Instance.menuWindowIsOpen)
                    return;

                if (player.isPerformingAction)
                    return;

                if (player.playerCombatManager.isUsingItem)
                    return;
                
                player.playerEquipmentManager.SwitchQuickSlotItem();
            }
        }

        private void HandleInteractionInput()
        {
            if (interaction_Input)
            {
                interaction_Input = false;

                player.playerInteractionManager.Interact();
            }
        }

        private void QueInput(ref bool quedInput)   //  PASSING A REFERENCE MEANS WE PASS A SPECIFIC BOOL, AND NOT THE VALUE OF THAT BOOL (TRUE OR FALSE)
        {
            //  RESET ALL QUED INPUTS SO ONLY ONE CAN QUE AT A TIME
            que_RB_Input = false;
            que_RT_Input = false;
            //que_LB_Input = false;
            //que_LT_Input = false;

            //  CHECK FOR UI WINDOW BEING OPEN, IF ITS OPEN RETURN

            if (player.isPerformingAction || player.playerNetworkManager.isJumping.Value)
            {
                quedInput = true;
                que_Input_Timer = default_Que_Input_Time;
                input_Que_Is_Active = true;
            }
        }

        private void ProcessQuedInput()
        {
            if (player.isDead.Value)
                return;

            if (que_RB_Input)
                RB_Input = true;

            if (que_RT_Input)
                RT_Input = true;
        }

        private void HandleQuedInputs()
        {
            if (input_Que_Is_Active)
            {
                //  WHILE THE TIMER IS ABOVE 0, KEEP ATTEMPTING TO PRESS THE INPUT
                if (que_Input_Timer > 0)
                {
                    que_Input_Timer -= Time.deltaTime;
                    ProcessQuedInput();
                }
                else
                {
                    //  RESET ALL QUED INPUTS
                    que_RB_Input = false;
                    que_RT_Input = false;

                    input_Que_Is_Active = false;
                    que_Input_Timer = 0;
                }
            }
        }

        private void HandleOpenCharacterMenuInput()
        {
            if (openCharacterMenuInput)
            {
                openCharacterMenuInput = false;

                //PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUpWindows();
                //PlayerUIManager.instance.CloseAllMenuWindows();
                StartCoroutine(WaitThenOpenMenu());
            }
        }
        
        private IEnumerator WaitThenOpenMenu()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            //PlayerUIManager.instance.playerUICharacterMenuManager.OpenMenu();
            GUIController.Instance.HandleTab();
        }

        private void HandleCloseUIInput()
        {
            if (closeMenuInput)
            {
                closeMenuInput = false;

                if (GUIController.Instance.menuWindowIsOpen)
                {
                    GUIController.Instance.CloseGUI();
                }
            }
        }
    }
}
