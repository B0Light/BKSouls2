using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


namespace BK.Inventory
{
    public class InventoryItem : MonoBehaviour
    {
        public GridItem itemData;

        [HideInInspector] public ItemGrid previousItemGrid = null;

        [SerializeField] private Image itemIcon;
        [SerializeField] private Image itemFrame;
        public int Height => rotated ? itemData.width : itemData.height;
        public int Width => rotated ? itemData.height : itemData.width;

        public int onGridPositionX;
        public int onGridPositionY;

        public bool rotated = false;

        internal void Rotate()
        {
            rotated = !rotated;

            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.rotation = Quaternion.Euler(0, 0, rotated ? 90f : 0f);
        }

        internal void Set()
        {
            Vector2 size = new Vector2(
                itemData.width * ItemGrid.TileSizeWidth,
                itemData.height * ItemGrid.TileSizeHeight);
            onGridPositionX = (int)size.x;
            onGridPositionY = (int)size.y;
            GetComponent<RectTransform>().sizeDelta = size;
            itemIcon.GetComponent<RectTransform>().sizeDelta = size;
            itemFrame.color = WorldItemDatabase.Instance.GetItemColorByTier(itemData.itemTier);
            ChangeSprite(itemIcon, itemData.itemIcon);
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

            // 현재 RectTransform 크기 가져오기
            Vector2 currentSize = rectTransform.sizeDelta;

            // 짧은 쪽을 기준으로 비율 유지
            float aspectRatio = newSpriteSize.y / newSpriteSize.x;
            if (currentSize.x < currentSize.y)
            {
                rectTransform.sizeDelta = new Vector2(currentSize.x, currentSize.x * aspectRatio);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(currentSize.y / aspectRatio, currentSize.y);
            }

            // 스프라이트 교체
            uiImage.sprite = newSprite;
        }
    }
}