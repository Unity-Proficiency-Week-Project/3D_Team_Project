using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;
    
    public GameObject inventoryUI;
    public Transform slotPanel;
    //public Transform dropPosition; 캐릭터 매니저 작성 이후 주석 해제

    [Header("Selected Item")]
    private ItemSlot selectedItem;
    private int selectedItemIndex;
    
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;

    public GameObject button;
    public GameObject dropButton;

    private int curEquipIndex;
    
    public void SelectItem(float index)
    {
        
    }
}
