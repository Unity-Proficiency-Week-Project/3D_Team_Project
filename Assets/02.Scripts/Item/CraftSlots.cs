using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftSlots : MonoBehaviour
{
    public List<ItemSlot> craftSlots = new List<ItemSlot>();
    public GameObject craftWindow;
    public Transform slotPanel;
    public UIInventory inventory;
    public CraftRecipeList recipeList; // 제작 레시피 목록
    public Transform recipeSlotPanel; // 레시피 슬롯 패널
    public List<ItemSlot> recipeSlots = new List<ItemSlot>();
    public Transform createSlotPanel; // 제작 슬롯 패널
    public List<ItemSlot> createSlots = new List<ItemSlot>();
    public Button createButton;
    
    void Start()
    {
        craftWindow.SetActive(true);
        craftSlots.Clear();
        recipeSlots.Clear();
        createSlots.Clear();

        // 백팩 슬롯 초기화
        for (int i = 0; i < slotPanel.childCount; i++)
        {
            ItemSlot slot = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slot.index = i;
            slot.Clear();
            craftSlots.Add(slot);
        }

        // 레시피 슬롯 초기화
        for (int i = 0; i < recipeSlotPanel.childCount; i++)
        {
            ItemSlot slot = recipeSlotPanel.GetChild(i).GetComponent<ItemSlot>();
            slot.index = i;
            slot.Clear();
            recipeSlots.Add(slot);
            slot.button.onClick.AddListener(() => OnRecipeSlotClicked(slot));
        }

        // 제작 슬롯 초기화
        for (int i = 0; i < createSlotPanel.childCount; i++)
        {
            ItemSlot slot = createSlotPanel.GetChild(i).GetComponent<ItemSlot>();
            slot.index = i;
            slot.Clear();
            createSlots.Add(slot);
        }

        // 레시피 목록 표시
        DisplayRecipes();

        // 제작 버튼 이벤트 추가
        createButton.onClick.AddListener(CraftItem);
    }

    private void Update()
    {
        TransferItemsFromInventory();
    }

    // 인벤토리 아이템을 백팩 슬롯에 동기화
    public void SyncInventory()
    {
        for (int i = 0; i < craftSlots.Count; i++)
        {
            if (i < inventory.slots.Count && inventory.slots[i].itemData != null)
            {
                craftSlots[i].itemData = inventory.slots[i].itemData;
                craftSlots[i].quantity = inventory.slots[i].quantity;
                craftSlots[i].Set();
            }
            else
            {
                craftSlots[i].Clear();
            }
        }
    }

    // 레시피 목록 표시
    void DisplayRecipes()
    {
        for (int i = 0; i < recipeSlots.Count; i++)
        {
            if (i < recipeList.recipes.Count && recipeList.recipes[i] != null)
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

    // 레시피 슬롯 클릭 시 제작 슬롯에 아이템 추가
    void OnRecipeSlotClicked(ItemSlot slot)
    {
        if (slot.itemData != null)
        {
            createSlots[0].itemData = slot.itemData;
            createSlots[0].quantity = 1;
            createSlots[0].Set();
        }
    }

    // 아이템 제작
    void CraftItem()
    {
        if (createSlots[0].itemData != null)
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

    // 제작 가능 여부 확인
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

    public void TransferItemsFromInventory()
    {
        for (int i = 0; i < inventory.slots.Count; i++)
        {
            // UIInventory의 슬롯이 비어있지 않으면 craftSlots에 추가
            if (inventory.slots[i].itemData != null)
            {
                // craftSlots에 아이템을 추가
                if (i < craftSlots.Count) // craftSlots가 UIInventory보다 적을 경우를 처리
                {
                    craftSlots[i].itemData = inventory.slots[i].itemData;
                    craftSlots[i].quantity = inventory.slots[i].quantity;
                    craftSlots[i].Set(); // UI 업데이트
                }
            }
            else
            {
                // 비어있으면 CraftSlots 슬롯을 클리어
                if (i < craftSlots.Count)
                {
                    craftSlots[i].Clear();
                }
            }
        }
    }
}