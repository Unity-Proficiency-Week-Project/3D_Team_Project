using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Condition : MonoBehaviour
{
    public float curVal;
    public float maxVal;
    public float startVal;
    public Image bar;

    /// <summary>
    /// ���� �� �ʱ�ȭ
    /// </summary>
    void Start()
    {
        curVal = startVal;   
    }

    /// <summary>
    /// �� ���� �Լ�, max �̻� ���� X
    /// </summary>
    /// <param name="value">���� ��</param>
    public void Add(float value)
    {
        curVal = Mathf.Min(curVal + value, maxVal);
    }

    /// <summary>
    /// �� ���� �Լ�, 0 ���� ���� X
    /// </summary>
    /// <param name="value">���� ��</param>
    public void Subtract(float value)
    {
        curVal = Mathf.Max(curVal - value, 0);
    }

    /// <summary>
    /// ���� ���� �ִ� �� ��� �󸶳� �����ִ��� Ȯ��
    /// </summary>
    /// <returns></returns>
    public float GetPercentage()
    {
        return curVal / maxVal;
    }
}
