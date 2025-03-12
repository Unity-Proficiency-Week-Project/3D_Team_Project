using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildObjectCreator : MonoBehaviour
{
    [SerializeField] private GameObject buildPrefab;
    [SerializeField] private LayerMask groundLayer;

    private bool canBuild;
    private GameObject previewObj;

    private void Update()
    {
        // Input 함수들 나중에 InputAction으로 수정 예정

        if(Input.GetKeyDown(KeyCode.F))
        {
            CreatePreviewObject(buildPrefab);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPreview();
        }


        if(canBuild&&Input.GetKeyDown(KeyCode.V))
        {
            GameObject go = Instantiate(previewObj);
            go.transform.position = previewObj.transform.position;
            go.GetComponent<MeshRenderer>().material.color = Color.white;
        }

        if(previewObj != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 cameraForward = Camera.main.transform.forward;

            // 카메라 앞 3m 지점에 프리뷰 오브젝트 배치
            previewObj.transform.position = cameraPosition + cameraForward * 3f;

            float x = previewObj.transform.eulerAngles.x;

            // 수평 유지 및 Y축 회전만 허용
            previewObj.transform.rotation = Quaternion.Euler(x, previewObj.transform.eulerAngles.y, previewObj.transform.eulerAngles.z);

            if (Input.GetKey(KeyCode.Q))
            {
                previewObj.transform.Rotate(0, 0, 0.1f);
            }
            
            else if(Input.GetKey(KeyCode.E))
            {
                previewObj.transform.Rotate(0, 0, -0.1f);
            }
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

        while (previewObj != null)
        {
            Vector3 bottomPoint = renderer.bounds.center;
            bottomPoint.y -= renderer.bounds.extents.y - 0.2f;

            if (CheckForObstacles(renderer))
            {
                Debug.DrawRay(bottomPoint, Vector3.down, Color.red, 1f);
                if (Physics.Raycast(bottomPoint, Vector3.down, out RaycastHit hitInfo, 1f, groundLayer))
                {
                    canBuild = true;
                    renderer.material.color = Color.green;

                    float objectHeightOffset = renderer.bounds.extents.y;

                    previewObj.transform.position = new Vector3(
                        previewObj.transform.position.x,
                        hitInfo.point.y + objectHeightOffset,
                        previewObj.transform.position.z
                    );
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
            if(collider.gameObject == previewObj) continue;

            if (collider.gameObject.layer != groundLayer)
                return false;
        }

        return true;
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
