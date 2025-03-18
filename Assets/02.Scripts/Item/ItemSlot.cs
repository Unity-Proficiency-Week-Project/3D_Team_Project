using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public ItemData itemData;
    public UIInventory inventory;
    public int index;
    public int quantity;
    public bool equipped;
    
    [Header("Setting")]
    public Image icon;
    public Button button;
    public TextMeshProUGUI quantityText;
    private Outline outline;

    private void Awake()
    {
        outline = GetComponent<Outline>();
    }

    private void OnEnable()
    {
        outline.enabled = equipped;
    }

    public void Set()
    {
        icon.gameObject.SetActive(true);
        icon.sprite = itemData?.icon;
        quantityText.text =  quantity > 1 ? quantity.ToString() : string.Empty;
        
        if(outline != null) outline.enabled = equipped;
    }

    public void Clear()
    {
        itemData = null;
        icon.gameObject.SetActive(false);
        quantityText.text = string.Empty;
    }

    public void OnClickButton()
    {
        inventory.SelectItem(index);
    }

    public void OnCraftButton()
    {
            
    }
}
