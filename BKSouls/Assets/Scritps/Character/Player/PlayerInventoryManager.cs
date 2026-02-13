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
        public WeaponItem currentRightHandWeapon;
        public WeaponItem currentLeftHandWeapon;
        public WeaponItem currentTwoHandWeapon;

        [Header("Quick Slots")]
        public SpellItem currentSpell;
        public VariableList<int> currentQuickSlotIDList = new VariableList<int>();
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
        
        public void SelectNextQuickSlotItem()
        {
            int maxCount = currentQuickSlotIDList.Count;
            for (int i = 0; i < maxCount; i++)
            {
                int searchIndex = (i + quickSlotItemIndex) % maxCount;

                if (player.playerNetworkManager.currentQuickSlotItemID.Value != currentQuickSlotIDList[searchIndex])
                {
                    player.playerNetworkManager.currentQuickSlotItemID.Value = currentQuickSlotIDList[searchIndex];
                    return;
                }
            }
        }
        
        public void OnAddQuickSlotItem(int itemID)
        {
            // 현재 퀵슬롯 (물약 창)이 비었을 때 새로운 아이템을 넣으면 해당 아이템을 퀵슬롯에 등록한다.
            if (player.playerNetworkManager.currentQuickSlotItemID.Value == 0)
            {
                player.playerNetworkManager.currentQuickSlotItemID.Value = itemID;
            }
        }
    
        public void OnRemoveQuickSlotItem(int itemID)
        {
            // 현재 퀵슬롯에 등록된 아이템이 사용된 아이템인지 확인 (인벤토리에서 바로 사용할 수도 있음)
            if (player.playerNetworkManager.currentQuickSlotItemID.Value == itemID)
            {
                // 퀵슬롯에 해당아이템의 여분이 남아있지 않다면 남은 퀵슬롯 아이템으로 퀵슬롯 대체
                if (!currentQuickSlotIDList.Contains(itemID))
                {
                    player.playerNetworkManager.currentQuickSlotItemID.Value = currentQuickSlotIDList.Count == 0 ? 0 : currentQuickSlotIDList[0];
                }
            }
        }
        
        public void OnQuickSlotClear()
        {
            player.playerNetworkManager.currentQuickSlotItemID.Value = 0;
        }
    }
}
