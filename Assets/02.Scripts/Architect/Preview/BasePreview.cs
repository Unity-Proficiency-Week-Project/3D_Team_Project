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
        // ������ ������Ʈ�� ��ġ�� ī�޶� �������� ����
        transform.position = cameraContainer.position + (cameraContainer.forward * 3f) + (cameraContainer.up * 1.5f);

        // ������ ������Ʈ�� �ʱ� X�� ȸ���� ����
        float fixedXRotation = transform.eulerAngles.x;

        // ī�޶��� Y�� ȸ���� ���
        Quaternion cameraRotation = Quaternion.Euler(0, cameraContainer.eulerAngles.y, 0);

        // ���������� X�� ���� �� Y�� ȸ�� ����
        transform.rotation = Quaternion.Euler(fixedXRotation, cameraRotation.eulerAngles.y, 0);
    }

    public virtual bool CanBuild()
    {
        return canBuild;
    }

    /// <summary>
    /// ������ ������Ʈ �ֺ� ������Ʈ ���� ���� Ȯ��
    /// </summary>
    /// <param name="renderer">������ ������Ʈ �޽�������</param>
    /// <returns>�ֺ��� ������Ʈ�� �ִٸ� false, ���ٸ� true</returns>
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
                return false; // ��ֹ� ����
        }

        return true; // ��ֹ� ����
    }
}
