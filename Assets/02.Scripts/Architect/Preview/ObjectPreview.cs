using System.Collections;
using UnityEngine;

public class ObjectPreview : BasePreview
{
    [SerializeField] private bool isFloorPivot;

    /// <summary>
    /// 배치 가능 여부 확인 코루틴
    /// </summary>
    /// <returns></returns>
    public override IEnumerator CanBuildCheckCoroutine()
    {
        gameObject.layer = 0;

        RaycastHit hitInfo;
        bool onGround = false;

        Collider objectCollider = GetComponent<Collider>();
        if (objectCollider == null)
        {
            Debug.LogError("Collider가 없습니다");
            yield break;
        }

        while (gameObject.activeSelf)
        {
            // 아래 방향으로 Ray를 발사하여 땅 아니면 바닥 건축물과 충돌한지 확인 후 오브젝트의 피벗이 중앙에 있는지 바닥에 있는지에 따라 위치 조절
            if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 1f, buildableLayer))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    // 오브젝트의 피벗이 바닥에 있다면 Ray가 충돌한 위치에 그대로 오브젝트를 이동시킴
                    if (isFloorPivot)
                        transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
                    // 오브젝트의 피벗이 중앙이라면 Ray가 충돌한 위치에 오브젝트의 콜라이더의 중앙에서 y까지의 값을 더해줌
                    else
                        transform.position = new Vector3(transform.position.x, hitInfo.point.y + objectCollider.bounds.extents.y, transform.position.z);
                    onGround = true;
                }
                else
                {
                    onGround = false;
                }
            }
            else
            {
                onGround = false;
            }

            // 땅/바닥에 붙었고 장애물이 감지되지 않았다면 배치 가능 
            if (onGround && CheckForObstacles())
            {
                canBuild = true;
            }
            else
            {
                canBuild = false;
            }

            yield return null;
        }
    }


    void OnDrawGizmos()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = Matrix4x4.TRS(collider.bounds.center, transform.rotation, Vector3.one); // 회전 적용
            Gizmos.DrawWireCube(Vector3.zero, collider.bounds.size);
        }
    }
}
