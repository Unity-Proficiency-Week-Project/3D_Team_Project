using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f;
    private float lastCheckTime;
    public float maxCheckDistance;
    public LayerMask layerMask;

    public GameObject curInteractGameObj;
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;
    public Camera camera;
    public CrossHairUI crosshair;

    private void Start()
    {
        camera = Camera.main;
        crosshair = FindObjectOfType<CrossHairUI>();
    }

    public void Update()
    {
        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

            if (EventSystem.current.IsPointerOverGameObject())
            {
                if (crosshair != null) crosshair.HideCrosshair();
            }
            else
            {
                if (crosshair != null) crosshair.ShowCrosshair();
            }


            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                if (hit.collider.gameObject != curInteractGameObj)
                {
                    curInteractGameObj = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();

                    if (crosshair != null) crosshair.SetCrosshairColor(Color.blue);
                }
            }
            else
            {
                curInteractGameObj = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false);

                if (crosshair != null) crosshair.SetCrosshairColor(Color.white);
            }
        }
    }

    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt();
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInteract();
            curInteractGameObj = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false);
        }
    }

}
