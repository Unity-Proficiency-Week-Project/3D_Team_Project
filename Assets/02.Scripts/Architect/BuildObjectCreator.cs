using UnityEngine;

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private BuildingData buildingData;
    [SerializeField] private LayerMask buildableLayer;

    private LayerMask previewOriginLayer;
    private GameObject previewObj;

    public UIInventory inventory;

    private void Start()
    {
        inventory = FindObjectOfType<UIInventory>(true);
    }

    private void Update()
    {
        // Input 함수들 나중에 InputAction으로 수정 예정

        if (previewObj != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape) && previewObj != null)
            {
                CancelPreview();
            }


            if (Input.GetKeyDown(KeyCode.V) && previewObj.GetComponent<BasePreview>().CanBuild())
            {
                PlaceObject();
            }
        }


        if (Input.GetKeyDown(KeyCode.T))
        {
            FindObjectOfType<BuildUI>(true).ChangeUIActive();
        }
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
        }
    }

    /// <summary>
    /// 프리뷰 건축물 배치(생성)
    /// </summary>
    private void PlaceObject()
    {
        if (previewObj == null) return;

        Debug.Log("배치");

        GameObject go = Instantiate(previewObj);
        Destroy(go.GetComponent<BasePreview>());
        go.transform.position = previewObj.transform.position;

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

        if(go.name.Contains("House") && go.TryGetComponent(out Collider col))
        {
            Destroy(col);
        }

        go.layer = previewOriginLayer;
    }
}
