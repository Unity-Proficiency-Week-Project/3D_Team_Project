using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCondition : Condition
{
    public EnemyData data;  //죽었을 때 드롭 아이템
    private Slider Healthbarslider;
    private Camera _camera;
    [SerializeField] private List<ItemData> droppableItems;

    public Action DamageFlash;

    /// <summary>
    /// 초기화 메서드. 체력바 슬라이더와 카메라를 설정합니다.
    /// </summary>
    private void Start()
    {
        Healthbarslider = slider;
        _camera = PlayerManager.Instance.Player.GetComponentInChildren<Camera>();
    }

    /// <summary>
    /// 매 프레임마다 체력바가 플레이어를 향하도록 회전시킵니다.
    /// </summary>
    private void LateUpdate()
    {
        Vector3 lookDirection = _camera.transform.forward;
        lookDirection.y = 0; // 수직 방향 변화를 막아 체력바가 뒤집히는 걸 방지
        Healthbarslider.transform.rotation = Quaternion.LookRotation(lookDirection);
    }

    /// <summary>
    /// 데미지를 받았을 때 호출됩니다. 체력을 감소시키고, 체력이 0 이하가 되면 죽습니다.
    /// </summary>
    /// <param name="damage">받은 데미지 양</param>
    public void TakeDamage(float damage)
    {
        Subtract(damage);
        if (curVal <= 0)
        {
            Die();
        }
        DamageFlash.Invoke();
    }

    /// <summary>
    /// 적이 죽었을 때 호출됩니다. 아이템을 드롭하고, 퀘스트 진행 상황을 업데이트한 후 적 오브젝트를 파괴합니다.
    /// </summary>
    public void Die()
    {
        List<ItemData> dropList = RandomDropItem();

        foreach (ItemData data in dropList)
        {
            if (data.dropPrefab != null)
            {
                Instantiate(data.dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity); 
            }
            else Debug.Log(data.dropPrefab == null);
        }


        string enemyName = gameObject.name;

        QuestManager.Instance.UpdateQuestProgress(QuestGoalType.KillAnyEnemy, enemyName, 1);
        QuestManager.Instance.UpdateQuestProgress(QuestGoalType.KillSpecificEnemy, enemyName, 1);

        Destroy(gameObject);
    }

    /// <summary>
    /// 드롭 가능한 아이템 목록에서 무작위로 아이템을 선택하여 반환합니다.
    /// </summary>
    /// <returns>드롭할 아이템 목록</returns>
    List<ItemData> RandomDropItem()
    {
        int randomCount = UnityEngine.Random.Range(1, droppableItems.Count);
        List<ItemData> list = new List<ItemData>();
        for (int i = 0; i < randomCount; i++)
        {
            ItemData item = droppableItems[UnityEngine.Random.Range(0, droppableItems.Count)]; 
            list.Add(item);
        }
        return list;
    }
}
