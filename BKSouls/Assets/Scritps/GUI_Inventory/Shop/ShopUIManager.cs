using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public abstract class ShopUIManager : GUIComponent, IShopUIManager
    {
        [Header("Shelves UI")] [SerializeField]
        protected GameObject shelfUI;

        [SerializeField] protected GameObject productShelfSlot;
        [SerializeField] protected Transform productContainer;

        [Header("Item On Shelf")] [SerializeField]
        protected GameObject itemProductPrefab;

        protected List<IShopShelfItem> onSaleItems = new List<IShopShelfItem>();

        [Header("Item Info")] [SerializeField] protected Sprite noneItemIcon;
        [Space(10)] [SerializeField] protected TextMeshProUGUI itemInfo_Name;
        [SerializeField] protected Image itemInfo_Icon;
        [SerializeField] protected TextMeshProUGUI itemInfo_Description;
        [SerializeField] protected Image itemInfo_BackgroundColor;

        [Space(10)] [Header("Item Price")]

        [SerializeField] protected GameObject costCashSlot;

        [SerializeField] protected GameObject notEnoughItemsComment;
        [SerializeField] protected GameObject notEnoughSlot;
        protected IShopShelfItem selectProduct;

        public virtual void OpenShop(List<int> itemIds, Interactable interactable = null)
        {
        } // build Shop

        public virtual void OpenShop(List<Item> items, Interactable interactable = null, bool isMasterShop = false)
        {
        } // Item Shop

        protected virtual void CloseSaleUI()
        {
        }

        public virtual void SelectItemToBuy(Item selectItem)
        {
        }

        protected void ResetShelf()
        {
            foreach (IShopShelfItem itemProduct in onSaleItems)
            {
                Destroy(((MonoBehaviour)itemProduct)?.gameObject);
            }

            onSaleItems.Clear();
        }

        protected void SetItemInfo(Item item)
        {
            itemInfo_Name.text = item.itemName;
            ChangeSprite(itemInfo_Icon, item.itemIcon);
            itemInfo_Description.text = item.itemDescription;
            itemInfo_BackgroundColor.color = WorldItemDatabase.Instance.GetItemBackgroundColorByTier(item.itemTier);

            foreach (var product in onSaleItems)
            {
                if (product.GetItem() == item)
                {
                    selectProduct = product;
                    return;
                }
            }
        }

        private void ChangeSprite(Image uiImage, Sprite newSprite)
        {
            if (uiImage == null || newSprite == null)
            {
                Debug.LogError("UI Image 또는 새로운 스프라이트가 설정되지 않았습니다!");
                return;
            }

            // 기존 RectTransform 참조
            RectTransform rectTransform = uiImage.GetComponent<RectTransform>();

            // 새로운 스프라이트의 크기 가져오기
            Vector2 newSpriteSize = new Vector2(newSprite.rect.width, newSprite.rect.height);

            // 현재 RectTransform의 너비에 기반해 비율 유지
            float aspectRatio = newSpriteSize.y / newSpriteSize.x;
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x * aspectRatio);

            // 스프라이트 교체
            uiImage.sprite = newSprite;
        }

        protected virtual void ResetItemInfo()
        {
            itemInfo_Name.text = "None";
            ChangeSprite(itemInfo_Icon, noneItemIcon);
            itemInfo_Description.text = "Select Item";
            itemInfo_BackgroundColor.color = WorldItemDatabase.Instance.GetItemBackgroundColorByTier(0);

            costCashSlot.SetActive(false);
        }

        public void SearchCategory(int itemType)
        {
            CloseSaleUI();

            if (itemType == -1)
            {
                return;
            }

            foreach (var item in onSaleItems)
            {
                if (item is ShopShelfItem itemProduct)
                {
                    itemProduct.gameObject.SetActive(itemProduct.GetItemCategory() == itemType);
                }
                else
                {
                    Debug.LogWarning($"onSaleItems에 ShopShelfItem이 아닌 객체가 포함되어 있습니다: {item.GetType()}");
                }
            }
        }

        public void ShowAllItem()
        {
            CloseSaleUI();

            foreach (var item in onSaleItems)
            {
                if (item is ShopShelfItem itemProduct)
                {
                    itemProduct.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"onSaleItems에 ShopShelfItem이 아닌 객체가 포함되어 있습니다: {item.GetType()}");
                }
            }
        }

        protected void DeleteAllChildren(Transform parentTransform)
        {
            for (int i = parentTransform.childCount - 1; i >= 0; i--)
            {
                Transform child = parentTransform.GetChild(i);
                Destroy(child.gameObject);
            }
        }
    }
}