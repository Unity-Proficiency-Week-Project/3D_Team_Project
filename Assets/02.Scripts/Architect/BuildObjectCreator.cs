using UnityEngine;

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject buildPrefab;
    [SerializeField] private LayerMask buildableLayer;

    private GameObject previewObj;

    private void Update()
    {
        // Input �Լ��� ���߿� InputAction���� ���� ����

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
    /// ������ ������Ʈ ���� �Լ�
    /// </summary>
    /// <param name="obj">������ ������Ʈ ������1</param>
    public void CreatePreviewObject(GameObject obj)
    {
        if (previewObj != null)
            Destroy(previewObj);

        previewObj = Instantiate(obj);

        previewObj.GetComponent<BasePreview>().Initialize(buildableLayer);
    }

    /// <summary>
    /// ���� ������ ��� �Լ�
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
    /// ������ ���๰ ��ġ(����)
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
