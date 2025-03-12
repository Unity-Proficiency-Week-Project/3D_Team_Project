using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Condition : MonoBehaviour
{
    public float curVal;
    public float maxVal;
    public float startVal;
    public Slider slider;

    /// <summary>
    /// 시작 값 초기화
    /// </summary>
    private void Start()
    {
        curVal = startVal;
        if (slider != null)
        {
            slider.maxValue = 1; // 최대 체력 설정
            slider.value = 1; // 초기 체력 설정
        }
    }

    /// <summary>
    /// 값 증가 함수, max 이상 증가 X
    /// </summary>
    /// <param name="value">증가 값</param>
    public void Add(float value)
    {
        curVal = Mathf.Min(curVal + value, maxVal);
        UpdateUI();
    }

    /// <summary>
    /// 값 감소 함수, 0 이하 감소 X
    /// </summary>
    /// <param name="value">감소 값</param>
    public void Subtract(float value)
    {
        curVal = Mathf.Max(curVal - value, 0);
        UpdateUI();
    }

    /// <summary>
    /// 현재 값이 최대 값 대비 얼마나 남아있는지 확인
    /// </summary>
    /// <returns></returns>
    public float GetPercentage()
    {
        return curVal / maxVal;
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (slider != null)
        {
            slider.value = GetPercentage();
        }
    }
}
