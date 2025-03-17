using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CraftRecipe
{
    public ItemData outputItem;
    public List<Ingredient> ingredients = new List<Ingredient>();
}
[System.Serializable]
public class Ingredient
{
    public ItemData item;
    public int quantity;
}
[CreateAssetMenu(fileName = "CraftRecipeList", menuName = "Crafting/Recipe List")]
public class CraftRecipeList : ScriptableObject
{
    public List<CraftRecipe> recipes = new List<CraftRecipe>();
    public CraftRecipe FindRecipe(ItemData outputItem)
    {
        foreach (var recipe in recipes)
        {
            if (recipe.outputItem == outputItem)
            {
                return recipe;
            }
        }
        return null;
    }
}