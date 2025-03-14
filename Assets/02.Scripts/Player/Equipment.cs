using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
    public Equip curEquip;
    public Transform equipParent;

    private PlayerController controller;
    private PlayerCondition condition;

    // Start is called before the first frame update
    void Start()
    {
        controller = PlayerManager.Instance.Player.controller;
        condition = PlayerManager.Instance.Player.condition;
    }

    public void EquipNew(ItemData data)
    {
        Unequip();
        curEquip = Instantiate(data.equipPrefab, equipParent).GetComponent<Equip>();
        if(curEquip == null)
        {
            Debug.Log("무기 장비 실패");
        }
    }

    public void Unequip()
    {
        if(curEquip != null)
        {
            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }
}
