using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


namespace BK.Inventory
{
    public class WorldPlayerInventory : Singleton<WorldPlayerInventory>
    {
        // InventoryHUDManager 에서 가져와서 처리하지만 루팅 가치등 연산은 해당 클래스에서 수행 
        // InventoryHUDManager는 시각적인 정보만 처리 
        [SerializeField] private IncomeEventBalance incomeEvent;
        public Variable<int> balance = new Variable<int>(-1);
        public ClampedVariable<float> itemWeight = new ClampedVariable<float>(0, 0, 500);

        [Header("Inventory Info")] [SerializeField]
        private int boxWidth;

        [SerializeField] private int boxHeight;

        private ItemGrid _itemGrid;
        private ItemGrid _backpackGrid;
        private ItemGrid_Equipment _itemGridEquipmentWeapon;
        private ItemGrid_Equipment _itemGridEquipmentHelmet;
        private ItemGrid_Equipment _itemGridEquipmentArmor;
        private ItemGrid_Equipment _itemGridEquipmentConsumable;
        private ItemGrid _safeItemGrid;

        private ItemGrid _shareItemGrid;

        public ItemGridType curOpenedInventory;
        public ItemGrid curInteractItemGrid;

        private Dictionary<int, int> _initialItemDict = new Dictionary<int, int>();
        private Dictionary<int, int> _exitItemDict = new Dictionary<int, int>();
        public Dictionary<int, int> finalItemDict = new Dictionary<int, int>();

        public int TotalLootValue { get; private set; }

        private void OnEnable()
        {
            curOpenedInventory = ItemGridType.None;
            StartCoroutine(InitializeWithTimeout(5f));
        }

        private IEnumerator InitializeWithTimeout(float timeout)
        {
            float elapsedTime = 0f;

            // PlayerUIManager.Instance가 준비될 때까지 기다림
            while (GUIController.Instance == null && elapsedTime < timeout)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (GUIController.Instance == null)
            {
                Debug.LogError("PlayerUIManager.Instance is not initialized within the timeout.");
                yield break;
            }

            // playerUIInventoryManager와 playerInventoryItemGrid가 준비될 때까지 기다림
            elapsedTime = 0f;
            while ((GUIController.Instance.inventoryGUIManager == null ||
                    GUIController.Instance.inventoryGUIManager.playerInventoryItemGrid == null) &&
                   elapsedTime < timeout)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if (GUIController.Instance.inventoryGUIManager == null ||
                GUIController.Instance.inventoryGUIManager.playerInventoryItemGrid == null)
            {
                Debug.LogError("PlayerUIInventoryManager or InventoryItemGrid is not initialized within the timeout.");
                yield break;
            }

            _itemGrid = GUIController.Instance.inventoryGUIManager.playerInventoryItemGrid;
            _backpackGrid = GUIController.Instance.inventoryGUIManager.backpackItemGrid;
            _shareItemGrid = GUIController.Instance.inventoryGUIManager.shareInventoryItemGrid;
            _itemGridEquipmentWeapon = GUIController.Instance.inventoryGUIManager.playerWeapon;
            _itemGridEquipmentHelmet = GUIController.Instance.inventoryGUIManager.playerHelmet;
            _itemGridEquipmentArmor = GUIController.Instance.inventoryGUIManager.playerArmor;
            _itemGridEquipmentConsumable = GUIController.Instance.inventoryGUIManager.playerConsumable;
            _safeItemGrid = GUIController.Instance.inventoryGUIManager.safeInventoryItemGrid;

            _itemGrid.itemGridWeight.OnValueChanged += UpdateWeight;
            _backpackGrid.itemGridWeight.OnValueChanged += UpdateWeight;
            _itemGridEquipmentWeapon.itemGridWeight.OnValueChanged += UpdateWeight;
            _itemGridEquipmentHelmet.itemGridWeight.OnValueChanged += UpdateWeight;
            _itemGridEquipmentArmor.itemGridWeight.OnValueChanged += UpdateWeight;
            _itemGridEquipmentConsumable.itemGridWeight.OnValueChanged += UpdateWeight;
            _safeItemGrid.itemGridWeight.OnValueChanged += UpdateWeight;
        }

        private void OnDisable()
        {
            // OnDisable에서 이벤트 해제
            if (GetInventory() != null)
            {
                GetInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            if (GetBackpackInventory() != null)
            {
                GetBackpackInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            if (GetWeaponInventory() != null)
            {
                GetWeaponInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            if (GetHelmetInventory() != null)
            {
                GetHelmetInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            if (GetArmorInventory() != null)
            {
                GetArmorInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            if (GetConsumableInventory() != null)
            {
                GetConsumableInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            if (GetSafeInventory() != null)
            {
                GetSafeInventory().itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            balance.ClearAllSubscribers();
        }

        public bool TrySpend(int cost)
        {
            if (balance.Value < cost) return false;

            balance.Value -= cost;
            return true;
        }

        // buyObject에 대한 requireItem을 제거 
        public bool SpendItemInInventory(ItemData buyObject)
        {
            // 먼저 필요한 아이템이 충분한지 확인
            if (!CheckItemInInventoryToChangeItem(buyObject))
                return false;

            // 트랜잭션 생성
            var transaction = new Dictionary<ItemGrid, Dictionary<int, int>>();

            try
            {
                // 각 아이템별로 Remove 수행 (트랜잭션 기록은 내부에서 처리)
                foreach (var (itemId, requiredCount) in buyObject.GetCostDict())
                {
                    RemoveItemInInventory(itemId, requiredCount, transaction);
                }

                // 전부 성공했으면 true
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"아이템 소비 실패: {ex.Message}");
                RollbackTransaction(transaction);
                return false;
            }
        }

        // itemId에 해당하는 아이템을 requiredCount 만큼 제거
        private bool RemoveItemInInventory(int itemId, int requiredCount,
            Dictionary<ItemGrid, Dictionary<int, int>> transaction)
        {
            if (GetItemCountInAllInventory(itemId) < requiredCount)
                return false;

            ItemGrid share = GetShareInventory();
            ItemGrid inventory = GetInventory();
            ItemGrid backpack = GetBackpackInventory();

            int remainingToRemove = requiredCount;

            int removedFromShare = share.RemoveItemsById(itemId, remainingToRemove);
            if (removedFromShare > 0)
            {
                if (!transaction.ContainsKey(share))
                    transaction[share] = new Dictionary<int, int>();

                if (!transaction[share].ContainsKey(itemId))
                    transaction[share][itemId] = 0;

                transaction[share][itemId] += removedFromShare;

                remainingToRemove -= removedFromShare;
            }

            // 인벤토리에서 제거
            if (remainingToRemove > 0)
            {
                int removedFromInventory = inventory.RemoveItemsById(itemId, remainingToRemove);
                if (removedFromInventory > 0)
                {
                    if (!transaction.ContainsKey(inventory))
                        transaction[inventory] = new Dictionary<int, int>();

                    if (!transaction[inventory].ContainsKey(itemId))
                        transaction[inventory][itemId] = 0;

                    transaction[inventory][itemId] += removedFromInventory;

                    remainingToRemove -= removedFromInventory;
                }
            }

            // 백팩에서 제거
            if (remainingToRemove > 0)
            {
                int removedFromBackpack = backpack.RemoveItemsById(itemId, remainingToRemove);
                if (removedFromBackpack > 0)
                {
                    if (!transaction.ContainsKey(backpack))
                        transaction[backpack] = new Dictionary<int, int>();

                    if (!transaction[backpack].ContainsKey(itemId))
                        transaction[backpack][itemId] = 0;

                    transaction[backpack][itemId] += removedFromBackpack;

                    remainingToRemove -= removedFromBackpack;
                }
            }

            /*
            WorldSaveGameManager.Instance.currentGameData.shareInventoryItems.Clear();
            foreach (var pair in GetShareInventory().GetCurItemDictById())
            {
                WorldSaveGameManager.Instance.currentGameData.shareInventoryItems.Add(pair.Key, pair.Value);
            }
            */

            if (remainingToRemove > 0)
            {
                throw new InsufficientItemsException(itemId, remainingToRemove);
            }

            return true;
        }

        // RemoveItemInInventory transaction ver

        public bool RemoveItemInInventory(int itemId, int requiredCount = 1)
        {
            var transaction = new Dictionary<ItemGrid, Dictionary<int, int>>();

            try
            {
                return RemoveItemInInventory(itemId, requiredCount, transaction);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"자동 복구 수행 중: {ex.Message}");
                RollbackTransaction(transaction);
                return false;
            }
        }


        // 인벤토리와 백팩에 필요한 모든 아이템이 있는지 확인
        public bool CheckItemInInventoryToChangeItem(ItemData buyObject)
        {
            foreach (var (itemId, requiredCount) in buyObject.GetCostDict())
            {
                int totalCount = GetItemCountInAllInventory(itemId);
                if (totalCount < requiredCount)
                {
                    Debug.LogWarning($"아이템 부족: {itemId} / 필요: {requiredCount}, 보유: {totalCount}");
                    return false;
                }
            }

            return true;
        }

        public bool CheckItemInInventory(int itemCode)
        {
            return GetItemCountInAllInventory(itemCode) > 0;
        }

        public int GetItemCountInAllInventory(int itemCode)
        {
            ItemGrid inventory = GetInventory();
            ItemGrid backpack = GetBackpackInventory();
            ItemGrid shareInventory = GetShareInventory();

            int countInInventory = inventory.GetItemCountById(itemCode);
            int countInBackpack = backpack.GetItemCountById(itemCode);
            int countInShareInventory = shareInventory.GetItemCountById(itemCode);
            return countInInventory + countInBackpack + countInShareInventory;
        }

        // 트랜잭션 롤백 헬퍼 메서드
        private void RollbackTransaction(Dictionary<ItemGrid, Dictionary<int, int>> transaction)
        {
            foreach (var (grid, items) in transaction)
            {
                foreach (var (itemId, count) in items)
                {
                    grid.AddItemById(itemId, count);
                }
            }
        }


        public ItemGrid GetInventory()
        {
            return _itemGrid == null
                ? _itemGrid = GUIController.Instance.inventoryGUIManager.playerInventoryItemGrid
                : _itemGrid;
        }

        public ItemGrid GetBackpackInventory()
        {
            return _backpackGrid == null
                ? _backpackGrid = GUIController.Instance.inventoryGUIManager.backpackItemGrid
                : _backpackGrid;
        }

        public ItemGrid GetShareInventory()
        {
            return _shareItemGrid == null
                ? _shareItemGrid = GUIController.Instance.inventoryGUIManager.shareInventoryItemGrid
                : _shareItemGrid;
        }

        public ItemGrid_Equipment GetWeaponInventory()
        {
            return _itemGridEquipmentWeapon == null
                ? _itemGridEquipmentWeapon = GUIController.Instance.inventoryGUIManager.playerWeapon
                : _itemGridEquipmentWeapon;
        }

        public ItemGrid_Equipment GetHelmetInventory()
        {
            return _itemGridEquipmentHelmet == null
                ? _itemGridEquipmentHelmet = GUIController.Instance.inventoryGUIManager.playerHelmet
                : _itemGridEquipmentHelmet;
        }

        public ItemGrid_Equipment GetArmorInventory()
        {
            return _itemGridEquipmentArmor == null
                ? _itemGridEquipmentArmor = GUIController.Instance.inventoryGUIManager.playerArmor
                : _itemGridEquipmentArmor;
        }

        public ItemGrid_Equipment GetConsumableInventory()
        {
            return _itemGridEquipmentConsumable == null
                ? _itemGridEquipmentConsumable = GUIController.Instance.inventoryGUIManager.playerConsumable
                : _itemGridEquipmentConsumable;
        }

        public ItemGrid GetSafeInventory()
        {
            return _safeItemGrid == null
                ? _safeItemGrid = GUIController.Instance.inventoryGUIManager.safeInventoryItemGrid
                : _safeItemGrid;
        }

        public bool AddItem(GameObject item) =>
            GetInventory().AddItem(item, false) ||
            GetBackpackInventory().AddItem(item, false);

        public int AddItemById(int itemCode, int itemCnt)
        {
            int remaining = itemCnt; // 첫 번째 인벤토리에 넣을 목표 개수
            remaining = GetInventory().AddItemById_FailCount(itemCode, remaining, false);

            if (remaining > 0)
                remaining = GetBackpackInventory().AddItemById_FailCount(itemCode, remaining, false);

            if (remaining > 0)
                remaining = GetShareInventory().AddItemById_FailCount(itemCode, remaining, false);

            return remaining;
        }

        public bool ReloadItemShareBox(ItemInfo itemInfoData)
        {
            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetShareInventory().AddItem(item);
        }

        public bool ReloadItemInventory(ItemInfo itemInfoData)
        {
            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetInventory().AddItem(item);
        }

        public bool ReloadItemBackpack(ItemInfo itemInfoData)
        {
            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetBackpackInventory().AddItem(item);
        }

        public bool ReloadItemWeapon(ItemInfo itemInfoData)
        {
            if (itemInfoData.itemCode == 0) return true;

            // 인벤토리에 해당 무기 추가 
            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetWeaponInventory().AddItem(item);
        }

        public bool ReloadItemHelmet(ItemInfo itemInfoData)
        {
            if (itemInfoData.itemCode == 0) return true;
            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetHelmetInventory().AddItem(item);
        }

        public bool ReloadItemArmor(ItemInfo itemInfoData)
        {
            if (itemInfoData.itemCode == 0) return true;

            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetArmorInventory().AddItem(item);
        }

        public bool ReloadItemQuickSlot(ItemInfo itemInfoData)
        {
            if (itemInfoData.itemCode == 0) return true;

            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return GetConsumableInventory().AddItem(item);
        }

        public bool ReloadItemSafe(ItemInfo itemInfoData)
        {
            GameObject item = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();

            return GetSafeInventory().AddItem(item);
        }

        private void UpdateWeight(float newValue)
        {
            float newItemWeight = 0;
            newItemWeight += _itemGrid.itemGridWeight.Value;
            newItemWeight += _itemGridEquipmentWeapon.itemGridWeight.Value;
            newItemWeight += _itemGridEquipmentHelmet.itemGridWeight.Value;
            newItemWeight += _itemGridEquipmentArmor.itemGridWeight.Value;
            newItemWeight += _itemGridEquipmentConsumable.itemGridWeight.Value;
            newItemWeight += _safeItemGrid.itemGridWeight.Value;
            itemWeight.Value = newItemWeight;
        }

        public void SetStartItemValue()
        {
            _initialItemDict.Clear();
            MergeItemValue(_initialItemDict, _itemGrid);
            MergeItemValue(_initialItemDict, _backpackGrid);
            MergeItemValue(_initialItemDict, _itemGridEquipmentWeapon);
            MergeItemValue(_initialItemDict, _itemGridEquipmentHelmet);
            MergeItemValue(_initialItemDict, _itemGridEquipmentArmor);
            MergeItemValue(_initialItemDict, _itemGridEquipmentConsumable);
            MergeItemValue(_initialItemDict, _safeItemGrid);
        }

        private void MergeItemValue(Dictionary<int, int> dict, ItemGrid grid)
        {
            var itemDict = grid.GetCurItemDictById();
            foreach (var kvp in itemDict)
            {
                if (dict.ContainsKey(kvp.Key))
                    dict[kvp.Key] += kvp.Value;
                else
                    dict[kvp.Key] = kvp.Value;
            }
        }

        private void SetExitItemValue()
        {
            _exitItemDict.Clear();
            MergeItemValue(_exitItemDict, _itemGrid);
            MergeItemValue(_exitItemDict, _backpackGrid);
            MergeItemValue(_exitItemDict, _itemGridEquipmentWeapon);
            MergeItemValue(_exitItemDict, _itemGridEquipmentHelmet);
            MergeItemValue(_exitItemDict, _itemGridEquipmentArmor);
            MergeItemValue(_exitItemDict, _itemGridEquipmentConsumable);
            MergeItemValue(_exitItemDict, _safeItemGrid);
        }

        public void CalculateFinalLoot()
        {
            SetExitItemValue();
            finalItemDict.Clear();
            TotalLootValue = 0;
            foreach (var kvp in _exitItemDict)
            {
                int itemId = kvp.Key;
                int exitCount = kvp.Value;

                int initialCount = 0;
                _initialItemDict.TryGetValue(itemId, out initialCount);

                int newCount = exitCount - initialCount;
                if (newCount > 0)
                {
                    finalItemDict[itemId] = newCount;
                    TotalLootValue += WorldDatabase_Item.Instance.GetItemByID(itemId).purchaseCost;
                }
            }
        }

        public int GetMostValuableItem()
        {
            int maxValue = 0;
            int maxValueItemId = 0;
            foreach (var itemId in finalItemDict.Keys)
            {
                int curItemPrice = WorldDatabase_Item.Instance.GetItemByID(itemId).purchaseCost;
                if (maxValue < curItemPrice)
                {
                    maxValue = curItemPrice;
                    maxValueItemId = itemId;
                }
            }

            return maxValueItemId;
        }
    }
}

public class InsufficientItemsException : Exception
{
    public int ItemId { get; }
    public int RemainingCount { get; }
    
    public InsufficientItemsException(int itemId, int remainingCount)
        : base($"아이템 ID {itemId} 부족: 추가로 {remainingCount}개 필요")
    {
        ItemId = itemId;
        RemainingCount = remainingCount;
    }
}