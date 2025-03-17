using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class RoofPreview : BasePreview
{
    private bool isSnap;

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

        gameObject.layer = 0;

        RaycastHit hitInfo;

        while (gameObject.activeSelf)
        {
            Debug.DrawRay(frontPivot.position, frontPivot.TransformDirection(Vector3.back), Color.cyan, 2f);
            Debug.DrawRay(backPivot.position, backPivot.TransformDirection(Vector3.forward), Color.cyan, 2f);

            if (CheckForObstacles())
            {
                if (Physics.Raycast(backPivot.position, backPivot.TransformDirection(Vector3.back), out hitInfo, 2f, buildableLayer))
                {
                    if(hitInfo.collider.name.Contains("Roof"))
                    {
                        Vector3 forwardPos = hitInfo.transform.TransformDirection(Vector3.forward) * 2f;

                        transform.rotation = hitInfo.transform.rotation;
                        transform.position = hitInfo.transform.position + forwardPos;

                        canBuild = true;
                        isSnap = true;

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
                        isSnap = true;

                        yield return null;
                        continue;
                    }
                }


                if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 2f, buildableLayer))
                {
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject") && hitInfo.collider.name.Contains("Wall"))
                    {
                        Transform hitTransform = hitInfo.collider.transform;

                        Vector3 upPos = hitTransform.TransformDirection(Vector3.up) * 2.75f;
                        Vector3 forwardPos = hitTransform.TransformDirection(Vector3.forward);
                        Vector3 leftPos = transform.name.Contains("Right") ? -hitTransform.TransformDirection(Vector3.left) * 0.1f : hitTransform.TransformDirection(Vector3.left) * 0.1f;

                        transform.rotation = hitTransform.rotation * originRotation;
                        transform.position = hitTransform.position + upPos + forwardPos + leftPos;
                        canBuild = true;
                        isSnap = true;

                        yield return null;
                        continue;
                    }
                    else
                    {
                        isSnap = false;
                        canBuild = false;
                    }
                }
                else
                {
                    isSnap = false;
                    canBuild = false;
                }
            }
            else
            {
                isSnap = false;
                canBuild = false;
            }

            yield return null;
        }
    }
}
