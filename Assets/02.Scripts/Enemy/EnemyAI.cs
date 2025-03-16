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
    private AIState aiState; //현재 AI 상태
    private float moveSpeed; //현재 이동 속도 (data.walkSpeed와 data.runSpeed를 총괄)
    private float normalizedSpeed;

    [Header("Wandering")]
    public float minWanderingDistance; //최소 방황 거리
    public float maxWanderingDistance; //최대 방황 거리
    public float minWanderWaitTime; //최소 방황 대기시간
    public float maxWanderWaitTime; //최대 방황 대기시간

    [Header("Combat")]
    private EnemyType type;
    private float lastAttackTime;
    private float playerDistance; //플레이어와의 거리
    public float fieldOfView = 60f;//시야각
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;

    public EnemyData data;
    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers; //데미지 받을 때 플래시 효과 줄때 사용하는 변수
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

        if (type == EnemyType.Close) //근거리타입
        {
            if (aiState == AIState.Wandering || aiState == AIState.Chasing) //방황, 추적 상태일때 
            {
                float targetSpeed = agent.velocity.magnitude;
                moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime *20f);

                if (moveSpeed < 0.1f) moveSpeed = 0f;

                normalizedSpeed = Mathf.InverseLerp(0, data.runSpeed, moveSpeed);

                animator.SetFloat("MoveSpeed", normalizedSpeed);

                //animator.speed = 1.0f + (normalizedSpeed * 0.2f); //애니메이션이 walk일때는 1.0배속, run일때 최대 1.2배속
            }
            else //멈춤, 공격 상태일때
            {
                animator.SetFloat("MoveSpeed", 0); //멈춤
                //animator.speed = data.animationMoveSpeed;
            }
        }
        //if (type == EnemyType.Far) // 원거리일때, 공격중이라면 1.2배속, 나머지 1배속
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

        if (playerDistance < data.attackDistance) //추적을 계속하다가 공격범위 안으로 들어오면 공격
        {
            SetState(AIState.Attacking);
        }
        else if (playerDistance < data.detectDistance) //감지범위 안에 있을 때 추적
        {
            agent.isStopped = false;

            if (agent.CalculatePath(PlayerManager.Instance.Player.transform.position, path)) //경로가 유효하면 추적
            {
                agent.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else //경로가 유효하지 않을경우 Wandering 상태 전환
            {
                agent.isStopped = true;
                SetState(AIState.Wandering);
            }
        }
        else //감지범위를 벗어나면 Wandering 상태 전환
        {
            agent.isStopped = true;
            SetState(AIState.Wandering);
        }
    }

    void AttackingUpdate()
    {
        if (playerDistance < data.attackDistance && IsPlayerInFieldOfView()) //공격범위 안에 있고 시야각 안에 있을 때 공격
        {
            agent.isStopped = true;
            if (Time.time - lastAttackTime > data.attackRate)
            {
                lastAttackTime = Time.time;
                //근거리 원거리에 따라 공격 다르게 
                if (type == EnemyType.Close)
                {
                    PlayerManager.Instance.Player.condition.GetComponent<IDamageable>().TakePhysicalDamage(data.damage);
                    animator.SetTrigger("Attack");
                    Debug.Log($"플레이어에게 공격을 입혔습니다.");
                }
                else if (type == EnemyType.Far)
                {
                    ShootProjectile();
                    Debug.Log($"플레이어에게 공격을 입혔습니다. {data.damage}");
                }
            }
        }
        else if (playerDistance < data.attackDistance && !IsPlayerInFieldOfView()) // 공격범위 안에 있지만 시야각 밖에 있을 때 회전
        {
            Vector3 dir = PlayerManager.Instance.Player.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        }
        else if (playerDistance < data.detectDistance) // 감지거리 안에 있을 때 추적
        {
            SetState(AIState.Chasing);
        }
        else //감지거리 밖에 있을 때 
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
    public void ShootProjectile() //원거리 타입일 때 공격
    {
        if (data.projectilePrefab == null || data.enemyType != EnemyType.Far) return; //투사체 프리팹이 없거나, 원거리 타입이 아니라면 반환
        if (firePoint == null) return;

        projectilePrefab = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = data.projectilePrefab.GetComponent<Rigidbody>();
        Vector3 dir = PlayerManager.Instance.Player.transform.position - firePoint.position;

        projectilePrefab.GetComponent<Projectile>().startPos = firePoint.position;
        projectilePrefab.GetComponent<Projectile>().SetDirection(dir);

        rb.velocity = dir * data.projectileSpeed;
    }
}
