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
    private float moveSpeed; //���� �̵� �ӵ� (data.walkSpeed�� data.runSpeed�� �Ѱ�)
    private float normalizedSpeed;

    [Header("Wandering")]
    public float minWanderingDistance; //�ּ� ��Ȳ �Ÿ�
    public float maxWanderingDistance; //�ִ� ��Ȳ �Ÿ�
    public float minWanderWaitTime; //�ּ� ��Ȳ ���ð�
    public float maxWanderWaitTime; //�ִ� ��Ȳ ���ð�

    [Header("Combat")]
    private EnemyType type;
    private float lastAttackTime;
    private float playerDistance; //�÷��̾���� �Ÿ�
    public float fieldOfView = 60f;//�þ߰�
    [SerializeField] private Transform firePoint;
    public GameObject projectilePrefab;

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
        type = data.enemyType;


        //if (type == EnemyType.Far)
        //{
        //    projectilePrefab = data.projectilePrefab;
        //    //projectilePrefab.GetComponent<Projectile>().startPos = firePoint.position;
        //}

        if (agent == null)
            Debug.Log("agent == null!!!!");

        if (!agent.isOnNavMesh)
            Debug.LogError($"{gameObject.name} NavMesh�� ��ġ���� ����!");
    }

    void Update()
    {
        playerDistance = Vector3.Distance(transform.position, PlayerManager.Instance.Player.transform.position);

        switch (aiState)
        {
            case AIState.Idle:
            case AIState.Wandering:
                PassiveUpdate();
                break;
            case AIState.Chasing:
                ChasingUpdate();
                break;
            case AIState.Attacking:
                AttackingUpdate();
                break;
        }

        AnimationSpeedMultiplier();
    }

    void AnimationSpeedMultiplier()
    {
        if (animator == null) return;

        if (aiState == AIState.Wandering || aiState == AIState.Chasing) //��Ȳ, ���� �����϶� 
        {
            float targetSpeed = agent.velocity.magnitude;
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 10f);

            if (moveSpeed < 0.1f) moveSpeed = 0f;

            normalizedSpeed = Mathf.InverseLerp(0, data.runSpeed, moveSpeed);

            animator.SetFloat("MoveSpeed", normalizedSpeed);

            animator.speed = 1.0f + (normalizedSpeed * 0.2f); //�ִϸ��̼��� walk�϶��� 1.0���, run�϶� �ִ� 1.2���
        }
        else //����, ���� �����϶�
        {
            animator.SetFloat("MoveSpeed", 0); //����
            animator.speed = data.animationMoveSpeed;
        }

        if (animator.speed < 0.1f) //�ִϸ��̼� �ӵ��� ������ ������ ��� 1������� ���ó��
        {
            animator.speed = 1.0f;
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
            case AIState.Chasing:
                agent.speed = data.runSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attacking:
                agent.speed = 0f;
                agent.isStopped = true;
                break;
        }
    }

    void PassiveUpdate()
    {
        if (aiState == AIState.Wandering && agent.remainingDistance < agent.stoppingDistance)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }
        if (playerDistance < data.detectDistance)
        {
            SetState(AIState.Chasing);
        }
        if (playerDistance < data.attackDistance)
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
    void ChasingUpdate()
    {
        if (aiState != AIState.Chasing) return;

        if (playerDistance < data.attackDistance) //������ ����ϴٰ� ���ݹ��� ������ ������ ����
        {
            SetState(AIState.Attacking);
        }

        if (playerDistance < data.detectDistance) //�������� �ȿ� ���� �� ����
        {

            if (agent.CalculatePath(PlayerManager.Instance.Player.transform.position, path)) //��ΰ� ��ȿ�ϸ� ����
            {
                agent.isStopped = false;
                agent.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else //��ΰ� ��ȿ���� ������� Wandering ���� ��ȯ
            {
                SetState(AIState.Wandering);
            }
        }
        else //���������� ����� Wandering ���� ��ȯ
        {
            agent.isStopped = false;
            SetState(AIState.Wandering);
        }
    }

    void AttackingUpdate()
    {
        if (playerDistance < data.attackDistance && IsPlayerInFieldOfView()) //���ݹ��� �ȿ� �ְ� �þ߰� �ȿ� ���� �� ����
        {
            agent.isStopped = true;
            if (Time.time - lastAttackTime > data.attackRate)
            {
                lastAttackTime = Time.time;
                //�ٰŸ� ���Ÿ��� ���� ���� �ٸ��� 
                if (type == EnemyType.Close)
                {
                    PlayerManager.Instance.Player.condition.GetComponent<IDamageable>().TakePhysicalDamage(data.damage);
                    animator.SetTrigger("Attack");
                    Debug.Log($"�÷��̾�� ������ �������ϴ�.");
                }
                else if (type == EnemyType.Far)
                {
                    ShootProjectile();
                    Debug.Log($"�÷��̾�� ������ �������ϴ�. {data.damage}");
                }
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
            SetState(AIState.Chasing);
        }
        else //�����Ÿ� �ۿ� ���� �� 
        {
            agent.isStopped = false;
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
    public void ShootProjectile() //���Ÿ� Ÿ���� �� ����
    {
        if (data.projectilePrefab == null || data.enemyType != EnemyType.Far) return; //����ü �������� ���ų�, ���Ÿ� Ÿ���� �ƴ϶�� ��ȯ
        if (firePoint == null) return;

        GameObject projectilePrefabInstantiate = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = projectilePrefabInstantiate.GetComponent<Rigidbody>();
        Vector3 dir = (PlayerManager.Instance.Player.transform.position - firePoint.position).normalized;

        projectilePrefabInstantiate.GetComponent<Projectile>().startPos = firePoint.position;
        projectilePrefabInstantiate.GetComponent<Projectile>().SetDirection(dir);

        rb.velocity = dir * data.projectileSpeed;
    }
}
