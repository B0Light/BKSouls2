using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace BK.Inventory
{
    public class ItemSaleUIManager : MonoBehaviour
    {
        [SerializeField] private ItemGrid itemGrid;
        private int _saleValue;

        [Header("UI")] [SerializeField] private TextMeshProUGUI totalItemCostText;
        [SerializeField] private Button sellButton;
        private List<int> _initItemList;

        private ItemGrid _inventoryViewGrid;

        public void Init(int width, int height, List<int> itemIdList, ItemGrid inventoryViewGrid = null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellItems);

            _inventoryViewGrid = inventoryViewGrid;
            _initItemList = itemIdList;
            itemGrid.SetGrid(width, height, itemIdList);
        }

        private void FixedUpdate()
        {
            totalItemCostText.text = $"Value : {CalculateSaleValue()}";
        }

        private int CalculateSaleValue()
        {
            return _saleValue = itemGrid.totalItemValue.Value;
        }

        private void SellItems()
        {
            if (_inventoryViewGrid != null)
            {
                var viewItems = new Dictionary<int, int>(_inventoryViewGrid.GetCurItemDictById());
                foreach (var (itemId, count) in viewItems)
                    WorldPlayerInventory.Instance.RemoveItemInInventory(itemId, count);
                _inventoryViewGrid.ResetItemGrid();
            }

            // sale grid 아이템이 view grid 복사본에서 왔을 경우 실제 인벤토리에 원본이 남으므로 제거
            // 실제 인벤토리에서 드래그된 경우 이미 제거되어 있으므로 RemoveItemInInventory가 실패해도 무해
            var saleItems = new Dictionary<int, int>(itemGrid.GetCurItemDictById());
            foreach (var (itemId, count) in saleItems)
                WorldPlayerInventory.Instance.RemoveItemInInventory(itemId, count);

            totalItemCostText.text = "Sale complete.";
            WorldPlayerInventory.Instance.balance.Value += _saleValue;
            _saleValue = 0;
            itemGrid.ResetItemGrid();
            _initItemList.Clear();
        }

        public ItemGrid GetItemGrid => itemGrid;
    }
}
