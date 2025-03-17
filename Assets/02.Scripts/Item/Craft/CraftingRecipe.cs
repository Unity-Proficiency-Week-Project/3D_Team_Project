using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CraftingIngredient
{
    public ItemData itemData;
    public int quantity;
}

[CreateAssetMenu(fileName = "Crafting Recipe", menuName = "Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public List<CraftingIngredient> ingredients;
    public ItemData resultItem;
    public int resultAmount = 1;
}