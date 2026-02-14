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

        // (ItemType, WeaponSlotType) -> 네트워크 변수 세팅 함수
        private Dictionary<(ItemType, WeaponSlotType), Action<PlayerManager, int>> _applyIdMap;

        #region Mapping

        private void EnsureMappings()
        {
            if (_applyIdMap != null) return;

            _applyIdMap = new Dictionary<(ItemType, WeaponSlotType), Action<PlayerManager, int>>
            {
                // Weapon
                [(ItemType.Weapon, WeaponSlotType.Right)] = (pm, id) => pm.playerNetworkManager.currentRightHandWeaponID.Value = id,
                [(ItemType.Weapon, WeaponSlotType.Left)]  = (pm, id) => pm.playerNetworkManager.currentLeftHandWeaponID.Value = id,
                [(ItemType.Weapon, WeaponSlotType.Sub)]   = (pm, id) => pm.playerNetworkManager.currentSubWeaponID.Value = id,

                // Armor
                [(ItemType.Armor,    WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.bodyEquipmentID.Value = id,
                [(ItemType.Helmet,   WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.headEquipmentID.Value = id,
                [(ItemType.Gauntlet, WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.handEquipmentID.Value = id,
                [(ItemType.Leggings, WeaponSlotType.None)] = (pm, id) => pm.playerNetworkManager.legEquipmentID.Value = id,
            };
        }

        private void ApplyEquipmentNetworkId(int itemID)
        {
            EnsureMappings();

            if (!TryGetPlayerManager(out var pm))
            {
                Debug.LogError("NO PLAYER MANAGER");
                return;
            }

            if (pm.playerNetworkManager == null)
            {
                Debug.LogError("NO PlayerNetworkManager");
                return;
            }

            var key = itemType == ItemType.Weapon ? (itemType, weaponSlotType) : (itemType, WeaponSlotType.None);

            if (_applyIdMap.TryGetValue(key, out var apply))
            {
                apply(pm, itemID);
            }
            else
            {
                Debug.LogWarning($"No mapping for key: {key}");
            }
        }

        #endregion

        #region Core Logic

        public override bool CheckPlaceItem(InventoryItem inventoryItem, int posX, int posY)
        {
            if (!base.CheckPlaceItem(inventoryItem, posX, posY)) return false;
            if (inventoryItem == null || inventoryItem.itemData == null) return false;

            if (itemType != inventoryItem.itemData.itemType)
            {
                Debug.LogWarning($"Item Type Mismatch: {inventoryItem.itemData.itemType} / Grid: {itemType}");
                return false;
            }

            return true;
        }

        public override bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad)
        {
            if (_equippedItem != null)
                return false;

            if (!base.PlaceItem(inventoryItem, posX, posY, isLoad))
                return false;

            if (!TryGetPlayerManager(out _))
            {
                Debug.LogError("NO PLAYER MANAGER");
                return false;
            }

            _equippedItem = inventoryItem;

            ApplyEquipmentNetworkId(inventoryItem.itemData.itemID);

            return true;
        }

        public override InventoryItem PickUpItem(int x, int y)
        {
            var pickedItem = base.PickUpItem(x, y);
            if (pickedItem == null) return null;

            // 장착 해제
            ApplyEquipmentNetworkId(0);

            if (_equippedItem == pickedItem)
                _equippedItem = null;

            return pickedItem;
        }

        protected override void PlaceItemsAuto(List<int> setItemList, bool isLoad = true)
        {
            foreach (var itemCode in setItemList)
            {
                if (AddItemById(itemCode, isLoad: isLoad))
                    continue;

                InventoryController.DropItem(itemCode);

                // 자동 로드 실패 시 해제
                ApplyEquipmentNetworkId(0);
                _equippedItem = null;
            }
        }

        public override void ResetItemGrid()
        {
            if (_equippedItem == null)
                return;

            var item = _equippedItem;
            _equippedItem = null;

            ApplyEquipmentNetworkId(0);
            RemoveItem(item);
        }

        public void RemoveItemAtGrid(int itemID)
        {
            if (_equippedItem == null) return;
            if (_equippedItem.itemData.itemID != itemID) return;

            var item = _equippedItem;
            _equippedItem = null;

            ApplyEquipmentNetworkId(0);
            RemoveItem(item);
        }

        #endregion

        #region PlayerManager

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
