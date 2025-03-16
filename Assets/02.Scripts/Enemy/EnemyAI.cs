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
    [SerializeField] private GameObject projectilePrefab;

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

        //animator.speed = 1.0f;

        if (type == EnemyType.Close) //�ٰŸ�Ÿ��
        {
            if (aiState == AIState.Wandering || aiState == AIState.Chasing) //��Ȳ, ���� �����϶� 
            {
                float targetSpeed = agent.velocity.magnitude;
                moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime *20f);

                if (moveSpeed < 0.1f) moveSpeed = 0f;

                normalizedSpeed = Mathf.InverseLerp(0, data.runSpeed, moveSpeed);

                animator.SetFloat("MoveSpeed", normalizedSpeed);

                //animator.speed = 1.0f + (normalizedSpeed * 0.2f); //�ִϸ��̼��� walk�϶��� 1.0���, run�϶� �ִ� 1.2���
            }
            else //����, ���� �����϶�
            {
                animator.SetFloat("MoveSpeed", 0); //����
                //animator.speed = data.animationMoveSpeed;
            }
        }
        //if (type == EnemyType.Far) // ���Ÿ��϶�, �������̶�� 1.2���, ������ 1���
        //{
        //    animator.speed = (aiState == AIState.Attacking) ? data.animationMoveSpeed : 1.0f;
        //}

        //if(animator.speed < 0.1f)
        //{
        //    animator.speed = 1.0f;
        //}
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
        if (aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
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
        else if (playerDistance < data.detectDistance) //�������� �ȿ� ���� �� ����
        {
            agent.isStopped = false;

            if (agent.CalculatePath(PlayerManager.Instance.Player.transform.position, path)) //��ΰ� ��ȿ�ϸ� ����
            {
                agent.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else //��ΰ� ��ȿ���� ������� Wandering ���� ��ȯ
            {
                agent.isStopped = true;
                SetState(AIState.Wandering);
            }
        }
        else //���������� ����� Wandering ���� ��ȯ
        {
            agent.isStopped = true;
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
            SetState(AIState.Chasing);
        }
        else //�����Ÿ� �ۿ� ���� �� 
        {
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

        projectilePrefab = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = data.projectilePrefab.GetComponent<Rigidbody>();
        Vector3 dir = PlayerManager.Instance.Player.transform.position - firePoint.position;

        projectilePrefab.GetComponent<Projectile>().startPos = firePoint.position;
        projectilePrefab.GetComponent<Projectile>().SetDirection(dir);

        rb.velocity = dir * data.projectileSpeed;
    }
}
