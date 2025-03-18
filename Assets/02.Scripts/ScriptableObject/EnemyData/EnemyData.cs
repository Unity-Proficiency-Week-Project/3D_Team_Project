using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Close, //근거리
    Far, //원거리
    Timid //공격X
}

[CreateAssetMenu(fileName = "Enemy", menuName = "New Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName;
    public string discription;
    public EnemyType enemyType;
    //public Sprite Icon;

    [Header("Stats")]
    public float health;
    public int damage;
    public float walkSpeed;
    public float runSpeed;
    public float attackRate; //공격속도
    public float attackDistance; //공격범위
    public float detectDistance; //감지범위
    public float fleeDistance; //도망범위
    public float rotSpeed;

    [Header("Animation")]
    public float animationMoveSpeed; //애니메이션 속도, enemy의 이동속도에 따라 조절 가능

    [Header("Projectile Settings")] //원거리 타입일 때 
    public GameObject projectilePrefab;  // 투사체 프리팹
    public float projectileSpeed;        // 투사체 속도
  
}
