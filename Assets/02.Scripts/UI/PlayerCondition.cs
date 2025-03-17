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

    [Header("Base Stats")]
    public float baseAtk = 10f;
    public float baseAtkSpeed = 1f;
    public float baseStaminaCost = 2f;
    public float baseDef = 0f;

    private float curAtk;
    private float curAtkSpeed;
    private float curStaminaCost;
    private float curDef;

    public float noHungerHealthDecay;
    public float NoThristHealthDecay;
    public float staminaRecoverRate;
    public float hungerReduceRate;
    public float thristReduceRate;

    private void Start()
    {
        ResetStats();
    }
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

    public void SubtrackThirst(float value)
    {
        thirst.Subtract(value);
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

    public void ColdDamage(float damage)
    {
        health.Subtract(damage);

        if(health.curVal == 0)
        {
            Die();
        }
    }

    public void RecoverStamina()
    {
        stamina.Add(Time.deltaTime * staminaRecoverRate);
    }

    public bool IsUsableStamina(float value)
    {
        return stamina.curVal >= value;
    }

    public void UseStamina(float value)
    {
        stamina.Subtract(value);
    }

    public void ResetStats()
    {
        curAtk = baseAtk;
        curAtkSpeed = baseAtkSpeed;
        curStaminaCost = baseStaminaCost;
        curDef = baseDef;

        Debug.Log("기본 스탯으로 복귀");
    }

    public void ApplyEquipStats(Equip equip)
    {
        if(equip is EquipTool equipTool)
        {
            curAtk = baseAtk + equipTool.Atk;
            curDef = baseDef + equipTool.Def;
            curAtkSpeed = baseAtkSpeed * equipTool.attackRate;
            curStaminaCost = baseStaminaCost * (equipTool.attackRate > 1 ? equipTool.attackRate : 1);
        }
        else
        {
            ResetStats();
        }
    }

    public float GetAtk() => curAtk;
    public float GetAtkSpd() => curAtkSpeed;
    public float GetDef() => curDef;
    public float GetStamina() => curStaminaCost;
}
