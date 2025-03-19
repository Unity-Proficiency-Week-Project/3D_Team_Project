using UnityEngine;

public enum EnemyType
{
    Close, //근거리
    Far, //원거리
    Timid //공격X
}
/// <summary>
/// 이 클래스는 적 캐릭터의 다양한 속성을 정의하여, 게임 내에서 적의 행동과 능력을 쉽게 설정하고 관리할 수 있도록 합니다.
/// </summary>
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
