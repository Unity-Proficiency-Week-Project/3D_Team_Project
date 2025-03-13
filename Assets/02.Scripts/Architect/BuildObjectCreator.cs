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
        // Input 함수들 나중에 InputAction으로 수정 예정

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
            // 프리뷰 오브젝트의 위치를 카메라 기준으로 조정
            previewObj.transform.position = cameraContainer.position + (cameraContainer.forward * 3f) + (cameraContainer.up * 1.5f);

            // 프리뷰 오브젝트의 초기 X축 회전값 고정
            float fixedXRotation = previewObj.transform.eulerAngles.x;

            // 카메라의 Y축 회전값 계산
            Quaternion cameraRotation = Quaternion.Euler(0, cameraContainer.eulerAngles.y, 0);

            //// 키 입력에 따른 Y축 회전값 계산
            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    additionalRotation *= Quaternion.Euler(0, -90f, 0);
            //}
            //else if (Input.GetKeyDown(KeyCode.E))
            //{
            //    additionalRotation *= Quaternion.Euler(0, 90f, 0);
            //}

            // 최종적으로 X축 고정 및 Y축 회전 적용
            previewObj.transform.rotation = Quaternion.Euler(fixedXRotation, cameraRotation.eulerAngles.y, 0);
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

        // 프리뷰 오브젝트의 피벗들을 가져온 후 자신의 트랜스폼은 삭제
        previewObjPivots = previewObj.GetComponentsInChildren<Transform>().ToList();

        previewObjPivots.Remove(previewObjPivots.Find(x => x.name == previewObj.name));

        StartCoroutine(CanBuildRayCheck());
    }

    /// <summary>
    /// 건축 프리뷰 취소 함수
    /// </summary>
    public void CancelPreview()
    {
        Destroy(previewObj);
    }

    /// <summary>
    /// 건축 가능 여부 체크 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator CanBuildRayCheck()
    {
        MeshRenderer renderer = previewObj.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer가 없습니다");
            yield break;
        }

        // Ray에 자신이 감지되는걸 방지하기 위해 프리뷰 오브젝트의 레이어를 디폴트로 변경
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
                                    // 회전값을 같게 만들어줌
                                    previewObj.transform.rotation = nearPivot.rotation;

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
                            previewObj.transform.position = new Vector3(
                                previewObj.transform.position.x,
                                hitInfo.point.y,
                                previewObj.transform.position.z);

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
    /// 프리뷰 오브젝트 주변 오브젝트 존재 여부 확인
    /// </summary>
    /// <param name="renderer">프리뷰 오브젝트 메쉬렌더러</param>
    /// <returns>주변에 오브젝트가 있다면 false, 없다면 true</returns>
    private bool CheckForObstacles(MeshRenderer renderer)
    {
        Vector3 boxCenter = renderer.bounds.center;
        Vector3 boxSize = new Vector3(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2.1f, Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == previewObj) continue;

            if (collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                return false; // 다른 건축물과 겹침
        }

        return true; // 장애물 없음
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

    /// <summary>
    /// 프리뷰 건축물 배치(생성)
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
