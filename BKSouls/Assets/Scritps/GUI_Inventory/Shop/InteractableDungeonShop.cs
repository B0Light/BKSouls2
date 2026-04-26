using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BK.Inventory
{
    public class InteractableDungeonShop : InteractableShop
    {
        [Header("Dungeon Shop Settings")]
        [SerializeField] [Range(5, 10)] private int minItemCount = 5;
        [SerializeField] [Range(5, 10)] private int maxItemCount = 10;
        [SerializeField] private int rerollRuneCost = 100;

        public int RerollRuneCost => rerollRuneCost;

        protected override void InitializeShop()
        {
            if (TryGenerateSaleItems())
                MarkShopInitialized();
        }

        public bool TryReroll(PlayerManager player)
        {
            if (player == null || player.playerStatsManager == null)
                return false;

            if (player.playerStatsManager.runes < rerollRuneCost)
                return false;

            if (!TryGenerateSaleItems())
                return false;

            player.playerStatsManager.AddRunes(-rerollRuneCost);
            MarkShopInitialized();
            return true;
        }

        private bool TryGenerateSaleItems()
        {
            if (WorldItemDatabase.Instance == null)
            {
                Debug.LogWarning($"{nameof(InteractableDungeonShop)}: WorldItemDatabase.Instance is null.");
                return false;
            }

            int stageIndex = RunManager.Instance != null ? RunManager.Instance.CurrentRoomIndex : 0;
            ItemTier maxTier = CalculateTierFromStage(stageIndex);

            List<Item> available = WorldItemDatabase.Instance.GetItemsByTypeAndTierRange(
                saleItemType,
                ItemTier.Common,
                maxTier
            );

            if (available == null || available.Count == 0)
            {
                Debug.LogWarning($"{nameof(InteractableDungeonShop)}: No items found for type={saleItemType}, maxTier={maxTier}");
                return false;
            }

            int count = Mathf.Min(Random.Range(minItemCount, maxItemCount + 1), available.Count);

            ClearSaleItems();
            foreach (Item original in available.OrderBy(_ => Random.value).Take(count))
                saleItemList.Add(Object.Instantiate(original));

            return saleItemList.Count > 0;
        }

        protected override void EnterShop()
        {
            GUIController.Instance.OpenShopWithRunes(saleItemList, this);
        }

        private static ItemTier CalculateTierFromStage(int stageIndex)
        {
            int tier = stageIndex / 3;
            return (ItemTier)Mathf.Clamp(tier, (int)ItemTier.Common, (int)ItemTier.Legendary);
        }
    }
}
