using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public List<ItemSlot> slots = new List<ItemSlot>();
    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;

    [Header("Selected Item")] private ItemSlot selectedItem;
    private int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;

    [Header("Button")] public GameObject actionButton;
    public TextMeshProUGUI actionButtonText;
    public GameObject dropButton;

    private int curEquipIndex;

    private UnityAction currentAction;

    private PlayerController controller;
    private PlayerCondition condition;

    private Player player;
    void Start()
    {
        player = PlayerManager.Instance.Player;
        
        controller = player.controller;
        condition = player.condition;
        dropPosition = player.dropPosition;

        controller.inventory += Toggle;
        player.addItem += AddItem;

        inventoryWindow.SetActive(false);
        slots.Clear();
        
        for (int i = 0; i < slotPanel.childCount; i++)
        {
            ItemSlot slot = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slot.index = i;
            slot.inventory = this;
            slot.Clear();
            slots.Add(slot);
        }
    
        ClearSelectedItemWindow();
        
        InitButton();
    }

    private void InitButton()
    {
        var button = actionButton.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => { currentAction?.Invoke(); });
    }
    
    void ClearSelectedItemWindow()
    {
        selectedItem = null;

        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        actionButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void Toggle()
    {
        inventoryWindow.SetActive(!IsOpen());
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public void AddItem()
    {
        ItemData data = player.itemData;

        if (data.Stackable)
        {
            ItemSlot slot = GetItemStack(data);
            if (slot != null)
            {
                slot.quantity++;
                UpdateUI();
                player.itemData = null;
                return;
            }
        }

        ItemSlot emptySlot = GetEmptySlot();

        if (emptySlot != null)
        {
            emptySlot.itemData = data;
            emptySlot.quantity = 1;
            UpdateUI();
            player.itemData = null;
            return;
        }

        ThrowItem(data);
        player.itemData = null;
    }

    public void UpdateUI()
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemData != null)
            {
                slot.Set();
            }
            else
            {
                slot.Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemData == data && slot.quantity < data.maxStackAmount)
            {
                return slot;
            }
        }

        return null;
    }

    ItemSlot GetEmptySlot()
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemData == null)
            {
                return slot;
            }
        }

        return null;
    }

    public void ThrowItem(ItemData data)
    {
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }


    public void SelectItem(int index)
    {
        if (slots[index].itemData == null)
        {
            ClearSelectedItemWindow();
            return;
        }

        selectedItem = slots[index];
        selectedItemIndex = index;

        selectedItemName.text = selectedItem.itemData.displayName;
        selectedItemDescription.text = selectedItem.itemData.description;
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        for (int i = 0; i < selectedItem.itemData.consumables.Length; i++)
        {
            selectedItemStatName.text += selectedItem.itemData.consumables[i].consumableType.ToString() + "\n";
            selectedItemStatValue.text += selectedItem.itemData.consumables[i].value.ToString() + "\n";
        }

        actionButton.SetActive(true);
        if (selectedItem.itemData.itemType == ItemType.Consumable)
        {
            actionButtonText.text = "사용";
            currentAction = OnUseButton;
        }
        else if (selectedItem.itemData.itemType == ItemType.Equipable)
        {
            if (!slots[index].equipped)
            {
                actionButtonText.text = "장착";
                currentAction = () => OnEquipButton(index);
            }
            else
            {
                actionButtonText.text = "해제";
                currentAction = () => UnEquip(index);
            }
        }
        else
        {
            actionButtonText.text = string.Empty;
            currentAction = null;
        }

        dropButton.SetActive(true);
    }

    public void OnUseButton()
    {
        if (selectedItem.itemData.itemType == ItemType.Consumable)
        {
            foreach (var consumable in selectedItem.itemData.consumables)
            {
                switch (consumable.consumableType)
                {
                    case ConsumableType.Health:
                        condition.Heal(consumable.value); break;
                    case ConsumableType.Hunger:
                        condition.Eat(consumable.value); break;
                    case ConsumableType.Thirst:
                        condition.Drink(consumable.value); break;
                }
            }

            RemoveSelectedItem();
        }
    }

    public void OnEquipButton(int index)
    {
        slots[index].equipped = true;
        UpdateUI();
    }

    public void UnEquip(int index)
    {
        slots[index].equipped = false;
        UpdateUI();
    }


    public void OnDropButton()
    {
        ThrowItem(selectedItem.itemData);
        RemoveSelectedItem();
    }

    void RemoveSelectedItem()
    {
        selectedItem.quantity--;

        if (selectedItem.quantity <= 0)
        {
            if (slots[selectedItemIndex].equipped)
            {
                //UnEquip(selectedItemIndex);
            }

            selectedItem.itemData = null;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }

    public bool HasItem(ItemData item, int quantity)
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemData == item && slot.quantity >= quantity)
            {
                return true;
            }
        }
        return false;
    }
}