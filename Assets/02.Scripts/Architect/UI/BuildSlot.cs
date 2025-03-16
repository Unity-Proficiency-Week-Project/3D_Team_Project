using TMPro;
using Unity.VisualScripting;
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

    private void Start()
    {
        nameText.text = string.Empty;
        descriptionText.text = string.Empty;
        ingredientsText.text = string.Empty;

        buildButton.onClick.AddListener(OnClickBuildButton);

        creator = FindObjectOfType<BuildObjectCreator>();

        SetSlot();
    }

    public void SetData(BuildingData data) => buildingData = data;

    private void SetSlot()
    {
        if (buildingData == null)
        {
            Debug.LogError($"BuildData를 찾지 못했습니다. From : {name}");
            return;
        }

        GameObject go = Instantiate(buildingData.objPrefab, objParent);

        if (go.TryGetComponent(out BasePreview preview))
        {
            preview.enabled = false;
        }

        go.transform.localPosition = buildingData.uiPosOffset;
        go.transform.localScale = buildingData.uiScaleOffset;

        nameText.text = buildingData.displayName;
        descriptionText.text = buildingData.description;

        ingredientsText.text = string.Empty;

        if (buildingData.ingredients != null)
        {
            for (int i = 0; i < buildingData.ingredients.Count; i++)
            {
                ingredientsText.text += $"{buildingData.ingredients[i].itemData.displayName} : {buildingData.ingredients[i].quantity}";

                if (i != buildingData.ingredients.Count - 1)
                {
                    ingredientsText.text += "\n";
                }
            }
        }
    }

    private void OnClickBuildButton()
    {
        bool canBuild = true;

        foreach (var ingredient in buildingData.ingredients)
        {
            if (creator.inventory.HasItem(ingredient.itemData, ingredient.quantity))
            {
                canBuild = false;
                break;
            }
        }

        if (canBuild)
        {
            creator.CreatePreviewObject(buildingData);
            // ui 닫기
        }
    }
}
