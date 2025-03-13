using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PivotDirection
{
    Up,
    Down,
    Left,
    Right,
}

public class WallPreview : BasePreview
{
    protected List<Transform> previewObjPivots;

    public override void Initialize(LayerMask buildableLayer)
    {
        base.Initialize(buildableLayer);

        // �ǹ� �ʱ�ȭ
        previewObjPivots = new List<Transform>(gameObject.GetComponentsInChildren<Transform>());
        previewObjPivots.Remove(transform); // �ڽ��� ����

        StartCoroutine(CanBuildRayCheck());
    }

    /// <summary>
    /// ���� ���� ���� üũ �ڷ�ƾ
    /// </summary>
    /// <returns></returns>
    private IEnumerator CanBuildRayCheck()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer�� �����ϴ�");
            yield break;
        }

        // Ray�� �ڽ��� �����Ǵ°� �����ϱ� ���� ������ ������Ʈ�� ���̾ ����Ʈ�� ����
        gameObject.layer = 0;

        RaycastHit hitInfo;

        while (gameObject.activeSelf)
        {
            if (CheckForObstacles())
            {
                Vector3[] directions = new Vector3[]
                {
                    transform.TransformDirection(Vector3.up),
                    transform.TransformDirection(Vector3.down),
                    transform.TransformDirection(Vector3.left),
                    transform.TransformDirection(Vector3.right)
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
                            Transform nearPivot = FindNearPivot(transform.position, hitInfo.collider.gameObject);

                            if (nearPivot != null)
                            {
                                if (directions[i] == Vector3.up)
                                {
                                    transform.position = hitInfo.collider.transform.position + (Vector3.down * 2f);
                                }

                                else if (directions[i] == Vector3.down)
                                {
                                    transform.position = nearPivot.position;
                                }

                                else if (directions[i] == -previewObjPivots[(int)PivotDirection.Left].right)
                                {
                                    transform.position = hitInfo.collider.transform.position + (hitInfo.collider.transform.right * 2f);
                                }

                                else if (directions[i] == previewObjPivots[(int)PivotDirection.Right].right)
                                {
                                    transform.position = hitInfo.collider.transform.position + (-hitInfo.collider.transform.right * 2f);
                                }

                                if (CheckForObstacles())
                                {
                                    // ȸ������ ���� �������
                                    transform.rotation = nearPivot.rotation;

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
                            transform.position = new Vector3(
                                transform.position.x,
                                hitInfo.point.y,
                                transform.position.z);

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
}
