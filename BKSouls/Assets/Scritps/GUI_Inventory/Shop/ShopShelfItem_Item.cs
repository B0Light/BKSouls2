using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ShopShelfItem_Item : ShopShelfItem
    {
        private GridItem _itemInfo;

        private IShopUIManager _playerUIShopManager;

        [Header("Item Cost")] [SerializeField] private GameObject costItemSlot;
        [SerializeField] private GameObject costItemPrefab;
        [SerializeField] private GameObject costCashSlot;

        public void Init(Item data, IShopUIManager shopUIManager)
        {
            _itemInfo = data as GridItem;
            if (!_itemInfo) return;
            costCashSlot.SetActive(true);

            itemButton.onClick.AddListener(SelectThisItem);
            _playerUIShopManager = shopUIManager;

            base.Init(data);
        }

        private void SelectThisItem() => _playerUIShopManager.SelectItemToBuy(itemData);

        public override int GetItemCategory() => (int)_itemInfo.itemType;

    }
}