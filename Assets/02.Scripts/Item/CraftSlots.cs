using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftSlots : MonoBehaviour
{
    [Header("Backpack")]
    public List<ItemSlot> backPacks = new List<ItemSlot>();
    public Transform slotPanel;

    [Header("Recipe")]
    public CraftRecipeList recipeList; // 제작 레시피 목록
    public Transform recipeSlotPanel; // 레시피 슬롯 패널
    public List<ItemSlot> recipeSlots = new List<ItemSlot>();

    [Header("Create")]
    public Transform createSlotPanel; // 제작 슬롯 패널
    public List<ItemSlot> createSlots = new List<ItemSlot>();
    public GameObject createSlotPrefab; // 제작 슬롯 프리팹
    public Button createButton;
    public TextMeshProUGUI createRecipeText1;
    public TextMeshProUGUI createRecipeText2;


    [HideInInspector]
    public UIInventory inventory;

    void Start()
    {
        inventory = FindObjectOfType<UIInventory>(true);
        InitializeSlots();
        DisplayRecipes();
        createButton.onClick.AddListener(CraftItem);
        
        SyncInventory();
    }

    void InitializeSlots()
    {
        backPacks.Clear();
        recipeSlots.Clear();
        createSlots.Clear();

        // 백팩 슬롯 초기화
        for (int i = 0; i < slotPanel.childCount; i++)
        {
            ItemSlot slot = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            backPacks.Add(slot);
        }

        // 레시피 슬롯 초기화
        for (int i = 0; i < recipeSlotPanel.childCount; i++)
        {
            ItemSlot slot = recipeSlotPanel.GetChild(i).GetComponent<ItemSlot>();
            recipeSlots.Add(slot);
            slot.button.onClick.AddListener(() => OnRecipeSlotClicked(slot));
        }
    }

    void DisplayRecipes()
    {
        for (int i = 0; i < recipeSlots.Count; i++)
        {
            if (i < recipeList.recipes.Count)
            {
                recipeSlots[i].itemData = recipeList.recipes[i].outputItem;
                recipeSlots[i].quantity = 1;
                recipeSlots[i].Set();
            }
            else
            {
                recipeSlots[i].Clear();
            }
        }
    }

    void OnRecipeSlotClicked(ItemSlot slot)
    {
        if (slot.itemData != null)
        {
            CraftRecipe recipe = recipeList.FindRecipe(slot.itemData);
            if (recipe != null)
            {
                foreach (Transform child in createSlotPanel)
                {
                    if (child.GetComponent<TextMeshProUGUI>() == null)
                    {
                        Destroy(child.gameObject);
                    }
                }

                createSlots.Clear();

                createRecipeText1.text = string.Empty;
                createRecipeText2.text = string.Empty;


                // 결과물 표시 (선택한 제작 아이템)
                GameObject createSlotObj = Instantiate(createSlotPrefab, createSlotPanel);
                ItemSlot createSlot = createSlotObj.GetComponent<ItemSlot>();

                createSlot.itemData = recipe.outputItem;
                createSlot.Set();
                createSlots.Add(createSlot);

                // 재료 표시(최대 2개)
                for (int i = 0; i < recipe.ingredients.Count; i++)
                {
                    var ingredient = recipe.ingredients[i];
                    switch (i)
                    {
                        case 0:
                            createRecipeText1.text = $"{ingredient.item.displayName} : {ingredient.quantity}"; break;
                        case 1:
                            createRecipeText2.text = $"{ingredient.item.displayName} : {ingredient.quantity}"; break;
                    }
                }
            }
        }
    }
    /* 3개 이상 재료 필요시에 재료 생성 코드를 바꾸면 됨

    public Transform ingredientTextPrefab;
    public GameObject ingredientTextPanel;

    foreach (Transform child in ingredientTextPanel)
    {
        Destroy(child.gameObject);
    }

    foreach (var ingredient in recipe.ingredients)
    {
        GameObject textobj = Instantiate(ingredientTextPrefab, ingredientTextPanel);
        TextMeshProUGUI text = textobj.GetComponent<TextMeshProUGUI>();
        text.text = $"{ingredient.item.displayName} : {ingredient.quantity}";
    }
    */
    void CraftItem()
    {
        if (createSlots.Count > 0 && createSlots[0].itemData != null)
        {
            CraftRecipe recipe = recipeList.FindRecipe(createSlots[0].itemData);
            if (recipe != null && CanCraft(recipe))
            {
                foreach (var ingredient in recipe.ingredients)
                {
                    inventory.RemoveItem(ingredient.item, ingredient.quantity);
                }
                inventory.AddItem(recipe.outputItem);
                SyncInventory();
            }
            else
            {
                Debug.Log("제작 조건을 충족하지 못했습니다.");
            }
        }
    }

    bool CanCraft(CraftRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if(ingredient.quantity <= 0 ) continue;
            if (!inventory.HasItem(ingredient.item, ingredient.quantity))
            {
                return false;
            }
        }
        return true;
    }

    public void SyncInventory()
    {
        if (inventory == null && inventory.slots == null) return;
        for (int i = 0; i < backPacks.Count; i++)
        {
            if (i < inventory.slots.Count)
            {
                backPacks[i].itemData = inventory.slots[i].itemData;
                backPacks[i].quantity = inventory.slots[i].quantity;
            }
            else
            {
                backPacks[i].itemData = null;
                backPacks[i].quantity = 0;
            }
            backPacks[i].Set();
        }
    }
}