using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BK.Inventory
{
    public class PopupOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Transform recipeSlot;
        [SerializeField] private GameObject resourcePrefab;

        private CraftingRecipeData _recipeData;

        public void Init(CraftingRecipeData recipeData)
        {
            if (canvasGroup != null)
                SetActivePanel(false);

            _recipeData = recipeData;
            SetSlot();
        }

        private void SetSlot()
        {
            recipeSlot.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));

            foreach (var recipeIngredient in _recipeData.recipe.ingredients)
            {
                ShopCostItem shopCostItem = Instantiate(resourcePrefab, recipeSlot).GetComponent<ShopCostItem>();
                shopCostItem.Init(recipeIngredient.itemData, recipeIngredient.quantity);
            }
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            SetSlot();
            if (canvasGroup != null)
                SetActivePanel(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (canvasGroup != null)
                SetActivePanel(false);
        }

        private void SetActivePanel(bool value)
        {
            canvasGroup.alpha = value ? 1 : 0;
            canvasGroup.interactable = value;
            canvasGroup.blocksRaycasts = value;
        }
    }
}
