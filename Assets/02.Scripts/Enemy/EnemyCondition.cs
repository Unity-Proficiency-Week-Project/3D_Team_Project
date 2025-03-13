using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCondition : Condition
{
    public ItemData[] dropOnDeath;  //죽었을 때 드롭 아이템

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
        Debug.Log("적 사망!");
        
        for (int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        
        }
        Destroy(GetComponentInParent<EnemyAI>().gameObject); //최상위 부모 오브젝트가 사라지도록
    }
}
