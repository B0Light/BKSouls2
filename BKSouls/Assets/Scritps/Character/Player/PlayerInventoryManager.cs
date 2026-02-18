using System.Collections;
using System.Collections.Generic;
using BK.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace BK
{
    public class PlayerInventoryManager : CharacterInventoryManager
    {
        PlayerManager player;
        
        [Header("Weapons")] 
        // 인벤토리 상태가 아닌 현재 장비한 아이템을 기준으로 삼음 
        public WeaponItem currentRightHandWeapon;
        public WeaponItem currentLeftHandWeapon;
        public WeaponItem currentRightSubWeapon;
        public WeaponItem currentLeftSubWeapon;
        
        public WeaponItem currentTwoHandWeapon;
        
        [Header("Quick Slots")]
        public SpellItem currentSpell;
        public QuickSlotItem[] quickSlotItemsInQuickSlots = new QuickSlotItem[3];
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
        
        protected override void Awake()
        {
            base.Awake();

            player = GetComponent<PlayerManager>();
        }

        public void AddItemToInventory(Item item)
        {
            WorldShopManager.Instance.BuyItem(WorldItemDatabase.Instance.GetItemByID(item.itemID));
        }

        public void RemoveItemFromInventory(Item item)
        {
            
        }
    }
}
