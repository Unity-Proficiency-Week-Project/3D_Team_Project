using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCondition : Condition
{
    private Slider Healthbarslider;
    private Camera _camera;
    // 아이템 드랍 풀 (Inspector에서 할당)
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
        Debug.Log("적 사망!");

        List<ItemData> dropList = RandomDropItem();

        foreach (ItemData data in dropList)
        {
            if (data.dropPrefab != null)
            {
                Instantiate(data.dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity); // 아이템 프리팹 생성
            }
            else Debug.Log(data.dropPrefab == null);
        }

        Destroy(gameObject);
    }

    List<ItemData> RandomDropItem()
    {
        int randomCount = UnityEngine.Random.Range(1, droppableItems.Count); // 1~최대 개수까지 랜덤 개수 선택
        //중복허용 O 코드
        List<ItemData> list = new List<ItemData>();
        for (int i = 0; i < randomCount; i++)
        {
            ItemData item = droppableItems[UnityEngine.Random.Range(0, droppableItems.Count)]; // 중복 허용 랜덤 선택
            list.Add(item);
        }
        return list;
    }
}
