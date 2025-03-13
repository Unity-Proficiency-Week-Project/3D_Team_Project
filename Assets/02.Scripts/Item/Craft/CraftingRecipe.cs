using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftingRecipe
{
    public List<ItemData> requiredItems;  
    public ItemData resultItem;          
}