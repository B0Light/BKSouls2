using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK.Inventory
{
    public class WorldPlayerInventory : Singleton<WorldPlayerInventory>
    {
        [SerializeField] private IncomeEventBalance incomeEvent;

        public Variable<int> balance = new Variable<int>(-1);
        public ClampedVariable<float> itemWeight = new ClampedVariable<float>(0, 0, 500);

        [Header("Inventory Info")]
        [SerializeField] private int boxWidth;
        [SerializeField] private int boxHeight;

        // Grids
        private ItemGrid _itemGrid;
        private ItemGrid _backpackGrid;
        private ItemGrid _safeItemGrid;
        private ItemGrid _shareItemGrid;

        private ItemGrid_Equipment _rightWeapon;
        private ItemGrid_Equipment _leftWeapon;
        private ItemGrid_Equipment _rightSubWeapon;
        private ItemGrid_Equipment _leftSubWeapon;

        private ItemGrid_Equipment _helmet;
        private ItemGrid_Equipment _armor;
        private ItemGrid_Equipment _gauntlet;
        private ItemGrid_Equipment _leggings;

        public ItemGridType curOpenedInventory;
        public ItemGrid curInteractItemGrid;

        private readonly Dictionary<int, int> _initialItemDict = new();
        private readonly Dictionary<int, int> _exitItemDict = new();
        public readonly Dictionary<int, int> finalItemDict = new();

        public int TotalLootValue { get; private set; }

        public event Action OnInventoryChanged;

        //반복 처리를 위한 컬렉션
        private readonly List<ItemGrid> _mainGrids = new();
        private readonly List<ItemGrid> _weightGrids = new();
        private readonly List<ItemGrid> _lootGrids = new();
        private readonly List<ItemGrid> _removePriorityGrids = new();

        private void OnEnable()
        {
            curOpenedInventory = ItemGridType.None;
            StartCoroutine(InitializeWithTimeout(5f));
        }

        private IEnumerator InitializeWithTimeout(float timeout)
        {
            float elapsed = 0f;

            // GUIController 준비 대기
            while (GUIController.Instance == null && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (GUIController.Instance == null)
            {
                Debug.LogError("GUIController.Instance is not initialized within the timeout.");
                yield break;
            }

            // InventoryGUIManager 준비 대기
            elapsed = 0f;
            while ((GUIController.Instance.inventoryGUIManager == null ||
                    GUIController.Instance.inventoryGUIManager.playerInventoryItemGrid == null) &&
                   elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            var gui = GUIController.Instance.inventoryGUIManager;
            if (gui == null || gui.playerInventoryItemGrid == null)
            {
                Debug.LogError("InventoryGUIManager or playerInventoryItemGrid is not initialized within the timeout.");
                yield break;
            }
            
            _itemGrid = gui.playerInventoryItemGrid;
            _backpackGrid = gui.backpackItemGrid;
            _shareItemGrid = gui.shareInventoryItemGrid;
            _safeItemGrid = gui.safeInventoryItemGrid;

            _rightWeapon = gui.playerRightWeapon;
            _leftWeapon = gui.playerLeftWeapon;
            _rightSubWeapon = gui.playerRightSubWeapon;
            _leftSubWeapon = gui.playerLeftSubWeapon;

            _helmet = gui.playerHelmet;
            _armor = gui.playerArmor;
            _gauntlet = gui.playerGauntlet;
            _leggings = gui.playerLeggings;

            // 리스트 구성 및 이벤트 구독
            BuildGridLists();
            SubscribeWeightEvents();

            // 초기 weight 계산 한번
            RecalculateWeight();
        }

        private void OnDisable()
        {
            UnsubscribeWeightEvents();
            balance.ClearAllSubscribers();
        }

        #region Grid Lists & Events

        private void BuildGridLists()
        {
            _weightGrids.Clear();
            _lootGrids.Clear();
            _removePriorityGrids.Clear();

            // weight 계산에 포함될 그리드들
            AddIfNotNull(_weightGrids, _itemGrid);
            AddIfNotNull(_weightGrids, _backpackGrid);
            AddIfNotNull(_weightGrids, _rightWeapon);
            AddIfNotNull(_weightGrids, _leftWeapon);
            AddIfNotNull(_weightGrids, _rightSubWeapon);
            AddIfNotNull(_weightGrids, _leftSubWeapon);
            AddIfNotNull(_weightGrids, _helmet);
            AddIfNotNull(_weightGrids, _armor);
            AddIfNotNull(_weightGrids, _gauntlet);
            AddIfNotNull(_weightGrids, _leggings);
            AddIfNotNull(_weightGrids, _safeItemGrid);

            // 메인 그리드: weight 계산과 포션 갯수 계산에 포함될 그리드들
            AddIfNotNull(_mainGrids, _itemGrid);
            AddIfNotNull(_mainGrids, _backpackGrid);

            // 루팅 가치 계산에 포함될 그리드들
            _lootGrids.AddRange(_weightGrids);

            // 제거 우선순위: share -> inventory -> backpack (기존 로직과 동일)
            AddIfNotNull(_removePriorityGrids, _shareItemGrid);
            AddIfNotNull(_removePriorityGrids, _itemGrid);
            AddIfNotNull(_removePriorityGrids, _backpackGrid);
        }

        private static void AddIfNotNull(List<ItemGrid> list, ItemGrid grid)
        {
            if (grid != null) list.Add(grid);
        }

        private void SubscribeWeightEvents()
        {
            foreach (var grid in _weightGrids)
            {
                if (grid == null) continue;
                grid.itemGridWeight.OnValueChanged += UpdateWeight;
            }

            foreach (var grid in _mainGrids)
            {
                if (grid == null) continue;
                grid.itemGridWeight.OnValueChanged += UpdatePotionCount;
            }
        }

        private void UnsubscribeWeightEvents()
        {
            foreach (var grid in _weightGrids)
            {
                if (grid == null) continue;
                grid.itemGridWeight.OnValueChanged -= UpdateWeight;
            }

            foreach (var grid in _mainGrids)
            {
                if (grid == null) continue;
                grid.itemGridWeight.OnValueChanged -= UpdatePotionCount;
            }
        }

        private void UpdateWeight(float _)
        {
            RecalculateWeight();
        }

        private void RecalculateWeight()
        {
            float total = 0f;
            foreach (var grid in _weightGrids)
            {
                if (grid == null) continue;
                total += grid.itemGridWeight.Value;
            }
            itemWeight.Value = total;
        }

        private void UpdatePotionCount(float _)
        {
            // 포션은 무게가 0 이 아님
            // 포션의 갯수가 변동되면 인벤토리 변동 이벤트 발생 -> playerGUIManager에서 포션 갯수 업데이트
            OnInventoryChanged?.Invoke();
        }

        #endregion

        #region Money

        public bool TrySpend(int cost)
        {
            if (balance.Value < cost) return false;
            balance.Value -= cost;
            return true;
        }

        #endregion

        #region Transaction Remove (Method 1 유지)

        public bool RemoveItemInInventory(int itemId, int requiredCount = 1)
        {
            var transaction = new Dictionary<ItemGrid, Dictionary<int, int>>();

            try
            {
                bool result = RemoveItemInGrids(itemId, requiredCount, _removePriorityGrids, transaction);
                if (result) OnInventoryChanged?.Invoke();
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"자동 복구 수행 중: {ex.Message}");
                RollbackTransaction(transaction);
                return false;
            }
        }

        public bool RemoveItemInInventoryAndBackpack(int itemId, int requiredCount = 1)
        {
            var transaction = new Dictionary<ItemGrid, Dictionary<int, int>>();
            var saleSourceGrids = new List<ItemGrid> { GetInventory(), GetBackpackInventory() };

            try
            {
                bool result = RemoveItemInGrids(itemId, requiredCount, saleSourceGrids, transaction);
                if (result) OnInventoryChanged?.Invoke();
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
                RollbackTransaction(transaction);
                return false;
            }
        }

        private bool RemoveItemInGrids(int itemId, int requiredCount, List<ItemGrid> grids,
            Dictionary<ItemGrid, Dictionary<int, int>> transaction)
        {
            if (GetItemCountInGrids(itemId, grids) < requiredCount)
                return false;

            int remaining = requiredCount;

            foreach (var grid in grids)
            {
                if (grid == null || remaining <= 0) break;

                int removed = grid.RemoveItemsById(itemId, remaining);
                if (removed <= 0) continue;

                if (!transaction.TryGetValue(grid, out var itemMap))
                {
                    itemMap = new Dictionary<int, int>();
                    transaction[grid] = itemMap;
                }

                itemMap[itemId] = itemMap.TryGetValue(itemId, out var cur) ? cur + removed : removed;
                remaining -= removed;
            }

            if (remaining > 0)
                throw new InsufficientItemsException(itemId, remaining);

            return true;
        }
        

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

        public bool CheckItemInInventory(int itemCode, int amount = 1) => GetItemCountInAllInventory(itemCode) >= amount;
        public bool CheckItemInInventoryAndBackpack(int itemCode, int amount = 1) => GetItemCountInInventoryAndBackpack(itemCode) >= amount;

        public int GetItemCountInAllInventory(int itemCode)
        {
            int sum = 0;
            if (GetInventory() != null) sum += GetInventory().GetItemCountById(itemCode);
            if (GetBackpackInventory() != null) sum += GetBackpackInventory().GetItemCountById(itemCode);
            if (GetShareInventory() != null) sum += GetShareInventory().GetItemCountById(itemCode);
            return sum;
        }

        // 인벤토리에서 특정 ProjectileClass(Arrow/Bolt)에 해당하는 화살을 찾아 반환
        public int GetItemCountInInventoryAndBackpack(int itemCode)
        {
            return GetItemCountInGrids(itemCode, new List<ItemGrid> { GetInventory(), GetBackpackInventory() });
        }

        private int GetItemCountInGrids(int itemCode, List<ItemGrid> grids)
        {
            int sum = 0;

            foreach (var grid in grids)
            {
                if (grid == null) continue;
                sum += grid.GetItemCountById(itemCode);
            }

            return sum;
        }

        public RangedProjectileItem FindProjectileInInventory(ProjectileClass projectileClass)
        {
            var grids = new List<ItemGrid> { GetInventory(), GetBackpackInventory(), GetShareInventory() };

            foreach (var grid in grids)
            {
                if (grid == null) continue;

                var itemDict = grid.GetCurItemDictById();
                foreach (var itemId in itemDict.Keys)
                {
                    if (itemDict[itemId] <= 0) continue;

                    var projectile = WorldItemDatabase.Instance.GetProjectileByID(itemId);
                    if (projectile != null && projectile.projectileClass == projectileClass)
                        return projectile;
                }
            }

            return null;
        }

        #endregion

        #region Getters

        public ItemGrid GetInventory() => _itemGrid ??= GUIController.Instance.inventoryGUIManager.playerInventoryItemGrid;
        public ItemGrid GetBackpackInventory() => _backpackGrid ??= GUIController.Instance.inventoryGUIManager.backpackItemGrid;
        public ItemGrid GetShareInventory() => _shareItemGrid ??= GUIController.Instance.inventoryGUIManager.shareInventoryItemGrid;
        public ItemGrid GetSafeInventory() => _safeItemGrid ??= GUIController.Instance.inventoryGUIManager.safeInventoryItemGrid;

        public ItemGrid_Equipment GetRightWeaponInventory() => _rightWeapon ??= GUIController.Instance.inventoryGUIManager.playerRightWeapon;
        public ItemGrid_Equipment GetLeftWeaponInventory() => _leftWeapon ??= GUIController.Instance.inventoryGUIManager.playerLeftWeapon;
        public ItemGrid_Equipment GetRightSubWeaponInventory() => _rightSubWeapon ??= GUIController.Instance.inventoryGUIManager.playerRightSubWeapon;
        public ItemGrid_Equipment GetLeftSubWeaponInventory() => _leftSubWeapon ??= GUIController.Instance.inventoryGUIManager.playerLeftSubWeapon;
        public ItemGrid_Equipment GetHelmetInventory() => _helmet ??= GUIController.Instance.inventoryGUIManager.playerHelmet;
        public ItemGrid_Equipment GetArmorInventory() => _armor ??= GUIController.Instance.inventoryGUIManager.playerArmor;
        public ItemGrid_Equipment GetGauntletInventory() => _gauntlet ??= GUIController.Instance.inventoryGUIManager.playerGauntlet;
        public ItemGrid_Equipment GetLeggingsInventory() => _leggings ??= GUIController.Instance.inventoryGUIManager.playerLeggings;

        #endregion

        #region Dungeon

        public void ClearInventoryAndBackpack()
        {
            GetInventory()?.ResetItemGrid();
            GetBackpackInventory()?.ResetItemGrid();

            CharacterSaveData currentCharacterData = WorldSaveGameManager.Instance.currentCharacterData;
            currentCharacterData?.inventoryItems?.Clear();
            currentCharacterData?.backpackItems?.Clear();

            OnInventoryChanged?.Invoke();
        }

        public void ClearEquipmentSlots()
        {
            GetRightWeaponInventory()?.ResetItemGrid();
            GetLeftWeaponInventory()?.ResetItemGrid();
            GetRightSubWeaponInventory()?.ResetItemGrid();
            GetLeftSubWeaponInventory()?.ResetItemGrid();

            GetHelmetInventory()?.ResetItemGrid();
            GetArmorInventory()?.ResetItemGrid();
            GetGauntletInventory()?.ResetItemGrid();
            GetLeggingsInventory()?.ResetItemGrid();

            GUIController.Instance.inventoryGUIManager.ToggleBackpackInventory(false);
            GUIController.Instance.inventoryGUIManager.backpackItemGrid.UpdateItemGridSize(Vector2Int.zero);

            GUIController.Instance.localPlayer?.playerEquipmentManager?.ClearEquipmentSlots();
            ClearEquipmentSlotsFromCurrentCharacterData();

            OnInventoryChanged?.Invoke();
        }

        private void ClearEquipmentSlotsFromCurrentCharacterData()
        {
            CharacterSaveData currentCharacterData = WorldSaveGameManager.Instance.currentCharacterData;
            if (currentCharacterData == null) return;

            currentCharacterData.rightMainWeaponItemCode = 0;
            currentCharacterData.leftMainWeaponItemCode = 0;
            currentCharacterData.rightSubWeaponItemCode = 0;
            currentCharacterData.leftSubWeaponItemCode = 0;

            currentCharacterData.helmetItemCode = -1;
            currentCharacterData.armorItemCode = -1;
            currentCharacterData.gauntletItemCode = -1;
            currentCharacterData.leggingsItemCode = -1;
        }

        public void SaveInventoryAndBackpackToCurrentCharacterData()
        {
            CharacterSaveData currentCharacterData = WorldSaveGameManager.Instance.currentCharacterData;
            if (currentCharacterData == null) return;

            currentCharacterData.inventoryItems.Clear();
            currentCharacterData.backpackItems.Clear();

            CopyGridItemCounts(GetInventory(), currentCharacterData.inventoryItems);
            CopyGridItemCounts(GetBackpackInventory(), currentCharacterData.backpackItems);
        }

        private void CopyGridItemCounts(ItemGrid sourceGrid, SerializableDictionary<int, int> targetItems)
        {
            if (sourceGrid == null || targetItems == null) return;

            foreach (var pair in sourceGrid.GetCurItemDictById())
            {
                targetItems[pair.Key] = pair.Value;
            }
        }

        public void MoveInventoryToShare()
        {
            var inventory = GetInventory();
            var backpack = GetBackpackInventory();
            var share = GetShareInventory();

            if (share == null) return;

            MoveGridToShare(inventory, share);
            MoveGridToShare(backpack, share);
        }

        private void MoveGridToShare(ItemGrid source, ItemGrid share)
        {
            if (source == null) return;

            var itemsToMove = new Dictionary<int, int>(source.GetCurItemDictById());
            foreach (var (itemId, count) in itemsToMove)
            {
                source.RemoveItemsById(itemId, count);
                share.AddItemById(itemId, count, false);
            }
        }

        #endregion

        #region Add Item

        public bool AddItem(GameObject item)
        {
            bool result = GetInventory().AddItem(item, false) || GetBackpackInventory().AddItem(item, false);
            if (result) OnInventoryChanged?.Invoke();
            return result;
        }

        public int AddItemById(int itemCode, int itemCnt)
        {
            int remaining = itemCnt;
            remaining = GetInventory().AddItemById_FailCount(itemCode, remaining, false);

            if (remaining > 0)
                remaining = GetBackpackInventory().AddItemById_FailCount(itemCode, remaining, false);

            if (remaining > 0)
                remaining = GetShareInventory().AddItemById_FailCount(itemCode, remaining, false);

            if (remaining < itemCnt) OnInventoryChanged?.Invoke();
            return remaining;
        }

        #endregion

        #region Reload Helpers (중복 제거 핵심)

        private InventoryItem CreateInventoryItem(Item itemInfoData)
        {
            GameObject go = Instantiate(WorldShopManager.Instance.inventoryItemRef);
            var inventoryItem = go.GetComponent<InventoryItem>();
            inventoryItem.itemData = itemInfoData as GridItem;
            inventoryItem.Set();
            return inventoryItem;
        }

        private bool ReloadToGrid(ItemGrid targetGrid, Item itemInfoData, bool skipIfZeroId)
        {
            if (targetGrid == null) return false;
            if (itemInfoData == null) return false;
            if (skipIfZeroId && itemInfoData.itemID == 0) return true;
            targetGrid.ResetItemGrid();
            var invItem = CreateInventoryItem(itemInfoData);
            return targetGrid.AddItem(invItem.gameObject);
        }

        public bool ReloadItemShareBox(Item itemInfoData) => ReloadToGrid(GetShareInventory(), itemInfoData, skipIfZeroId: false);
        public bool ReloadItemInventory(Item itemInfoData) => ReloadToGrid(GetInventory(), itemInfoData, skipIfZeroId: false);
        public bool ReloadItemBackpack(Item itemInfoData) => ReloadToGrid(GetBackpackInventory(), itemInfoData, skipIfZeroId: false);

        public bool ReloadItemRightMainWeapon(Item itemInfoData) => ReloadToGrid(GetRightWeaponInventory(), itemInfoData, skipIfZeroId: true);
        public bool ReloadItemLeftMainWeapon(Item itemInfoData) => ReloadToGrid(GetLeftWeaponInventory(), itemInfoData, skipIfZeroId: true);
        public bool ReloadItemRightSubWeapon(Item itemInfoData) => ReloadToGrid(GetRightSubWeaponInventory(), itemInfoData, skipIfZeroId: true);
        
        public bool ReloadItemLeftSubWeapon(Item itemInfoData) => ReloadToGrid(GetLeftSubWeaponInventory(), itemInfoData, skipIfZeroId: true);

        public bool ReloadItemHelmet(Item itemInfoData) => ReloadToGrid(GetHelmetInventory(), itemInfoData, skipIfZeroId: true);
        public bool ReloadItemArmor(Item itemInfoData) => ReloadToGrid(GetArmorInventory(), itemInfoData, skipIfZeroId: true);
        public bool ReloadItemGauntlet(Item itemInfoData) => ReloadToGrid(GetGauntletInventory(), itemInfoData, skipIfZeroId: true);
        public bool ReloadItemLeggings(Item itemInfoData) => ReloadToGrid(GetLeggingsInventory(), itemInfoData, skipIfZeroId: true);

        public bool ReloadItemSafe(Item itemInfoData) => ReloadToGrid(GetSafeInventory(), itemInfoData, skipIfZeroId: false);

        #endregion

        #region Loot Value Calculation

        public void SetStartItemValue()
        {
            _initialItemDict.Clear();
            foreach (var grid in _lootGrids)
            {
                MergeItemValue(_initialItemDict, grid);
            }
        }

        private void SetExitItemValue()
        {
            _exitItemDict.Clear();
            foreach (var grid in _lootGrids)
            {
                MergeItemValue(_exitItemDict, grid);
            }
        }

        private void MergeItemValue(Dictionary<int, int> dict, ItemGrid grid)
        {
            if (grid == null) return;

            var itemDict = grid.GetCurItemDictById();
            foreach (var (itemId, count) in itemDict)
            {
                dict[itemId] = dict.TryGetValue(itemId, out var cur) ? cur + count : count;
            }
        }

        public void CalculateFinalLoot()
        {
            SetExitItemValue();
            finalItemDict.Clear();
            TotalLootValue = 0;

            foreach (var (itemId, exitCount) in _exitItemDict)
            {
                _initialItemDict.TryGetValue(itemId, out int initialCount);

                int newCount = exitCount - initialCount;
                if (newCount <= 0) continue;

                finalItemDict[itemId] = newCount;

                TotalLootValue += WorldItemDatabase.Instance.GetItemByID(itemId).cost * newCount;
            }
        }

        public int GetMostValuableItem()
        {
            int maxValue = 0;
            int maxValueItemId = 0;

            foreach (var itemId in finalItemDict.Keys)
            {
                int price = WorldItemDatabase.Instance.GetItemByID(itemId).cost;
                if (price > maxValue)
                {
                    maxValue = price;
                    maxValueItemId = itemId;
                }
            }

            return maxValueItemId;
        }

        #endregion
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
