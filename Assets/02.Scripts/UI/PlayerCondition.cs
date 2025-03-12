using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCondition : MonoBehaviour, IDamageable
{
    public UICondition uiCondition;
    Condition health { get { return uiCondition.health; } }
    Condition hunger { get { return uiCondition.hunger; } }
    Condition thirst { get { return uiCondition.thrist; } }
    Condition stamina { get { return uiCondition.stamina; } }

    public float noHungerHealthDecay;
    public float NoThristHealthDecay;
    public float staminaRecoverRate;
    public float hungerReduceRate;
    public float thristReduceRate;

    void Update()
    {
        hunger.Subtract(hungerReduceRate * Time.deltaTime);
        thirst.Subtract(thristReduceRate * Time.deltaTime);
        if(hunger.curVal > 0 && thirst.curVal > 0)
        {
            stamina.Add(staminaRecoverRate * Time.deltaTime);
        }
        
        if(thirst.curVal <= 0)
        {
            health.Subtract(NoThristHealthDecay * Time.deltaTime);
        }
        if(hunger.curVal <= 0)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
        }

        if (health.curVal <= 0f)
        {
            Die();
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
        health.Subtract(damage);

        if (health.curVal == 0)
        {
            Die();
        }
    }

    public void RecoverStamina()
    {
        stamina.Add(Time.deltaTime * staminaRecoverRate);
    }
}
