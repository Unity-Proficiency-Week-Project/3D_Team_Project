using System.Collections.Generic;
using UnityEngine;
using static CraftingRecipe;

[CreateAssetMenu(fileName = "Build Data", menuName = "New Build Data")]
public class BuildingData : ScriptableObject
{
    public string displayName;
    public string description;

    public List<Ingredient> ingredients = new List<Ingredient>();

    public GameObject objPrefab;

    public Vector3 uiPosOffset;
    public Vector3 uiRotOffset;
    public Vector3 uiScaleOffset;
}
