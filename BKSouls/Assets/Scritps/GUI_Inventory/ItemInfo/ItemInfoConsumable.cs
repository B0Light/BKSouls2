using UnityEngine;

namespace BK.Inventory
{
    [CreateAssetMenu(menuName = "Item/Consumable")]
    public class ItemInfoConsumable : ItemInfo
    {
        public int immediateEffectValue;
        public int continuousEffectValue;
        public int continuousEffectDuration;
    }
}