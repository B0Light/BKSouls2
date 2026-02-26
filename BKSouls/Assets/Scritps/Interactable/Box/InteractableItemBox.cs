using UnityEngine;

namespace BK
{
    public class InteractableItemBox : InteractableBox
    {
        [SerializeField] private int _itemCount = 5;

        protected override void Start()
        {
            base.Start();
            InitBox();
        }

        private void InitBox()
        {
            switch (boxType)
            {
                case BoxType.WeaponBox:
                    for (int i = 0; i < _itemCount; i++)
                    {
                        ItemTier randomTier = (ItemTier)Random.Range(0, boxTier + 1);
                        // itemIdList.Add(WorldItemDatabase.Instance.GetRandomItemByTier<EquipmentItemInfoWeapon>(randomTier).itemCode);
                    }

                    break;
                case BoxType.FoodBox:
                    for (int i = 0; i < _itemCount; i++)
                    {
                        ItemTier randomTier = (ItemTier)Random.Range(0, boxTier + 1);
                        // itemIdList.Add(WorldDatabase_Item.Instance.GetRandomItemByTier<ItemInfoConsumable>(randomTier).itemCode);
                    }

                    break;
                case BoxType.SupplyBox:
                    for (int i = 0; i < _itemCount; i++)
                    {
                        ItemTier randomTier = (ItemTier)Random.Range(0, boxTier + 1);
                        // itemIdList.Add(WorldDatabase_Item.Instance.GetRandomItemByTier<ItemInfo>(randomTier).itemCode);
                    }

                    break;
                case BoxType.MiscBox:
                    for (int i = 0; i < _itemCount; i++)
                    {
                        ItemTier randomTier = (ItemTier)Random.Range(0, boxTier + 1);
                        // itemIdList.Add(WorldDatabase_Item.Instance.GetRandomItemByTier<ItemInfoMisc>(randomTier).itemCode);
                    }

                    break;
                default:
                    for (int i = 0; i < _itemCount; i++)
                    {
                        ItemTier randomTier = (ItemTier)Random.Range(0, boxTier + 1);
                        // itemIdList.Add(WorldDatabase_Item.Instance.GetRandomItemByTier<ItemInfo>(randomTier).itemCode);
                    }

                    break;
            }
        }
    }
}