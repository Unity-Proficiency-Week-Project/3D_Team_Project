using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Close, //�ٰŸ�
    Far, //���Ÿ�
    //���� ����..?
}

[System.Serializable]
public class AttackType //�� Ÿ�Կ� ���� ü��, �����, ���ݼӵ�, ���ݹ��� �޶���
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
