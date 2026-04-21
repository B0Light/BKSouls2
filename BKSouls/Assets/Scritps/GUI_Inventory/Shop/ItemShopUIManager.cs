using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ItemShopUIManager : ShopUIManager
    {
        [SerializeField] private Transform abilityContainer; // 모든 능력이 들어갈 컨테이너

        [SerializeField] private GameObject abilityUIPrefab; // HUD_SelectedItemAbility 프리팹

        private List<HUD_SelectedItemAbility> abilityUIs = new List<HUD_SelectedItemAbility>();

        [SerializeField] private Button itemBuyButton_Cash;
        [SerializeField] private Button itemQuantityIncreaseButton;
        [SerializeField] private Button itemQuantityDecreaseButton;
        [SerializeField] private TextMeshProUGUI itemQuantityText;
        [SerializeField] private TextMeshProUGUI totalCostText;

        private int _purchaseQuantity = 1;
        private int _selectedItemCost = 0;

        [SerializeField] private CanvasGroup itemInfoCanvasGroup;

        [Header("Sale")]
        [SerializeField] private ItemSaleUIManager itemSaleUIManager;
        [SerializeField] private CanvasGroup saleCanvasGroup;
        [SerializeField] private Button openSaleButton;
        private int saleGridWidth = 8;
        private int saleGridHeight = 10;
        [SerializeField] private ItemGrid inventorySaleViewGrid;
        [SerializeField] private CanvasGroup inventorySaleViewCanvasGroup;


        private bool _isShopOpen = false;

        public override void OpenShop(List<Item> items, Interactable interactable = null, bool isMasterShop = false)
        {
            _isShopOpen = true;

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
        }

        public override void CloseGUI()
        {
            base.CloseGUI();

            SetSaleVisible(false);
            SetInventoryViewVisible(false);

            if (!_isShopOpen) return;
            _isShopOpen = false;
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            WorldSaveGameManager.Instance.SaveGame();
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
                    inventorySaleViewGrid
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
                    itemDict[id] = itemDict.TryGetValue(id, out int cur) ? cur + count : count;
            }

            if (backpack != null)
            {
                foreach (var (id, count) in backpack.GetCurItemDictById())
                    itemDict[id] = itemDict.TryGetValue(id, out int cur) ? cur + count : count;
            }

            inventorySaleViewGrid.SetGrid(6, 10, new List<int>());

            foreach (var (id, count) in itemDict)
            {
                for (int i = 0; i < count; i++)
                    inventorySaleViewGrid.AddItemById(id, 1, false);
            }
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
            _purchaseQuantity = Mathf.Max(1, quantity);

            if (itemQuantityText != null)
                itemQuantityText.text = _purchaseQuantity.ToString();

            if (totalCostText != null)
            {
                int totalCost = _selectedItemCost * _purchaseQuantity;
                int balance = WorldPlayerInventory.Instance.balance.Value;
                totalCostText.text = $"{totalCost} / {balance}";
                totalCostText.color = totalCost > balance ? Color.red : Color.white;
            }
        }

        private void BuyWithCash(Item item)
        {
            int totalCost = item.cost * _purchaseQuantity;

            // 돈이 없다면 구매불가
            if (WorldPlayerInventory.Instance.balance.Value < totalCost)
            {
                notEnoughItemsComment.SetActive(true);
                return;
            }

            // 플레이어가 구매할 수 있다면 구매하고 대금 지불
            for (int i = 0; i < _purchaseQuantity; i++)
            {
                if (!WorldShopManager.Instance.BuyItem(item))
                {
                    // 슬롯 부족 등으로 아이템 구매 불가
                    notEnoughSlot.SetActive(true);
                    return;
                }
            }

            WorldPlayerInventory.Instance.balance.Value -= totalCost;
            BuyItemProcess();
            WorldSaveGameManager.Instance.SaveGame();
            Debug.Log("Buy Success" + item.itemName);
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