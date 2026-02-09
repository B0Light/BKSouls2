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
    }

    [System.Serializable]
    public class RecipeIngredient
    {
        public GridItem itemData;
        public int quantity;
    }
}