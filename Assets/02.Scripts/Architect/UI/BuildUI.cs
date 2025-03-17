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
        viewport.GetWorldCorners(viewportCorners);

        float padding = viewport.rect.height * 0.1f;

        Vector2 viewportMin = new Vector2(viewportCorners[0].x, viewportCorners[0].y); 
        Vector2 viewportMax = new Vector2(viewportCorners[2].x, viewportCorners[2].y - viewportPadding); 

        foreach (var obj in slotObejctList)
        {
            Vector3 worldPosition = obj.transform.position;

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
