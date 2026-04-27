using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace BK
{
    public class PlayerCombatManager : CharacterCombatManager
    {
        PlayerManager player;

        public WeaponItem currentWeaponBeingUsed;
        public ProjectileSlot currentProjectileBeingUsed;

        [Header("Projectile")]
        private Vector3 projectileAimDirection;

        [Header("Flags")]
        public bool canComboWithMainHandWeapon = false;
        public bool canComboWithOffHandWeapon = false;
        public bool isUsingItem = false;


        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void PerformWeaponBasedAction(WeaponItemAction weaponAction, WeaponItem weaponPerformingAction)
        {
            if (player.IsOwner)
            {
                //  PERFORM THE ACTION
                weaponAction?.AttemptToPerformAction(player, weaponPerformingAction);
            }
        }

        public override void CloseAllDamageColliders()
        {
            base.CloseAllDamageColliders();

            player.playerEquipmentManager.rightWeaponManager.meleeDamageCollider.DisableDamageCollider();
            player.playerEquipmentManager.leftWeaponManager.meleeDamageCollider.DisableDamageCollider();
        }

        //  CRITICAL ATTACKS
        public override void AttemptRiposte(RaycastHit hit)
        {
            CharacterManager targetCharacter = hit.transform.gameObject.GetComponent<CharacterManager>();

            //  IF FOR SOME REASON THE TARGET CHARACTER IS NULL, RETURN
            if (targetCharacter == null)
                return;

            //  IF SOME HOW SINCE THE INITIAL CHECK THE CHARACTER CAN NO LONGER BE RIPOSTED, RETURN
            if (!targetCharacter.characterNetworkManager.isRipostable.Value)
                return;

            //  IF SOMEBODY ELSE IS ALREADY PERFORMING A CRITICAL STRIKE ON THE CHARACTER (OR WE ALREADY ARE), RETURN
            if (targetCharacter.characterNetworkManager.isBeingCriticallyDamaged.Value)
                return;

            //  YOU CAN ONLY RIPOSTE WITH A MELEE WEAPON ITEM
            MeleeWeaponItem riposteWeapon;
            MeleeWeaponDamageCollider riposteCollider;

            if (player.playerNetworkManager.isTwoHandingLeftWeapon.Value)
            {
                riposteWeapon = player.playerInventoryManager.currentLeftHandWeapon as MeleeWeaponItem;
                riposteCollider = player.playerEquipmentManager.leftWeaponManager.meleeDamageCollider;
            }
            else
            {
                riposteWeapon = player.playerInventoryManager.currentRightHandWeapon as MeleeWeaponItem;
                riposteCollider = player.playerEquipmentManager.rightWeaponManager.meleeDamageCollider;
            }

            //  THE RIPSOTE ANIMATION WILL CHANGE DEPENDING ON THE WEAPON'S ANIMATOR CONTROLLER, SO THE ANIMATION CAN BE CHOOSEN THERE, THE NAME WILL ALWAYS BE THE SAME
            character.characterAnimatorManager.PlayTargetActionAnimationInstantly("Riposte_01", true);

            //  WHILST PERFORMING A CRITICAL STRIKE, YOU CANNOT BE DAMAGED
            if (character.IsOwner)
                character.characterNetworkManager.isInvulnerable.Value = true;

            // 1. CREATE A NEW DAMAGE EFFECT FOR THIS TYPE OF DAMAGE
            TakeCriticalDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeCriticalDamageEffect);

            // 2. APPLY ALL OF THE DAMAGE STATS FROM THE COLLIDER TO THE DAMAGE EFFECT
            damageEffect.physicalDamage = riposteCollider.physicalDamage;
            damageEffect.holyDamage = riposteCollider.holyDamage;
            damageEffect.fireDamage = riposteCollider.fireDamage;
            damageEffect.lightningDamage = riposteCollider.lightningDamage;
            damageEffect.magicDamage = riposteCollider.magicDamage;
            damageEffect.poiseDamage = riposteCollider.poiseDamage;

            // 3. MULTIPLY DAMAGE BY WEAPONS RIPOSTE MODIFIER
            damageEffect.physicalDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.holyDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.fireDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.lightningDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.magicDamage *= riposteWeapon.riposte_Attack_01_Modifier;
            damageEffect.poiseDamage *= riposteWeapon.riposte_Attack_01_Modifier;

            // 4. USING A SERVER RPC SEND THE RIPOSTE TO THE TARGET, WHERE THEY WILL PLAY THE PROPER ANIMATIONS ON THEIR END, AND TAKE THE DAMAGE
            targetCharacter.characterNetworkManager.NotifyTheServerOfRiposteServerRpc(
                targetCharacter.NetworkObjectId,
                character.NetworkObjectId,
                "Riposted_01",
                riposteWeapon.itemID,
                damageEffect.physicalDamage,
                damageEffect.magicDamage,
                damageEffect.fireDamage,
                damageEffect.holyDamage,
                damageEffect.poiseDamage);
        }

        public override void AttemptBackstab(RaycastHit hit)
        {
            CharacterManager targetCharacter = hit.transform.gameObject.GetComponent<CharacterManager>();

            //  IF FOR SOME REASON THE TARGET CHARACTER IS NULL, RETURN
            if (targetCharacter == null)
                return;

            //  IF SOME HOW SINCE THE INITIAL CHECK THE CHARACTER CAN NO LONGER BE RIPOSTED, RETURN
            if (!targetCharacter.characterCombatManager.canBeBackstabbed)
                return;

            //  IF SOMEBODY ELSE IS ALREADY PERFORMING A CRITICAL STRIKE ON THE CHARACTER (OR WE ALREADY ARE), RETURN
            if (targetCharacter.characterNetworkManager.isBeingCriticallyDamaged.Value)
                return;

            //  YOU CAN ONLY RIPOSTE WITH A MELEE WEAPON ITEM
            MeleeWeaponItem backstabWeapon;
            MeleeWeaponDamageCollider backstabCollider;

            //  TODO: CHECK IF WE ARE TWO HANDING LEFT WEAPON OR RIGHT WEAPON (THIS WILL CHANGE THE RIPOSTE WEAPON)

            if (player.playerNetworkManager.isTwoHandingLeftWeapon.Value)
            {
                backstabWeapon = player.playerInventoryManager.currentLeftHandWeapon as MeleeWeaponItem;
                backstabCollider = player.playerEquipmentManager.leftWeaponManager.meleeDamageCollider;
            }
            else
            {
                backstabWeapon = player.playerInventoryManager.currentRightHandWeapon as MeleeWeaponItem;
                backstabCollider = player.playerEquipmentManager.rightWeaponManager.meleeDamageCollider;
            }

            //  THE RIPSOTE ANIMATION WILL CHANGE DEPENDING ON THE WEAPON'S ANIMATOR CONTROLLER, SO THE ANIMATION CAN BE CHOOSEN THERE, THE NAME WILL ALWAYS BE THE SAME
            character.characterAnimatorManager.PlayTargetActionAnimationInstantly("Backstab_01", true);

            //  WHILST PERFORMING A CRITICAL STRIKE, YOU CANNOT BE DAMAGED
            if (character.IsOwner)
                character.characterNetworkManager.isInvulnerable.Value = true;

            // 1. CREATE A NEW DAMAGE EFFECT FOR THIS TYPE OF DAMAGE
            TakeCriticalDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.Instance.takeCriticalDamageEffect);

            // 2. APPLY ALL OF THE DAMAGE STATS FROM THE COLLIDER TO THE DAMAGE EFFECT
            damageEffect.physicalDamage = backstabCollider.physicalDamage;
            damageEffect.holyDamage = backstabCollider.holyDamage;
            damageEffect.fireDamage = backstabCollider.fireDamage;
            damageEffect.lightningDamage = backstabCollider.lightningDamage;
            damageEffect.magicDamage = backstabCollider.magicDamage;
            damageEffect.poiseDamage = backstabCollider.poiseDamage;

            // 3. MULTIPLY DAMAGE BY WEAPONS RIPOSTE MODIFIER
            damageEffect.physicalDamage *= backstabWeapon.backstab_Attack_01_Modifier;
            damageEffect.holyDamage *= backstabWeapon.backstab_Attack_01_Modifier;
            damageEffect.fireDamage *= backstabWeapon.backstab_Attack_01_Modifier;
            damageEffect.lightningDamage *= backstabWeapon.backstab_Attack_01_Modifier;
            damageEffect.magicDamage *= backstabWeapon.backstab_Attack_01_Modifier;
            damageEffect.poiseDamage *= backstabWeapon.backstab_Attack_01_Modifier;

            // 4. USING A SERVER RPC SEND THE RIPOSTE TO THE TARGET, WHERE THEY WILL PLAY THE PROPER ANIMATIONS ON THEIR END, AND TAKE THE DAMAGE
            targetCharacter.characterNetworkManager.NotifyTheServerOfBackstabServerRpc(
                targetCharacter.NetworkObjectId,
                character.NetworkObjectId,
                "Backstabbed_01",
                backstabWeapon.itemID,
                damageEffect.physicalDamage,
                damageEffect.magicDamage,
                damageEffect.fireDamage,
                damageEffect.holyDamage,
                damageEffect.poiseDamage);
        }

        public virtual void DrainStaminaBasedOnAttack()
        {
            if (!player.IsOwner)
                return;

            if (currentWeaponBeingUsed == null)
                return;

            float staminaDeducted = 0;

            switch (currentAttackType)
            {
                case AttackType.LightAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.LightAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.LightJumpingAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.HeavyJumpingAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.ChargedAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.chargedAttackStaminaCostMultiplier;
                    break;
                case AttackType.ChargedAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.chargedAttackStaminaCostMultiplier;
                    break;
                case AttackType.RunningAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.runningAttackStaminaCostMultiplier;
                    break;
                case AttackType.RollingAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.rollingAttackStaminaCostMultiplier;
                    break;
                case AttackType.BackstepAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.backstepAttackStaminaCostMultiplier;
                    break;
                case AttackType.DualAttack01:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.DualAttack02:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.lightAttackStaminaCostMultiplier;
                    break;
                case AttackType.DualJumpAttack:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.heavyAttackStaminaCostMultiplier;
                    break;
                case AttackType.DualRunAttack:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.runningAttackStaminaCostMultiplier;
                    break;
                case AttackType.DualRollAttack:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.rollingAttackStaminaCostMultiplier;
                    break;
                case AttackType.DualBackstepAttack:
                    staminaDeducted = currentWeaponBeingUsed.baseStaminaCost * currentWeaponBeingUsed.backstepAttackStaminaCostMultiplier;
                    break;
                default:
                    break;
            }

            player.playerNetworkManager.currentStamina.Value -= Mathf.RoundToInt(staminaDeducted);
        }

        public override void SetTarget(CharacterManager newTarget)
        {
            base.SetTarget(newTarget);

            if (player.IsOwner)
            {
                PlayerCamera.Instance.SetLockCameraHeight();
            }
        }

        //  ANIMATION EVENT CALLS

        //  COMBO
        public override void EnableCanDoCombo()
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                player.playerCombatManager.canComboWithMainHandWeapon = true;
            }
            else
            {
                player.playerCombatManager.canComboWithOffHandWeapon = true;
            }
        }

        public override void DisableCanDoCombo()
        {
            player.playerCombatManager.canComboWithMainHandWeapon = false;
            player.playerCombatManager.canComboWithOffHandWeapon = false;
        }

        //  PROJECTILE
        public void ReleaseArrow()
        {
            if (player.IsOwner)
                player.playerNetworkManager.hasArrowNotched.Value = false;

            //  DESTROY THE "WARM UP" PROJECTILE
            if (player.playerEffectsManager.activeDrawnProjectileFX != null)
                Destroy(player.playerEffectsManager.activeDrawnProjectileFX);

            //  PLAY RELEASE ARROW SFX
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(WorldSoundFXManager.Instance.releaseArrowSFX));

            // ANIMATE THE BOW
            Animator bowAnimator;

            if (player.playerNetworkManager.isTwoHandingLeftWeapon.Value)
            {
                bowAnimator = player.playerEquipmentManager.leftHandWeaponModel.GetComponentInChildren<Animator>();
            }
            else
            {
                bowAnimator = player.playerEquipmentManager.rightHandWeaponModel.GetComponentInChildren<Animator>();
            }

            //  ANIMATE THE BOW
            bowAnimator.SetBool("isDrawn", false);
            bowAnimator.Play("Bow_Fire_01");

            if (!player.IsOwner)
                return;

            //  THE PROJECTILE WE ARE FIRING
            RangedProjectileItem projectileItem = null;

            switch (currentProjectileBeingUsed)
            {
                case ProjectileSlot.Main:
                    projectileItem = player.playerInventoryManager.mainProjectile;
                    break;
                case ProjectileSlot.Secondary:
                    projectileItem = player.playerInventoryManager.secondaryProjectile;
                    break;
                default:
                    break;
            }

            if (projectileItem == null)
                return;

            if (projectileItem.currentAmmoAmount <= 0)
                return;

            Transform projectileInstantiationLocation;
            GameObject projectileGameObject;
            Rigidbody projectileRigidbody;
            RangedProjectileDamageCollider projectileDamageCollider;

            //  SUBTRACT AMMO
            projectileItem.currentAmmoAmount -= 1;
            //  (TODO MAKE AND UPDATE ARROW COUNT UI)
            switch (currentProjectileBeingUsed)
            {
                case ProjectileSlot.Main:
                    GUIController.Instance.playerUIHudManager.SetMainProjectileQuickSlotIcon(projectileItem);
                    break;
                case ProjectileSlot.Secondary:
                    GUIController.Instance.playerUIHudManager.SetSecondaryProjectileQuickSlotIcon(projectileItem);
                    break;
                default:
                    break;
            }

            projectileInstantiationLocation = player.playerCombatManager.lockOnTransform;
            projectileGameObject = Instantiate(projectileItem.releaseProjectileModel, projectileInstantiationLocation);
            projectileDamageCollider = projectileGameObject.GetComponent<RangedProjectileDamageCollider>();
            projectileRigidbody = projectileGameObject.GetComponent<Rigidbody>();

            WeaponItem bow = player.playerNetworkManager.isTwoHandingLeftWeapon.Value
                ? player.playerInventoryManager.currentLeftHandWeapon
                : player.playerInventoryManager.currentRightHandWeapon;
            var net = player.playerNetworkManager;
            WeaponManager.CalculateRangedProjectileDamage(
                bow,
                projectileItem,
                net.strength.Value + net.strengthModifier.Value,
                net.dexterity.Value,
                net.intelligence.Value,
                net.faith.Value,
                out projectileDamageCollider.physicalDamage,
                out projectileDamageCollider.magicDamage,
                out projectileDamageCollider.fireDamage,
                out projectileDamageCollider.lightningDamage,
                out projectileDamageCollider.holyDamage);
            projectileDamageCollider.characterShootingProjectile = player;

            //  FIRE AN ARROW BASED ON 1 OF 3 VARIATIONS

            float yRotationDuringFire = player.transform.localEulerAngles.y;

            // AIMING
            if (player.playerNetworkManager.isAiming.Value)
            {
                Ray newRay = new Ray(player.playerCombatManager.lockOnTransform.position, PlayerCamera.Instance.aimDirection);
                projectileAimDirection = newRay.GetPoint(5);
                projectileGameObject.transform.LookAt(projectileAimDirection);
            }
            else
            {
                // LOCKED AND NOT AIMING
                if (player.playerCombatManager.currentTarget != null)
                {
                    Quaternion arrowRotation = Quaternion.LookRotation(player.playerCombatManager.currentTarget.characterCombatManager.lockOnTransform.position
                        - projectileGameObject.transform.position);
                    projectileGameObject.transform.rotation = arrowRotation;
                }
                // UNLOCKED AND NOT AIMING
                else
                {
                    //  TEMPORARY, IN THE FUTURE THE ARROW WILL USE THE CAMERA'S LOOK DIRECTION TO ALIGN ITS UP/DOWN ROTATION VALUE
                    //  HINT IF YOU WANT TO DO THIS ON YOUR OWN LOOK AT THE FORWARD DIRECTION VALUE OF THE CAMERA, AND DIRECT THE ARROW ACCORDINGLY
                    Quaternion arrowRotation = Quaternion.LookRotation(player.transform.forward);
                    projectileGameObject.transform.rotation = arrowRotation;
                }
            }

            //  GET ALL CHARACTER COLLIDERS AND IGNORE SELF
            Collider[] characterColliders = player.GetComponentsInChildren<Collider>();
            List<Collider> collidersArrowWillIgnore = new List<Collider>();

            foreach (var item in characterColliders)
                collidersArrowWillIgnore.Add(item);

            foreach (Collider hitBox in collidersArrowWillIgnore)
                Physics.IgnoreCollision(projectileDamageCollider.damageCollider, hitBox, true);

            projectileRigidbody.AddForce(projectileGameObject.transform.forward * projectileItem.forwardVelocity);
            projectileGameObject.transform.parent = null;

            //  TO DO (SYNC ARRROW FIRE WITH SERVER RPC)
            player.playerNetworkManager.NotifyServerOfReleasedProjectileServerRpc(
                player.OwnerClientId, 
                projectileItem.itemID, 
                projectileAimDirection.x, 
                projectileAimDirection.y, 
                projectileAimDirection.z,
                yRotationDuringFire);
        }

        //  SPELL

        public void InstantiateSpellWarmUpFX()
        {
            if (player.playerInventoryManager.currentSpell == null)
                return;

            player.playerInventoryManager.currentSpell.InstantiateWarmUpSpellFX(player);
        }

        public void SuccessfullyCastSpell()
        {
            if (player.playerInventoryManager.currentSpell == null)
                return;

            player.playerInventoryManager.currentSpell.SuccessfullyCastSpell(player);
            ResetSpellActionFlags();
        }

        public void SuccessfullyChargeSpell()
        {
            if (player.playerInventoryManager.currentSpell == null)
                return;

            player.playerInventoryManager.currentSpell.SuccessfullyChargeSpell(player);
        }

        public void SuccessfullyCastSpellFullCharge()
        {
            if (player.playerInventoryManager.currentSpell == null)
                return;

            player.playerInventoryManager.currentSpell.SuccessfullyCastSpellFullCharge(player);
            ResetSpellActionFlags();
        }

        private void ResetSpellActionFlags()
        {
            if (!player.IsOwner)
                return;

            player.playerNetworkManager.isAttacking.Value = false;
            player.playerNetworkManager.isChargingAttack.Value = false;
            player.playerNetworkManager.isChargingRightSpell.Value = false;
            player.playerNetworkManager.isChargingLeftSpell.Value = false;
            player.playerNetworkManager.isHoldingArrow.Value = false;
        }

        //  QUICK SLOT
        public void SuccessfullyUseQuickSlotItem()
        {
            if (player.playerInventoryManager.currentQuickSlotItem != null)
                player.playerInventoryManager.currentQuickSlotItem.SuccessfullyUseItem(player);
        }

        //  SLASH FX

        // 애니메이션 이벤트에서 호출 — prefabIndex로 모션별 다른 VFX 선택
        // Animation Event 설정: Function = SpawnSlashFX, Int = 0 (Light01), 1 (Light02), ...
        public void SpawnSlashFX(int prefabIndex = 0)
        {
            if (!player.IsOwner)
                return;

            // 현재 사용 중인 손의 무기 데이터 참조
            bool usingRight = player.playerNetworkManager.isUsingRightHand.Value;
            WeaponItem currentWeapon = usingRight
                ? player.playerInventoryManager.currentRightHandWeapon
                : player.playerInventoryManager.currentLeftHandWeapon;

            if (currentWeapon == null)
                return;

            GameObject[] prefabs = currentWeapon.slashFXPrefabs;
            if (prefabs == null || prefabIndex < 0 || prefabIndex >= prefabs.Length)
                return;

            GameObject prefab = prefabs[prefabIndex];
            if (prefab == null)
                return;

            // 오른손/왼손 무기 콜라이더에서 데미지 수치 참조
            MeleeWeaponDamageCollider sourceCollider = usingRight
                ? player.playerEquipmentManager.rightWeaponManager?.meleeDamageCollider
                : player.playerEquipmentManager.leftWeaponManager?.meleeDamageCollider;

            if (sourceCollider == null)
                return;

            // 무기 모델 위치에 스폰 (없으면 캐릭터 위치)
            Transform weaponModel = usingRight
                ? player.playerEquipmentManager.rightHandWeaponModel?.transform
                : player.playerEquipmentManager.leftHandWeaponModel?.transform;

            Transform spawnPoint = weaponModel != null ? weaponModel : player.transform;
            GameObject fx = Instantiate(prefab, spawnPoint.position, player.transform.rotation);

            SlashFXDamageCollider slashCollider = fx.GetComponent<SlashFXDamageCollider>();
            if (slashCollider != null)
            {
                slashCollider.characterCausingDamage = player;
                slashCollider.physicalDamage = sourceCollider.physicalDamage;
                slashCollider.magicDamage = sourceCollider.magicDamage;
                slashCollider.fireDamage = sourceCollider.fireDamage;
                slashCollider.holyDamage = sourceCollider.holyDamage;
                slashCollider.poiseDamage = sourceCollider.poiseDamage;
                slashCollider.damageModifier = currentWeapon.slashFXDamageModifier;
                slashCollider.isAIAttack = false;
            }
        }

        //  ASH OF WAR

        public WeaponItem SelectWeaponToPerformAshOfWar()
        {
            WeaponItem selectedWeapon;
            bool isRightHand;

            if (player.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                selectedWeapon = player.playerInventoryManager.currentTwoHandWeapon;
                isRightHand = player.playerNetworkManager.isTwoHandingRightWeapon.Value;
            }
            else
            {
                selectedWeapon = player.playerInventoryManager.currentLeftHandWeapon;
                isRightHand = false;
            }

            player.playerNetworkManager.SetCharacterActionHand(isRightHand);
            player.playerNetworkManager.currentWeaponBeingUsed.Value = selectedWeapon.itemID;
            return selectedWeapon;
        }
    }
}
