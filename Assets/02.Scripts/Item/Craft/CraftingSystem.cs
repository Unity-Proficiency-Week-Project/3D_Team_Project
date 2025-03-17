using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Progress;

public class CraftingSystem : MonoBehaviour
{
    public GameObject craftingWindow;
    public List<CraftRecipeList> recipes; // 레시피 목록
    public UIInventory inventory; // UIInventory 추가

    private InputAction craftingAction;
    private Player player;

    private void Awake()
    {
        craftingWindow.SetActive(false);

    }
    private void Start()
    {

        player = PlayerManager.Instance.Player;

        playerInput = player.GetComponent<PlayerInput>();

        // craftingAction이 제대로 할당되었는지 확인
        craftingAction = playerInput.actions["Craft"];
        craftingAction.performed += ToggleCraftingUI;
        craftingAction.Enable();

        player.controller.craft += Toggle;
    }

    public void CraftItem(CraftRecipe recipe)
    {
        // 레시피가 null인 경우 처리
        if (recipe == null)
        {
            Debug.LogError("레시피가 null입니다.");
            return;
        }

        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventory.HasItem(ingredient.item, ingredient.quantity))
            {
                Debug.LogError("필요한 재료가 부족합니다.");
                return;
            }
        }

        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.item, ingredient.quantity);
        }

        player.addItem?.Invoke(recipe.outputItem);
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
