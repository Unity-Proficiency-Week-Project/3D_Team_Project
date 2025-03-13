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

        // 피벗 초기화
        previewObjPivots = new List<Transform>(gameObject.GetComponentsInChildren<Transform>());
        previewObjPivots.Remove(transform); // 자신은 제외

        StartCoroutine(CanBuildRayCheck());
    }

    /// <summary>
    /// 건축 가능 여부 체크 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator CanBuildRayCheck()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer가 없습니다");
            yield break;
        }

        // Ray에 자신이 감지되는걸 방지하기 위해 프리뷰 오브젝트의 레이어를 디폴트로 변경
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

                // 피벗별로 Ray 검사
                for (int i = 0; i < previewObjPivots.Count; i++)
                {
                    // 현재 피벗에서 정해둔 방향으로 Ray를 발사, buildableLayer에 포함되는 오브젝트가 충돌하면 hitInfo에 오브젝트 넣은 후 true 반환
                    if (Physics.Raycast(previewObjPivots[i].position, directions[i], out hitInfo, 1f, buildableLayer))
                    {
                        // 오브젝트의 레이어가 BuildObject라면
                        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                        {
                            // 프리뷰 오브젝트 기준 가장 근처에 있는 피벗을 찾음
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
                                    // 회전값을 같게 만들어줌
                                    transform.rotation = nearPivot.rotation;

                                    // 프리뷰 오브젝트의 색을 초록색으로 변경하여 건설 가능 지점임을 알려줌
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
                        // 오브젝트의 레이어가 Ground라면 y값을 충돌한 오브젝트와 맞춤
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
                    // Ray가 오브젝트와 충돌하지 않았을 경우
                    else
                    {
                        renderer.material.color = Color.red;
                        canBuild = false;
                    }
                }
            }

            // 프리뷰 오브젝트 위치에 장애물이 있을 경우
            else
            {
                renderer.material.color = Color.red;
                canBuild = false;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 주변에 다른 건축물의 피벗이 존재하는지 체크
    /// </summary>
    /// <param name="previewPosition">프리뷰 오브젝트 위치값</param>
    /// <param name="targetObject">Ray를 통해 감지된 건축물 오브젝트</param>
    /// <returns>프리뷰 오브젝트와 가장 가까운 위치에 있는 피벗의 Transform</returns>
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
