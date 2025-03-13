using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCondition : Condition
{
    public ItemData[] dropOnDeath;  //�׾��� �� ��� ������

    public void TakeDamage(float damage)
    {
        Subtract(damage);
        if (curVal <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("�� ���!");
        
        for (int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        
        }
        Destroy(GetComponentInParent<EnemyAI>().gameObject); //�ֻ��� �θ� ������Ʈ�� ���������
    }
}
