using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace BK.Inventory
{
    public class ItemGrid_Equipment : ItemGrid
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private ItemType itemType;

        [Header("Only Weapon")]
        [SerializeField] private WeaponSlotType weaponSlotType;

        private InventoryItem _equippedItem;

        // (ItemType, WeaponSlotType) -> 네트워크 변수 세팅 함수 (전 인스턴스 공유)
        private static readonly Dictionary<(ItemType, WeaponSlotType), Action<PlayerManager, int>> ApplyIdMap
            = new()
            {
                // Weapon
                [(ItemType.Weapon, WeaponSlotType.Right)] = (pm, id) => pm.playerNetworkManager.currentRightHandWeaponID.Value = id,
                [(ItemType.Weapon, WeaponSlotType.Left)]  = (pm, id) => pm.playerNetworkManager.currentLeftHandWeaponID.Value = id,
                [(ItemType.Weapon, WeaponSlotType.RightSub)]   = (pm, id) => pm.playerNetworkManager.currentRightSubWeaponID.Value = id,
                [(ItemType.Weapon, WeaponSlotType.LeftSub)]   = (pm, id) => pm.playerNetworkManager.currentLeftSubWeaponID.Value = id,

                // Armor
                [(ItemType.Armor,    WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.bodyEquipmentID.Value = id,
                [(ItemType.Helmet,   WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.headEquipmentID.Value = id,
                [(ItemType.Gauntlet, WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.handEquipmentID.Value = id,
                [(ItemType.Leggings, WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.legEquipmentID.Value = id,
            };

        #region Core

        public override bool CheckPlaceItem(InventoryItem inventoryItem, int posX, int posY)
        {
            if (!base.CheckPlaceItem(inventoryItem, posX, posY)) return false;
            if (inventoryItem?.itemData == null) return false;

            if (itemType != inventoryItem.itemData.itemType)
            {
                Debug.LogWarning($"Item Type Mismatch: {inventoryItem.itemData.itemType} / Grid: {itemType}");
                return false;
            }

            return true;
        }

        public override bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad)
        {
            if (_equippedItem != null) return false;
            if (!base.PlaceItem(inventoryItem, posX, posY, isLoad)) return false;

            if (!TryGetValidPlayerManager(out var pm)) return false;

            SetEquippedItem(inventoryItem);
            ApplyEquipmentNetworkId(pm, inventoryItem.itemData.itemID);

            return true;
        }

        public override InventoryItem PickUpItem(int x, int y)
        {
            var picked = base.PickUpItem(x, y);
            if (picked == null) return null;

            if (!TryGetValidPlayerManager(out var pm))
            {
                // base.PickUpItem은 이미 아이템을 뺐으니 여기서는 상태만 정리
                _equippedItem = null;
                return picked;
            }

            // 장착 해제
            UnequipIfMatches(pm, picked);

            return picked;
        }

        protected override void PlaceItemsAuto(List<int> setItemList, bool isLoad = true)
        {
            if (!TryGetValidPlayerManager(out var pm))
            {
                // 플레이어가 없으면 그냥 드랍 + 내부 상태 정리
                foreach (var itemCode in setItemList)
                    InventoryController.DropItem(itemCode);

                _equippedItem = null;
                return;
            }

            foreach (var itemCode in setItemList)
            {
                if (AddItemById(itemCode, isLoad: isLoad))
                    continue;

                InventoryController.DropItem(itemCode);

                // 자동 로드 실패 시 해제
                UnequipCurrent(pm);
            }
        }

        public override void ResetItemGrid()
        {
            if (_equippedItem == null) return;
            if (!TryGetValidPlayerManager(out var pm)) return;

            var item = _equippedItem;
            _equippedItem = null;

            ApplyEquipmentNetworkId(pm, 0);
            RemoveItem(item);
        }

        public void RemoveItemAtGrid(int itemID)
        {
            if (_equippedItem == null) return;
            if (_equippedItem.itemData.itemID != itemID) return;
            if (!TryGetValidPlayerManager(out var pm)) return;

            var item = _equippedItem;
            _equippedItem = null;

            ApplyEquipmentNetworkId(pm, 0);
            RemoveItem(item);
        }

        #endregion

        #region Equip/Unequip helpers

        private void SetEquippedItem(InventoryItem item) => _equippedItem = item;

        private void UnequipIfMatches(PlayerManager pm, InventoryItem picked)
        {
            // 그리드에서 실제로 뽑힌 아이템이 현재 장착 아이템이라면 해제
            if (_equippedItem == picked)
            {
                _equippedItem = null;
                ApplyEquipmentNetworkId(pm, 0);
            }
            else
            {
                // 혹시 모를 상태 불일치 대비: 그리드에서 빠졌는데 장착 상태면 해제
                ApplyEquipmentNetworkId(pm, 0);
                _equippedItem = null;
            }
        }

        private void UnequipCurrent(PlayerManager pm)
        {
            ApplyEquipmentNetworkId(pm, 0);
            _equippedItem = null;
        }

        #endregion

        #region Network apply

        private void ApplyEquipmentNetworkId(PlayerManager pm, int itemID)
        {
            var key = (itemType, itemType == ItemType.Weapon ? weaponSlotType : WeaponSlotType.None);

            if (!ApplyIdMap.TryGetValue(key, out var apply))
            {
                Debug.LogWarning($"No mapping for key: {key}");
                return;
            }

            apply(pm, itemID);

            // 양잡 해제 조건 처리
            TryReleaseTwoHandingIfNeeded(pm, itemID);
        }

        private void TryReleaseTwoHandingIfNeeded(PlayerManager pm, int itemID)
        {
            var net = pm.playerNetworkManager;
            if (net == null) return;

            if (!net.isTwoHandingWeapon.Value) return;

            // "현재 손에 들고 있는 무기가 사라지면 양잡 해제"
            if (itemType != ItemType.Weapon) return;
            if (itemID != 0)
            {
                net.isTwoHandingWeapon.Value = false;
                net.isTwoHandingRightWeapon.Value = false;
                net.isTwoHandingLeftWeapon.Value = false;
            }
            else
            {
                if (weaponSlotType == WeaponSlotType.Right && net.isTwoHandingRightWeapon.Value)
                {
                    net.isTwoHandingWeapon.Value = false;
                    net.isTwoHandingRightWeapon.Value = false;
                }
                else if (weaponSlotType == WeaponSlotType.Left && net.isTwoHandingLeftWeapon.Value)
                {
                    net.isTwoHandingWeapon.Value = false;
                    net.isTwoHandingLeftWeapon.Value = false;
                }
            }
        }

        #endregion

        #region PlayerManager

        private bool TryGetValidPlayerManager(out PlayerManager pm)
        {
            if (TryGetPlayerManager(out pm))
            {
                if (pm.playerNetworkManager != null) return true;

                Debug.LogError("NO PlayerNetworkManager");
                return false;
            }

            Debug.LogError("NO PLAYER MANAGER");
            return false;
        }

        private bool TryGetPlayerManager(out PlayerManager pm)
        {
            if (_playerManager != null)
            {
                pm = _playerManager;
                return true;
            }

            pm = null;

            var nm = NetworkManager.Singleton;
            if (nm == null || nm.LocalClient?.PlayerObject == null)
                return false;

            _playerManager = nm.LocalClient.PlayerObject.GetComponent<PlayerManager>();
            pm = _playerManager;

            return pm != null;
        }

        #endregion
    }
}
