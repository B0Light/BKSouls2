using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ItemShopUIManager : ShopUIManager
    {
        [SerializeField] private Transform abilityContainer;
        [SerializeField] private GameObject abilityUIPrefab;

        private List<HUD_SelectedItemAbility> abilityUIs = new List<HUD_SelectedItemAbility>();

        [SerializeField] private Button itemBuyButton_Cash;
        [SerializeField] private Button itemQuantityIncreaseButton;
        [SerializeField] private Button itemQuantityDecreaseButton;
        [SerializeField] private TextMeshProUGUI itemQuantityText;
        [SerializeField] private TextMeshProUGUI totalCostText;

        private int _purchaseQuantity = 1;
        private int _selectedItemCost = 0;
        private Item _selectedShopItem;

        [SerializeField] private CanvasGroup itemInfoCanvasGroup;

        [Header("Sale")]
        [SerializeField] private ItemSaleUIManager itemSaleUIManager;
        [SerializeField] private CanvasGroup saleCanvasGroup;
        [SerializeField] private Button openSaleButton;
        private int saleGridWidth = 8;
        private int saleGridHeight = 10;
        [SerializeField] private ItemGrid inventorySaleViewGrid;
        [SerializeField] private CanvasGroup inventorySaleViewCanvasGroup;

        [Header("Dungeon Shop Reroll")]
        [SerializeField] private Button rerollButton;
        [SerializeField] private TextMeshProUGUI rerollCostText;

        private bool _isShopOpen = false;
        private bool _forceRunes = false;
        private InteractableShop _currentShop;
        private InteractableDungeonShop _dungeonShop;

        private bool UseRunes => _forceRunes || !WorldSaveGameManager.Instance.IsHoldScene;

        private PlayerManager LocalPlayer => GUIController.Instance?.localPlayer;

        private int CurrentCurrency
        {
            get
            {
                if (UseRunes)
                    return LocalPlayer != null ? LocalPlayer.playerStatsManager.runes : 0;
                return WorldPlayerInventory.Instance != null ? WorldPlayerInventory.Instance.balance.Value : 0;
            }
        }

        public override void OpenShop(List<Item> items, Interactable interactable = null, bool isMasterShop = false)
        {
            OpenShop(items, interactable, isMasterShop, false);
        }

        public void OpenShop(List<Item> items, Interactable interactable, bool isMasterShop, bool forceRunes)
        {
            _forceRunes = forceRunes;
            _isShopOpen = true;
            _currentShop = interactable as InteractableShop;
            _dungeonShop = interactable as InteractableDungeonShop;

            if (!UseRunes)
                WorldPlayerInventory.Instance.balance.OnValueChanged += OnBalanceChanged;

            SetUpShelf(items);
            ResetItemInfo();
            ShowAllItem();

            SetSaleVisible(false);
            SetInventoryViewVisible(false);
            SetItemInfoVisible(true);

            if (openSaleButton != null)
            {
                openSaleButton.onClick.RemoveAllListeners();
                openSaleButton.onClick.AddListener(ToggleSale);
            }

            SetupRerollButton();
        }

        public override void CloseGUI()
        {
            base.CloseGUI();

            if (!UseRunes && WorldPlayerInventory.Instance != null)
                WorldPlayerInventory.Instance.balance.OnValueChanged -= OnBalanceChanged;

            SetSaleVisible(false);
            SetInventoryViewVisible(false);

            if (!_isShopOpen) return;
            _isShopOpen = false;
            _currentShop = null;
            _dungeonShop = null;
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            WorldSaveGameManager.Instance.SaveGame();
        }

        private void OnBalanceChanged(int value)
        {
            SetPurchaseQuantity(_purchaseQuantity);
            RefreshRerollButtonState();
        }

        private void SetupRerollButton()
        {
            EnsureRerollButton();

            if (rerollButton == null)
                return;

            bool canReroll = _dungeonShop != null;
            rerollButton.gameObject.SetActive(canReroll);
            rerollButton.onClick.RemoveAllListeners();

            if (!canReroll)
                return;

            rerollButton.onClick.AddListener(TryRerollDungeonShop);

            if (rerollCostText != null)
                rerollCostText.text = $"Reroll { _dungeonShop.RerollRuneCost }";

            RefreshRerollButtonState();
        }

        private void EnsureRerollButton()
        {
            if (rerollButton != null || openSaleButton == null)
                return;

            GameObject rerollButtonObject = Instantiate(openSaleButton.gameObject, openSaleButton.transform.parent);
            rerollButtonObject.name = "Button_Reroll_DungeonShop";
            rerollButton = rerollButtonObject.GetComponent<Button>();
            rerollCostText = rerollButtonObject.GetComponentInChildren<TextMeshProUGUI>(true);

            if (rerollCostText != null)
                rerollCostText.text = "Reroll";
        }

        private void TryRerollDungeonShop()
        {
            if (_dungeonShop == null)
                return;

            PlayerManager player = LocalPlayer;
            if (!_dungeonShop.TryReroll(player))
            {
                notEnoughItemsComment.SetActive(true);
                RefreshRerollButtonState();
                return;
            }

            notEnoughItemsComment.SetActive(false);
            ResetItemInfo();
            SetUpShelf(_dungeonShop.saleItemList);
            ShowAllItem();
            SetupRerollButton();
            WorldSaveGameManager.Instance.SaveGame();
        }

        private void RefreshRerollButtonState()
        {
            if (rerollButton == null || _dungeonShop == null)
                return;

            rerollButton.interactable = LocalPlayer != null &&
                                        LocalPlayer.playerStatsManager.runes >= _dungeonShop.RerollRuneCost;
        }

        private void ToggleSale()
        {
            SetSaleVisible(true);
            SetInventoryViewVisible(true);
            SetItemInfoVisible(false);

            if (itemSaleUIManager != null)
            {
                PopulateInventoryForSale();
                itemSaleUIManager.Init(
                    saleGridWidth,
                    saleGridHeight,
                    new List<int>(),
                    inventorySaleViewGrid,
                    PopulateInventoryForSale
                );
                WorldPlayerInventory.Instance.curInteractItemGrid = itemSaleUIManager.GetItemGrid;
            }
        }

        protected override void CloseSaleUI()
        {
            SetSaleVisible(false);
            SetInventoryViewVisible(false);
            SetItemInfoVisible(true);
        }

        private void SetItemInfoVisible(bool isActive)
        {
            if (itemInfoCanvasGroup == null) return;
            itemInfoCanvasGroup.alpha = isActive ? 1 : 0;
            itemInfoCanvasGroup.interactable = isActive;
            itemInfoCanvasGroup.blocksRaycasts = isActive;
        }

        private void SetInventoryViewVisible(bool isActive)
        {
            if (inventorySaleViewCanvasGroup == null) return;
            inventorySaleViewCanvasGroup.alpha = isActive ? 1 : 0;
            inventorySaleViewCanvasGroup.interactable = isActive;
            inventorySaleViewCanvasGroup.blocksRaycasts = isActive;
        }

        private void PopulateInventoryForSale()
        {
            if (inventorySaleViewGrid == null) return;

            var inventory = WorldPlayerInventory.Instance.GetInventory();
            var backpack = WorldPlayerInventory.Instance.GetBackpackInventory();

            var itemDict = new Dictionary<int, int>();

            if (inventory != null)
            {
                foreach (var (id, count) in inventory.GetCurItemDictById())
                {
                    if (!CanSellItem(id))
                        continue;

                    itemDict[id] = itemDict.TryGetValue(id, out int cur) ? cur + count : count;
                }
            }

            if (backpack != null)
            {
                foreach (var (id, count) in backpack.GetCurItemDictById())
                {
                    if (!CanSellItem(id))
                        continue;

                    itemDict[id] = itemDict.TryGetValue(id, out int cur) ? cur + count : count;
                }
            }

            if (itemSaleUIManager != null)
            {
                foreach (var (id, count) in itemSaleUIManager.GetQueuedSaleItems())
                {
                    if (!itemDict.ContainsKey(id))
                        continue;

                    itemDict[id] -= count;
                    if (itemDict[id] <= 0)
                        itemDict.Remove(id);
                }
            }

            inventorySaleViewGrid.ResetItemGrid();
            inventorySaleViewGrid.SetGrid(6, 10, new List<int>());

            foreach (var (id, count) in itemDict)
            {
                for (int i = 0; i < count; i++)
                    inventorySaleViewGrid.AddItemById(id, 1, false);
            }
        }

        private static bool CanSellItem(int itemId)
        {
            Item item = WorldItemDatabase.Instance.GetItemByID(itemId);
            return item != null && item.itemTier != ItemTier.None;
        }

        private void SetSaleVisible(bool isActive)
        {
            if (saleCanvasGroup == null) return;
            saleCanvasGroup.alpha = isActive ? 1 : 0;
            saleCanvasGroup.interactable = isActive;
            saleCanvasGroup.blocksRaycasts = isActive;
        }

        protected override void ResetItemInfo()
        {
            base.ResetItemInfo();

            itemBuyButton_Cash.gameObject.SetActive(false);
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            _selectedItemCost = 0;
            _selectedShopItem = null;
            SetPurchaseQuantity(1);

            if (totalCostText != null)
                totalCostText.text = string.Empty;
        }

        public override void SelectItemToBuy(Item item)
        {
            GridItem selectItemInfo = item as GridItem;
            if (!selectItemInfo) return;
            SetItemInfo(selectItemInfo);

            ClearAbilities();

            if (selectItemInfo.itemAbilities != null)
            {
                foreach (ItemAbility ability in selectItemInfo.itemAbilities)
                {
                    CreateAbilityFromItemAbility(ability);
                }
            }

            costCashSlot.SetActive(true);

            itemBuyButton_Cash.gameObject.SetActive(true);
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            _selectedItemCost = selectItemInfo.cost;
            _selectedShopItem = selectItemInfo;
            SetPurchaseQuantity(1);

            itemBuyButton_Cash.onClick.RemoveAllListeners();
            itemBuyButton_Cash.onClick.AddListener(() => BuyWithCash(selectItemInfo));

            itemQuantityIncreaseButton.onClick.RemoveAllListeners();
            itemQuantityIncreaseButton.onClick.AddListener(() => SetPurchaseQuantity(_purchaseQuantity + 1));

            itemQuantityDecreaseButton.onClick.RemoveAllListeners();
            itemQuantityDecreaseButton.onClick.AddListener(() => SetPurchaseQuantity(_purchaseQuantity - 1));
        }

        private void CreateAbilityFromItemAbility(ItemAbility ability)
        {
            if (abilityUIPrefab == null || abilityContainer == null) return;

            GameObject abilityObj = Instantiate(abilityUIPrefab, abilityContainer);
            HUD_SelectedItemAbility abilityComponent = abilityObj.GetComponent<HUD_SelectedItemAbility>();

            if (abilityComponent != null)
            {
                abilityComponent.Init_ability(ability);
                abilityUIs.Add(abilityComponent);
            }
        }

        private void ClearAbilities()
        {
            foreach (var ability in abilityUIs)
            {
                if (ability != null && ability.gameObject != null)
                    DestroyImmediate(ability.gameObject);
            }

            abilityUIs.Clear();
        }

        private void SetPurchaseQuantity(int quantity)
        {
            int maxQuantity = GetRemainingPurchaseCount(_selectedShopItem);
            _purchaseQuantity = maxQuantity > 0 ? Mathf.Clamp(quantity, 1, maxQuantity) : 0;

            if (itemQuantityText != null)
                itemQuantityText.text = _purchaseQuantity.ToString();

            if (totalCostText != null)
            {
                int totalCost = _selectedItemCost * _purchaseQuantity;
                int currency = CurrentCurrency;
                totalCostText.text = $"{totalCost} / {currency}";
                totalCostText.color = totalCost > currency ? Color.red : Color.white;
            }

            if (itemQuantityIncreaseButton != null)
                itemQuantityIncreaseButton.interactable = _purchaseQuantity > 0 && _purchaseQuantity < maxQuantity;

            if (itemQuantityDecreaseButton != null)
                itemQuantityDecreaseButton.interactable = _purchaseQuantity > 1;

            if (itemBuyButton_Cash != null)
                itemBuyButton_Cash.interactable = _purchaseQuantity > 0;
        }

        private void BuyWithCash(Item item)
        {
            int remainingCount = GetRemainingPurchaseCount(item);
            if (remainingCount <= 0)
            {
                notEnoughItemsComment.SetActive(true);
                SetPurchaseQuantity(0);
                return;
            }

            _purchaseQuantity = Mathf.Min(_purchaseQuantity, remainingCount);
            int totalCost = item.cost * _purchaseQuantity;

            if (UseRunes)
            {
                PlayerManager player = LocalPlayer;
                if (player == null || player.playerStatsManager.runes < totalCost)
                {
                    notEnoughItemsComment.SetActive(true);
                    return;
                }

                for (int i = 0; i < _purchaseQuantity; i++)
                {
                    if (!WorldShopManager.Instance.BuyItem(item))
                    {
                        notEnoughSlot.SetActive(true);
                        return;
                    }
                }

                player.playerStatsManager.AddRunes(-totalCost);
                _currentShop?.RecordPurchase(item, _purchaseQuantity);
                SetPurchaseQuantity(_purchaseQuantity);
                RefreshRerollButtonState();
            }
            else
            {
                if (WorldPlayerInventory.Instance.balance.Value < totalCost)
                {
                    notEnoughItemsComment.SetActive(true);
                    return;
                }

                for (int i = 0; i < _purchaseQuantity; i++)
                {
                    if (!WorldShopManager.Instance.BuyItem(item))
                    {
                        notEnoughSlot.SetActive(true);
                        return;
                    }
                }

                WorldPlayerInventory.Instance.balance.Value -= totalCost;
                _currentShop?.RecordPurchase(item, _purchaseQuantity);
            }

            SetPurchaseQuantity(_purchaseQuantity);
            BuyItemProcess();
            WorldSaveGameManager.Instance.SaveGame();
            Debug.Log("Buy Success" + item.itemName);
        }

        private int GetRemainingPurchaseCount(Item item)
        {
            if (item == null)
                return 0;

            return _currentShop != null ? _currentShop.GetRemainingPurchaseCount(item) : int.MaxValue;
        }

        private void BuyItemProcess()
        {
        }

        private void SetUpShelf(List<Item> items)
        {
            ResetShelf();
            foreach (var itemInfoData in items)
            {
                GameObject saleItem = Instantiate(itemProductPrefab, productContainer);
                ShopShelfItem_Item shelfItemProduct = saleItem.GetComponent<ShopShelfItem_Item>();
                if (shelfItemProduct)
                    shelfItemProduct.Init(itemInfoData, this);
                onSaleItems.Add(shelfItemProduct);
            }
        }
    }
}
