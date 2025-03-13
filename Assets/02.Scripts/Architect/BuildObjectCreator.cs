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

    private Quaternion additionalRotation; // 플레이어 입력 회전값 저장

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
            // 프리뷰오브젝트의 위치를 카메라 위치 기준으로 조정
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
    /// 프리뷰 오브젝트 생성 함수
    /// </summary>
    /// <param name="obj">생성할 오브젝트 프리팹1</param>
    public void CreatePreviewObject(GameObject obj)
    {
        if (previewObj != null)
            Destroy(previewObj);

        previewObj = Instantiate(obj);

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
                return false;
        }

        return true;
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

        // 모든 자식 중 "Pivot" 태그가 있는 Transform 탐색
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
