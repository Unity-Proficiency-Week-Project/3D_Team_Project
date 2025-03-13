using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossHairUI : MonoBehaviour
{
    public Image verticalLine;
    public Image horizontalLine;

    public Color defaultColor = Color.white;
    public float lineThickness = 4f;
    public float lineLength = 32f;

    private void Start()
    {
        UpdateCrosshair();
    }

    /// <summary>
    /// 조준선 색 바꾸기
    /// </summary>
    /// <param name="newColor">바꿀 색</param>
    public void SetCrosshairColor(Color newColor)
    {
        verticalLine.color = newColor;
        horizontalLine.color = newColor;
    }

    public void SetCrosshairSize(float newSize)
    {
        lineLength = newSize;
        UpdateCrosshair();
    }


    private void UpdateCrosshair()
    {
        if (verticalLine != null)
        {
            verticalLine.rectTransform.sizeDelta = new Vector2(lineThickness, lineLength);
        }
        if (horizontalLine != null)
        {
            horizontalLine.rectTransform.sizeDelta = new Vector2(lineLength, lineThickness);
        }
    }


    /// <summary>
    /// 조준선 숨기기
    /// </summary>
    public void HideCrosshair()
    {
        verticalLine.gameObject.SetActive(false);
        horizontalLine.gameObject.SetActive(false);
    }

    /// <summary>
    /// 조준선 보이기
    /// </summary>
    public void ShowCrosshair()
    {
        verticalLine.gameObject.SetActive(true);
        horizontalLine.gameObject.SetActive(true);
    }

}
