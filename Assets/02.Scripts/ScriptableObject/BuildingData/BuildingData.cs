using System.Collections.Generic;
using UnityEngine;
using static CraftRecipe;

[CreateAssetMenu(fileName = "Build Data", menuName = "New Build Data")]
public class BuildingData : ScriptableObject
{
    public string displayName; // 표시될 이름
    public string description; // 설명

    public List<Ingredient> ingredients = new List<Ingredient>(); // 필요한 재료

    public GameObject objPrefab; // 생성할 프리팹

    // 슬롯에서 표시될 위치, 회전, 스케일 값
    public Vector3 uiPosOffset;
    public Vector3 uiRotOffset;
    public Vector3 uiScaleOffset;
}
