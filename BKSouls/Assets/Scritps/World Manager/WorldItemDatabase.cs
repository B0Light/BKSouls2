using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BK.Inventory;

namespace BK
{
    public class WorldItemDatabase : Singleton<WorldItemDatabase>
    {
        [Header("Default")]
        public WeaponItem unarmedWeapon;
        public GameObject pickUpItemPrefab;

        [Header("Weapons")]
        [SerializeField] private List<WeaponItem> weapons = new();

        [Header("Head Equipment")]
        [SerializeField] private List<HeadEquipmentItem> headEquipment = new();

        [Header("Body Equipment")]
        [SerializeField] private List<BodyEquipmentItem> bodyEquipment = new();

        [Header("Leg Equipment")]
        [SerializeField] private List<LegEquipmentItem> legEquipment = new();

        [Header("Hand Equipment")]
        [SerializeField] private List<HandEquipmentItem> handEquipment = new();

        [Header("Ashes Of War")]
        [SerializeField] private List<AshOfWar> ashesOfWar = new();

        [Header("Spells")]
        [SerializeField] private List<SpellItem> spells = new();

        [Header("Projectiles")]
        [SerializeField] private List<RangedProjectileItem> projectiles = new();

        [Header("Quick Slot")]
        [SerializeField] private List<QuickSlotItem> quickSlotItems = new();

        [Header("Resources")]
        [SerializeField] private List<ResourceItem> resources = new();

        [Header("Currencies")]
        [SerializeField] private GridItem shelterCoinItem;

        [Header("Icons")]
        [SerializeField] private List<Sprite> defaultItemIcon = new();
        public Sprite unknownIcon;

        public GridItem ShelterCoinItem => shelterCoinItem;

        private readonly List<Item> _allItems = new();

        // 빠른 조회용
        private readonly Dictionary<int, Item> _itemsById = new();

        // 타입별 / 티어별 분류용
        private Dictionary<ItemType, Dictionary<ItemTier, List<Item>>> _itemsByTypeAndTier;
        private Dictionary<ItemTier, List<Item>> _allItemsByTier;

        protected override void Awake()
        {
            base.Awake();
            BuildDatabase();
        }

        #region Build Database

        private void BuildDatabase()
        {
            _allItems.Clear();
            _itemsById.Clear();

            InitializeTierDictionaries();

            RegisterItems(weapons);
            RegisterItems(headEquipment);
            RegisterItems(bodyEquipment);
            RegisterItems(legEquipment);
            RegisterItems(handEquipment);
            RegisterItems(ashesOfWar);
            RegisterItems(spells);
            RegisterItems(projectiles);
            RegisterItems(quickSlotItems);
            RegisterItems(resources);

            AssignItemIDs();
            IndexItems();
        }

        private void InitializeTierDictionaries()
        {
            _allItemsByTier = CreateTierDictionary();
            _itemsByTypeAndTier = new Dictionary<ItemType, Dictionary<ItemTier, List<Item>>>();

            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                if (!_itemsByTypeAndTier.ContainsKey(itemType))
                    _itemsByTypeAndTier[itemType] = CreateTierDictionary();
            }
        }

        private Dictionary<ItemTier, List<Item>> CreateTierDictionary()
        {
            var dict = new Dictionary<ItemTier, List<Item>>();

            foreach (ItemTier tier in Enum.GetValues(typeof(ItemTier)))
            {
                dict[tier] = new List<Item>();
            }

            return dict;
        }

        private void RegisterItems<T>(IEnumerable<T> source) where T : Item
        {
            if (source == null) return;

            foreach (T item in source)
            {
                if (item == null)
                    continue;

                _allItems.Add(item);
            }
        }

        private void AssignItemIDs()
        {
            for (int i = 0; i < _allItems.Count; i++)
            {
                _allItems[i].itemID = i;
            }
        }

        private void IndexItems()
        {
            foreach (Item item in _allItems)
            {
                if (item == null)
                    continue;

                _itemsById[item.itemID] = item;

                if (_allItemsByTier.TryGetValue(item.itemTier, out List<Item> allTierList))
                {
                    allTierList.Add(item);
                }

                if (_itemsByTypeAndTier.TryGetValue(item.itemType, out Dictionary<ItemTier, List<Item>> tierDict))
                {
                    if (tierDict.TryGetValue(item.itemTier, out List<Item> tierList))
                    {
                        tierList.Add(item);
                    }
                }
            }
        }

        #endregion

        #region Get Item By ID / Type

        public List<Item> GetAllItem()
        {
            return _allItems;
        }

        public Item GetItemByID(int id)
        {
            _itemsById.TryGetValue(id, out Item item);
            return item;
        }

        public T GetItemByID<T>(int id) where T : Item
        {
            return GetItemByID(id) as T;
        }

        public WeaponItem GetWeaponByID(int id) => GetItemByID<WeaponItem>(id);
        public HeadEquipmentItem GetHeadEquipmentByID(int id) => GetItemByID<HeadEquipmentItem>(id);
        public BodyEquipmentItem GetBodyEquipmentByID(int id) => GetItemByID<BodyEquipmentItem>(id);
        public LegEquipmentItem GetLegEquipmentByID(int id) => GetItemByID<LegEquipmentItem>(id);
        public HandEquipmentItem GetHandEquipmentByID(int id) => GetItemByID<HandEquipmentItem>(id);
        public AshOfWar GetAshOfWarByID(int id) => GetItemByID<AshOfWar>(id);
        public SpellItem GetSpellByID(int id) => GetItemByID<SpellItem>(id);
        public RangedProjectileItem GetProjectileByID(int id) => GetItemByID<RangedProjectileItem>(id);
        public QuickSlotItem GetQuickSlotItemByID(int id) => GetItemByID<QuickSlotItem>(id);
        public ResourceItem GetResourceByID(int id) => GetItemByID<ResourceItem>(id);

        #endregion

        #region Serialization

        public WeaponItem GetWeaponFromSerializedData(SerializableWeapon serializableWeapon)
        {
            WeaponItem weapon = null;

            WeaponItem sourceWeapon = GetWeaponByID(serializableWeapon.itemID);
            if (sourceWeapon != null)
            {
                weapon = Instantiate(sourceWeapon);
            }

            if (weapon == null)
                return Instantiate(unarmedWeapon);

            AshOfWar sourceAshOfWar = GetAshOfWarByID(serializableWeapon.ashOfWarID);
            if (sourceAshOfWar != null)
            {
                AshOfWar ashOfWar = Instantiate(sourceAshOfWar);
                weapon.ashOfWarAction = ashOfWar;
            }

            return weapon;
        }

        public RangedProjectileItem GetRangedProjectileFromSerializedData(SerializableRangedProjectile serializableProjectile)
        {
            RangedProjectileItem sourceProjectile = GetProjectileByID(serializableProjectile.itemID);
            if (sourceProjectile == null)
                return null;

            RangedProjectileItem rangedProjectile = Instantiate(sourceProjectile);
            rangedProjectile.currentAmmoAmount = serializableProjectile.itemAmount;

            return rangedProjectile;
        }

        public FlaskItem GetFlaskFromSerializedData(SerializableFlask serializableFlask)
        {
            QuickSlotItem sourceItem = GetQuickSlotItemByID(serializableFlask.itemID);
            if (sourceItem == null)
                return null;

            return Instantiate(sourceItem) as FlaskItem;
        }

        public QuickSlotItem GetQuickSlotItemFromSerializedData(SerilalizableQuickSlotItem serializableQuickSlotItem)
        {
            QuickSlotItem sourceItem = GetQuickSlotItemByID(serializableQuickSlotItem.itemID);
            if (sourceItem == null)
                return null;

            QuickSlotItem quickSlotItem = Instantiate(sourceItem);
            quickSlotItem.itemAmount = serializableQuickSlotItem.itemAmount;

            return quickSlotItem;
        }

        #endregion

        #region Query By Tier / Type

        public List<Item> GetItemsByTier(ItemTier tier)
        {
            if (_allItemsByTier.TryGetValue(tier, out List<Item> items))
                return new List<Item>(items);

            return new List<Item>();
        }

        public List<Item> GetItemsByType(ItemType itemType)
        {
            List<Item> result = new();

            if (!_itemsByTypeAndTier.TryGetValue(itemType, out Dictionary<ItemTier, List<Item>> tierDict))
                return result;

            foreach (var pair in tierDict)
            {
                result.AddRange(pair.Value);
            }

            return result;
        }

        public List<Item> GetItemsByTypeAndTierRange(ItemType itemType, ItemTier minTier, ItemTier maxTier)
        {
            var result = new List<Item>();

            if (!_itemsByTypeAndTier.TryGetValue(itemType, out Dictionary<ItemTier, List<Item>> tierDict))
                return result;

            foreach (ItemTier tier in GetTierRange(minTier, maxTier))
            {
                if (tierDict.TryGetValue(tier, out List<Item> items))
                {
                    result.AddRange(items);
                }
            }

            return result;
        }

        public List<Item> GetItemsByTierRange(ItemTier minTier, ItemTier maxTier)
        {
            var result = new List<Item>();

            foreach (ItemTier tier in GetTierRange(minTier, maxTier))
            {
                if (_allItemsByTier.TryGetValue(tier, out List<Item> items))
                {
                    result.AddRange(items);
                }
            }

            return result;
        }

        private IEnumerable<ItemTier> GetTierRange(ItemTier minTier, ItemTier maxTier)
        {
            var tierValues = Enum.GetValues(typeof(ItemTier)).Cast<ItemTier>().ToList();

            int minIndex = tierValues.IndexOf(minTier);
            int maxIndex = tierValues.IndexOf(maxTier);

            if (minIndex < 0 || maxIndex < 0 || minIndex > maxIndex)
                yield break;

            for (int i = minIndex; i <= maxIndex; i++)
            {
                yield return tierValues[i];
            }
        }

        #endregion

        #region Random Item
        
        public Item GetRandomItem(ItemType itemType, ItemTier minTier, ItemTier maxTier)
        {
            List<Item> candidates = GetItemsByTypeAndTierRange(itemType, minTier, maxTier);

            if (candidates == null || candidates.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[randomIndex];
        }
        
        public int GetRandomItemId(ItemType itemType, ItemTier minTier, ItemTier maxTier)
        {
            Item sourceItem = GetRandomItem(itemType, minTier, maxTier);
            if (sourceItem == null)
                return 0;

            return sourceItem.itemID;
        }

        /// <summary>
        /// 모든 타입 포함, 티어 범위 내 랜덤 아이템 반환
        /// </summary>
        public Item GetRandomItem(ItemTier minTier, ItemTier maxTier)
        {
            List<Item> candidates = GetItemsByTierRange(minTier, maxTier);

            if (candidates == null || candidates.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[randomIndex];
        }

        /// <summary>
        /// 타입 + 티어 범위 + 추가 필터까지 적용해서 랜덤 아이템 반환
        /// 예: 판매 가능한 아이템만, 소비 아이템만 등
        /// </summary>
        public Item GetRandomItem(
            ItemType itemType,
            ItemTier minTier,
            ItemTier maxTier,
            Predicate<Item> extraFilter)
        {
            List<Item> candidates = GetItemsByTypeAndTierRange(itemType, minTier, maxTier);

            if (extraFilter != null)
                candidates = candidates.FindAll(extraFilter);

            if (candidates == null || candidates.Count == 0)
                return null;

            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[randomIndex];
        }

        #endregion

        #region Icon

        public Sprite GetDefaultIcon(ItemEffect itemEffect)
        {
            int iconIndex = (int)itemEffect;

            if (iconIndex < 0 || iconIndex >= defaultItemIcon.Count)
                return unknownIcon;

            return defaultItemIcon[iconIndex];
        }

        #endregion

        #region Item Color By Tier

        public Color GetItemColorByTier(ItemTier rarity)
        {
            switch (rarity)
            {
                case ItemTier.Common:
                    return new Color(220f / 255f, 220f / 255f, 220f / 255f);
                case ItemTier.Uncommon:
                    return new Color(152f / 255f, 251f / 255f, 152f / 255f);
                case ItemTier.Rare:
                    return new Color(173f / 255f, 216f / 255f, 230f / 255f);
                case ItemTier.Epic:
                    return new Color(216f / 255f, 191f / 255f, 216f / 255f);
                case ItemTier.Legendary:
                    return new Color(255f / 255f, 223f / 255f, 186f / 255f);
                case ItemTier.Mythic:
                    return new Color(255f / 255f, 182f / 255f, 193f / 255f);
                default:
                    return Color.white;
            }
        }

        public Color GetItemBackgroundColorByTier(ItemTier rarity)
        {
            switch (rarity)
            {
                case ItemTier.Common:
                    return new Color(45f / 255f, 45f / 255f, 45f / 255f);
                case ItemTier.Uncommon:
                    return new Color(50f / 255f, 90f / 255f, 50f / 255f);
                case ItemTier.Rare:
                    return new Color(50f / 255f, 75f / 255f, 100f / 255f);
                case ItemTier.Epic:
                    return new Color(95f / 255f, 75f / 255f, 95f / 255f);
                case ItemTier.Legendary:
                    return new Color(100f / 255f, 80f / 255f, 50f / 255f);
                case ItemTier.Mythic:
                    return new Color(100f / 255f, 50f / 255f, 60f / 255f);
                default:
                    return new Color(45f / 255f, 45f / 255f, 45f / 255f);
            }
        }

        #endregion
    }
}
