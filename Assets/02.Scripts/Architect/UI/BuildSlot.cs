using TMPro;
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

    private bool isInitialized = false;

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

    private void OnEnable()
    {
        // 활성화 할 때 마다 재료의 갯수들을 업데이트
        if(isInitialized)
            UpdateQuantityText();
    }

    public void SetData(BuildingData data) => buildingData = data;

    // 슬롯 세팅
    private void SetSlot()
    {
        if (buildingData == null)
        {
            Debug.LogError($"{gameObject.name} 슬롯에서 BuildData를 찾지 못했습니다.");
            return;
        }

        GameObject go = Instantiate(buildingData.objPrefab, objParent);

        buildUI.slotObejctList.Add(go);

        if (go.TryGetComponent(out BasePreview preview))
        {
            preview.enabled = false;
        }

        go.transform.localPosition = buildingData.uiPosOffset;
        go.transform.localEulerAngles += buildingData.uiRotOffset;
        go.transform.localScale = buildingData.uiScaleOffset;

        nameText.text = buildingData.displayName;
        descriptionText.text = buildingData.description;

        UpdateQuantityText();

        isInitialized = true;
    }

    private void UpdateQuantityText()
    {
        ingredientsText.text = string.Empty;

        if (buildingData.ingredients != null)
        {
            for (int i = 0; i < buildingData.ingredients.Count; i++)
            {
                var ingredient = buildingData.ingredients[i];
                int currentQuantity = GetItemQuantity(ingredient.item);

                // 현재 재료를 가지고 있지 않다면 빨간색 폰트 가지고 있다면 초록색 폰트로 변경
                string color = currentQuantity >= ingredient.quantity ? "green" : "red";

                ingredientsText.text +=
                    $"{ingredient.item.displayName} " +
                    $"<color={color}>{currentQuantity}</color>/{ingredient.quantity}";

                if (i != buildingData.ingredients.Count - 1)
                {
                    ingredientsText.text += "\n";
                }
            }
        }
    }

    /// <summary>
    /// 아이템 보유 갯수 찾기
    /// </summary>
    /// <param name="item">아이템 데이터</param>
    /// <returns>보유갯수 반환</returns>
    private int GetItemQuantity(ItemData item)
    {
        ItemSlot slot = creator.inventory.slots.Find(x => x.itemData != null && x.itemData.displayName == item.displayName);

        return slot != null ? slot.quantity : 0;
    }

    private void OnClickBuildButton()
    {
        foreach (var ingredient in buildingData.ingredients)
        {
            if (!creator.inventory.HasItem(ingredient.item, ingredient.quantity))
                return;
        }

        creator.CreatePreviewObject(buildingData);
        buildUI.ChangeUIActive();
    }
}
