using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingWindow;
    public List<CraftRecipeList> recipes; // 레시피 목록
    public UIInventory inventory;

    private PlayerInput playerInput;
    private InputAction craftingAction;

    private Player player;
    private void Awake()
    {
        player = PlayerManager.Instance.Player;
        playerInput = player.GetComponent<PlayerInput>();

        player.controller.craft += Toggle;
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

    public void CraftItem(CraftRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventory.HasItem(ingredient.item, ingredient.quantity))
            {
                return;
            }
        }

        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.item, ingredient.quantity);
        }

        //player.addItem?.Invoke(recipe.outputItem); // recipe.outputItem으로 변경

        inventory.UpdateUI();
    }

    private bool isCraftingUIActive = false;

    private void ToggleCraftingUI(InputAction.CallbackContext context)
    {
        if (inventory.gameObject.activeSelf) return;

        isCraftingUIActive = !isCraftingUIActive;
        craftingWindow.SetActive(isCraftingUIActive);
    }
    public void Toggle()
    {
        craftingWindow.SetActive(!IsOpen());
    }
    public bool IsOpen()
    {
        return craftingWindow.activeInHierarchy;
    }
}