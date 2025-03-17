﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildSlot : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;

    [SerializeField] private Transform objParent;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI ingredientsText;

    [SerializeField] private Button buildButton;

    private BuildObjectCreator creator;
    private BuildUI buildUI;

    private void Start()
    {
        nameText.text = string.Empty;
        descriptionText.text = string.Empty;
        ingredientsText.text = string.Empty;

        buildButton.onClick.AddListener(OnClickBuildButton);

        creator = FindObjectOfType<BuildObjectCreator>();
        buildUI = FindObjectOfType<BuildUI>();

        SetSlot();
    }

    public void SetData(BuildingData data) => buildingData = data;

    private void SetSlot()
    {
        if (buildingData == null)
        {
            Debug.LogError($"{name}번 슬롯에서 BuildData를 찾지 못했습니다.");
            return;
        }

        GameObject go = Instantiate(buildingData.objPrefab, objParent);

        if (go.TryGetComponent(out BasePreview preview))
        {
            preview.enabled = false;
        }

        go.transform.localPosition = buildingData.uiPosOffset;
        go.transform.localEulerAngles += buildingData.uiRotOffset;
        go.transform.localScale = buildingData.uiScaleOffset;

        nameText.text = buildingData.displayName;
        descriptionText.text = buildingData.description;

        ingredientsText.text = string.Empty;

        if (buildingData.ingredients != null)
        {
            for (int i = 0; i < buildingData.ingredients.Count; i++)
            {
                var ingredient = buildingData.ingredients[i];
                int currentQuantity = GetItemQuantity(ingredient.itemData);

                string color = currentQuantity >= ingredient.quantity ? "green" : "red";

                ingredientsText.text +=
                    $"{ingredient.itemData.displayName} " +
                    $"<color={color}>{currentQuantity}</color>/{ingredient.quantity}";

                if (i != buildingData.ingredients.Count - 1)
                {
                    ingredientsText.text += "\n";
                }
            }
        }
    }

    private int GetItemQuantity(ItemData item)
    {
        ItemSlot slot = creator.inventory.slots.Find(x => x.itemData != null && x.itemData.displayName == item.displayName);

        return slot != null ? slot.quantity : 0;
    }

    private void OnClickBuildButton()
    {
        //foreach (var ingredient in buildingData.ingredients)
        //{
        //    if (!creator.inventory.HasItem(ingredient.itemData, ingredient.quantity))
        //        return;
        //}  UI 테스트를 위해 소지 아이템 검사 비활성화

        creator.CreatePreviewObject(buildingData);
        buildUI.ChangeUIActive();
    }
}
