using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ShopShelfItem_Item : ShopShelfItem
    {
        private ItemInfo _itemInfo;

        private IShopUIManager _playerUIShopManager;

        [Header("Item Cost")] [SerializeField] private GameObject costItemSlot;
        [SerializeField] private GameObject costItemPrefab;
        [SerializeField] private GameObject costCashSlot;

        public void Init(ItemData data, IShopUIManager shopUIManager)
        {
            _itemInfo = data as ItemInfo;
            if (!_itemInfo) return;
            costItemSlot.SetActive(_itemInfo.purChaseWithItem);
            costCashSlot.SetActive(!_itemInfo.purChaseWithItem);

            // 해당 아이템이 아이템 교환으로 거래된다면 해당 아이템의 아이콘을 띄운다 
            if (_itemInfo.purChaseWithItem)
            {
                foreach (var costItemPair in _itemInfo.GetCostDict())
                {
                    GameObject spawnedCostItem = Instantiate(costItemPrefab, costItemSlot.transform);
                    spawnedCostItem.GetComponent<ShopCostItem>()?.Init(costItemPair.Key, costItemPair.Value);
                }
            }

            itemButton.onClick.AddListener(SelectThisItem);
            _playerUIShopManager = shopUIManager;

            base.Init(data);
        }

        private void SelectThisItem() => _playerUIShopManager.SelectItemToBuy(itemData);

        public override int GetItemCategory() => (int)_itemInfo.itemType;

    }
}