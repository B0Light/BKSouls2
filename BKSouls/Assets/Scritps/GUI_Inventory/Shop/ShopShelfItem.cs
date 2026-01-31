using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class ShopShelfItem : MonoBehaviour, IShopShelfItem
    {
        protected ItemData itemData;

        [Header("Item Info")] [SerializeField] private int itemCode;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemName;
        [SerializeField] private Image itemTierBackground;

        [Header("Item Cost")] [SerializeField] protected TextMeshProUGUI itemCost;

        [Header("BUY")] [SerializeField] protected Button itemButton;

        public virtual void Init(ItemData data)
        {
            itemData = data;
            itemCode = data.itemCode;
            ChangeSprite(itemIcon, data.itemIcon);

            /* UI */
            if (itemName)
            {
                itemName.text = data.itemName;
                itemName.color = WorldDatabase_Item.Instance.GetItemColorByTier(data.itemTier);
            }

            if (itemCost)
            {
                itemCost.text = data.purchaseCost.ToString();
            }

            if (itemTierBackground)
            {
                itemTierBackground.color = WorldDatabase_Item.Instance.GetItemBackgroundColorByTier(data.itemTier);
            }
        }

        private void ChangeSprite(Image uiImage, Sprite newSprite)
        {
            if (uiImage == null || newSprite == null)
            {
                if (uiImage == null)
                    Debug.LogError("NO UI IMAGE");
                else
                    Debug.LogError("NO SPRITE");
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

        public int GetItemCode() => itemCode;

        public ItemData GetItem() => itemData;

        public virtual int GetItemCategory() => 0;
    }
}