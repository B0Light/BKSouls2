using UnityEngine;

namespace BK
{
    public class InteractableItemBox : InteractableBox
    {
        [Header("Box Settings")]
        [SerializeField] private bool autoFill = true;
        [SerializeField] private bool clearBeforeFill = true;
        [SerializeField] private BoxType boxType;
        [SerializeField] private ItemTier boxTier = ItemTier.Common;
        [SerializeField] private int itemCount = 5;
        

        private bool _wasSetup;

        protected override void Start()
        {
            base.Start();

            if (autoFill && !_wasSetup)
            {
                InitBox();
            }
        }

        /// <summary>
        /// 런타임에 BoxType과 ItemTier를 지정하여 초기화합니다.
        /// RoomManager 등에서 프리팹 스폰 직후 호출하세요.
        /// </summary>
        public void Setup(BoxType type, ItemTier tier)
        {
            boxType = type;
            boxTier = tier;
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
            {
                itemIdList.Clear();
            }

            for (int i = 0; i < itemCount; i++)
            {
                int itemId = GetRandomItemIdByBoxType(boxType, boxTier);

                if (itemId >= 0)
                {
                    itemIdList.Add(itemId);
                }
            }
        }

        private int GetRandomItemIdByBoxType(BoxType targetBoxType, ItemTier maxTier)
        {
            switch (targetBoxType)
            {
                case BoxType.WeaponBox:
                    return GetRandomItemId(ItemType.Weapon, ItemTier.Common, maxTier);

                case BoxType.ArmorBox:
                    return GetArmorBoxRandomItemId(maxTier);

                case BoxType.FoodBox:
                    return GetRandomItemId(ItemType.Consumables, ItemTier.Common, maxTier);

                case BoxType.SupplyBox:
                    return GetSupplyBoxRandomItemId(maxTier);

                case BoxType.MiscBox:
                    return GetRandomItemId(ItemType.Misc, ItemTier.Common, maxTier);

                default:
                    return -1;
            }
        }

        private int GetSupplyBoxRandomItemId(ItemTier maxTier)
        {
            // SupplyBox는 상황에 따라 소비 아이템 / 투사체 / 퀵슬롯 아이템 등을 섞어서 줄 수 있음
            // 현재 프로젝트 ItemType 구성에 맞게 필요하면 조정
            int random = Random.Range(0, 3);

            switch (random)
            {
                case 0:
                    return GetRandomItemId(ItemType.Consumables, ItemTier.Common, maxTier);

                case 1:
                    return GetRandomItemId(ItemType.Misc, ItemTier.Common, maxTier);

                case 2:
                    return GetRandomItemId(ItemType.None, ItemTier.Common, maxTier);
            

                default:
                    return -1;
            }
        }

        private int GetArmorBoxRandomItemId(ItemTier maxTier)
        {
            int random = Random.Range(0, 4);

            switch (random)
            {
                case 0:
                    return GetRandomItemId(ItemType.Armor, ItemTier.Common, maxTier);

                case 1:
                    return GetRandomItemId(ItemType.Helmet, ItemTier.Common, maxTier);

                case 2:
                    return GetRandomItemId(ItemType.Gauntlet, ItemTier.Common, maxTier);

                case 3:
                    return GetRandomItemId(ItemType.Leggings, ItemTier.Common, maxTier);

                default:
                    return -1;
            }
        }

    

        private int GetRandomItemId(ItemType itemType, ItemTier minTier, ItemTier maxTier)
        {
            Item item = WorldItemDatabase.Instance.GetRandomItem(itemType, minTier, maxTier);

            if (item == null)
            {
                Debug.LogWarning(
                    $"{nameof(InteractableItemBox)}: No item found. " +
                    $"boxType={boxType}, itemType={itemType}, minTier={minTier}, maxTier={maxTier}"
                );
                return -1;
            }

            return item.itemID;
        }
    }
}