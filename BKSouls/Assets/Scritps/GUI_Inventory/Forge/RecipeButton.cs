using UnityEngine;
using UnityEngine.UI;

namespace BK.Inventory
{
    public class RecipeButton : MonoBehaviour
    {
        private CraftingRecipeData _recipeData;
        private Button _button;
        private PopupOnHover _popupOnHover;
        private ForgeGUIManager _forgeGUIManager;
        [SerializeField] private Image itemIcon;
        [SerializeField] private Image itemBackground;

        private void Awake()
        {
            _button = GetComponentInChildren<Button>();
            _popupOnHover = GetComponent<PopupOnHover>();
        }

        public void Init(ForgeGUIManager forgeGUIManager, CraftingRecipeData recipeData)
        {
            _forgeGUIManager = forgeGUIManager;
            _recipeData = recipeData;
            itemIcon.sprite = recipeData.recipe.resultItem.itemIcon;
            itemBackground.color =
                WorldItemDatabase.Instance.GetItemColorByTier(recipeData.recipe.resultItem.itemTier);
            _button.onClick.AddListener(SetItem);
            CheckCanCreateItem();
            _popupOnHover.Init(recipeData);
        }

        public void CheckCanCreateItem()
        {
            foreach (var recipeIngredient in _recipeData.recipe.ingredients)
            {
                if (WorldPlayerInventory.Instance.GetItemCountInAllInventory(recipeIngredient.itemData.itemID) <
                    recipeIngredient.quantity)
                {
                    _button.interactable = false;
                    return;
                }
            }

            _button.interactable = true;
        }

        private void SetItem()
        {
            foreach (var itemPair in _forgeGUIManager.GetItemGrid.GetCurItemDictById())
            {
                WorldPlayerInventory.Instance.AddItemById(itemPair.Key, itemPair.Value);
            }

            _forgeGUIManager.GetItemGrid.ResetItemGrid();

            foreach (var recipeIngredient in _recipeData.recipe.ingredients)
            {
                WorldPlayerInventory.Instance.RemoveItemInInventory(recipeIngredient.itemData.itemID,
                    recipeIngredient.quantity);
                _forgeGUIManager.GetItemGrid.AddItemById(recipeIngredient.itemData.itemID, recipeIngredient.quantity,
                    false);
            }

            CheckCanCreateItem();
        }
    }
}