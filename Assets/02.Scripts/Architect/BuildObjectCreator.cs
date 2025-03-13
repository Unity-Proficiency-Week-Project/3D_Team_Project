using UnityEngine;

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject buildPrefab;
    [SerializeField] private LayerMask buildableLayer;

    private GameObject previewObj;

    private void Update()
    {
        // Input 함수들 나중에 InputAction으로 수정 예정

        if (Input.GetKeyDown(KeyCode.F) && previewObj == null)
        {
            CreatePreviewObject(buildPrefab);
        }

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
    }

    /// <summary>
    /// 프리뷰 오브젝트 생성 함수
    /// </summary>
    /// <param name="obj">생성할 오브젝트 프리팹1</param>
    public void CreatePreviewObject(GameObject obj)
    {
        if (previewObj != null)
            Destroy(previewObj);

        previewObj = Instantiate(obj);

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

        GameObject go = Instantiate(previewObj);
        Destroy(go.GetComponent<BasePreview>());
        go.transform.position = previewObj.transform.position;

        go.GetComponent<MeshRenderer>().material.color = Color.white;
        go.layer = LayerMask.NameToLayer("BuildObject");
    }
}
