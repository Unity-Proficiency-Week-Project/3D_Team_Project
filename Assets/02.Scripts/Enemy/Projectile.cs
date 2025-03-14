using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private EnemyData data;

    private void Start()
    {
        data.projectilePrefab = this.gameObject;
    }
}
