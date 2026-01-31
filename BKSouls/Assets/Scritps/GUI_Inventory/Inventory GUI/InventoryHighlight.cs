using System;
using UnityEngine;
using UnityEngine.UI;


namespace BK.Inventory
{
    public class InventoryHighlight : MonoBehaviour
    {
        [SerializeField] RectTransform highlighter; // 새로운 위치 
        [SerializeField] RectTransform selector; // 기존 위치 

        [SerializeField] private Image highlighterImg;

        [SerializeField] private Color enableColor;
        [SerializeField] private Color disableColor;

        public void ShowHighlighter(bool isShow)
        {
            highlighter.gameObject.SetActive(isShow);
        }

        public void ShowSelector(bool isShow)
        {
            selector.gameObject.SetActive(isShow);
        }

        public void HighlightToSelect(InventoryItem targetItem, ItemGrid targetGrid, bool show)
        {
            ShowHighlighter(show);
            SetSize(targetItem);
            SetPosition(targetGrid, targetItem);
            SetSelectorParent(targetGrid);
        }

        public void UpdateHighlight(InventoryItem targetItem, ItemGrid targetGrid, int posX, int posY)
        {
            ShowHighlighter(targetGrid.BoundaryCheck(posX, posY, targetItem.Width, targetItem.Height));
            SetHighlightColor(targetGrid.CheckPlaceItem(targetItem, posX, posY));

            SetSize(targetItem, false);
            SetPosition(targetGrid, targetItem, posX, posY);
        }

        private void SetSize(InventoryItem targetItem, bool selectorChange = true)
        {
            Vector2 size = new Vector2();
            size.x = targetItem.Width * ItemGrid.TileSizeWidth;
            size.y = targetItem.Height * ItemGrid.TileSizeHeight;
            highlighter.sizeDelta = size;
            if (selectorChange)
                selector.sizeDelta = size;
        }

        public void SetPosition(ItemGrid targetGrid, InventoryItem targetItem)
        {
            if (targetGrid == null || targetItem == null) return;
            Vector2 pos = targetGrid.CalculatePositionOnGrid(
                targetItem,
                targetItem.onGridPositionX,
                targetItem.onGridPositionY
            );

            highlighter.localPosition = pos;
            selector.localPosition = pos;

            highlighter.SetAsLastSibling();
            selector.SetAsLastSibling();
        }

        public void SetParent(ItemGrid targetGrid)
        {
            if (targetGrid == null)
            {
                return;
            }

            highlighter.SetParent(targetGrid.GetComponent<RectTransform>());
            highlighter.SetAsLastSibling();
        }

        private void SetSelectorParent(ItemGrid targetGrid)
        {
            if (targetGrid == null)
            {
                return;
            }

            selector.SetParent(targetGrid.GetComponent<RectTransform>());
            selector.SetAsLastSibling();
        }

        private void SetPosition(ItemGrid targetGrid, InventoryItem targetItem, int posX, int posY)
        {
            Vector2 pos = targetGrid.CalculatePositionOnGrid(
                targetItem,
                posX,
                posY
            );

            highlighter.localPosition = pos;
            highlighter.SetAsLastSibling();
        }

        private void SetHighlightColor(bool isAble)
        {
            highlighterImg.color = isAble ? enableColor : disableColor;
        }
    }
}