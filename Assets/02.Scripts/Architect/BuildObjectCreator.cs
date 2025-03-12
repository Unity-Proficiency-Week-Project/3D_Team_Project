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


    private Quaternion additionalRotation = Quaternion.identity;

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
            GameObject go = Instantiate(previewObj);
            go.transform.position = previewObj.transform.position;
            go.GetComponent<MeshRenderer>().material.color = Color.white;
            go.layer = LayerMask.NameToLayer("BuildObject");
        }

        if (previewObj != null)
        {
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

    public void CreatePreviewObject(GameObject obj)
    {
        if (previewObj != null)
            Destroy(previewObj);

        previewObj = Instantiate(obj);

        StartCoroutine(CanBuildRayCheck());
    }

    public void CancelPreview()
    {
        Destroy(previewObj);
    }

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
                    canBuild = true;
                    renderer.material.color = Color.green;

                    if(hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                    {
                        Transform closestPivot = FindClosestPivot(previewObj.transform.position, hitInfo.collider.gameObject);

                        if (closestPivot != null)
                        {
                            previewObj.transform.position = closestPivot.position;
                            yield return null;
                            continue;
                        }
                    }
                    else if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        float objectHeightOffset = renderer.bounds.extents.y;

                        previewObj.transform.position = new Vector3(
                            previewObj.transform.position.x,
                            hitInfo.point.y + objectHeightOffset,
                            previewObj.transform.position.z
                        );
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

    private bool CheckForObstacles(MeshRenderer renderer)
    {
        Vector3 boxCenter = renderer.bounds.center;

        Vector3 boxSize = new Vector3(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2, Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == previewObj) continue;

            // collider의 레이어가 buildableLayer에 포함되지 않는다면 false 반환
            if ((buildableLayer & (1 << collider.gameObject.layer)) == 0)
                return false;
        }

        return true;
    }

    private Transform FindClosestPivot(Vector3 previewPosition, GameObject targetObject)
    {
        Transform closestPivot = null;
        float closestDistance = Mathf.Infinity;

        // 모든 자식 중 "Pivot" 태그가 있는 Transform 탐색
        foreach (Transform child in targetObject.transform)
        {
            if (child.CompareTag("Pivot"))
            {
                float distance = Vector3.Distance(previewPosition, child.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPivot = child;
                }
            }
        }

        return closestPivot;
    }

    void OnDrawGizmos()
    {
        if (previewObj != null)
        {
            MeshRenderer renderer = previewObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Vector3 boxCenter = renderer.bounds.center;

                Vector3 boxSize = new Vector3(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(boxCenter, boxSize);
            }
        }
    }
}
