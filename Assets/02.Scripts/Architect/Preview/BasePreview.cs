using System.Collections;
using UnityEngine;

public abstract class BasePreview : MonoBehaviour
{
    protected LayerMask buildableLayer;
    protected bool canBuild;

    protected MeshRenderer mesh;

    protected Transform cameraContainer;
    protected Quaternion originRotation;

    /// <summary>
    /// 프리뷰 오브젝트 초기화
    /// </summary>
    /// <param name="buildableLayer">건설 가능한 레이어</param>
    public virtual void Initialize(LayerMask buildableLayer)
    {
        mesh = GetComponent<MeshRenderer>();

        this.buildableLayer = buildableLayer;

        originRotation = transform.localRotation;

        cameraContainer = PlayerManager.Instance.Player.controller.cameraContainer;

        StartCoroutine(CanBuildCheckCoroutine());


    }

    protected virtual void Update()
    {
        if (cameraContainer != null)
            UpdatePreview();
    }

    public abstract IEnumerator CanBuildCheckCoroutine();

    /// <summary>
    /// 프리뷰 오브젝트 카메라 기준으로 위치 유지
    /// </summary>
    public virtual void UpdatePreview()
    {
        
        if (cameraContainer == null)
            Debug.LogError("카메라 찾지 못함");

        if (transform == null)
            Debug.LogError("transform 찾지 못함");

        transform.position = cameraContainer.position + (cameraContainer.forward * 3f) + (cameraContainer.up * 1.5f);

        Quaternion cameraYRotation = Quaternion.Euler(0, cameraContainer.eulerAngles.y, 0);

        transform.rotation = cameraYRotation * originRotation;

        mesh.material.color = canBuild ? Color.green : Color.red;
    }

    /// <summary>
    /// 배치 가능 여부 반환
    /// </summary>
    /// <returns></returns>
    public virtual bool CanBuild()
    {
        return canBuild;
    }

    /// <summary>
    /// 프리뷰 오브젝트 주변 오브젝트 존재 여부 확인
    /// </summary>
    /// <param name="renderer">프리뷰 오브젝트 메쉬렌더러</param>
    /// <returns>주변에 오브젝트가 있다면 false, 없다면 true</returns>
    protected virtual bool CheckForObstacles()
    {
        Collider objCollider = GetComponent<Collider>();
        if (objCollider == null) return false;

        Vector3 boxCenter = objCollider.bounds.center;
        Vector3 boxSize = new Vector3(objCollider.bounds.size.x * 0.8f, objCollider.bounds.size.y, objCollider.bounds.size.z * 0.8f);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2f, transform.rotation);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            if (collider.gameObject.layer == LayerMask.NameToLayer("BuildObject") || collider.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                Debug.Log($"장애물 {collider.gameObject.name}");
                return false; // 장애물 있음
            }
        }

        return true; // 장애물 없음
    }
}
