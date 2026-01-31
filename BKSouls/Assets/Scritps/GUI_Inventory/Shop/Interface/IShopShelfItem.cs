using UnityEngine;

namespace BK.Inventory
{
    public interface IShopShelfItem
    {
        public void Init(ItemData data);

        int GetItemCategory();

        ItemData GetItem();

        int GetItemCode();
    }
}