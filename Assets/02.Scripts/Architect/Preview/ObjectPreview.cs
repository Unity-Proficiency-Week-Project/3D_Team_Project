using System.Collections;
using UnityEngine;

public class ObjectPreview : BasePreview
{
    protected override void Awake()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
        base.Awake();

    }

    protected override void Update()
    {
        base.Update();
    }

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

        float objectHeight = objectCollider.bounds.size.y;

        float yOffset = objectHeight / 2f; 

        while (gameObject.activeSelf)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, 1f, buildableLayer))
            {
                if (hitInfo.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    transform.position = new Vector3(transform.position.x, hitInfo.point.y + yOffset, transform.position.z);
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
