using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK.Inventory
{
    public class ItemGrid_Forge : ItemGrid
    {
        private List<InventoryItem> _selectMaterials = new List<InventoryItem>();

        public override bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad = false)
        {
            if (base.PlaceItem(inventoryItem, posX, posY, isLoad))
            {
                _selectMaterials.Add(inventoryItem);
                return true;
            }

            return false;
        }

        public override InventoryItem PickUpItem(int x, int y)
        {
            InventoryItem pickUpItem = base.PickUpItem(x, y);
            if (pickUpItem == null) return null;

            _selectMaterials.Remove(pickUpItem);
            return pickUpItem;
        }
    }
}