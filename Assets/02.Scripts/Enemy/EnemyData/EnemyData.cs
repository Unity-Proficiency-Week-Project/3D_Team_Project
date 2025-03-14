using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Close, //근거리
    Far, //원거리
    //범위 공격..?
}

[System.Serializable]
public class AttackType //적 타입에 따라 체력, 대미지, 공격속도, 공격범위 달라짐
{
    public EnemyType type;
    public float health;
    public int damage;
    public float attackRate;
    public float attackDistance;
    public float detectDistance;
}

[CreateAssetMenu(fileName = "Enemy", menuName = "New Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName;
    public string discription;
    public EnemyType enemyType;
    //public Sprite Icon;
    public GameObject dropOnDeath;

    public AttackType[] attacks;
}
