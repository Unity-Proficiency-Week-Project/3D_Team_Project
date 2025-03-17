using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftSlots : MonoBehaviour
{
    public List<ItemSlot> craftSlots = new List<ItemSlot>();
    public GameObject craftWindow;
    public Transform slotPanel;
    public UIInventory inventory;

    void Start()
    {
        craftWindow.SetActive(true);
        craftSlots.Clear();
        
        for (int i = 0; i < slotPanel.childCount; i++)
        {
            ItemSlot slot = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slot.index = i;
            //slot.inventory = this;
            slot.Clear();
            craftSlots.Add(slot);
        }
    }

    public void AddItemToCraftSlot(ItemData item)
    {
        foreach (ItemSlot slot in craftSlots)
        {
            if (slot.itemData == null)
            {
                slot.itemData = item;
                slot.quantity = 1;
                slot.Set();
                //inventory.AddItem(item,1);
                return;
            }
        }
        Debug.Log("모든 제작 슬롯이 가득 찼습니다.");
    }
}