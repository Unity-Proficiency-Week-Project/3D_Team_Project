using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    private Vector3 direction;

    public float maxDistance = 10f;

    public Vector3 startPos;

    /// <summary>
    /// 매 프레임마다 투사체를 이동시키고, 일정 거리 이상 이동하면 파괴합니다.
    /// </summary>
    private void Update() // 일정거리 밖으로 벗어나면 자동으로 오브젝트 파괴
    {
        if(gameObject == null) return;

        transform.position += direction * data.projectileSpeed * Time.deltaTime;

        DestroyProjectile();
    }

    /// <summary>
    /// 투사체의 이동 방향을 설정합니다.
    /// </summary>
    /// <param name="dir">이동 방향</param>
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    /// <summary>
    /// 투사체가 최대 거리를 초과하면 파괴합니다.
    /// </summary>
    void DestroyProjectile()
    {
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 플레이어와 충돌 시 데미지를 입히고 투사체를 파괴합니다.
    /// </summary>
    /// <param name="collision">충돌 정보</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.Player.condition.GetComponent<IDamageable>().TakePhysicalDamage(data.damage);
            Destroy(gameObject);
        }
    }
}
