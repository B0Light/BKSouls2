using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace BK.Inventory
{
    public class ItemSaleUIManager : MonoBehaviour
    {
        [SerializeField] private ItemGrid itemGrid;
        [SerializeField] [Range(0f, 1f)] private float saleRate;
        private int _saleValue;

        [Header("UI")] [SerializeField] private TextMeshProUGUI totalItemCostText;
        [SerializeField] private Button sellButton;
        private List<int> _initItemList;

        public void Init(int width, int height, List<int> itemIdList)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(SellItems);

            _initItemList = itemIdList;
            itemGrid.SetGrid(width, height, itemIdList);
        }

        private void FixedUpdate()
        {
            totalItemCostText.text = $"Value : {CalculateSaleValue()}";
        }

        private int CalculateSaleValue()
        {
            _saleValue = (int)(itemGrid.totalItemValue.Value * saleRate);
            return _saleValue;
        }

        private void SellItems()
        {
            totalItemCostText.text = "Sale complete.";
            WorldPlayerInventory.Instance.balance.Value += _saleValue;
            _saleValue = 0;
            itemGrid.ResetItemGrid();
            _initItemList.Clear();
        }

        public ItemGrid GetItemGrid => itemGrid;
    }
}
