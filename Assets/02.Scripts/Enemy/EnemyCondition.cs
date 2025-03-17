using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCondition : Condition
{
    public EnemyData data;  //�׾��� �� ��� ������
    private Slider Healthbarslider;
    private Camera _camera;

    
    public Action DamageFlash;

    private void Start()
    {
        Healthbarslider = slider;
        _camera = PlayerManager.Instance.Player.GetComponentInChildren<Camera>();
        data = GetComponent<EnemyAI>().data;
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
        DamageFlash.Invoke();
    }

    public void Die()
    {
        Debug.Log("�� ���!");
        
        for (int i = 0; i < data.dropOnDeath.Length; i++)
        {
            Instantiate(data.dropOnDeath[i], transform.position + Vector3.up * 2, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
