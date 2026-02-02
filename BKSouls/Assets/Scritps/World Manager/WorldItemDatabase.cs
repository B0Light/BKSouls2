using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BK
{
    public class WorldItemDatabase : MonoBehaviour
    {
        public static WorldItemDatabase Instance;

        public WeaponItem unarmedWeapon;

        public GameObject pickUpItemPrefab;

        [Header("Weapons")]
        [SerializeField] List<WeaponItem> weapons = new List<WeaponItem>();

        [Header("Head Equipment")]
        [SerializeField] List<HeadEquipmentItem> headEquipment = new List<HeadEquipmentItem>();

        [Header("Body Equipment")]
        [SerializeField] List<BodyEquipmentItem> bodyEquipment = new List<BodyEquipmentItem>();

        [Header("Leg Equipment")]
        [SerializeField] List<LegEquipmentItem> legEquipment = new List<LegEquipmentItem>();

        [Header("Hand Equipment")]
        [SerializeField] List<HandEquipmentItem> handEquipment = new List<HandEquipmentItem>();

        [Header("Ashes Of War")]
        [SerializeField] List<AshOfWar> ashesOfWar = new List<AshOfWar>();

        [Header("Spells")]
        [SerializeField] List<SpellItem> spells = new List<SpellItem>();

        [Header("Projectiles")]
        [SerializeField] List<RangedProjectileItem> projectiles = new List<RangedProjectileItem>();

        [Header("Quick Slot")]
        [SerializeField] List<QuickSlotItem> quickSlotItems = new List<QuickSlotItem>();

        //  A LIST OF EVERY ITEM WE HAVE IN THE GAME
        [Header("Items")]
        private List<Item> items = new List<Item>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            //  ADD ALL OF OUR WEAPONS TO THE LIST OF ITEMS
            foreach (var weapon in weapons)
            {
                items.Add(weapon);
            }

            foreach (var item in headEquipment)
            {
                items.Add(item);
            }

            foreach (var item in bodyEquipment)
            {
                items.Add(item);
            }

            foreach (var item in legEquipment)
            {
                items.Add(item);
            }

            foreach (var item in handEquipment)
            {
                items.Add(item);
            }

            foreach (var item in ashesOfWar)
            {
                items.Add(item);
            }

            foreach (var item in spells)
            {
                items.Add(item);
            }

            foreach (var item in projectiles)
            {
                items.Add(item);
            }

            foreach (var item in quickSlotItems)
            {
                items.Add(item);
            }

            //  ASSIGN ALL OF OUR ITEMS A UNIQUE ITEM ID
            for (int i = 0; i < items.Count; i++)
            {
                items[i].itemID = i;
            }
        }

        //  ITEM DATABASE

        public Item GetItemByID(int ID)
        {
            return items.FirstOrDefault(item => item.itemID == ID);
        }

        public WeaponItem GetWeaponByID(int ID)
        {
            return weapons.FirstOrDefault(weapon => weapon.itemID == ID);
        }

        public HeadEquipmentItem GetHeadEquipmentByID(int ID)
        {
            return headEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public BodyEquipmentItem GetBodyEquipmentByID(int ID)
        {
            return bodyEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public LegEquipmentItem GetLegEquipmentByID(int ID)
        {
            return legEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public HandEquipmentItem GetHandEquipmentByID(int ID)
        {
            return handEquipment.FirstOrDefault(equipment => equipment.itemID == ID);
        }

        public AshOfWar GetAshOfWarByID(int ID)
        {
            return ashesOfWar.FirstOrDefault(item => item.itemID == ID);
        }

        public SpellItem GetSpellByID(int ID)
        {
            return spells.FirstOrDefault(item => item.itemID == ID);
        }

        public RangedProjectileItem GetProjectileByID(int ID)
        {
            return projectiles.FirstOrDefault(item => item.itemID == ID);
        }

        public QuickSlotItem GetQuickSlotItemByID(int ID)
        {
            return quickSlotItems.FirstOrDefault(item => item.itemID == ID);
        }

        //  ITEM SERIALIZATION
        
        public WeaponItem GetWeaponFromSerializedData(SerializableWeapon serializableWeapon)
        {
            WeaponItem weapon = null;

            if (GetWeaponByID(serializableWeapon.itemID))
                weapon = Instantiate(GetWeaponByID(serializableWeapon.itemID));

            if (weapon == null)
                return Instantiate(unarmedWeapon);

            if (GetAshOfWarByID(serializableWeapon.ashOfWarID))
            {
                AshOfWar ashOfWar = Instantiate(GetAshOfWarByID(serializableWeapon.ashOfWarID));
                weapon.ashOfWarAction = ashOfWar;
            }

            return weapon;
        }

        public RangedProjectileItem GetRangedProjectileFromSerializedData(SerializableRangedProjectile serializableProjectile)
        {
            RangedProjectileItem rangedProjectile = null;

            if (GetProjectileByID(serializableProjectile.itemID))
            {
                rangedProjectile = Instantiate(GetProjectileByID(serializableProjectile.itemID));
                rangedProjectile.currentAmmoAmount = serializableProjectile.itemAmount;
            }

            return rangedProjectile;
        }

        public FlaskItem GetFlaskFromSerializedData(SerializableFlask serializableFlask)
        {
            FlaskItem flask = null;

            if (GetQuickSlotItemByID(serializableFlask.itemID))
                flask = Instantiate(GetQuickSlotItemByID(serializableFlask.itemID)) as FlaskItem;

            return flask;
        }

        public QuickSlotItem GetQuickSlotItemFromSerializedData(SerilalizableQuickSlotItem serializableQuickSlotItem)
        {
            QuickSlotItem quickSlotItem = null;

            if (GetQuickSlotItemByID(serializableQuickSlotItem.itemID))
            {
                quickSlotItem = Instantiate(GetQuickSlotItemByID(serializableQuickSlotItem.itemID));
                quickSlotItem.itemAmount = serializableQuickSlotItem.itemAmount;
            }

            return quickSlotItem;
        }
        
        public Color GetItemColorByTier(ItemTier rarity)
        {
            switch (rarity)
            {
                case ItemTier.Common: // 일반
                    return new Color(220f / 255f, 220f / 255f, 220f / 255f); // RGB(220, 220, 220) - 연한 회색
                case ItemTier.Uncommon: // 희귀
                    return new Color(152f / 255f, 251f / 255f, 152f / 255f); // RGB(152, 251, 152) - 연한 연두색
                case ItemTier.Rare: // 희귀
                    return new Color(173f / 255f, 216f / 255f, 230f / 255f); // RGB(173, 216, 230) - 연한 파란색
                case ItemTier.Epic: // 고급
                    return new Color(216f / 255f, 191f / 255f, 216f / 255f); // RGB(216, 191, 216) - 연한 보라색
                case ItemTier.Legendary: // 특급
                    return new Color(255f / 255f, 223f / 255f, 186f / 255f); // RGB(255, 223, 186) - 연한 주황색
                case ItemTier.Mythic: // 신화
                    return new Color(255f / 255f, 182f / 255f, 193f / 255f); // RGB(255, 182, 193) - 연한 핑크색
                default:
                    return Color.white; // 기본 색상 - 흰색
            }
        }

        public Color GetItemBackgroundColorByTier(ItemTier rarity)
        {
            switch (rarity)
            {
                case ItemTier.Common: // 일반
                    return new Color(45f / 255f, 45f / 255f, 45f / 255f); // 어두운 회색
                case ItemTier.Uncommon: // 희귀
                    return new Color(50f / 255f, 90f / 255f, 50f / 255f); // 어두운 연두색
                case ItemTier.Rare: // 희귀
                    return new Color(50f / 255f, 75f / 255f, 100f / 255f); // 어두운 파란색
                case ItemTier.Epic: // 고급
                    return new Color(95f / 255f, 75f / 255f, 95f / 255f); // 어두운 보라색
                case ItemTier.Legendary: // 특급
                    return new Color(100f / 255f, 80f / 255f, 50f / 255f); // 어두운 주황색
                case ItemTier.Mythic: // 신화
                    return new Color(100f / 255f, 50f / 255f, 60f / 255f); // 어두운 빨간색
                default:
                    return new Color(45f / 255f, 45f / 255f, 45f / 255f); // 기본 색상 - 어두운 회색
            }
        }
    }
}
