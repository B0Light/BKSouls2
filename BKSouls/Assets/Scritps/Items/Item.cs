using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BK
{
    public class Item : ScriptableObject
    {
        [Header("Item Information")]
        public int itemID;
        public string itemName;
        public Sprite itemIcon;
        public ItemTier itemTier;
        public ItemType itemType;
        public int cost = 0;
        [TextArea] public string itemDescription;
        
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Item other = (Item)obj;
            return itemID == other.itemID;
        }
        
        public override int GetHashCode()
        {
            return itemID.GetHashCode();
        }
    }
}

