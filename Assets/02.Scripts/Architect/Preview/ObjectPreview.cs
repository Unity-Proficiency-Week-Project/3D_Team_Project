using System.Collections;
using UnityEngine;

public class ObjectPreview : BasePreview
{
    protected override void Update()
    {
        base.Update();
    }

    public override IEnumerator CanBuildCheckCoroutine()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("MeshRenderer가 없습니다");
            yield break;
        }

        gameObject.layer = 0; // Ray에 자신 감지 방지

        RaycastHit hitInfo;
        bool onGround = false; // Ground 레이어 위에 있는지 여부

        while (gameObject.activeSelf)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 1f, buildableLayer))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
                    onGround = true; // Ground 레이어 위에 있음
                }
                else
                {
                    onGround = false; // Ground 레이어 위에 없음 (다른 레이어 충돌)
                }
            }
            else
            {
                onGround = false; // Raycast가 아무것과도 충돌하지 않음
            }

            if (onGround && CheckForObstacles()) // 땅에 붙었고, 장애물도 없다면 건축 가능
            {
                renderer.material.color = Color.green;
                canBuild = true;
            }
            else
            {
                renderer.material.color = Color.red;
                canBuild = false;
            }
            yield return null;
        }
    }


    void OnDrawGizmos()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(collider.bounds.center, transform.rotation, Vector3.one); // 회전 적용
            Gizmos.DrawWireCube(Vector3.zero, collider.bounds.size);
        }
    }
}
