using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private EnemyData data;

    private Vector3 direction;

    public float maxDistance = 10f;

    public Vector3 startPos;

    private void Update() // �����Ÿ� ������ ����� �ڵ����� ������Ʈ �ı�
    {
        if(gameObject == null) return;

        transform.position += direction * data.projectileSpeed * Time.deltaTime;

        DestroyProjectile();
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void DestroyProjectile()
    {
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerManager.Instance.Player.condition.GetComponent<IDamageable>().TakePhysicalDamage(data.damage);
            Destroy(gameObject);
        }
    }
}
