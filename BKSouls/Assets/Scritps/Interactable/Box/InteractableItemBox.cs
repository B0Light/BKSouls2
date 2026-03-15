using UnityEngine;

namespace BK
{
    public class InteractableItemBox : InteractableBox
    {
        [Header("Box Settings")]
        [SerializeField] private BoxType boxType;
        [SerializeField] private ItemTier boxTier = ItemTier.Common;
        [SerializeField] private bool autoFill = true;
        [SerializeField] private int itemCount = 5;
        [SerializeField] private bool clearBeforeFill = true;

        protected override void Start()
        {
            base.Start();

            if (autoFill)
            {
                InitBox();
            }
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