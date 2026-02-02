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

        private bool _isShopOpen = false;

        private bool _isMasterShop = false;

        private Interactable _interactableObject;

        public override void OpenShop(List<ItemInfo> items, Interactable interactable = null, bool isMasterShop = false)
        {
            _interactableObject = interactable;
            _isShopOpen = true;
            _isMasterShop = isMasterShop;

            SetUpShelf(items);
            ResetItemInfo();
            ShowAllItem();
        }

        public override void CloseGUI()
        {
            base.CloseGUI();

            //if (_interactableObject) _interactableObject.ResetInteraction();
            _interactableObject = null;

            if (!_isShopOpen) return;
            _isShopOpen = false;
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            WorldSaveGameManager.instance.SaveGame();
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

        public override void SelectItemToBuy(ItemData item)
        {
            ItemInfo selectItemInfo = item as ItemInfo;
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

            costItemSlot.SetActive(selectItemInfo.purChaseWithItem);
            costCashSlot.SetActive(!selectItemInfo.purChaseWithItem);

            itemBuyButton_Item.gameObject.SetActive(selectItemInfo.purChaseWithItem);
            itemBuyButton_Cash.gameObject.SetActive(!selectItemInfo.purChaseWithItem);
            itemSaleButton.gameObject.SetActive(false);
            notEnoughItemsComment.SetActive(false);
            notEnoughSlot.SetActive(false);

            if (selectItemInfo.purChaseWithItem)
            {
                DeleteAllChildren(costItemSpawnSlot);

                foreach (var costItemPair in selectItemInfo.GetCostDict())
                {
                    GameObject spawnedCostItem = Instantiate(costItemPrefab, costItemSpawnSlot);

                    spawnedCostItem.GetComponent<ShopCostItem>()?.Init(costItemPair.Key, costItemPair.Value);
                }

                itemBuyButton_Item.onClick.RemoveAllListeners();
                itemBuyButton_Item.onClick.AddListener(() => BuyWithItem(selectItemInfo));
            }
            else
            {
                itemCostText.text = selectItemInfo.purchaseCost.ToString();
                itemBuyButton_Cash.onClick.RemoveAllListeners();
                itemBuyButton_Cash.onClick.AddListener(() => BuyWithCash(selectItemInfo));
            }
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

        private void BuyWithItem(ItemInfo itemInfo)
        {
            // 아이템 구매를 위한 전체 프로세스 관리
            if (!WorldPlayerInventory.Instance.CheckItemInInventoryToChangeItem(itemInfo))
            {
                notEnoughItemsComment.SetActive(true);
                Debug.LogWarning($"구매 실패: 아이템 부족 (이름: {itemInfo.itemName})");
                return;
            }

            // 구매 시도 (인벤토리 슬롯 체크 등)
            if (!WorldShopManager.Instance.BuyItem(itemInfo))
            {
                notEnoughSlot.SetActive(true);
                Debug.LogWarning($"구매 실패: 인벤토리 슬롯 부족 (이름: {itemInfo.itemName})");
                return;
            }

            // 구매 비용 지불 (아이템 제거)
            if (!WorldPlayerInventory.Instance.SpendItemInInventory(itemInfo))
            {
                Debug.LogError($"심각한 오류: 비용 지불 중 문제 발생 (이름: {itemInfo.itemName})");
                WorldPlayerInventory.Instance.RemoveItemInInventory(itemInfo.itemID);
                return;
            }

            // 구매 완료 처리
            BuyItemProcess();
            Debug.Log($"구매 성공: {itemInfo.itemName}");
        }

        private void BuyWithCash(ItemInfo itemInfo)
        {
            // 돈이 없다면 구매불가
            if (WorldPlayerInventory.Instance.balance.Value < itemInfo.purchaseCost)
            {
                notEnoughItemsComment.SetActive(true);
                return;
            }

            // 플레이어가 구매할 수 있다면 구매하고 대금 지불 
            if (!WorldShopManager.Instance.BuyItem(itemInfo))
            {
                // 슬롯 부족 등으로 아이템 구매 불가 
                notEnoughSlot.SetActive(true);
                return;
            }

            WorldPlayerInventory.Instance.balance.Value -= itemInfo.purchaseCost;
            BuyItemProcess();
            Debug.Log("Buy Success" + itemInfo.itemName);
        }

        private void BuyItemProcess()
        {
            if (selectProduct == null) return;
            if (_isMasterShop) return;
            ItemInfo itemInfo = selectProduct.GetItem() as ItemInfo;
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

        private void SetUpShelf(List<ItemInfo> items)
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