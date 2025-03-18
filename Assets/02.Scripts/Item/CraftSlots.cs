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
    public TextMeshProUGUI createRecipeText_1;
    public TextMeshProUGUI createRecipeText_2;


    [HideInInspector]
    public UIInventory inventory;

    void Start()
    {
        InitializeSlots();
        DisplayRecipes();
        createButton.onClick.AddListener(CraftItem);
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
                // 결과물 표시
                if (createSlots.Count == 0)
                {
                    GameObject createSlotObj = Instantiate(createSlotPrefab, createSlotPanel);
                    ItemSlot createSlot = createSlotObj.GetComponent<ItemSlot>();
                    createSlots.Add(createSlot);
                }
                createSlots[0].itemData = recipe.outputItem;
                createSlots[0].quantity = 1;
                createSlots[0].Set();

                // 재료 표시
                foreach (var ingredient in recipe.ingredients)
                {
                    CreateIngredientSlot(ingredient.item, ingredient.quantity);
                }
                
            }
        }
    }

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
            if (!inventory.HasItem(ingredient.item, ingredient.quantity))
            {
                return false;
            }
        }
        return true;
    }

    public void SyncInventory()
    {
        for (int i = 0; i < backPacks.Count; i++)
        {
            if (i < inventory.slots.Count && inventory.slots[i].itemData != null)
            {
                backPacks[i].itemData = inventory.slots[i].itemData;
                backPacks[i].quantity = inventory.slots[i].quantity;
                backPacks[i].Set();
            }
            else
            {
                backPacks[i].Clear();
            }
        }
    }
    void CreateIngredientSlot(ItemData item, int quantity)
    {
        GameObject slotObj = Instantiate(createSlotPrefab, createSlotPanel);
        ItemSlot newSlot = slotObj.GetComponent<ItemSlot>();
        
        newSlot.itemData = item;
        newSlot.Set();

        createRecipeText_1.text = $"{item.displayName} : {quantity}";
        createRecipeText_2.text = $"{item.displayName} : {quantity}";
    }
}
