using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace BK.Inventory
{
    public class WorldDatabase_Item : Singleton<WorldDatabase_Item>
    {
        public GameObject emptyInteractItemPrefab;

        [Header("All")] [SerializeField] private List<ItemInfo> allItems = new List<ItemInfo>();

        [Header("Equipment")] [SerializeField]
        private List<EquipmentItemInfo> equipmentItems = new List<EquipmentItemInfo>();

        [Header("Consumable")] [SerializeField]
        private List<ItemInfoConsumable> consumableItems = new List<ItemInfoConsumable>();

        [Header("Items")] [SerializeField] private List<ItemInfo> miscItems = new List<ItemInfo>();

        [Header("Blueprint")] [SerializeField]
        private List<ItemInfoBlueprint> blueprintItems = new List<ItemInfoBlueprint>();

        [SerializeField] private List<ItemInfo> notSaleItem = new List<ItemInfo>();
        [SerializeField] private List<ItemInfo> onSaleItem = new List<ItemInfo>();

        [Header("Icons")] [SerializeField] private List<Sprite> defaultItemIcon = new List<Sprite>();
        [SerializeField] private Sprite unknownIcon;

        private Dictionary<ItemTier, List<ItemInfo>> _allItemsByTier;
        private Dictionary<ItemTier, List<ItemInfo>> _miscItemsByTier;
        private Dictionary<ItemTier, List<ItemInfo>> _onSaleItemsByTier;
        private Dictionary<ItemTier, List<ItemInfo>> _weaponItemsByTier;
        private Dictionary<ItemTier, List<ItemInfo>> _equipmentItemsByTier;
        private Dictionary<ItemTier, List<ItemInfo>> _consumableItemsByTier;
        private Dictionary<ItemTier, List<ItemInfo>> _blueprintItemsByTier;

        protected override void Awake()
        {
            base.Awake();
            InitTierLists();

            InitAllItems();
            ClassifyItemsByTier();
        }

        private void InitTierLists()
        {
            _allItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();
            _miscItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();
            _onSaleItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();
            _weaponItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();
            _equipmentItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();
            _consumableItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();
            _blueprintItemsByTier = new Dictionary<ItemTier, List<ItemInfo>>();

            foreach (ItemTier tier in Enum.GetValues(typeof(ItemTier)))
            {
                _allItemsByTier[tier] = new List<ItemInfo>();
                _miscItemsByTier[tier] = new List<ItemInfo>();
                _onSaleItemsByTier[tier] = new List<ItemInfo>();
                _weaponItemsByTier[tier] = new List<ItemInfo>();
                _equipmentItemsByTier[tier] = new List<ItemInfo>();
                _consumableItemsByTier[tier] = new List<ItemInfo>();
                _blueprintItemsByTier[tier] = new List<ItemInfo>();
            }
        }

        private void InitAllItems()
        {
            /* Equipment Item */
            foreach (var itemInfo in equipmentItems)
            {
                itemInfo.costItemList.Clear();
            }

            /* Item Consumable */

            foreach (var itemConsumable in consumableItems)
            {
                itemConsumable.costItemList.Clear();
            }

            /* Item Misc */
            foreach (var item in miscItems)
            {
                item.costItemList.Clear();
                if (1290 <= item.itemID) // id 1290 이상은 구매 불가 아이템
                {
                    notSaleItem.Add(item);
                }
                else
                {
                    onSaleItem.Add(item);
                }
            }

            foreach (var blueprint in blueprintItems)
            {
                blueprint.costItemList.Clear();
            }

            allItems.AddRange(equipmentItems);
            allItems.AddRange(consumableItems);
            allItems.AddRange(miscItems);
            allItems.AddRange(blueprintItems);
        }

        private void ClassifyItemsByTier()
        {
            Debug.Log("Classify item cnt : " + allItems.Count);
            foreach (ItemInfo item in allItems)
            {
                if (item.itemID != 0)
                {
                    _allItemsByTier[item.itemTier].Add(item);
                }
            }

            foreach (ItemInfo item in miscItems)
            {
                _miscItemsByTier[item.itemTier].Add(item);
            }

            foreach (ItemInfo item in onSaleItem)
            {
                _onSaleItemsByTier[item.itemTier].Add(item);
            }

            foreach (EquipmentItemInfo item in equipmentItems)
            {
                if (item.itemID != 0 && item.itemID < 100)
                {
                    _weaponItemsByTier[item.itemTier].Add(item);
                }
            }

            foreach (EquipmentItemInfo item in equipmentItems)
            {
                if (item.itemID != 0)
                {
                    _equipmentItemsByTier[item.itemTier].Add(item);
                }
            }

            foreach (ItemInfoConsumable item in consumableItems)
            {
                _consumableItemsByTier[item.itemTier].Add(item);
            }

            foreach (var item in blueprintItems)
            {
                _blueprintItemsByTier[item.itemTier].Add(item);
            }
        }

        public Sprite GetDefaultIcon(ItemEffect itemEffect)
        {
            int iconIndex = (int)itemEffect;
            if (iconIndex < 0 || iconIndex >= defaultItemIcon.Count) return unknownIcon;
            return defaultItemIcon[iconIndex];
        }

        public List<ItemInfo> GetAllItem() => allItems;

        public ItemInfo GetItemByID(int id)
        {
            return allItems.FirstOrDefault(item => item.itemID == id);
        }

        // 상점 에서 재료로 구매시 활용 
        public ItemInfo GetMiscItemByID(int id)
        {
            return onSaleItem.FirstOrDefault(item => item.itemID == id);
        }

        public ItemInfo GetRandomItemByTier<T>(ItemTier tier) where T : ItemInfo
        {
            List<ItemInfo> baseItems = null;

            while (tier != ItemTier.None) // `tier`가 null이 될 때까지 반복
            {
                if (typeof(T) == typeof(ItemInfo))
                {
                    _allItemsByTier.TryGetValue(tier, out baseItems);
                }
                else if (typeof(T) == typeof(ItemInfoMisc))
                {
                    _onSaleItemsByTier.TryGetValue(tier, out baseItems);
                }
                else if (typeof(T) == typeof(EquipmentItemInfoWeapon))
                {
                    _weaponItemsByTier.TryGetValue(tier, out baseItems);
                }
                else if (typeof(T) == typeof(EquipmentItemInfo))
                {
                    _equipmentItemsByTier.TryGetValue(tier, out baseItems);
                }
                else if (typeof(T) == typeof(ItemInfoConsumable))
                {
                    _consumableItemsByTier.TryGetValue(tier, out baseItems);
                }

                // 아이템이 존재하면 무작위로 반환
                if (baseItems != null && baseItems.Count > 0)
                {
                    return baseItems[Random.Range(0, baseItems.Count)];
                }

                Debug.LogWarning("LowerItem : " + tier);
                tier = GetLowerTier(tier);
            }

            // 만약 해당하는 아이템이 없다면 실행
            // 가장 기본이되는 잡동사니 아이템 중 최저 티어 중 하나 (무조건 존재 )를 랜덤으로 반환
            _onSaleItemsByTier.TryGetValue(ItemTier.Common, out baseItems);

            if (baseItems != null && baseItems.Count > 0)
            {
                return baseItems[Random.Range(0, baseItems.Count)];
            }

            // 오류가 아닌 이상 null 반환은 불가 
            return null;

        }

        private ItemTier GetLowerTier(ItemTier currentTier)
        {
            // 예시: Enum일 경우 이전 값을 반환
            var values = Enum.GetValues(typeof(ItemTier)).Cast<ItemTier>().ToList();
            int index = values.IndexOf(currentTier);
            return index > 0 ? values[index - 1] : ItemTier.None;
        }

        /// <summary>
        /// ItemType을 사용하여 특정 타입의 특정 티어 아이템들을 모두 반환
        /// </summary>
        /// <param name="itemType">아이템 타입 (Weapon, Armor, Helmet, Consumables, Misc)</param>
        /// <param name="tier">원하는 티어</param>
        /// <returns>해당 타입 및 티어의 모든 아이템 리스트</returns>
        public List<ItemInfo> GetItemsByTypeAndTier(ItemInfo.ItemType itemType, ItemTier tier)
        {
            Dictionary<ItemTier, List<ItemInfo>> targetDictionary = GetDictionaryByItemType(itemType);

            if (targetDictionary != null && targetDictionary.TryGetValue(tier, out List<ItemInfo> items))
            {
                return new List<ItemInfo>(items); // 복사본 반환으로 원본 보호
            }

            return new List<ItemInfo>(); // 빈 리스트 반환
        }

        /// <summary>
        /// ItemType을 사용하여 특정 타입의 티어 범위에 해당하는 모든 아이템들을 반환
        /// </summary>
        /// <param name="itemType">아이템 타입</param>
        /// <param name="minTier">최소 티어</param>
        /// <param name="maxTier">최대 티어</param>
        /// <returns>해당 타입 및 티어 범위의 모든 아이템 리스트</returns>
        public List<ItemInfo> GetItemsByTypeAndTierRange(ItemInfo.ItemType itemType, ItemTier minTier, ItemTier maxTier)
        {
            Dictionary<ItemTier, List<ItemInfo>> targetDictionary = GetDictionaryByItemType(itemType);
            List<ItemInfo> result = new List<ItemInfo>();

            if (targetDictionary == null) return result;

            // 티어 순서 가져오기
            var tierValues = Enum.GetValues(typeof(ItemTier)).Cast<ItemTier>().ToList();
            int minIndex = tierValues.IndexOf(minTier);
            int maxIndex = tierValues.IndexOf(maxTier);

            // 인덱스 유효성 검사
            if (minIndex == -1 || maxIndex == -1 || minIndex > maxIndex) return result;

            // 범위 내의 모든 티어에서 아이템 수집
            for (int i = minIndex; i <= maxIndex; i++)
            {
                ItemTier currentTier = tierValues[i];
                if (targetDictionary.TryGetValue(currentTier, out List<ItemInfo> items))
                {
                    result.AddRange(items);
                }
            }

            return result;
        }

        /// <summary>
        /// 여러 ItemType의 특정 티어 아이템들을 반환
        /// </summary>
        /// <param name="itemTypes">아이템 타입 배열</param>
        /// <param name="tier">원하는 티어</param>
        /// <returns>해당 타입들 및 티어의 모든 아이템 리스트</returns>
        public List<ItemInfo> GetItemsByMultipleTypesAndTier(ItemInfo.ItemType[] itemTypes, ItemTier tier)
        {
            List<ItemInfo> result = new List<ItemInfo>();

            foreach (ItemInfo.ItemType itemType in itemTypes)
            {
                Dictionary<ItemTier, List<ItemInfo>> targetDictionary = GetDictionaryByItemType(itemType);

                if (targetDictionary != null && targetDictionary.TryGetValue(tier, out List<ItemInfo> items))
                {
                    result.AddRange(items);
                }
            }

            return result;
        }

        /// <summary>
        /// ItemType에 따른 적절한 딕셔너리 반환
        /// </summary>
        /// <param name="itemType">아이템 타입</param>
        /// <returns>해당 타입의 티어별 딕셔너리</returns>
        private Dictionary<ItemTier, List<ItemInfo>> GetDictionaryByItemType(ItemInfo.ItemType itemType)
        {
            switch (itemType)
            {
                case ItemInfo.ItemType.Weapon:
                    return _weaponItemsByTier;
                case ItemInfo.ItemType.Armor:
                case ItemInfo.ItemType.Helmet:
                    return _equipmentItemsByTier; // 방어구들은 모두 장비 딕셔너리에서
                case ItemInfo.ItemType.Consumables:
                    return _consumableItemsByTier;
                case ItemInfo.ItemType.Misc:
                    return _onSaleItemsByTier; // 또는 _miscItemsByTier 사용 가능
                case ItemInfo.ItemType.None:
                default:
                    return _allItemsByTier; // 전체 아이템
            }
        }

        /// <summary>
        /// ItemType에 따라 아이템을 필터링 (방어구 세분화용)
        /// </summary>
        /// <param name="items">필터링할 아이템 리스트</param>
        /// <param name="itemType">원하는 아이템 타입</param>
        /// <returns>필터링된 아이템 리스트</returns>
        private List<ItemInfo> FilterItemsByType(List<ItemInfo> items, ItemInfo.ItemType itemType)
        {
            List<ItemInfo> filteredItems = new List<ItemInfo>();

            foreach (ItemInfo item in items)
            {
                // ItemInfo에 itemType 필드가 있다고 가정
                // 만약 없다면 아이템 코드나 다른 방식으로 구분
                if (HasItemType(item, itemType))
                {
                    filteredItems.Add(item);
                }
            }

            return filteredItems;
        }

        /// <summary>
        /// 아이템이 특정 타입인지 확인 (구현 방식은 실제 아이템 구조에 따라 조정 필요)
        /// </summary>
        /// <param name="item">확인할 아이템</param>
        /// <param name="itemType">확인할 타입</param>
        /// <returns>해당 타입 여부</returns>
        private bool HasItemType(ItemInfo item, ItemInfo.ItemType itemType)
        {
            // 방법 1: ItemInfo에 itemType 필드가 있는 경우
            // return item.itemType == itemType;

            // 방법 2: 아이템 코드로 구분하는 경우 (기존 코드 기준)
            switch (itemType)
            {
                case ItemInfo.ItemType.Weapon:
                    return item is EquipmentItemInfoWeapon || item.itemID < 100;
                case ItemInfo.ItemType.Armor:
                    return item is EquipmentItemInfo && item.itemID >= 100 && item.itemID < 200;
                case ItemInfo.ItemType.Helmet:
                    return item is EquipmentItemInfo && item.itemID >= 200 && item.itemID < 300;
                case ItemInfo.ItemType.Consumables:
                    return item is ItemInfoConsumable;
                case ItemInfo.ItemType.Misc:
                    return !(item is EquipmentItemInfo) && !(item is ItemInfoConsumable);
                case ItemInfo.ItemType.None:
                default:
                    return true;
            }

            // 방법 3: 클래스 타입으로 구분하는 경우
            // switch (itemType)
            // {
            //     case ItemType.Weapon:
            //         return item is EquipmentItemInfoWeapon;
            //     case ItemType.Armor:
            //         return item is EquipmentItemInfoArmor;
            //     case ItemType.Helmet:
            //         return item is EquipmentItemInfoHelmet;
            //     case ItemType.Consumables:
            //         return item is ItemInfoConsumable;
            //     case ItemType.Misc:
            //         return item is ItemInfoMisc;
            //     default:
            //         return true;
            // }
        }

        // 방어구 세분화가 필요한 경우 사용할 개선된 함수
        /// <summary>
        /// ItemType을 사용하여 세분화된 필터링으로 아이템 반환 (Armor/Helmet 구분)
        /// </summary>
        /// <param name="itemType">아이템 타입</param>
        /// <param name="tier">원하는 티어</param>
        /// <returns>해당 타입 및 티어의 모든 아이템 리스트</returns>
        public List<ItemInfo> GetItemsByTypeAndTierWithFilter(ItemInfo.ItemType itemType, ItemTier tier)
        {
            // 기본 딕셔너리에서 아이템들을 가져온 후
            List<ItemInfo> baseItems = GetItemsByTypeAndTier(itemType, tier);

            // Armor나 Helmet 같이 세분화가 필요한 경우 추가 필터링
            if (itemType == ItemInfo.ItemType.Armor || itemType == ItemInfo.ItemType.Helmet)
            {
                return FilterItemsByType(baseItems, itemType);
            }

            return baseItems;
        }

        // ===========================================
        // 편의성을 위한 래퍼 함수들
        // ===========================================

        /// <summary>
        /// 모든 무기 아이템 (특정 티어)
        /// </summary>
        public List<ItemInfo> GetAllWeaponsByTier(ItemTier tier)
            => GetItemsByTypeAndTier(ItemInfo.ItemType.Weapon, tier);

        /// <summary>
        /// 모든 방어구 아이템 (특정 티어)
        /// </summary>
        public List<ItemInfo> GetAllArmorByTier(ItemTier tier)
            => GetItemsByTypeAndTierWithFilter(ItemInfo.ItemType.Armor, tier);

        /// <summary>
        /// 모든 헬멧 아이템 (특정 티어)
        /// </summary>
        public List<ItemInfo> GetAllHelmetsByTier(ItemTier tier)
            => GetItemsByTypeAndTierWithFilter(ItemInfo.ItemType.Helmet, tier);

        /// <summary>
        /// 모든 소비 아이템 (특정 티어)
        /// </summary>
        public List<ItemInfo> GetAllConsumablesByTier(ItemTier tier)
            => GetItemsByTypeAndTier(ItemInfo.ItemType.Consumables, tier);

        /// <summary>
        /// 모든 잡화 아이템 (특정 티어)
        /// </summary>
        public List<ItemInfo> GetAllMiscItemsByTier(ItemTier tier)
            => GetItemsByTypeAndTier(ItemInfo.ItemType.Misc, tier);

        /// <summary>
        /// 특정 티어 범위의 모든 무기
        /// </summary>
        public List<ItemInfo> GetWeaponsByTierRange(ItemTier minTier, ItemTier maxTier)
            => GetItemsByTypeAndTierRange(ItemInfo.ItemType.Weapon, minTier, maxTier);

        /// <summary>
        /// 특정 티어 범위의 모든 장비 (방어구 + 헬멧)
        /// </summary>
        public List<ItemInfo> GetEquipmentByTierRange(ItemTier minTier, ItemTier maxTier)
        {
            List<ItemInfo> result = new List<ItemInfo>();
            result.AddRange(GetItemsByTypeAndTierRange(ItemInfo.ItemType.Armor, minTier, maxTier));
            result.AddRange(GetItemsByTypeAndTierRange(ItemInfo.ItemType.Helmet, minTier, maxTier));
            return result;
        }
    }
}