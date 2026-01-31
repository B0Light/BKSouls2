using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK.Inventory
{
    public class WorldShopManager : Singleton<WorldShopManager>
    {
        [Header("Inventory Item UI")] public GameObject inventoryItemRef;

        public bool BuyItem(ItemInfo itemInfoData)
        {
            GameObject item = Instantiate(inventoryItemRef);
            InventoryItem inventoryItem = item.GetComponent<InventoryItem>();
            inventoryItem.itemInfoData = itemInfoData;
            inventoryItem.Set();
            return WorldPlayerInventory.Instance.AddItem(item);
        }
    }
}
