using System.Collections;
using UnityEngine;

public class RoofPreview : BasePreview
{
    [SerializeField] private Transform frontPivot;
    [SerializeField] private Transform backPivot;

    public override IEnumerator CanBuildCheckCoroutine()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer가 없습니다");
            yield break;
        }

        Collider roofCollider = GetComponent<Collider>();
        if (roofCollider == null)
        {
            Debug.LogError("Collider가 없습니다.");
            yield break;
        }

        // 레이어 겹침 방지를 위해 오브젝트의 레이어을 디폴트로 변경
        gameObject.layer = 0;

        RaycastHit hitInfo;

        while (gameObject.activeSelf)
        {
            if (CheckForObstacles())
            {
                // 피벗 기준 앞뒤로 Ray 검사를 진행하여 지붕과 충돌하는지 검사
                if (Physics.Raycast(backPivot.position, backPivot.TransformDirection(Vector3.back), out hitInfo, 2f, buildableLayer))
                {
                    if(hitInfo.collider.name.Contains("Roof"))
                    {
                        // 지붕과 충돌했다면 스냅 될 수 있도록 위치 조정
                        Vector3 forwardPos = hitInfo.transform.TransformDirection(Vector3.forward) * 2f;

                        transform.rotation = hitInfo.transform.rotation;
                        transform.position = hitInfo.transform.position + forwardPos;

                        canBuild = true;

                        yield return null;
                        continue;
                    }
                }

                if (Physics.Raycast(frontPivot.position, frontPivot.TransformDirection(Vector3.forward), out hitInfo, 2f, buildableLayer))
                {
                    if (hitInfo.collider.name.Contains("Roof"))
                    {
                        Vector3 backPos = hitInfo.transform.TransformDirection(Vector3.back) * 2f;

                        transform.rotation = hitInfo.transform.rotation;
                        transform.position = hitInfo.transform.position + backPos;

                        canBuild = true;

                        yield return null;
                        continue;
                    }
                }

                // Ray가 벽과 충돌했다면
                if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 2f, buildableLayer))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject") && hitInfo.collider.name.Contains("Wall"))
                    {
                        Transform hitTransform = hitInfo.collider.transform;

                        // 지붕을 벽 위에 위치 하도록 함
                        Vector3 upPos = hitTransform.TransformDirection(Vector3.up) * 2.75f;
                        Vector3 forwardPos = hitTransform.TransformDirection(Vector3.forward);
                        Vector3 leftPos = transform.name.Contains("Right") ? -hitTransform.TransformDirection(Vector3.left) * 0.1f : hitTransform.TransformDirection(Vector3.left) * 0.1f;

                        transform.rotation = hitTransform.rotation * originRotation;
                        transform.position = hitTransform.position + upPos + forwardPos + leftPos;
                        canBuild = true;

                        yield return null;
                        continue;
                    }
                    else
                    {
                        canBuild = false;
                    }
                }
                else
                {
                    canBuild = false;
                }
            }
            else
            {
                canBuild = false;
            }

            yield return null;
        }
    }
}
