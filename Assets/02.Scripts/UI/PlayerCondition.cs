using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCondition : Condition, IDamageable
{
    Condition health;
    Condition hunger;
    Condition thirst;
    Condition stamina;

    private void Start()
    {
        if (health == null || hunger == null || thirst == null || stamina == null)
        {
            Debug.LogError("PlayerCondition: Condition 컴포넌트가 설정되지 않았습니다.");
        }
    }

    /// <summary>
    /// 사망 시 처리
    /// </summary>
    public void Die()
    {
        Debug.Log("플레이어가 사망했습니다.");
        // TODO: 게임 오버 처리, 리스폰 기능 추가
    }
    /// <summary>
    /// 캐릭터 힐
    /// </summary>
    /// <param name="value">힐량</param>
    public void Heal(float value)
    {
        health.Add(value);
    }
    /// <summary>
    /// 허기 완화
    /// </summary>
    /// <param name="value">허기 완화량</param>
    public void Eat(float value)
    {
        hunger.Add(value);
    }
    /// <summary>
    /// 갈증 완화
    /// </summary>
    /// <param name="value">갈증 완화량</param>
    public void Drink(float value)
    {
        thirst.Add(value);
    }


    /// <summary>
    /// 적으로부터 데미지를 받았을 시 처리
    /// </summary>
    /// <param name="damage"></param>
    public void TakePhysicalDamage(int damage)
    {
        health.curVal -= damage;

        if(health.curVal == 0)
        {
            Die();
        }
    }
}
