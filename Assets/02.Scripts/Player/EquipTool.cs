using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
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

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnAttackInput()
    {
        if(!attacking)
        {
            attacking = true;
            animator.SetTrigger("Attack");
            Invoke("OnCanAttack", attackRate);
        }
    }

    void OnCanAttack()
    {
        attacking = false;
    }
}
