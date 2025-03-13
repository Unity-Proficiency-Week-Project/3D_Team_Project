using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum PivotDirection
{
    Up,
    Down,
    Left,
    Right,
}

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject buildPrefab;
    [SerializeField] private LayerMask buildableLayer;
    [SerializeField] private List<Transform> previewObjPivots;

    private bool canBuild;
    private GameObject previewObj;
    private Transform cameraContainer;

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
            // ������ ������Ʈ�� ��ġ�� ī�޶� �������� ����
            previewObj.transform.position = cameraContainer.position + (cameraContainer.forward * 3f) + (cameraContainer.up * 1.5f);

            // ������ ������Ʈ�� �ʱ� X�� ȸ���� ����
            float fixedXRotation = previewObj.transform.eulerAngles.x;

            // ī�޶��� Y�� ȸ���� ���
            Quaternion cameraRotation = Quaternion.Euler(0, cameraContainer.eulerAngles.y, 0);

            //// Ű �Է¿� ���� Y�� ȸ���� ���
            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    additionalRotation *= Quaternion.Euler(0, -90f, 0);
            //}
            //else if (Input.GetKeyDown(KeyCode.E))
            //{
            //    additionalRotation *= Quaternion.Euler(0, 90f, 0);
            //}

            // ���������� X�� ���� �� Y�� ȸ�� ����
            previewObj.transform.rotation = Quaternion.Euler(fixedXRotation, cameraRotation.eulerAngles.y, 0);
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

        // ������ ������Ʈ�� �ǹ����� ������ �� �ڽ��� Ʈ�������� ����
        previewObjPivots = previewObj.GetComponentsInChildren<Transform>().ToList();

        previewObjPivots.Remove(previewObjPivots.Find(x => x.name == previewObj.name));

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

        // Ray�� �ڽ��� �����Ǵ°� �����ϱ� ���� ������ ������Ʈ�� ���̾ ����Ʈ�� ����
        previewObj.layer = 0;

        RaycastHit hitInfo;

        while (previewObj != null)
        {
            if (CheckForObstacles(renderer))
            {
                Vector3[] directions = new Vector3[]
                {
                    previewObj.transform.TransformDirection(Vector3.up),
                    previewObj.transform.TransformDirection(Vector3.down),
                    previewObj.transform.TransformDirection(Vector3.left),
                    previewObj.transform.TransformDirection(Vector3.right)
                };

                // �ǹ����� Ray �˻�
                for (int i = 0; i < previewObjPivots.Count; i++)
                {
                    // ���� �ǹ����� ���ص� �������� Ray�� �߻�, buildableLayer�� ���ԵǴ� ������Ʈ�� �浹�ϸ� hitInfo�� ������Ʈ ���� �� true ��ȯ
                    if (Physics.Raycast(previewObjPivots[i].position, directions[i], out hitInfo, 1f, buildableLayer))
                    {
                        // ������Ʈ�� ���̾ BuildObject���
                        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                        {
                            // ������ ������Ʈ ���� ���� ��ó�� �ִ� �ǹ��� ã��
                            Transform nearPivot = FindNearPivot(previewObj.transform.position, hitInfo.collider.gameObject);

                            if (nearPivot != null)
                            {
                                if (directions[i] == Vector3.up)
                                {
                                    previewObj.transform.position = hitInfo.collider.transform.position + (Vector3.down * 2f);
                                }

                                else if (directions[i] == Vector3.down)
                                {
                                    previewObj.transform.position = nearPivot.position;
                                }

                                else if (directions[i] == -previewObjPivots[(int)PivotDirection.Left].right)
                                {
                                    previewObj.transform.position = hitInfo.collider.transform.position + (hitInfo.collider.transform.right * 2f);
                                }

                                else if (directions[i] == previewObjPivots[(int)PivotDirection.Right].right)
                                {
                                    previewObj.transform.position = hitInfo.collider.transform.position + (-hitInfo.collider.transform.right * 2f);
                                }

                                if (CheckForObstacles(renderer))
                                {
                                    // ȸ������ ���� �������
                                    previewObj.transform.rotation = nearPivot.rotation;

                                    // ������ ������Ʈ�� ���� �ʷϻ����� �����Ͽ� �Ǽ� ���� �������� �˷���
                                    canBuild = true;
                                    renderer.material.color = Color.green;

                                    break;
                                }
                                else
                                {
                                    renderer.material.color = Color.red;
                                    canBuild = false;
                                }
                            }
                        }
                        // ������Ʈ�� ���̾ Ground��� y���� �浹�� ������Ʈ�� ����
                        else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                        {
                            previewObj.transform.position = new Vector3(
                                previewObj.transform.position.x,
                                hitInfo.point.y,
                                previewObj.transform.position.z);

                            canBuild = true;
                            renderer.material.color = Color.green;

                            break;
                        }
                    }
                    // Ray�� ������Ʈ�� �浹���� �ʾ��� ���
                    else
                    {
                        renderer.material.color = Color.red;
                        canBuild = false;
                    }
                }
            }

            // ������ ������Ʈ ��ġ�� ��ֹ��� ���� ���
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
                return false; // �ٸ� ���๰�� ��ħ
        }

        return true; // ��ֹ� ����
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
