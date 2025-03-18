using UnityEngine;
using UnityEngine.InputSystem;

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;
    [SerializeField] private LayerMask buildableLayer;

    private LayerMask previewOriginLayer;
    private GameObject previewObj;

    private BuildUI buildUi;

    public UIInventory inventory;


    private void Start()
    {
        inventory = FindObjectOfType<UIInventory>(true);
        buildUi = FindObjectOfType<BuildUI>(true);
    }

    /// <summary>
    /// 프리뷰 오브젝트 생성 함수
    /// </summary>
    /// <param name="obj">생성할 오브젝트 프리팹1</param>
    public void CreatePreviewObject(BuildingData data)
    {
        if (previewObj != null)
            Destroy(previewObj);

        buildingData = data;

        previewObj = Instantiate(data.objPrefab);

        previewOriginLayer = previewObj.layer;

        previewObj.GetComponent<BasePreview>().Initialize(buildableLayer);
    }

    /// <summary>
    /// 건축 프리뷰 취소 함수
    /// </summary>
    public void CancelPreview()
    {
        if (previewObj != null)
        {
            Destroy(previewObj);
            previewObj = null;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    /// <summary>
    /// 프리뷰 건축물 배치(생성)
    /// </summary>
    private void PlaceObject()
    {
        if (previewObj == null) return;

        // 프리뷰 오브젝트와 같은 오브젝트를 배치
        GameObject go = Instantiate(previewObj);

        // 프리뷰 오브젝트에 존재하는 프리뷰 클래스 제거
        Destroy(go.GetComponent<BasePreview>());
        go.transform.position = previewObj.transform.position;

        // 머티리얼 색상 및 콜라이더, 레이어를 원래대로 되돌림
        MeshRenderer renderer = go.GetComponent<MeshRenderer>();

        if (renderer != null)
            renderer.material.color = Color.white;

        MeshRenderer[] renderers = go.GetComponentsInChildren<MeshRenderer>();

        if (renderers != null)
        {
            foreach (var mesh in renderers)
            {
                mesh.material.color = Color.white;
            }
        }

        Collider[] colliders = go.GetComponentsInChildren<Collider>();

        if (colliders != null)
        {
            foreach (var collider in colliders)
            {
                collider.enabled = true;
            }
        }

        if (go.name.Contains("House") && go.TryGetComponent(out Collider col))
        {
            Destroy(col);
        }

        // 배치 한 건축물의 재료를 아이템 창에서 감소 시킴
        //foreach (var ingredient in buildingData.ingredients)
        //{
        //    inventory.RemoveItem(ingredient.itemData, ingredient.quantity);
        //}

        go.layer = previewOriginLayer;
    }

    public void OnBuildUIInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            buildUi.ChangeUIActive();
            CancelPreview();
        }
    }

    public void OnBuildInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && previewObj != null && previewObj.TryGetComponent(out BasePreview preview))
        {
            if (preview.CanBuild())
                PlaceObject();
        }
    }

    public void OnBuildCancelInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            if (previewObj != null)
            {
                CancelPreview();
            }
        }
    }

    public bool IsPreviewObject() => previewObj == null;
}
