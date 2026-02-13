using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace BK.Inventory
{
    public class ItemGrid_Equipment : ItemGrid
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private ItemType itemType;
        
        private List<InventoryItem> _curEquipItem = new List<InventoryItem>();
        
        [Header("Only Weapon")]
        [SerializeField] private bool right;

        public override bool CheckPlaceItem(InventoryItem inventoryItem, int posX, int posY)
        {
            if (base.CheckPlaceItem(inventoryItem, posX, posY))
            {
                if (itemType != inventoryItem.itemData.itemType)
                {
                    Debug.LogWarning($"item Type Mismatch \n" +
                                     $"{inventoryItem.itemData.name} : {inventoryItem.itemData.itemType} \n" +
                                     $"inventory Type : {itemType}");
                    return false;
                }
                return true;
            }
            return false;
        }

        public override bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad)
        {
            if (itemType != ItemType.Consumables && _curEquipItem.Count > 0) return false;

            if (base.PlaceItem(inventoryItem, posX, posY, isLoad))
            {
                if (!GetPlayerManager())
                {
                    Debug.LogError("NO PLAYER MANAGER");
                }
                _curEquipItem.Add(inventoryItem);
                switch (itemType)
                {
                    case ItemType.Weapon:
                        if(right)
                            _playerManager.playerNetworkManager.currentRightHandWeaponID.Value = inventoryItem.itemData.itemID;
                        else
                            _playerManager.playerNetworkManager.currentLeftHandWeaponID.Value = inventoryItem.itemData.itemID;
                        break;
                    case ItemType.Armor:
                        _playerManager.playerNetworkManager.bodyEquipmentID.Value = inventoryItem.itemData.itemID;
                        break;
                    case ItemType.Helmet:
                        _playerManager.playerNetworkManager.headEquipmentID.Value = inventoryItem.itemData.itemID;
                        break;
                    case ItemType.Gauntlet:
                        _playerManager.playerNetworkManager.handEquipmentID.Value = inventoryItem.itemData.itemID;
                        break;
                    case ItemType.Leggings:
                        _playerManager.playerNetworkManager.legEquipmentID.Value = inventoryItem.itemData.itemID;
                        break;
                    
                    case ItemType.Consumables:
                        _playerManager.playerInventoryManager.currentQuickSlotIDList.Add(inventoryItem.itemData.itemID);
                        break;
                }

                return true;
            }

            return false;
        }

        protected override void PlaceItemsAuto(List<int> setItemList, bool isLoad = true)
        {
            foreach (var itemCode in setItemList)
            {
                if (AddItemById(itemCode, isLoad: isLoad)) continue;

                InventoryController.DropItem(itemCode);

                switch (itemType)
                {
                    case ItemType.Weapon:
                        if(right)
                            _playerManager.playerNetworkManager.currentRightHandWeaponID.Value = 0;
                        else
                            _playerManager.playerNetworkManager.currentLeftHandWeaponID.Value = 0;
                        break;
                    case ItemType.Armor:
                        _playerManager.playerNetworkManager.bodyEquipmentID.Value = 0;
                        break;
                    case ItemType.Helmet:
                        _playerManager.playerNetworkManager.headEquipmentID.Value = 0;
                        break;
                    case ItemType.Gauntlet:
                        _playerManager.playerNetworkManager.handEquipmentID.Value = 0;
                        break;
                    case ItemType.Leggings:
                        _playerManager.playerNetworkManager.legEquipmentID.Value = 0;
                        break;
                }
            }
        }

        public override InventoryItem PickUpItem(int x, int y)
        {
            InventoryItem pickUpItem = base.PickUpItem(x, y);
            if (pickUpItem == null) return null;

            if (!GetPlayerManager())
            {
                Debug.LogError("NO PLAYER MANAGER");
            }

            switch (itemType)
            {
                case ItemType.Weapon:
                    if(right)
                        _playerManager.playerNetworkManager.currentRightHandWeaponID.Value = 0;
                    else
                        _playerManager.playerNetworkManager.currentLeftHandWeaponID.Value = 0;
                    break;
                case ItemType.Armor:
                    _playerManager.playerNetworkManager.bodyEquipmentID.Value = 0;
                    break;
                case ItemType.Helmet:
                    _playerManager.playerNetworkManager.headEquipmentID.Value = 0;
                    break;
                case ItemType.Gauntlet:
                    _playerManager.playerNetworkManager.handEquipmentID.Value = 0;
                    break;
                case ItemType.Leggings:
                    _playerManager.playerNetworkManager.legEquipmentID.Value = 0;
                    break;
                case ItemType.Consumables:
                    _playerManager.playerInventoryManager.currentQuickSlotIDList.Remove(pickUpItem.itemData.itemID);
                    break;
            }

            _curEquipItem.Remove(pickUpItem);
            return pickUpItem;
        }

        public override void ResetItemGrid()
        {
            // 리스트를 복사한 후 반복문을 돌면서 안전하게 제거
            var itemsToRemove = new List<InventoryItem>(_curEquipItem);

            foreach (var inventoryItem in itemsToRemove)
            {
                _curEquipItem.Remove(inventoryItem); // 리스트에서 제거
                RemoveItem(inventoryItem); // 게임 오브젝트 삭제
            }
        }


        public void RemoveItemAtGrid(int itemID)
        {
            foreach (var inventoryItem in _curEquipItem)
            {
                if (inventoryItem.itemData.itemID != itemID) continue;

                _curEquipItem.Remove(inventoryItem);
                RemoveItem(inventoryItem);
                return;
            }
        }

        private bool GetPlayerManager()
        {
            return _playerManager == null ? _playerManager = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerManager>() : _playerManager;
        }
    }
}