using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingWindow;
    public List<CraftingRecipe> recipes;
    public UIInventory inventory;
    

    [Header("UI Components")]
    public TextMeshProUGUI recipeResultText;
    public TextMeshProUGUI feedbackText;
    
    private PlayerInput playerInput;
    private InputAction craftingAction;

    private Player player;
    private void Awake()
    {
        player = FindObjectOfType<Player>();  // 플레이어 찾기
        playerInput = player.GetComponent<PlayerInput>();  // PlayerInput 가져오기
    
        craftingAction = playerInput.actions["Craft"];
        craftingAction.performed += ToggleCraftingUI;
        craftingAction.Enable();
        craftingWindow.SetActive(false);
    }

    private void OnEnable()
    {
        craftingAction.performed += ToggleCraftingUI;
        craftingAction.Enable();
    }

    private void OnDisable()
    {
        craftingAction.performed -= ToggleCraftingUI;
        craftingAction.Disable();
    }

    public void CraftItem(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventory.HasItem(ingredient.itemData, ingredient.quantity))
            {
                feedbackText.text = "재료가 부족합니다.";
                return;
            }
        }

        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.itemData, ingredient.quantity);
        }

        for (int i = 0; i < recipe.resultAmount; i++)
        {
            player.addItem?.Invoke(recipe.resultItem);
        }

        feedbackText.text = $"{recipe.resultItem.displayName} 제작 완료";
        inventory.UpdateUI();
    }

    public void ShowRecipe(CraftingRecipe recipe)
    {
        recipeResultText.text = $"{recipe.resultItem.displayName} x {recipe.resultAmount}";
    }
    private bool isCraftingUIActive = false;

    private void ToggleCraftingUI(InputAction.CallbackContext context)
    {
        if (inventory.gameObject.activeSelf) return;

        isCraftingUIActive = !isCraftingUIActive;
        craftingWindow.SetActive(isCraftingUIActive);
    }
}

