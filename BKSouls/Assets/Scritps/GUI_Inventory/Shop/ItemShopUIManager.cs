using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ItemShopUIManager : ShopUIManager
    {
        [SerializeField] private Transform abilityContainer; // 모든 능력이 들어갈 컨테이너

        [SerializeField] private GameObject abilityUIPrefab; // HUD_SelectedItemAbility 프리팹

        private List<HUD_SelectedItemAbility> abilityUIs = new List<HUD_SelectedItemAbility>();

        [SerializeField] private Button itemBuyButton_Item;
        [SerializeField] private Button itemBuyButton_Cash;
        [SerializeField] private Button itemSaleButton;

        [Header("Sale")]
        [SerializeField] private ItemSaleUIManager itemSaleUIManager;
        [SerializeField] private CanvasGroup saleCanvasGroup;
        [SerializeField] private Button openSaleButton;
        [SerializeField] private int saleGridWidth = 4;
        [SerializeField] private int saleGridHeight = 2;

        private bool _isSaleOpen = false;
        private bool _isShopOpen = false;
        private bool _isMasterShop = false;
        private Interactable _interactableObject;

        public override void OpenShop(List<Item> items, Interactable interactable = null, bool isMasterShop = false)
        {
            _interactableObject = interactable;
            _isShopOpen = true;
            _isMasterShop = isMasterShop;

            SetUpShelf(items);
            ResetItemInfo();
            ShowAllItem();

            _isSaleOpen = false;
            SetSaleVisible(false);

            if (openSaleButton != null)
            {
                openSaleButton.onClick.RemoveAllListeners();
                openSaleButton.onClick.AddListener(ToggleSale);
            }
        }

        public override void CloseGUI()
        {
            base.CloseGUI();

            //if (_interactableObject) _interactableObject.ResetInteraction();
            _interactableObject = null;

            _isSaleOpen = false;
            SetSaleVisible(false);

            if (!_isShopOpen) return;
            _isShopOpen = false;
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            WorldSaveGameManager.Instance.SaveGame();
        }

        private void ToggleSale()
        {
            _isSaleOpen = !_isSaleOpen;
            SetSaleVisible(_isSaleOpen);

            if (_isSaleOpen && itemSaleUIManager != null)
            {
                itemSaleUIManager.Init(
                    saleGridWidth,
                    saleGridHeight,
                    new System.Collections.Generic.List<int>()
                );
                WorldPlayerInventory.Instance.curInteractItemGrid = itemSaleUIManager.GetItemGrid;
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

            itemBuyButton_Item.gameObject.SetActive(false);
            itemBuyButton_Cash.gameObject.SetActive(false);
            itemSaleButton.gameObject.SetActive(false);
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);
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
            itemSaleButton.gameObject.SetActive(false);
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            
            itemCostText.text = selectItemInfo.cost.ToString();
            itemBuyButton_Cash.onClick.RemoveAllListeners();
            itemBuyButton_Cash.onClick.AddListener(() => BuyWithCash(selectItemInfo));
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
        
        private void BuyWithCash(Item item)
        {
            // 돈이 없다면 구매불가
            if (WorldPlayerInventory.Instance.balance.Value < item.cost)
            {
                notEnoughItemsComment.SetActive(true);
                return;
            }

            // 플레이어가 구매할 수 있다면 구매하고 대금 지불 
            if (!WorldShopManager.Instance.BuyItem(item))
            {
                // 슬롯 부족 등으로 아이템 구매 불가 
                notEnoughSlot.SetActive(true);
                return;
            }

            WorldPlayerInventory.Instance.balance.Value -= item.cost;
            BuyItemProcess();
            Debug.Log("Buy Success" + item.itemName);
        }

        private void BuyItemProcess()
        {
            if (selectProduct == null) return;
            if (_isMasterShop) return;
            var itemInfo = selectProduct.GetItem();
            //진열대에서 상품을 제거
            onSaleItems.Remove(selectProduct);
            Destroy(((MonoBehaviour)selectProduct)?.gameObject);
            //해당 매장에도 물건을 제거 

            InteractableShop interactableShop = _interactableObject.GetComponent<InteractableShop>();
            if (interactableShop)
            {
                interactableShop.saleItemList.Remove(itemInfo);
            }

            ResetItemInfo();
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