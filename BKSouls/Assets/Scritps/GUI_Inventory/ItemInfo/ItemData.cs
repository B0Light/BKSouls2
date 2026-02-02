using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace BK.Inventory
{
    public class ItemData : ScriptableObject
    {
        [Header("Default Info")] 
        public int itemID;
        public string itemName;
        public Sprite itemIcon;
        
        [TextArea] public string itemDescription;
        
        public ItemTier itemTier;
        public int purchaseCost = 0;
        public List<int> costItemList = new List<int>();

        public int width = 1;
        public int height = 1;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ItemData other = (ItemData)obj;
            return itemID == other.itemID;
        }

        public override int GetHashCode()
        {
            return itemID.GetHashCode();
        }

        public Dictionary<int, int> GetCostDict()
        {
            var requiredItems = costItemList
                .GroupBy(x => x)
                .ToDictionary(g => g.Key, g => g.Count());
            return requiredItems;
        }
    }
}


