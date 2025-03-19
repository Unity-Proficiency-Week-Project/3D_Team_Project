
using System.Collections.Generic;
using UnityEngine;

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
