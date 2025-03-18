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
    public int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;

    [Header("Button")] public GameObject actionButton;
    public TextMeshProUGUI actionButtonText;
    public GameObject dropButton;

    public int curEquipIndex;

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

    public void AddItem(ItemData item)
    {
        if (item.Stackable)
        {
            ItemSlot slot = GetItemStack(item);
            if (slot != null)
            {
                slot.quantity++;
                UpdateUI();
                return;
            }
        }

        ItemSlot emptySlot = GetEmptySlot();

        if (emptySlot != null)
        {
            emptySlot.itemData = item;
            emptySlot.quantity = 1;
            UpdateUI();
            return;
        }

        ThrowItem(item);
    }

    public void UpdateUI()
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemData != null)
            {
                slot.Set();
                
                InitButton();
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
        dropButton.SetActive(true);
        
        switch (selectedItem.itemData.itemType)
        {
            case ItemType.Consumable:
                actionButtonText.text = "사용";
                currentAction = () => OnUseButton();
                break;

            case ItemType.Equipable:
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
                actionButton.SetActive(true);
                break;

            default:
                actionButtonText.text = string.Empty;
                currentAction = null;
                actionButton.SetActive(false);
                break;
        }


        InitButton();
        
    }
    public void OnUseButton()
    {
        foreach (var consumable in selectedItem.itemData.consumables)
        {
            switch (consumable.consumableType)
            {
                case ConsumableType.Health:
                    condition.Heal(consumable.value); break;
                case ConsumableType.Hunger:
                    condition.Eat(consumable.value); break;
            }
        }

        RemoveSelectedItem();
        actionButton.SetActive(false);
    }

    public void OnEquipButton(int index)
    {
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }
        slots[selectedItemIndex].equipped = true;
        curEquipIndex = selectedItemIndex;
        PlayerManager.Instance.Player.equip.EquipNew(selectedItem.itemData);
        UpdateUI();

        SelectItem(selectedItemIndex);
    }

    public void UnEquip(int index)
    {
        slots[index].equipped = false;
        PlayerManager.Instance.Player.equip.Unequip();

        if (selectedItemIndex == index)
        {
            SelectItem(selectedItemIndex);
        }
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
                UnEquip(selectedItemIndex);
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

    public void RemoveItem(ItemData item, int quantity)
    {
        foreach (ItemSlot slot in slots)
        {
            if (slot.itemData == item)
            {
                slot.quantity -= quantity;

                if (slot.quantity <= 0)
                {
                    slot.itemData = null;
                    slot.quantity = 0;
                }
                UpdateUI();
                return;
            }
        }
    }
    
    public void OnRecipeSelected(Object obj)
    {
        var inventory = FindObjectOfType<UIInventory>();
        if (inventory != null)
        {
            inventory.AddItem(obj as ItemData);
            Debug.Log($"레시피 선택됨: {obj.name}");
        
            if (!inventory.IsOpen())
            {
                inventory.Toggle();
            }
        }
    }
}
