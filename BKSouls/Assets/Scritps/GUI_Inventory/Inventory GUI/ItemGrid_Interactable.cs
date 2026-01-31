using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace BK.Inventory
{
    public class ItemGrid_Interactable : ItemGrid
    {
        [SerializeField] private RectTransform parentRectTransform;
        private List<int> _interactableItemIDList;

        protected override void Init(int width, int height)
        {
            base.Init(width, height);
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = transform.GetChild(i).gameObject;
                InventoryItem inventoryItem = child.GetComponent<InventoryItem>();
                if (inventoryItem) Destroy(child);
            }

            if (parentRectTransform)
            {
                if (GUISize.x > 640) GUISize.x = 640;
                if (GUISize.y > 800) GUISize.y = 800;
                parentRectTransform.sizeDelta = new Vector2(GUISize.x + 30, GUISize.y + 30);
            }

        }

        public override void SetGrid(int width, int height, List<int> setItemList)
        {
            itemIdToCntDict = new SerializedDictionary<int, int>();
            _interactableItemIDList = setItemList;
            base.SetGrid(width, height, setItemList);
        }

        public override bool PlaceItem(InventoryItem inventoryItem, int posX, int posY, bool isLoad = false)
        {
            if (base.PlaceItem(inventoryItem, posX, posY, isLoad))
            {
                if (!isLoad)
                    _interactableItemIDList.Add(inventoryItem.itemInfoData.itemCode);
                return true;
            }
            else
            {
                return false;
            }
        }

        public override InventoryItem PickUpItem(int x, int y)
        {
            InventoryItem pickUpItem = base.PickUpItem(x, y);
            if (pickUpItem)
            {
                _interactableItemIDList.Remove(pickUpItem.itemInfoData.itemCode);
            }

            return pickUpItem;
        }
    }
}