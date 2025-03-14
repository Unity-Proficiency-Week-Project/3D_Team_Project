using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : MonoBehaviour
{
    public float attackRate;
    private bool attacking;
    public float attackDistance;

    [Header("Resource")]
    public bool doesGatherResources;

    [Header("Combat")]
    public bool doesDealDamage;
    public float Atk;
    public float Def;
}
