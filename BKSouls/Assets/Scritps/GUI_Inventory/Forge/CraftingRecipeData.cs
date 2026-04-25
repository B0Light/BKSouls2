using System.Collections.Generic;
using UnityEngine;


namespace BK.Inventory
{
    [CreateAssetMenu(fileName = "CraftingRecipe", menuName = "Crafting/Recipe")]
    public class CraftingRecipeData : ScriptableObject
    {
        public CraftingRecipe recipe;
    }

    [System.Serializable]
    public class CraftingRecipe
    {
        public List<RecipeIngredient> ingredients;
        public GridItem resultItem;
        public int resultQuantity;
        // 분해 레시피 등 다중 결과물 지원
        public List<RecipeIngredient> additionalResults = new List<RecipeIngredient>();
    }

    [System.Serializable]
    public class RecipeIngredient
    {
        public GridItem itemData;
        public int quantity;
    }
}