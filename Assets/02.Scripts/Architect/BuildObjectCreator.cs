using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject buildPrefab;
    [SerializeField] private LayerMask buildableLayer;

    private bool canBuild;
    private GameObject previewObj;
    private Transform cameraContainer;

    private Quaternion additionalRotation; // �÷��̾� �Է� ȸ���� ����

    private void Start()
    {
        cameraContainer = PlayerManager.Instance.Player.controller.cameraContainer;
    }

    private void Update()
    {
        // Input �Լ��� ���߿� InputAction���� ���� ����

        if (Input.GetKeyDown(KeyCode.F) && previewObj == null)
        {
            CreatePreviewObject(buildPrefab);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPreview();
        }


        if (canBuild && Input.GetKeyDown(KeyCode.V))
        {
            PlaceObject();
        }

        if (previewObj != null)
        {
            // �����������Ʈ�� ��ġ�� ī�޶� ��ġ �������� ����
            previewObj.transform.position = cameraContainer.position + (cameraContainer.forward * 3f) + (cameraContainer.up * 1.5f);

            float x = previewObj.transform.eulerAngles.x;

            Quaternion cameraRotation = Quaternion.Euler(x, cameraContainer.eulerAngles.y, 0);

            if (Input.GetKeyDown(KeyCode.Q))
            {
                additionalRotation *= Quaternion.Euler(0, -90f, 0);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                additionalRotation *= Quaternion.Euler(0, 90f, 0);
            }

            previewObj.transform.rotation = cameraRotation * additionalRotation;
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

        StartCoroutine(CanBuildRayCheck());
    }

    /// <summary>
    /// ���� ������ ��� �Լ�
    /// </summary>
    public void CancelPreview()
    {
        Destroy(previewObj);
    }

    /// <summary>
    /// ���� ���� ���� üũ �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator CanBuildRayCheck()
    {
        MeshRenderer renderer = previewObj.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer�� �����ϴ�");
            yield break;
        }

        previewObj.layer = 0;

        while (previewObj != null)
        {
            Vector3 bottomPoint = renderer.bounds.center;
            bottomPoint.y -= renderer.bounds.extents.y - 0.2f;

            if (CheckForObstacles(renderer))
            {
                Debug.DrawRay(bottomPoint, Vector3.down, Color.red, 1f);
                if (Physics.Raycast(bottomPoint, Vector3.down, out RaycastHit hitInfo, 1f, buildableLayer))
                {
                    if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                    {
                        Transform nearPivot = FindNearPivot(previewObj.transform.position, hitInfo.collider.gameObject);

                        if (nearPivot != null)
                        {
                            previewObj.transform.position = nearPivot.position;
                            previewObj.transform.rotation = nearPivot.rotation;

                            canBuild = true;
                            renderer.material.color = Color.green;

                            yield return null;
                            continue;
                        }
                    }
                    else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        previewObj.transform.position = new Vector3(
                            previewObj.transform.position.x,
                            hitInfo.point.y,
                            previewObj.transform.position.z);

                        canBuild = true;
                        renderer.material.color = Color.green;
                    }
                }
                else
                {
                    renderer.material.color = Color.red;
                    canBuild = false;
                }
            }
            else
            {
                renderer.material.color = Color.red;
                canBuild = false;
            }
            yield return null;
        }
    }

    /// <summary>
    /// ������ ������Ʈ �ֺ� ������Ʈ ���� ���� Ȯ��
    /// </summary>
    /// <param name="renderer">������ ������Ʈ �޽�������</param>
    /// <returns>�ֺ��� ������Ʈ�� �ִٸ� false, ���ٸ� true</returns>
    private bool CheckForObstacles(MeshRenderer renderer)
    {
        Vector3 boxCenter = renderer.bounds.center;

        Vector3 boxSize = new Vector3(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2.1f, Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == previewObj) continue;

            if (collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                return false;
        }

        return true;
    }

    /// <summary>
    /// �ֺ��� �ٸ� ���๰�� �ǹ��� �����ϴ��� üũ
    /// </summary>
    /// <param name="previewPosition">������ ������Ʈ ��ġ��</param>
    /// <param name="targetObject">Ray�� ���� ������ ���๰ ������Ʈ</param>
    /// <returns>������ ������Ʈ�� ���� ����� ��ġ�� �ִ� �ǹ��� Transform</returns>
    private Transform FindNearPivot(Vector3 previewPosition, GameObject targetObject)
    {
        Transform nearPivot = null;
        float nearDistance = Mathf.Infinity;

        // ��� �ڽ� �� "Pivot" �±װ� �ִ� Transform Ž��
        foreach (Transform child in targetObject.transform)
        {
            if (child.CompareTag("Pivot"))
            {
                float distance = Vector3.Distance(previewPosition, child.position);
                if (distance < nearDistance)
                {
                    nearDistance = distance;
                    nearPivot = child;
                }
            }
        }

        return nearPivot;
    }

    /// <summary>
    /// ������ ���๰ ��ġ(����)
    /// </summary>
    private void PlaceObject()
    {
        if (previewObj == null) return;

        GameObject go = Instantiate(previewObj);
        go.transform.position = previewObj.transform.position;
        go.GetComponent<MeshRenderer>().material.color = Color.white;
        go.layer = LayerMask.NameToLayer("BuildObject");
    }
}
