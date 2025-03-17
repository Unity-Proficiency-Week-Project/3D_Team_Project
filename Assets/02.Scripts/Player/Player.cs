using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerCondition condition;
    public PlayerTemperature temp;
    public Equipment equip;

    public ItemData itemData;
    public Action addItem;
    public CrossHairUI crosshair;

    public Transform dropPosition;

    /// <summary>
    /// PlayerManager의 player 항목과 할당된 컴포넌트를 설정함
    /// </summary>
    private void Awake()
    {
        PlayerManager.Instance.Player = this;
        controller = GetComponent<PlayerController>();
        condition = GetComponent<PlayerCondition>();
        temp = GetComponent<PlayerTemperature>();
        equip = GetComponent<Equipment>();
    }
}
