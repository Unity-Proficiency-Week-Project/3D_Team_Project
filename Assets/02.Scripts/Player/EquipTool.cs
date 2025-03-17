﻿using System.Collections;
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
    private Camera camera;

    private void Start()
    {
        animator = GetComponent<Animator>();
        camera = Camera.main;
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

    public void OnHit()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if(doesGatherResources &&Physics.Raycast(ray, out hit, attackDistance))
        {
            /*if(doesGatherResources && hit.collider.TryGetComponent(out Resource resource))
            {
                resource.Gather(hit.point, hit.normal);
            }*/

            if (doesDealDamage && hit.collider.TryGetComponent(out EnemyCondition enemy))
            {
                enemy.TakeDamage(PlayerManager.Instance.Player.condition.GetAtk());
            }
        }
    }
}
