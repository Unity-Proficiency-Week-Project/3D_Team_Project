using System.Collections;
using UnityEngine;

public abstract class BasePreview : MonoBehaviour
{
    protected LayerMask buildableLayer;
    protected bool canBuild;

    protected Transform cameraContainer;

    /// <summary>
    /// 프리뷰 오브젝트 초기화
    /// </summary>
    /// <param name="buildableLayer">건설 가능한 레이어</param>
    public virtual void Initialize(LayerMask buildableLayer)
    {
        this.buildableLayer = buildableLayer;

        cameraContainer = PlayerManager.Instance.Player.controller.cameraContainer;

        StartCoroutine(CanBuildCheckCoroutine());
    }

    protected virtual void Update()
    {
        UpdatePreview();
    }

    public abstract IEnumerator CanBuildCheckCoroutine();

    /// <summary>
    /// 프리뷰 오브젝트 카메라 기준으로 위치 유지
    /// </summary>
    public virtual void UpdatePreview()
    {
        // 프리뷰 오브젝트의 위치를 카메라 기준으로 조정
        transform.position = cameraContainer.position + (cameraContainer.forward * 3f) + (cameraContainer.up * 1.5f);

        // 프리뷰 오브젝트의 초기 X축 회전값 고정
        float fixedXRotation = transform.eulerAngles.x;

        // 카메라의 Y축 회전값 계산
        Quaternion cameraRotation = Quaternion.Euler(0, cameraContainer.eulerAngles.y, 0);

        // 최종적으로 X축 고정 및 Y축 회전 적용
        transform.rotation = Quaternion.Euler(fixedXRotation, cameraRotation.eulerAngles.y, 0);
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
        Vector3 boxSize = new Vector3(objCollider.bounds.size.x, objCollider.bounds.size.y, objCollider.bounds.size.z);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2f, transform.rotation);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            if (collider.gameObject.layer == LayerMask.NameToLayer("BuildObject") || collider.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                Debug.Log($"obstacle : {LayerMask.LayerToName(collider.gameObject.layer)}");
                return false; // 장애물 있음
            }
        }

        return true; // 장애물 없음
    }
}
