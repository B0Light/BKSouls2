using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ItemSaleUIManager : MonoBehaviour
    {
        private static ItemSaleUIManager activeSaleUI;

        [SerializeField] private ItemGrid itemGrid;
        [SerializeField] private TextMeshProUGUI totalItemCostText;
        [SerializeField] private Button sellButton;

        private int _saleValue;
        private List<int> _initItemList;
        private ItemGrid _inventoryViewGrid;
        private Action _onSaleCompleted;

        private bool IsInDungeon => !WorldSaveGameManager.Instance.IsHoldScene;

        public void Init(int width, int height, List<int> itemIdList, ItemGrid inventoryViewGrid = null, Action onSaleCompleted = null)
        {
            activeSaleUI = this;

            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellItems);

            _inventoryViewGrid = inventoryViewGrid;
            _onSaleCompleted = onSaleCompleted;
            _initItemList = itemIdList;

            itemGrid.ResetItemGrid();
            itemGrid.SetGrid(width, height, itemIdList);
        }

        public static bool TryMoveFromActiveInventoryView(ItemGrid sourceGrid, InventoryItem sourceItem)
        {
            return activeSaleUI != null && activeSaleUI.TryMoveFromInventoryView(sourceGrid, sourceItem);
        }

        public Dictionary<int, int> GetQueuedSaleItems()
        {
            return new Dictionary<int, int>(itemGrid.GetCurItemDictById());
        }

        private bool TryMoveFromInventoryView(ItemGrid sourceGrid, InventoryItem sourceItem)
        {
            if (sourceGrid == null || sourceItem == null || sourceGrid != _inventoryViewGrid)
                return false;

            int itemId = sourceItem.itemData.itemID;
            int ownedCount = WorldPlayerInventory.Instance.GetItemCountInAllInventory(itemId);
            int queuedCount = itemGrid.GetItemCountById(itemId);

            if (queuedCount >= ownedCount)
                return true;

            if (!itemGrid.AddItemById(itemId, 1, false))
                return true;

            _onSaleCompleted?.Invoke();
            return true;
        }

        private void FixedUpdate()
        {
            if (totalItemCostText != null)
                totalItemCostText.text = $"Value : {CalculateSaleValue()}";
        }

        private int CalculateSaleValue()
        {
            return _saleValue = itemGrid.totalItemValue.Value;
        }

        private void SellItems()
        {
            var saleItems = new Dictionary<int, int>(itemGrid.GetCurItemDictById());
            if (saleItems.Count == 0) return;

            foreach (var (itemId, count) in saleItems)
            {
                if (!WorldPlayerInventory.Instance.CheckItemInInventory(itemId, count))
                    return;
            }

            foreach (var (itemId, count) in saleItems)
                WorldPlayerInventory.Instance.RemoveItemInInventory(itemId, count);

            if (IsInDungeon)
            {
                PlayerManager localPlayer = GUIController.Instance?.localPlayer;
                if (localPlayer != null)
                    localPlayer.playerStatsManager.AddRunes(_saleValue);
            }
            else
            {
                WorldPlayerInventory.Instance.balance.Value += _saleValue;
            }

            _saleValue = 0;
            itemGrid.ResetItemGrid();
            _initItemList?.Clear();
            _inventoryViewGrid?.ResetItemGrid();
            _onSaleCompleted?.Invoke();

            if (totalItemCostText != null)
                totalItemCostText.text = "Sale complete.";

            WorldSaveGameManager.Instance.SaveGame();
        }

        public ItemGrid GetItemGrid => itemGrid;
    }
}
