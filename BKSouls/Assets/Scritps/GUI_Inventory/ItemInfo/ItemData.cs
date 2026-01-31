using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

namespace BK.Inventory
{
    public class ItemData : ScriptableObject
    {
        [Header("Default Info")] public int itemCode;
        public string itemName;
        public Sprite itemIcon;
        public ItemTier itemTier;
        [TextArea] public string itemDescription;

        public int purchaseCost = 0;
        public List<int> costItemList = new List<int>();

        public int width = 1;
        public int height = 1;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ItemData other = (ItemData)obj;
            return itemCode == other.itemCode;
        }

        public override int GetHashCode()
        {
            return itemCode.GetHashCode();
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

public enum ItemTier
{
    Common,     //0 white
    Uncommon,   //1 green  
    Rare,       //2 blue
    Epic,       //3 purple
    Legendary,  //4 orange
    Mythic,     //5 Red
    None,       // To out of index
}
