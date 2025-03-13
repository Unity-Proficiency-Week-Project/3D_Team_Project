using UnityEngine;
using UnityEngine.UI;

public class EnemyCondition : Condition
{
    public ItemData[] dropOnDeath;  //�׾��� �� ��� ������
    private Slider Healthbarslider;
    public Camera _camera;

    private void Start()
    {
        Healthbarslider = slider;
    }

    private void LateUpdate()
    {
        Vector3 lookDirection = _camera.transform.forward;
        lookDirection.y = 0; // ���� ���� ��ȭ�� ���� ü�¹ٰ� �������� �� ����
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
        Debug.Log("�� ���!");
        
        for (int i = 0; i < dropOnDeath.Length; i++)
        {
            Instantiate(dropOnDeath[i].dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        
        }
        //Destroy(GetComponentInParent<EnemyAI>().gameObject); //�ֻ��� �θ� ������Ʈ�� ���������
        Destroy(gameObject);
    }
}
