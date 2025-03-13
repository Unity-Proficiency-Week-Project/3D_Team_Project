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
        if (renderer != null)
        {
            // 박스 중심과 크기 계산
            Vector3 boxCenter = renderer.bounds.center;
            Vector3 boxSize = new Vector3(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);

            // Gizmos 색상 설정 (건설 가능 여부에 따라 초록색 또는 빨간색)
            Gizmos.color = Color.blue;

            // 박스 그리기
            Gizmos.DrawCube(boxCenter, boxSize);
        }
    }
}
