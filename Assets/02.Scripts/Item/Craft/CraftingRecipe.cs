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
