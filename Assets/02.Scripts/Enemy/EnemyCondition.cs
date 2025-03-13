using UnityEngine;
using UnityEngine.UI;

public class EnemyCondition : Condition
{
    public ItemData[] dropOnDeath;  //죽었을 때 드롭 아이템
    private Slider Healthbarslider;
    public Camera _camera;

    private void Start()
    {
        Healthbarslider = slider;
    }

    private void LateUpdate()
    {
        Vector3 lookDirection = _camera.transform.forward;
        lookDirection.y = 0; // 수직 방향 변화를 막아 체력바가 뒤집히는 걸 방지
        Healthbarslider.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

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
        //Destroy(GetComponentInParent<EnemyAI>().gameObject); //최상위 부모 오브젝트가 사라지도록
        Destroy(gameObject);
    }
}
