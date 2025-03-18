using System.Collections;
using UnityEngine;

public abstract class BasePreview : MonoBehaviour
{
    protected LayerMask buildableLayer;
    protected bool canBuild;

    protected MeshRenderer mesh;
    protected MeshRenderer[] childrenMeshes;

    protected Transform cameraContainer;
    protected Quaternion originRotation;

    protected virtual void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        childrenMeshes = GetComponentsInChildren<MeshRenderer>();
        cameraContainer = PlayerManager.Instance.Player.controller.cameraContainer;

        Collider[] colliders = GetComponentsInChildren<Collider>();

        // 장애물 체크를 위하여 가장 위에 존재하는 콜라이더를 제외한 자식 콜라이더 모두 비활성화
        foreach (Collider collider in colliders)
        {
            if (collider.transform == transform)
                continue;

            collider.enabled = false;
        }
    }

    /// <summary>
    /// 프리뷰 오브젝트 초기화
    /// </summary>
    /// <param name="buildableLayer">건설 가능한 레이어</param>
    public virtual void Initialize(LayerMask buildableLayer)
    {
        this.buildableLayer = buildableLayer;

        originRotation = transform.localRotation;

        StartCoroutine(CanBuildCheckCoroutine());

    }

    protected virtual void Update()
    {
        if (cameraContainer != null)
            UpdatePreview();
        else
            Debug.LogError("카메라를 찾지 못했습니다.");
    }

    public abstract IEnumerator CanBuildCheckCoroutine();

    /// <summary>
    /// 프리뷰 오브젝트 카메라 기준으로 위치 유지
    /// </summary>
    public virtual void UpdatePreview()
    {
        // 프리뷰 오브젝트의 위치를 카메라 위치 기준으로 유지
        transform.position = cameraContainer.position + (cameraContainer.forward * 4.5f) + (cameraContainer.up * 2f);

        // 카메라의 Y 회전값 가져오기
        Quaternion cameraYRotation = Quaternion.Euler(0, cameraContainer.eulerAngles.y, 0);

        // 오브젝트의 회전값을 카메라 Y 회전값 * 설정된 회전값으로 변경
        transform.rotation = cameraYRotation * originRotation;

        // 현재 배치 가능 여부에 따라 머티리얼 색상 변경
        if (mesh != null)
            mesh.material.color = canBuild ? Color.green : Color.red;

        if (childrenMeshes != null)
        {
            foreach (MeshRenderer mesh in childrenMeshes)
            {
                mesh.material.color = canBuild ? Color.green : Color.red;
            }
        }
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

        // 오브젝트의 콜라이더 크기
        Vector3 boxCenter = objCollider.bounds.center;
        Vector3 boxSize = new Vector3(
            objCollider.bounds.size.x * 0.9f, 
            objCollider.bounds.size.y, 
            objCollider.bounds.size.z * 0.9f
            );

        // 현재 설정한 박스 사이즈 안에 존재하는 콜라이더가 있는지 검사
        Collider[] colliders = Physics.OverlapBox(boxCenter, boxSize / 2f, transform.rotation);

        // 겹친 오브젝트들을 검사 후 장애물 겹침 여부 검사
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == gameObject || 
                gameObject.name.Contains("Roof") && 
                (collider.name.Contains("Wall") ||
                collider.name.Contains("Roof"))) continue;

            if (collider.transform.parent == gameObject) continue;

            if (collider.gameObject.layer == LayerMask.NameToLayer("BuildObject") || 
                collider.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                return false; // 장애물 있음
            }
        }

        return true; // 장애물 없음
    }
}
