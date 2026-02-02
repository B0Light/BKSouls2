using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK.Inventory
{
    public class WorldShopManager : Singleton<WorldShopManager>
    {
        [Header("Inventory Item UI")] public GameObject inventoryItemRef;

        public bool BuyItem(Item itemData)
        {
            GameObject itemObject = Instantiate(inventoryItemRef);
            InventoryItem inventoryItem = itemObject.GetComponent<InventoryItem>();
            inventoryItem.itemData = itemData as GridItem;
            inventoryItem.Set();
            return WorldPlayerInventory.Instance.AddItem(itemObject);
        }
    }
}
