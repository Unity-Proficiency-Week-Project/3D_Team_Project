using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCondition : Condition
{

    public EnemyData data;  //죽었을 때 드롭 아이템
    private Slider Healthbarslider;
    private Camera _camera;
    // ������ ��� Ǯ (Inspector���� �Ҵ�)
    [SerializeField] private List<ItemData> droppableItems;

    public Action DamageFlash;

    private void Start()
    {
        Healthbarslider = slider;
        _camera = PlayerManager.Instance.Player.GetComponentInChildren<Camera>();
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
        DamageFlash.Invoke();
    }

    public void Die()
    {
        List<ItemData> dropList = RandomDropItem();

        foreach (ItemData data in dropList)
        {
            if (data.dropPrefab != null)
            {
                Instantiate(data.dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity); // ������ ������ ����
            }
            else Debug.Log(data.dropPrefab == null);
        }


        string enemyName = gameObject.name;

        QuestManager.Instance.UpdateQuestProgress(QuestGoalType.KillAnyEnemy, enemyName, 1);
        QuestManager.Instance.UpdateQuestProgress(QuestGoalType.KillSpecificEnemy, enemyName, 1);
        Destroy(gameObject);
    }

    List<ItemData> RandomDropItem()
    {
        int randomCount = UnityEngine.Random.Range(1, droppableItems.Count); // 1~�ִ� �������� ���� ���� ����
        //�ߺ���� O �ڵ�
        List<ItemData> list = new List<ItemData>();
        for (int i = 0; i < randomCount; i++)
        {
            ItemData item = droppableItems[UnityEngine.Random.Range(0, droppableItems.Count)]; // �ߺ� ��� ���� ����
            list.Add(item);
        }
        return list;
    }
}
