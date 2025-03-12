using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCondition : Condition
{
    Condition health;


    private void Start()
    {
        health = GetComponent<Condition>();
        if(health == null)
        {
            Debug.Log("Enemy health is null");
        }
    }

    private void Update()
    {
        health.slider.enabled = true;
    }
}
