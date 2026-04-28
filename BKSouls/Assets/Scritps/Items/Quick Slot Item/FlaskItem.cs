using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    [CreateAssetMenu(menuName = "Items/Consumeables/Flask")]
    public class FlaskItem : QuickSlotItem
    {
        [Header("Flask Type")]
        public bool healthFlask = true;

        [Header("Restoration Value")]
        [SerializeField] int flaskRestoration = 50;

        [Header("Empty Item")]
        public GameObject emptyFlaskItem;
        public string emptyFlaskAnimation;

        public override bool CanIUseThisItem(PlayerManager player)
        {
            if (!player.playerCombatManager.isUsingItem && player.isPerformingAction)
                return false;

            if (player.playerNetworkManager.isAttacking.Value)
                return false;

            return true;
        }

        public override void AttemptToUseItem(PlayerManager player)
        {
            if (!CanIUseThisItem(player))
                return;

            int currentAmount = GetCurrentAmount(player);

            //  HEALTH FLASK CHECK
            if (healthFlask && currentAmount <= 0)
            {
                if (player.playerCombatManager.isUsingItem)
                    return;

                player.playerCombatManager.isUsingItem = true;

                if (player.IsOwner)
                {
                    player.playerAnimatorManager.PlayTargetActionAnimation(emptyFlaskAnimation, false, false, true, true, false);
                    player.playerNetworkManager.HideWeaponsServerRpc();
                }

                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);
                GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
                return;
            }

            //  FOCUS POINTS FLASK CHECK
            if (!healthFlask && currentAmount <= 0)
            {
                if (player.playerCombatManager.isUsingItem)
                    return;

                player.playerCombatManager.isUsingItem = true;

                if (player.IsOwner)
                {
                    player.playerAnimatorManager.PlayTargetActionAnimation(emptyFlaskAnimation, false, false, true, true, false);
                    player.playerNetworkManager.HideWeaponsServerRpc();
                }

                Destroy(player.playerEffectsManager.activeQuickSlotItemFX);
                GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
                player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
                return;
            }

            //  CHECK FOR CHUGGING
            if (player.playerCombatManager.isUsingItem)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.isChugging.Value = false;

                return;
            }

            player.playerCombatManager.isUsingItem = true;

            player.playerEffectsManager.activeQuickSlotItemFX = Instantiate(itemModel, player.playerEquipmentManager.rightHandWeaponSlot.transform);

            if (player.IsOwner)
            {
                player.playerAnimatorManager.PlayTargetActionAnimation(useItemAnimation, false, false, true, true, false);
                player.playerNetworkManager.HideWeaponsServerRpc();
            }
        }

        public override void SuccessfullyUseItem(PlayerManager player)
        {
            base.SuccessfullyUseItem(player);

            if (GetCurrentAmount(player) <= 0)
            {
                if (player.IsOwner)
                    player.playerNetworkManager.isChugging.Value = false;

                ShowEmptyFlask(player);
                return;
            }

            if (player.IsOwner)
            {
                if (healthFlask)
                {
                    int healBonus = WorldSaveGameManager.Instance != null ? WorldSaveGameManager.Instance.GetHealthFlaskHealBonus() : 0;
                    player.playerNetworkManager.currentHealth.Value += flaskRestoration + healBonus;

                    // 인벤토리 포션 먼저 소모, 없으면 기본 플라스크 소모
                    ConsumeOneFlask(player);
                }
                else
                {
                    int healBonus = WorldSaveGameManager.Instance != null ? WorldSaveGameManager.Instance.GetFocusPointFlaskHealBonus() : 0;
                    player.playerNetworkManager.currentFocusPoints.Value += flaskRestoration + healBonus;

                    ConsumeOneFlask(player);
                }

                GUIController.Instance.playerUIHudManager.SetQuickSlotItemQuickSlotIcon(player.playerInventoryManager.currentQuickSlotItem);
            }

            if (healthFlask && GetCurrentAmount(player) <= 0)
            {
                ShowEmptyFlask(player);
            }

            if (!healthFlask && GetCurrentAmount(player) <= 0)
            {
                ShowEmptyFlask(player);
            }

            PlayHealingFX(player);
        }

        private void ConsumeOneFlask(PlayerManager player)
        {
            if (GetInventoryFlaskCount() > 0)
            {
                BK.Inventory.WorldPlayerInventory.Instance.RemoveItemInInventory(itemID, 1);
                return;
            }

            if (healthFlask)
            {
                player.playerNetworkManager.remainingHealthFlasks.Value =
                    Mathf.Max(0, player.playerNetworkManager.remainingHealthFlasks.Value - 1);
            }
            else
            {
                player.playerNetworkManager.remainingFocusPointsFlasks.Value =
                    Mathf.Max(0, player.playerNetworkManager.remainingFocusPointsFlasks.Value - 1);
            }
        }

        private void ShowEmptyFlask(PlayerManager player)
        {
            Destroy(player.playerEffectsManager.activeQuickSlotItemFX);

            if (emptyFlaskItem == null)
                return;

            GameObject emptyFlask = Instantiate(emptyFlaskItem, player.playerEquipmentManager.rightHandWeaponSlot.transform);
            player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
        }

        private void PlayHealingFX(PlayerManager player)
        {
            Instantiate(healthFlask ? WorldCharacterEffectsManager.Instance.hpFlaskVFX : WorldCharacterEffectsManager.Instance.mpFlaskVFX, player.transform);
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.Instance.healingFlaskSFX);
        }

        private int GetInventoryFlaskCount()
        {
            if (BK.Inventory.WorldPlayerInventory.Instance == null) return 0;
            return BK.Inventory.WorldPlayerInventory.Instance.GetItemCountInAllInventory(itemID);
        }

        public override int GetCurrentAmount(PlayerManager player)
        {
            int baseAmount = healthFlask
                ? player.playerNetworkManager.remainingHealthFlasks.Value
                : player.playerNetworkManager.remainingFocusPointsFlasks.Value;

            return Mathf.Max(0, baseAmount + GetInventoryFlaskCount());
        }
    }
}
