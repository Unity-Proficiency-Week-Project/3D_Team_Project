using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crafting Recipe", menuName = "Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
{
    [System.Serializable]
    public struct Ingredient // 재료 및 수량
    {
        public ItemData itemData;
        public int quantity;
    }
    
    public List<Ingredient> ingredients = new List<Ingredient>();
    public ItemData resultItem;
    public int resultAmount = 1;
}