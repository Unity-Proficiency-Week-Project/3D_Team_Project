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

        previewObj.transform.position = transform.position + transform.forward * 3f;

        previewObj.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0f);

        previewObj.transform.parent = transform;

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
