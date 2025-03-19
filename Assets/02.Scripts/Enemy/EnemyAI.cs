﻿using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Wandering,
    Chasing,
    Attacking,
    Fleeing
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
    public GameObject projectilePrefab;

    public EnemyData data;
    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers; //데미지 받을 때 플래시 효과 줄때 사용하는 변수
    private EnemyCondition enemyCondition;
    private NavMeshPath path;

    /// <summary>
    /// 컴포넌트를 초기화합니다.
    /// </summary>
    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        enemyCondition = GetComponent<EnemyCondition>();
    }

    /// <summary>
    /// 초기 상태를 설정하고, 네비게이션 경로를 초기화합니다.
    /// </summary>
    void Start()
    {
        enemyCondition.DamageFlash = () => { StartCoroutine(DamageFlash()); };
        SetState(AIState.Wandering);
        path = new NavMeshPath();
        type = data.enemyType;

        if (agent == null)
            Debug.Log("agent == null!!!!");

        if (!agent.isOnNavMesh)
            Debug.LogError($"{gameObject.name} NavMesh에 배치되지 않음!");
    }

    /// <summary>
    /// AI 상태에 따라 적 캐릭터의 행동을 업데이트합니다.
    /// 상태와 이동속도에 따라 애니메이션 속도를 조정합니다.
    /// </summary>
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
            case AIState.Fleeing:
                FleeingUpdate();
                break;
        }

        AnimationSpeedMultiplier();
    }

    /// <summary>
    /// 이동 속도에 따라 애니메이션 속도를 조정합니다.
    /// </summary>
    void AnimationSpeedMultiplier()
    {
        if (animator == null) return;

        if (aiState == AIState.Wandering || aiState == AIState.Chasing) //방황, 추적 상태일때 
        {
            agent.isStopped = false;
            float targetSpeed = agent.velocity.magnitude;
            moveSpeed = Mathf.Lerp(moveSpeed, targetSpeed, Time.deltaTime * 10f);

            if (moveSpeed < 0.1f) moveSpeed = 0f;

            normalizedSpeed = Mathf.InverseLerp(0, data.runSpeed, moveSpeed);

            animator.SetFloat("MoveSpeed", normalizedSpeed);

            animator.speed = 1.0f + (normalizedSpeed * 0.2f); //애니메이션이 walk일때는 1.0배속, run일때 최대 1.2배속
        }
        else if (aiState == AIState.Idle || aiState == AIState.Attacking)//멈춤, 공격 상태일때
        {
            animator.SetFloat("MoveSpeed", 0); //멈춤
            agent.isStopped = true;
            animator.speed = data.animationMoveSpeed;
        }
        else if (aiState == AIState.Fleeing)
        {
            animator.SetFloat("MoveSpeed", 1);
            agent.isStopped = false;
            animator.speed = data.animationMoveSpeed;
        }

        if (animator.speed < 0.1f) //애니메이션 속도가 현저히 낮아질 경우 1배속으로 방어처리
        {
            animator.speed = 1.0f;
        }
    }

    /// <summary>
    /// AI 상태를 전환하고, 상태에 따라 속도와 애니메이션을 설정합니다.
    /// </summary>
    /// <param name="state"></param>
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
            case AIState.Fleeing:
                agent.speed = data.runSpeed;
                agent.isStopped = false;
                break;
        }
    }

    /// <summary>
    /// Idle 및 Wandering 상태에서의 동작을 처리합니다.
    /// </summary>
    void PassiveUpdate()
    {
        if (aiState == AIState.Wandering && agent.remainingDistance < agent.stoppingDistance)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }

        if (playerDistance < data.detectDistance)
        {
            if (type == EnemyType.Timid)
                SetState(AIState.Fleeing);
            else
                SetState(AIState.Chasing);
        }

        if (playerDistance < data.attackDistance && type != EnemyType.Timid)
        {
            SetState(AIState.Attacking);
        }
    }

    /// <summary>
    /// 새로운 방황 위치를 설정합니다.
    /// </summary>
    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    /// <summary>
    /// 방황할 위치를 계산합니다. 무한 루프를 방지하기 위해 최대 30번 시도합니다.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Chasing 상태에서의 동작을 처리합니다.
    /// </summary>
    void ChasingUpdate()
    {
        if (aiState != AIState.Chasing) return;

        if (playerDistance < data.attackDistance) //추적을 계속하다가 공격범위 안으로 들어오면 공격
        {
            SetState(AIState.Attacking);
        }

        if (playerDistance < data.detectDistance) //감지범위 안에 있을 때 추적
        {

            if (agent.CalculatePath(PlayerManager.Instance.Player.transform.position, path)) //경로가 유효하면 추적
            {
                agent.isStopped = false;
                agent.SetDestination(PlayerManager.Instance.Player.transform.position);
            }
            else //경로가 유효하지 않을경우 Wandering 상태 전환
            {
                SetState(AIState.Wandering);
            }
        }
        else //감지범위를 벗어나면 Wandering 상태 전환
        {
            agent.isStopped = false;
            SetState(AIState.Wandering);
        }
    }

    /// <summary>
    /// Fleeing 상태에서의 동작을 처리합니다.
    /// </summary>
    void FleeingUpdate()
    {
        if (aiState != AIState.Fleeing) return;

        if (agent.remainingDistance > agent.stoppingDistance) return;


        if (playerDistance < data.detectDistance) //감지범위 안으로 플레이어가 들어오면 임의의 좌표로 도망감
        {
            agent.isStopped = false;
            Vector3 fleedir = (transform.position - PlayerManager.Instance.Player.transform.position).normalized;

            Vector3 fleeTargetPos = transform.position + fleedir * data.fleeDistance;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeTargetPos, out hit, data.fleeDistance, NavMesh.AllAreas))
            {
                fleeTargetPos = hit.position;
            }

            Quaternion targetRot = Quaternion.LookRotation(fleedir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * data.rotSpeed);

            agent.SetDestination(fleeTargetPos);
        }
        else
        {
            SetState(AIState.Wandering);
        }
    }

    /// <summary>
    /// Attacking 상태에서의 동작을 처리합니다.
    /// </summary>
    void AttackingUpdate()
    {
        if (aiState != AIState.Attacking) return;

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
                    animator.SetTrigger("Attack");
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
            agent.isStopped = false;
            SetState(AIState.Chasing);
        }
        else //감지거리 밖에 있을 때 
        {
            agent.isStopped = false;
            SetState(AIState.Wandering);
        }
    }

    /// <summary>
    /// 플레이어가 시야각 내에 있는지 확인합니다.
    /// </summary>
    /// <returns></returns>
    bool IsPlayerInFieldOfView()
    {
        Vector3 directionToPlayer = PlayerManager.Instance.Player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        return angle < fieldOfView * 0.5f;
    }

    /// <summary>
    /// 데미지를 받을 때 플래시 효과를 줍니다.
    /// </summary>
    /// <returns></returns>
    IEnumerator DamageFlash()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material.color = new Color(1.0f, 0.6f, 0.6f);

        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].material.color = Color.white;
    }

    /// <summary>
    /// 원거리 타입인 적이 공격 시 발사체를 발사합니다.
    /// </summary>
    public void ShootProjectile() //원거리 타입일 때 공격
    {
        if (data.projectilePrefab == null || data.enemyType != EnemyType.Far) return; //투사체 프리팹이 없거나, 원거리 타입이 아니라면 반환
        if (firePoint == null) return;

        GameObject projectilePrefabInstantiate = Instantiate(data.projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody rb = projectilePrefabInstantiate.GetComponent<Rigidbody>();
        Vector3 dir = (PlayerManager.Instance.Player.transform.position - firePoint.position).normalized;

        projectilePrefabInstantiate.GetComponent<Projectile>().startPos = firePoint.position;
        projectilePrefabInstantiate.GetComponent<Projectile>().SetDirection(dir);

        rb.velocity = dir * data.projectileSpeed;
    }
}
