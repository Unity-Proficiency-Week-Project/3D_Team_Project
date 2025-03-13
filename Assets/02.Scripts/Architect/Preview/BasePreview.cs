using UnityEngine;

public abstract class BasePreview : MonoBehaviour
{
    protected LayerMask buildableLayer;
    protected bool canBuild;

    protected Transform cameraContainer;

    public virtual void Initialize(LayerMask buildableLayer)
    {
        this.buildableLayer = buildableLayer;
        cameraContainer = PlayerManager.Instance.Player.controller.cameraContainer;
    }

    private void Update()
    {
        UpdatePreview();
    }

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
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null) return false;

        Vector3 boxCenter = renderer.bounds.center;
        Vector3 boxSize = new Vector3(renderer.bounds.size.x, renderer.bounds.size.y, renderer.bounds.size.z);

        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2.1f, Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject) continue;

            if (collider.gameObject.layer == LayerMask.NameToLayer("BuildObject"))
                return false; // 장애물 있음
        }

        return true; // 장애물 없음
    }
}
