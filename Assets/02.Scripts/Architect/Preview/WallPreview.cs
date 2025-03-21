﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallPreview : BasePreview
{
    protected List<Transform> previewObjPivots;

    protected bool isSnap;

    public override void Initialize(LayerMask buildableLayer)
    {
        // 피벗 초기화
        previewObjPivots = new List<Transform>(gameObject.GetComponentsInChildren<Transform>());
        previewObjPivots.Remove(transform); // 자신은 제외

        base.Initialize(buildableLayer);
    }

    public override void UpdatePreview()
    {
        if (!isSnap || Vector3.Distance(transform.position, PlayerManager.Instance.Player.controller.cameraContainer.position) > 3.5f)
        {
            isSnap = false;
            base.UpdatePreview();
        }
    }

    /// <summary>
    /// 건축 가능 여부 체크 코루틴
    /// </summary>
    /// <returns></returns>
    public override IEnumerator CanBuildCheckCoroutine()
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
                for (int i = 0; i < directions.Length; i++)
                {
                    Debug.DrawRay(previewObjPivots[i].position, directions[i], Color.blue, 1f);
                    // 현재 피벗에서 정해둔 방향으로 Ray를 발사, buildableLayer에 포함되는 오브젝트가 충돌하면 hitInfo에 오브젝트 넣은 후 true 반환
                    if (Physics.Raycast(previewObjPivots[i].position, directions[i], out hitInfo, 1f, buildableLayer))
                    {
                        // 오브젝트의 레이어가 BuildObject라면
                        if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                        {
                            // 프리뷰 오브젝트 기준 가장 근처에 있는 피벗을 찾음
                            Transform nearPivot = FindNearPivot(transform.position, hitInfo.collider.gameObject);

                            // 가까운 피벗의 방향에 따라 프리뷰 오브젝트 위치 변경
                            if (nearPivot != null)
                            {
                                if (nearPivot.name.Contains("Down"))
                                {
                                    transform.position = hitInfo.collider.transform.position + (-hitInfo.collider.transform.up * 2.001f);
                                }

                                else if (nearPivot.name.Contains("Up"))
                                {
                                    transform.position = hitInfo.collider.transform.position + (hitInfo.collider.transform.up * 2.001f);
                                }

                                else if (nearPivot.name.Contains("Right"))
                                {
                                    transform.position = hitInfo.collider.transform.position + (hitInfo.collider.transform.right * 2f);
                                }

                                else if (nearPivot.name.Contains("Left"))
                                {
                                    transform.position = hitInfo.collider.transform.position + (-hitInfo.collider.transform.right * 2f);
                                }

                                else
                                {
                                    Debug.LogError("피벗의 이름이 잘못 설정되었습니다.");
                                }

                                if (CheckForObstacles())
                                {
                                    // 회전값을 같게 만들어줌
                                    transform.rotation = nearPivot.rotation * originRotation;

                                    // 프리뷰 오브젝트의 색을 초록색으로 변경하여 건설 가능 지점임을 알려줌
                                    canBuild = true;

                                    isSnap = true;

                                    break;
                                }
                                else
                                {
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

                            break;
                        }
                    }
                    // Ray가 오브젝트와 충돌하지 않았을 경우
                    else
                    {
                        if (!isSnap)
                            canBuild = false;
                    }
                }
            }

            // 프리뷰 오브젝트 위치에 장애물이 있을 경우
            else
            {
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

    // 디버그용 Gizmo 생성
    void OnDrawGizmos()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(collider.bounds.center, transform.rotation, Vector3.one); 
            Gizmos.DrawWireCube(Vector3.zero, collider.bounds.size);
        }
    }

}