using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;

    [Header("Selected Item")]
    private ItemSlot selectedItem;
    private int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;
    
    [Header("Button")]
    public GameObject actionButton;
    public TextMeshProUGUI actionButtonText;
    public GameObject dropButton;

    private int curEquipIndex;

    private UnityAction currentAction;
    
    private PlayerController controller;
    private PlayerCondition condition;

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;

        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;

        inventoryWindow.SetActive(false);
        slots = new ItemSlot[slotPanel.childCount];

        for(int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
            slots[i].Clear();
        }

        ClearSelectedItemWindow();
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
        if (IsOpen())
        {
            inventoryWindow.SetActive(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }
    
    public void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.itemData;

        if (data.Stackable)
        {
            ItemSlot slot = GetItemStack(data);
            if(slot != null)
            {
                slot.quantity++;
                UpdateUI();
                CharacterManager.Instance.Player.itemData = null;
                return;
            }
        }

        ItemSlot emptySlot = GetEmptySlot();

        if(emptySlot != null)
        {
            emptySlot.itemData = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }

        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    public void UpdateUI()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemData != null)
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemData == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }

    ItemSlot GetEmptySlot()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].itemData == null)
            {
                return slots[i];
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
        
        for(int i = 0; i< selectedItem.itemData.consumables.Length; i++)
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
        if(selectedItem.itemData.itemType == ItemType.Consumable)
        {
            for(int i = 0; i < selectedItem.itemData.consumables.Length; i++)
            {
                switch (selectedItem.itemData.consumables[i].consumableType)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.itemData.consumables[i].value); break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.itemData.consumables[i].value); break;
                    case ConsumableType.Thirst:
                        condition.Drink(selectedItem.itemData.consumables[i].value); break;
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

        if(selectedItem.quantity <= 0)
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
        return false;
    }
}