using System;
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

        int enemyLayerMask = LayerMask.GetMask("Enemy");
        int resourceLayerMask = LayerMask.GetMask("Resource");
        int combinedLayerMask = enemyLayerMask | resourceLayerMask;
        if (Physics.Raycast(ray, out hit, attackDistance, enemyLayerMask))
        {
            Debug.Log("사정거리 내 감지 성공: " + hit.collider.gameObject.name);

            if (doesGatherResources && hit.collider.gameObject.layer == LayerMask.NameToLayer("Resource"))
            {
                if (hit.collider.TryGetComponent(out Resource resource))
                {
                    Debug.Log("자원 채집 실행");
                    resource.Gather(hit.point, hit.normal);
                }
            }

            if (doesDealDamage && hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                if (hit.collider.TryGetComponent(out EnemyCondition enemy))
                {
                    Debug.Log("공격 실행");
                    enemy.TakeDamage(PlayerManager.Instance.Player.condition.GetAtk());
                }
            }
        }
    }
}
