using System.Collections;
using UnityEngine;

public class RoofPreview : BasePreview
{
    private bool isSnap;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void UpdatePreview()
    {
        base.UpdatePreview();
    }

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
            if (CheckForObstacles())
            {
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
