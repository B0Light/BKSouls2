using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;

namespace BK
{
    public class PlayerInventoryManager : CharacterInventoryManager
    {
        [Header("Weapons")]
        public WeaponItem currentRightHandWeapon;
        public WeaponItem currentLeftHandWeapon;
        public WeaponItem currentTwoHandWeapon;

        [Header("Quick Slots")]
        public SpellItem currentSpell;
        public List<QuickSlotItem> quickSlotItemsInQuickSlots = new List<QuickSlotItem>();
        public int quickSlotItemIndex = 0;
        public QuickSlotItem currentQuickSlotItem;

        [Header("Armor")]
        public HeadEquipmentItem headEquipment;
        public BodyEquipmentItem bodyEquipment;
        public LegEquipmentItem legEquipment;
        public HandEquipmentItem handEquipment;
        
        [Header("Projectiles")]
        public RangedProjectileItem mainProjectile;
        public RangedProjectileItem secondaryProjectile;

        public void AddItemToInventory(Item item)
        {
            WorldShopManager.Instance.BuyItem(WorldItemDatabase.Instance.GetItemByID(item.itemID));
        }

        public void RemoveItemFromInventory(Item item)
        {
            
        }
    }
}
