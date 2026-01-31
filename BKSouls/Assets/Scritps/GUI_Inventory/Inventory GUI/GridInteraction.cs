using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace BK.Inventory
{
    public class GridInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private ItemGrid itemGrid;

        private void Awake()
        {
            itemGrid = GetComponent<ItemGrid>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GUIController.Instance.inventoryGUIManager.inventoryController.SelectedItemGrid = itemGrid;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            GUIController.Instance.inventoryGUIManager.inventoryController.SelectedItemGrid = null;
        }
    }
}