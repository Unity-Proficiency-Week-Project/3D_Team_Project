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
        playerInput = GetComponent<PlayerInput>();
        player = PlayerManager.Instance.Player;
        playerInput = new PlayerInput();
        //craftingAction = playerInput.;
    }

    private void OnEnable()
    {
        //craftingAction.performed += ToggleCraftingUI;
        craftingAction.Enable();
    }

    private void OnDisable()
    {
        //craftingAction.performed -= ToggleCraftingUI;
        craftingAction.Disable();
    }

    private void toggleCraftingUI(InputAction.CallbackContext context)
    {
        craftingWindow.SetActive(!craftingWindow.activeSelf);
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
            //PlayerManager.Instance.Player.addItem?.Invoke(recipe.resultItem);
        }

        feedbackText.text = $"{recipe.resultItem.displayName} 제작 완료";
        inventory.UpdateUI();
    }

    public void ShowRecipe(CraftingRecipe recipe)
    {
        recipeResultText.text = $"{recipe.resultItem.displayName} x {recipe.resultAmount}";
    }
}

