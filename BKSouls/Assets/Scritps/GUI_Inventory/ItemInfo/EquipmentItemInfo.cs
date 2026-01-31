using System;
using System.Linq;
using UnityEngine;

namespace BK.Inventory
{
    public class EquipmentItemInfo : ItemInfo
    {
        public int GetAbilityValue(ItemEffect effect)
        {
            return itemAbilities?.FirstOrDefault(a => a.itemEffect == effect)?.value ?? 0;
        }

        public bool HasAbility(ItemEffect effect)
        {
            return itemAbilities?.Any(a => a.itemEffect == effect) ?? false;
        }

        public ItemAbility GetAbility(ItemEffect effect)
        {
            return itemAbilities?.FirstOrDefault(a => a.itemEffect == effect);
        }
    }
}