using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;

namespace BK
{
    public class PlayerEquipmentManager : CharacterEquipmentManager
    {
        private PlayerManager player;

        [Header("Weapon Model Instantiation Slots")]
        [HideInInspector] public WeaponModelInstantiationSlot rightHandWeaponSlot;
        [HideInInspector] public WeaponModelInstantiationSlot leftHandWeaponSlot;
        [HideInInspector] public WeaponModelInstantiationSlot leftHandShieldSlot;
        [HideInInspector] public WeaponModelInstantiationSlot backSlot;

        [Header("Weapon Models")]
        [HideInInspector] public GameObject rightHandWeaponModel;
        [HideInInspector] public GameObject leftHandWeaponModel;
        [HideInInspector] public GameObject rangeWeaponModel;

        [Header("Weapon Managers")]
        public WeaponManager rightWeaponManager;
        public WeaponManager leftWeaponManager;
        public WeaponManager subWeaponManager;

        #region Equipment Model Groups

        [Header("General Equipment Models")]
        public GameObject hatsObject;                 [HideInInspector] public GameObject[] hats;
        public GameObject hoodsObject;                [HideInInspector] public GameObject[] hoods;
        public GameObject faceCoversObject;           [HideInInspector] public GameObject[] faceCovers;
        public GameObject helmetAccessoriesObject;    [HideInInspector] public GameObject[] helmetAccessories;
        public GameObject backAccessoriesObject;      [HideInInspector] public GameObject[] backAccessories;
        public GameObject hipAccessoriesObject;       [HideInInspector] public GameObject[] hipAccessories;
        public GameObject rightShoulderObject;        [HideInInspector] public GameObject[] rightShoulder;
        public GameObject rightElbowObject;           [HideInInspector] public GameObject[] rightElbow;
        public GameObject rightKneeObject;            [HideInInspector] public GameObject[] rightKnee;
        public GameObject leftShoulderObject;         [HideInInspector] public GameObject[] leftShoulder;
        public GameObject leftElbowObject;            [HideInInspector] public GameObject[] leftElbow;
        public GameObject leftKneeObject;             [HideInInspector] public GameObject[] leftKnee;

        [Header("Male Equipment Models")]
        public GameObject maleFullHelmetObject;       [HideInInspector] public GameObject[] maleHeadFullHelmets;
        public GameObject maleFullBodyObject;         [HideInInspector] public GameObject[] maleBodies;
        public GameObject maleRightUpperArmObject;    [HideInInspector] public GameObject[] maleRightUpperArms;
        public GameObject maleRightLowerArmObject;    [HideInInspector] public GameObject[] maleRightLowerArms;
        public GameObject maleRightHandObject;        [HideInInspector] public GameObject[] maleRightHands;
        public GameObject maleLeftUpperArmObject;     [HideInInspector] public GameObject[] maleLeftUpperArms;
        public GameObject maleLeftLowerArmObject;     [HideInInspector] public GameObject[] maleLeftLowerArms;
        public GameObject maleLeftHandObject;         [HideInInspector] public GameObject[] maleLeftHands;
        public GameObject maleHipsObject;             [HideInInspector] public GameObject[] maleHips;
        public GameObject maleRightLegObject;         [HideInInspector] public GameObject[] maleRightLegs;
        public GameObject maleLeftLegObject;          [HideInInspector] public GameObject[] maleLeftLegs;

        [Header("Female Equipment Models")]
        public GameObject femaleFullHelmetObject;     [HideInInspector] public GameObject[] femaleHeadFullHelmets;
        public GameObject femaleFullBodyObject;       [HideInInspector] public GameObject[] femaleBodies;
        public GameObject femaleRightUpperArmObject;  [HideInInspector] public GameObject[] femaleRightUpperArms;
        public GameObject femaleRightLowerArmObject;  [HideInInspector] public GameObject[] femaleRightLowerArms;
        public GameObject femaleRightHandObject;      [HideInInspector] public GameObject[] femaleRightHands;
        public GameObject femaleLeftUpperArmObject;   [HideInInspector] public GameObject[] femaleLeftUpperArms;
        public GameObject femaleLeftLowerArmObject;   [HideInInspector] public GameObject[] femaleLeftLowerArms;
        public GameObject femaleLeftHandObject;       [HideInInspector] public GameObject[] femaleLeftHands;
        public GameObject femaleHipsObject;           [HideInInspector] public GameObject[] femaleHips;
        public GameObject femaleRightLegObject;       [HideInInspector] public GameObject[] femaleRightLegs;
        public GameObject femaleLeftLegObject;        [HideInInspector] public GameObject[] femaleLeftLegs;

        #endregion

        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();

            InitializeWeaponSlots();
            InitializeArmorModels();
        }

        protected override void Start()
        {
            base.Start();
            EquipWeapons();
        }

        public void EquipArmor()
        {
            LoadHeadEquipment(player.playerInventoryManager.headEquipment);
            LoadBodyEquipment(player.playerInventoryManager.bodyEquipment);
            LoadLegEquipment(player.playerInventoryManager.legEquipment);
            LoadHandEquipment(player.playerInventoryManager.handEquipment);
        }

        #region Quick Slot

        public void SwitchQuickSlotItem()
        {
            if (!player.IsOwner) return;

            int startIndex = player.playerInventoryManager.quickSlotItemIndex + 1;

            for (int attempt = 0; attempt < 3; attempt++)
            {
                int idx = (startIndex + attempt) % 3;
                var item = player.playerInventoryManager.quickSlotItemsInQuickSlots[idx];
                if (item == null) continue;

                player.playerInventoryManager.quickSlotItemIndex = idx;
                player.playerNetworkManager.currentQuickSlotItemID.Value = item.itemID;
                return;
            }

            player.playerInventoryManager.quickSlotItemIndex = -1;
            player.playerNetworkManager.currentQuickSlotItemID.Value = -1;
        }

        #endregion

        #region Utilities

        private static GameObject[] ChildrenToArray(GameObject parent)
        {
            if (parent == null) return new GameObject[0];

            int count = parent.transform.childCount;
            var arr = new GameObject[count];
            for (int i = 0; i < count; i++)
                arr[i] = parent.transform.GetChild(i).gameObject;
            return arr;
        }

        private static void SetActiveAll(GameObject[] models, bool active)
        {
            if (models == null) return;
            foreach (var go in models)
            {
                if (go != null) go.SetActive(active);
            }
        }

        private void RecalculateArmorAbsorption()
        {
            player.playerStatsManager.CalculateTotalArmorAbsorption();
        }

        private void SetNetworkIdIfOwner(Unity.Netcode.NetworkVariable<int> netVar, int idOrMinusOne)
        {
            if (!player.IsOwner) return;
            netVar.Value = idOrMinusOne;
        }

        #endregion

        #region Armor Init

        private void InitializeArmorModels()
        {
            hats = ChildrenToArray(hatsObject);
            hoods = ChildrenToArray(hoodsObject);
            faceCovers = ChildrenToArray(faceCoversObject);
            helmetAccessories = ChildrenToArray(helmetAccessoriesObject);
            backAccessories = ChildrenToArray(backAccessoriesObject);
            hipAccessories = ChildrenToArray(hipAccessoriesObject);
            rightShoulder = ChildrenToArray(rightShoulderObject);
            rightElbow = ChildrenToArray(rightElbowObject);
            rightKnee = ChildrenToArray(rightKneeObject);
            leftShoulder = ChildrenToArray(leftShoulderObject);
            leftElbow = ChildrenToArray(leftElbowObject);
            leftKnee = ChildrenToArray(leftKneeObject);

            maleHeadFullHelmets = ChildrenToArray(maleFullHelmetObject);
            maleBodies = ChildrenToArray(maleFullBodyObject);
            maleRightUpperArms = ChildrenToArray(maleRightUpperArmObject);
            maleRightLowerArms = ChildrenToArray(maleRightLowerArmObject);
            maleRightHands = ChildrenToArray(maleRightHandObject);
            maleLeftUpperArms = ChildrenToArray(maleLeftUpperArmObject);
            maleLeftLowerArms = ChildrenToArray(maleLeftLowerArmObject);
            maleLeftHands = ChildrenToArray(maleLeftHandObject);
            maleHips = ChildrenToArray(maleHipsObject);
            maleRightLegs = ChildrenToArray(maleRightLegObject);
            maleLeftLegs = ChildrenToArray(maleLeftLegObject);

            femaleHeadFullHelmets = ChildrenToArray(femaleFullHelmetObject);
            femaleBodies = ChildrenToArray(femaleFullBodyObject);
            femaleRightUpperArms = ChildrenToArray(femaleRightUpperArmObject);
            femaleRightLowerArms = ChildrenToArray(femaleRightLowerArmObject);
            femaleRightHands = ChildrenToArray(femaleRightHandObject);
            femaleLeftUpperArms = ChildrenToArray(femaleLeftUpperArmObject);
            femaleLeftLowerArms = ChildrenToArray(femaleLeftLowerArmObject);
            femaleLeftHands = ChildrenToArray(femaleLeftHandObject);
            femaleHips = ChildrenToArray(femaleHipsObject);
            femaleRightLegs = ChildrenToArray(femaleRightLegObject);
            femaleLeftLegs = ChildrenToArray(femaleLeftLegObject);
        }

        #endregion

        #region Armor Load/Unload

        public void LoadHeadEquipment(HeadEquipmentItem equipment)
        {
            UnloadHeadEquipmentModels();

            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.headEquipmentID, -1);
                player.playerInventoryManager.headEquipment = null;
                return;
            }

            player.playerInventoryManager.headEquipment = equipment;

            switch (equipment.headEquipmentType)
            {
                case HeadEquipmentType.FullHelmet:
                    player.playerBodyManager.DisableHair();
                    player.playerBodyManager.DisableHead();
                    break;
                case HeadEquipmentType.Hood:
                    player.playerBodyManager.DisableHair();
                    break;
                case HeadEquipmentType.FaceCover:
                    player.playerBodyManager.DisableFacialHair();
                    break;
            }

            foreach (var model in equipment.equipmentModels)
                model.LoadModel(player, player.playerNetworkManager.isMale.Value);

            RecalculateArmorAbsorption();
            SetNetworkIdIfOwner(player.playerNetworkManager.headEquipmentID, equipment.itemID);
        }

        private void UnloadHeadEquipmentModels()
        {
            SetActiveAll(maleHeadFullHelmets, false);
            SetActiveAll(femaleHeadFullHelmets, false);
            SetActiveAll(hats, false);
            SetActiveAll(faceCovers, false);
            SetActiveAll(hoods, false);
            SetActiveAll(helmetAccessories, false);

            player.playerBodyManager.EnableHead();
            player.playerBodyManager.EnableHair();
        }

        public void LoadBodyEquipment(BodyEquipmentItem equipment)
        {
            UnloadBodyEquipmentModels();

            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.bodyEquipmentID, -1);
                player.playerInventoryManager.bodyEquipment = null;
                return;
            }

            player.playerInventoryManager.bodyEquipment = equipment;

            player.playerBodyManager.DisableBody();

            foreach (var model in equipment.equipmentModels)
                model.LoadModel(player, player.playerNetworkManager.isMale.Value);

            RecalculateArmorAbsorption();
            SetNetworkIdIfOwner(player.playerNetworkManager.bodyEquipmentID, equipment.itemID);
        }

        private void UnloadBodyEquipmentModels()
        {
            SetActiveAll(rightShoulder, false);
            SetActiveAll(rightElbow, false);
            SetActiveAll(leftShoulder, false);
            SetActiveAll(leftElbow, false);
            SetActiveAll(backAccessories, false);

            SetActiveAll(maleBodies, false);
            SetActiveAll(maleRightUpperArms, false);
            SetActiveAll(maleLeftUpperArms, false);

            SetActiveAll(femaleBodies, false);
            SetActiveAll(femaleRightUpperArms, false);
            SetActiveAll(femaleLeftUpperArms, false);

            player.playerBodyManager.EnableBody();
        }

        public void LoadLegEquipment(LegEquipmentItem equipment)
        {
            UnloadLegEquipmentModels();

            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.legEquipmentID, -1);
                player.playerInventoryManager.legEquipment = null;
                return;
            }

            player.playerInventoryManager.legEquipment = equipment;

            player.playerBodyManager.DisableLowerBody();

            foreach (var model in equipment.equipmentModels)
                model.LoadModel(player, player.playerNetworkManager.isMale.Value);

            RecalculateArmorAbsorption();
            SetNetworkIdIfOwner(player.playerNetworkManager.legEquipmentID, equipment.itemID);
        }

        private void UnloadLegEquipmentModels()
        {
            SetActiveAll(maleHips, false);
            SetActiveAll(femaleHips, false);
            SetActiveAll(leftKnee, false);
            SetActiveAll(rightKnee, false);
            SetActiveAll(maleLeftLegs, false);
            SetActiveAll(maleRightLegs, false);
            SetActiveAll(femaleLeftLegs, false);
            SetActiveAll(femaleRightLegs, false);

            player.playerBodyManager.EnableLowerBody();
        }

        public void LoadHandEquipment(HandEquipmentItem equipment)
        {
            UnloadHandEquipmentModels();

            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.handEquipmentID, -1);
                player.playerInventoryManager.handEquipment = null;
                return;
            }

            player.playerInventoryManager.handEquipment = equipment;

            player.playerBodyManager.DisableArms();

            foreach (var model in equipment.equipmentModels)
                model.LoadModel(player, player.playerNetworkManager.isMale.Value);

            RecalculateArmorAbsorption();
            SetNetworkIdIfOwner(player.playerNetworkManager.handEquipmentID, equipment.itemID);
        }

        private void UnloadHandEquipmentModels()
        {
            SetActiveAll(maleLeftLowerArms, false);
            SetActiveAll(maleRightLowerArms, false);
            SetActiveAll(femaleLeftLowerArms, false);
            SetActiveAll(femaleRightLowerArms, false);
            SetActiveAll(maleLeftHands, false);
            SetActiveAll(maleRightHands, false);
            SetActiveAll(femaleLeftHands, false);
            SetActiveAll(femaleRightHands, false);

            player.playerBodyManager.EnableArms();
        }

        #endregion

        #region Projectiles / Quick Slot Load

        public void LoadMainProjectileEquipment(RangedProjectileItem equipment)
        {
            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.mainProjectileID, -1);
                player.playerInventoryManager.mainProjectile = null;
                return;
            }

            player.playerInventoryManager.mainProjectile = equipment;
            SetNetworkIdIfOwner(player.playerNetworkManager.mainProjectileID, equipment.itemID);
        }

        public void LoadSecondaryProjectileEquipment(RangedProjectileItem equipment)
        {
            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.secondaryProjectileID, -1);
                player.playerInventoryManager.secondaryProjectile = null;
                return;
            }

            player.playerInventoryManager.secondaryProjectile = equipment;
            SetNetworkIdIfOwner(player.playerNetworkManager.secondaryProjectileID, equipment.itemID);
        }

        public void LoadQuickSlotEquipment(QuickSlotItem equipment)
        {
            if (equipment == null)
            {
                SetNetworkIdIfOwner(player.playerNetworkManager.currentQuickSlotItemID, -1);
                player.playerInventoryManager.currentQuickSlotItem = null;
                return;
            }

            player.playerInventoryManager.currentQuickSlotItem = equipment;
            SetNetworkIdIfOwner(player.playerNetworkManager.currentQuickSlotItemID, equipment.itemID);
        }

        #endregion

        #region Weapons

        private void InitializeWeaponSlots()
        {
            var weaponSlots = GetComponentsInChildren<WeaponModelInstantiationSlot>();

            foreach (var slot in weaponSlots)
            {
                switch (slot.weaponSlot)
                {
                    case WeaponModelSlot.RightHand:
                        rightHandWeaponSlot = slot;
                        break;
                    case WeaponModelSlot.LeftHandWeaponSlot:
                        leftHandWeaponSlot = slot;
                        break;
                    case WeaponModelSlot.LeftHandShieldSlot:
                        leftHandShieldSlot = slot;
                        break;
                    case WeaponModelSlot.BackSlot:
                        backSlot = slot;
                        break;
                }
            }
        }

        public void EquipWeapons()
        {
            LoadRightWeapon();
            LoadLeftWeapon();
        }

        public void SwitchMainWeapon()
        {
            if (!player.IsOwner) return;

            player.playerNetworkManager.isTwoHandingWeapon.Value = false;
            player.playerAnimatorManager.PlayTargetActionAnimation("Swap_Right_Weapon_01", false, false, true, true);

            int selectedWeaponID = player.playerInventoryManager.currentSubWeapon.itemID;
            int changedWeaponID = player.playerInventoryManager.currentRightHandWeapon.itemID;

            WorldPlayerInventory.Instance.GetRightWeaponInventory().ResetItemGrid();
            WorldPlayerInventory.Instance.GetSubWeaponInventory().ResetItemGrid();

            WorldPlayerInventory.Instance.GetRightWeaponInventory().AddItemById(selectedWeaponID);
            WorldPlayerInventory.Instance.GetSubWeaponInventory().AddItemById(changedWeaponID);
        }

        public void LoadRightWeapon()
        {
            var weapon = player.playerInventoryManager.currentRightHandWeapon;
            if (weapon == null) return;

            rightHandWeaponSlot.UnloadWeapon();

            rightHandWeaponModel = Instantiate(weapon.weaponModel);
            rightHandWeaponSlot.PlaceWeaponModelIntoSlot(rightHandWeaponModel);

            rightWeaponManager = rightHandWeaponModel.GetComponent<WeaponManager>();
            rightWeaponManager.SetWeaponDamage(player, weapon);

            player.playerAnimatorManager.UpdateAnimatorController(weapon.weaponAnimator);
        }

        public void LoadLeftWeapon()
        {
            var weapon = player.playerInventoryManager.currentLeftHandWeapon;
            if (weapon == null) return;

            if (leftHandWeaponSlot.currentWeaponModel != null)
                leftHandWeaponSlot.UnloadWeapon();

            if (leftHandShieldSlot.currentWeaponModel != null)
                leftHandShieldSlot.UnloadWeapon();

            leftHandWeaponModel = Instantiate(weapon.weaponModel);

            switch (weapon.weaponModelType)
            {
                case WeaponModelType.Weapon:
                    leftHandWeaponSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
                    break;
                case WeaponModelType.Shield:
                    leftHandShieldSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
                    break;
            }

            leftWeaponManager = leftHandWeaponModel.GetComponent<WeaponManager>();
            leftWeaponManager.SetWeaponDamage(player, weapon);
        }

        #endregion

        #region Two Hand + Damage Colliders + Unhide

        private bool IsUnarmed(WeaponItem weapon)
            => weapon == null || weapon == WorldItemDatabase.Instance.unarmedWeapon;

        private void PlaceLeftModelToProperSlot()
        {
            var left = player.playerInventoryManager.currentLeftHandWeapon;
            if (left == null || leftHandWeaponModel == null) return;

            if (left.weaponModelType == WeaponModelType.Weapon)
                leftHandWeaponSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
            else if (left.weaponModelType == WeaponModelType.Shield)
                leftHandShieldSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);
        }

        private void RefreshWeaponDamages()
        {
            var right = player.playerInventoryManager.currentRightHandWeapon;
            var left = player.playerInventoryManager.currentLeftHandWeapon;

            if (rightWeaponManager != null && right != null)
                rightWeaponManager.SetWeaponDamage(player, right);

            if (leftWeaponManager != null && left != null)
                leftWeaponManager.SetWeaponDamage(player, left);
        }

        public void UnTwoHandWeapon()
        {
            var right = player.playerInventoryManager.currentRightHandWeapon;
            if (right != null)
                player.playerAnimatorManager.UpdateAnimatorController(right.weaponAnimator);

            PlaceLeftModelToProperSlot();

            if (rightHandWeaponModel != null)
                rightHandWeaponSlot.PlaceWeaponModelIntoSlot(rightHandWeaponModel);

            RefreshWeaponDamages();
        }

        public void TwoHandRightWeapon()
        {
            var right = player.playerInventoryManager.currentRightHandWeapon;

            if (IsUnarmed(right))
            {
                if (player.IsOwner)
                {
                    player.playerNetworkManager.isTwoHandingRightWeapon.Value = false;
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                }
                return;
            }

            player.playerAnimatorManager.UpdateAnimatorController(right.weaponAnimator);

            var left = player.playerInventoryManager.currentLeftHandWeapon;
            if (leftHandWeaponModel != null && left != null)
                backSlot.PlaceWeaponModelInUnequippedSlot(leftHandWeaponModel, left.weaponClass, player);

            if (rightHandWeaponModel != null)
                rightHandWeaponSlot.PlaceWeaponModelIntoSlot(rightHandWeaponModel);

            RefreshWeaponDamages();
        }

        public void TwoHandLeftWeapon()
        {
            var left = player.playerInventoryManager.currentLeftHandWeapon;

            if (IsUnarmed(left))
            {
                if (player.IsOwner)
                {
                    player.playerNetworkManager.isTwoHandingLeftWeapon.Value = false;
                    player.playerNetworkManager.isTwoHandingWeapon.Value = false;
                }
                return;
            }

            player.playerAnimatorManager.UpdateAnimatorController(left.weaponAnimator);

            var right = player.playerInventoryManager.currentRightHandWeapon;
            if (rightHandWeaponModel != null && right != null)
                backSlot.PlaceWeaponModelInUnequippedSlot(rightHandWeaponModel, right.weaponClass, player);

            if (leftHandWeaponModel != null)
                rightHandWeaponSlot.PlaceWeaponModelIntoSlot(leftHandWeaponModel);

            RefreshWeaponDamages();
        }
        
        public void OpenDamageCollider()
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                if (rightWeaponManager?.meleeDamageCollider == null) return;

                rightWeaponManager.meleeDamageCollider.EnableDamageCollider();

                var whooshes = player.playerInventoryManager.currentRightHandWeapon?.whooshes;
                if (whooshes != null && whooshes.Length > 0)
                    player.characterSoundFXManager.PlaySoundFX(
                        WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(whooshes)
                    );
            }
            else if (player.playerNetworkManager.isUsingLeftHand.Value)
            {
                if (leftWeaponManager?.meleeDamageCollider == null) return;

                leftWeaponManager.meleeDamageCollider.EnableDamageCollider();

                var whooshes = player.playerInventoryManager.currentLeftHandWeapon?.whooshes;
                if (whooshes != null && whooshes.Length > 0)
                    player.characterSoundFXManager.PlaySoundFX(
                        WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(whooshes)
                    );
            }
        }

        public void CloseDamageCollider()
        {
            if (player.playerNetworkManager.isUsingRightHand.Value)
            {
                rightWeaponManager?.meleeDamageCollider?.DisableDamageCollider();
            }
            else if (player.playerNetworkManager.isUsingLeftHand.Value)
            {
                leftWeaponManager?.meleeDamageCollider?.DisableDamageCollider();
            }
        }
        
        public void OpenMainHandDamageCollider()
        {
            rightWeaponManager?.meleeDamageCollider?.EnableDamageCollider();
            var whooshes = player.playerInventoryManager.currentRightHandWeapon?.whooshes;
            if (whooshes != null && whooshes.Length > 0)
                player.characterSoundFXManager.PlaySoundFX(
                    WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(whooshes)
                );
        }

        public void CloseMainHandDamageCollider()
        {
            rightWeaponManager?.meleeDamageCollider?.DisableDamageCollider();
        }

        public void OpenOffHandDamageCollider()
        {
            leftWeaponManager?.meleeDamageCollider?.EnableDamageCollider();
            var whooshes = player.playerInventoryManager.currentLeftHandWeapon?.whooshes;
            if (whooshes != null && whooshes.Length > 0)
                player.characterSoundFXManager.PlaySoundFX(
                    WorldSoundFXManager.Instance.ChooseRandomSfxFromArray(whooshes)
                );
        }

        public void CloseOffHandDamageCollider()
        {
            leftWeaponManager?.meleeDamageCollider?.DisableDamageCollider();
        }

        public void OpenBothHandDamageCollider()
        {
            OpenMainHandDamageCollider();
            OpenOffHandDamageCollider();
        }

        public void CloseBothHandDamageCollider()
        {
            CloseMainHandDamageCollider();
            CloseOffHandDamageCollider();
        }

        public void UnHideWeapons()
        {
            if (rightHandWeaponModel != null)
                rightHandWeaponModel.SetActive(true);

            if (leftHandWeaponModel != null)
                leftHandWeaponModel.SetActive(true);
        }

        #endregion
    }
}
