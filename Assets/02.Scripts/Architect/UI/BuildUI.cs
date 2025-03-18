using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildUI : MonoBehaviour
{
    [SerializeField] private BuildingData[] buildingDatas;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    [SerializeField] private RectTransform viewport;
    [SerializeField] private Camera uiCamera;
    private float viewportPadding = 5f;
    public List<GameObject> slotObejctList;


    private void Start()
    {
        foreach (var data in buildingDatas)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            go.GetComponent<BuildSlot>().SetData(data);
        }
    }

    private void Update()
    {
        SetActiveSlotObject();
    }

    public void ChangeUIActive()
    {
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            PlayerManager.Instance.Player.controller.canLook = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            gameObject.SetActive(true);
            PlayerManager.Instance.Player.controller.canLook = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetActiveSlotObject()
    {
        Vector3[] viewportCorners = new Vector3[4];

        // 뷰포트의 각 코너의 위치 가져오기
        viewport.GetWorldCorners(viewportCorners);

        float padding = viewport.rect.height * 0.1f;

        // 뷰포트좌표의 최대값(우측 상단), 최소값(촤측 하단) 가져오기
        Vector2 viewportMin = new Vector2(viewportCorners[0].x, viewportCorners[0].y); 
        Vector2 viewportMax = new Vector2(viewportCorners[2].x, viewportCorners[2].y - viewportPadding); 

        foreach (var obj in slotObejctList)
        {
            // 슬롯 오브젝트 위치 가져오기
            Vector3 worldPosition = obj.transform.position;

            // 현재 슬롯이 뷰포트 범위 안에 위치하는지 검사 후 활성화/비활성화
            if (worldPosition.x >= viewportMin.x && worldPosition.x <= viewportMax.x &&
                worldPosition.y >= viewportMin.y && worldPosition.y <= viewportMax.y)
            {
                obj.SetActive(true); 
            }
            else
            {
                obj.SetActive(false); 
            }
        }
    }
}
