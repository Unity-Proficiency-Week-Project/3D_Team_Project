using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildUI : MonoBehaviour
{
    [SerializeField] private BuildingData[] buildingDatas;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    private void Start()
    {
        foreach (var data in buildingDatas)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            go.GetComponent<BuildSlot>().SetData(data);
        }
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

}
