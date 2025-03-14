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
        mesh.material.color = canBuild ? Color.green : Color.red;

        if (!isSnap || Vector3.Distance(transform.position, PlayerManager.Instance.Player.controller.cameraContainer.position) > 3.5f)
        {
            isSnap = false;
            base.UpdatePreview();
        }
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
                    if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                    {
                        Collider wallCollider = hitInfo.collider;
                        if (wallCollider != null)
                        {
                            float roofLeft = roofCollider.bounds.min.x;
                            float wallLeft = wallCollider.bounds.min.x;
                            float xOffset = wallLeft - roofLeft;

                            float wallTop = wallCollider.bounds.max.y;
                            float roofBottom = roofCollider.bounds.min.y;
                            float yOffset = wallTop - roofBottom + 0.01f; 

                            transform.position = new Vector3(transform.position.x + xOffset, hitInfo.point.y + yOffset, transform.position.z);
                            canBuild = true;
                            isSnap = true;

                            yield return null;
                            continue;
                        }
                    }
                    else
                    {
                        isSnap = false;
                        canBuild = false;

                        yield return null;
                        continue;
                    }
                }
                else
                {
                    isSnap = false;
                    canBuild = false;

                    yield return null;
                    continue;
                }
            }
            else
            {
                isSnap = false;
                canBuild = false;

                yield return null;
                continue;
            }
        }
    }
}
