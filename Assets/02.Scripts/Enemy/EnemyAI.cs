using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Wandering,
    Chasing,
    Attacking
}

public class EnemyAI : MonoBehaviour
{
    [Header("AI")]
    private NavMeshAgent agent;
    private AIState aiState; //���� AI ����

    [Header("Wandering")]
    public float minWanderingDistance; //�ּ� ��Ȳ �Ÿ�
    public float maxWanderingDistance; //�ִ� ��Ȳ �Ÿ�
    public float minWanderWaitTime; //�ּ� ��Ȳ ���ð�
    public float maxWanderWaitTime; //�ִ� ��Ȳ ���ð�

    [Header("Combat")]
    public float attackRate;
    private float lastAttackTime;
    private float playerDistance; //�÷��̾���� �Ÿ�
    public float fieldOfView = 60f;//�þ߰�
    private Transform firePoint;

    public EnemyData data;
    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers; //������ ���� �� �÷��� ȿ�� �ٶ� ����ϴ� ����
    private EnemyCondition enemyCondition;
    private NavMeshPath path;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        enemyCondition = GetComponent<EnemyCondition>();
    }

    void Start()
    {
        enemyCondition.DamageFlash = () => { StartCoroutine(DamageFlash()); };
        SetState(AIState.Wandering);
        path = new NavMeshPath();
    }

    void Update()
    {
        playerDistance = Vector3.Distance(transform.position, PlayerManager.Instance.Player.transform.position);

        animator.SetBool("IsMoving", aiState != AIState.Idle);

        switch (aiState)
        {
            case AIState.Idle:
            case AIState.Wandering:
                PassiveUpdate();
                break;
            case AIState.Attacking:
                AttackingUpdate();
                break;
        }
    }

    public void SetState(AIState state)
    {
        aiState = state;

        switch (aiState)
        {
            case AIState.Idle:
                agent.speed = data.walkSpeed;
                agent.isStopped = true;
                break;
            case AIState.Wandering:
                agent.speed = data.walkSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attacking:
                agent.speed = data.runSpeed;
                agent.isStopped = false;
                break;
        }
        animator.speed = agent.speed / data.walkSpeed;
    }

    void PassiveUpdate()
    {
        if (aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }
        if (playerDistance < data.detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }

    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderingDistance, maxWanderingDistance)), out hit, maxWanderingDistance, NavMesh.AllAreas);

        int i = 0;

        while (Vector3.Distance(transform.position, hit.position) < data.detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderingDistance, maxWanderingDistance)), out hit, maxWanderingDistance, NavMesh.AllAreas);
            i++;
            if (i == 30) break;
        }
        return hit.position;
    }

    void AttackingUpdate()
    {
        if (playerDistance < data.attackDistance && IsPlayerInFieldOfView()) //���ݹ��� �ȿ� �ְ� �þ߰� �ȿ� ���� �� ����
        {
            agent.isStopped = true;
            if (Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;

                PlayerManager.Instance.Player.condition.GetComponent<IDamageable>().TakePhysicalDamage(data.damage);
                animator.speed = 1;
                animator.SetTrigger("Attack");
                Debug.Log("�÷��̾�� ������ �������ϴ�.");
            }
        }
        else if (playerDistance < data.attackDistance && !IsPlayerInFieldOfView()) // ���ݹ��� �ȿ� ������ �þ߰� �ۿ� ���� �� ȸ��
        {
            Vector3 dir = PlayerManager.Instance.Player.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }

        else if (playerDistance < data.detectDistance) // �����Ÿ� �ȿ� ���� �� ����
        {
            agent.isStopped = false;

            if (agent.CalculatePath(PlayerManager.Instance.Player.transform.position, path))
            {
                agent.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else
            {
                agent.SetDestination(transform.position);
                agent.isStopped = true;
                SetState(AIState.Wandering);
            }
        }
        else //�����Ÿ� �ۿ� ���� �� 
        {
            agent.SetDestination(transform.position);
            agent.isStopped = true;
            SetState(AIState.Wandering);
        }

    }

    bool IsPlayerInFieldOfView()
    {
        Vector3 directionToPlayer = PlayerManager.Instance.Player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < fieldOfView * 0.5f;
    }

    IEnumerator DamageFlash()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material.color = new Color(1.0f, 0.6f, 0.6f);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material.color = Color.white;
    }

    private void ShootProjectile() //���Ÿ� Ÿ���� �� ����
    {
        if (data.projectilePrefab == null || data.enemyType != EnemyType.Far) return; //����ü �������� ���ų�, ���Ÿ� Ÿ���� �ƴ϶�� ��ȯ

        GameObject projectile = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        if(rb == null) return;

        if(rb != null)
        {
            Vector3 dir = (PlayerManager.Instance.Player.transform.position - firePoint.position).normalized;
            rb.velocity = dir * data.projectileSpeed;
        }
    }
}
