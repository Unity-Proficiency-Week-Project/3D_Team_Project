using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Close, //�ٰŸ�
    Far, //���Ÿ�
    //���� ����..?
}

[CreateAssetMenu(fileName = "Enemy", menuName = "New Enemy")]
public class EnemyData : ScriptableObject
{
    [Header("Info")]
    public string enemyName;
    public string discription;
    public EnemyType enemyType;
    //public Sprite Icon;
    public GameObject dropOnDeath; //���߿� óġ���� �� ������ ������ �ʹٸ� �迭�� �������ֱ�

    [Header("Stats")]
    public float health;
    public int damage;
    public float walkSpeed;
    public float runSpeed;
    public float attackRate; //���ݼӵ�
    public float attackDistance; //���ݹ���
    public float detectDistance; //��������

    [Header("Animation")]
    public float moveSpeed; //�ִϸ��̼� �ӵ�, enemy�� �̵��ӵ��� ���� ���� ����
    public float attackSpeed; //���� �ִϸ��̼� �ӵ�

    [Header("Projectile Settings")] //���Ÿ� Ÿ���� �� 
    public GameObject projectilePrefab;  // ����ü ������
    public float projectileSpeed;        // ����ü �ӵ�
}
