using UnityEngine;

namespace BK.Inventory
{
    public interface IShopShelfItem
    {
        public void Init(Item data);

        int GetItemCategory();

        Item GetItem();

        int GetItemCode();
    }
}