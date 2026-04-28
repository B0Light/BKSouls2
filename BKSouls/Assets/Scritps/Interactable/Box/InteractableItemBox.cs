using UnityEngine;

namespace BK
{
    public class InteractableItemBox : InteractableBox
    {
        [Header("Box Settings")]
        [SerializeField] private bool autoFill = true;
        [SerializeField] private bool clearBeforeFill = true;
        [SerializeField] private ItemTier boxTier = ItemTier.Common;
        public ItemTier BoxTier => boxTier;
        [SerializeField] private int itemCount = 5;

        [Header("Forced Content")]
        [SerializeField] private bool resourceOnly = false;
        [SerializeField] private ItemTier resourceMinTier = ItemTier.Common;
        [SerializeField] private ItemTier resourceMaxTier = ItemTier.Common;

        [Header("Drop Rates (%)")]
        [SerializeField] [Range(0, 100)] private int equipmentDropChance = 30;
        [SerializeField] [Range(0, 100)] private int potionDropChance = 10;
        // Resource = 100 - equipment - potion (~60%)

        private const int MaxPotionPerBox = 1;

        private bool _wasSetup;

        protected override void Start()
        {
            base.Start();

            if (autoFill || _wasSetup)
                InitBox();
        }

        public void Setup(ItemTier tier)
        {
            resourceOnly = false;
            boxTier = tier;
            _wasSetup = true;
            InitBox();
        }

        public void SetupResourceOnly(ItemTier maxTier)
        {
            resourceOnly = true;
            resourceMinTier = ItemTier.Common;
            resourceMaxTier = maxTier;
            boxTier = maxTier;
            _wasSetup = true;
            InitBox();
        }

        public void InitBox()
        {
            if (WorldItemDatabase.Instance == null)
            {
                Debug.LogWarning($"{nameof(InteractableItemBox)}: WorldItemDatabase.Instance is null.");
                return;
            }

            if (clearBeforeFill)
                itemIdList.Clear();

            int potionCount = 0;

            for (int i = 0; i < itemCount; i++)
            {
                int itemId;

                if (resourceOnly)
                {
                    itemId = GetRandomItemId(ItemType.Resource, resourceMinTier, resourceMaxTier);
                    if (itemId >= 0)
                        itemIdList.Add(itemId);

                    continue;
                }

                int roll = Random.Range(0, 100);

                if (roll < potionDropChance && potionCount < MaxPotionPerBox)
                {
                    itemId = GetPotionItemId();
                    if (itemId >= 0) potionCount++;
                }
                else if (roll < potionDropChance + equipmentDropChance)
                {
                    itemId = GetEquipmentItemId();
                }
                else
                {
                    itemId = GetResourceItemId();
                }

                if (itemId >= 0)
                    itemIdList.Add(itemId);
            }
        }

        private int GetPotionItemId()
        {
            // 포션은 항상 Common 티어 고정 (과도한 소모품 드롭 방지)
            ItemType itemType = ItemType.Consumables;
            int id = GetRandomItemId(itemType, ItemTier.Common, ItemTier.Common);
            if (id >= 0)
                return id;
            // fallback
            return GetRandomItemId(ItemType.Consumables, ItemTier.Common, ItemTier.Common);
        }

        private int GetEquipmentItemId()
        {
            int roll = Random.Range(0, 5);
            ItemType itemType = roll switch
            {
                0 => ItemType.Weapon,
                1 => ItemType.Armor,
                2 => ItemType.Helmet,
                3 => ItemType.Gauntlet,
                _ => ItemType.Leggings,
            };
            int id = GetRandomItemId(itemType, ItemTier.Common, boxTier);
            if (id >= 0) return id;
            return GetResourceItemId();
        }

        private int GetResourceItemId()
        {
            return GetResourceItemId(ItemTier.Common, boxTier);
        }

        private int GetResourceItemId(ItemTier minTier, ItemTier maxTier)
        {
            int id = GetRandomItemId(ItemType.Resource, minTier, maxTier);
            return id;
        }

        private int GetRandomItemId(ItemType itemType, ItemTier minTier, ItemTier maxTier)
        {
            Item item = WorldItemDatabase.Instance.GetRandomItem(itemType, minTier, maxTier);
            return item != null ? item.itemID : -1;
        }
    }
}
