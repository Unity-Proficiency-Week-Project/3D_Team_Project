using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Transform doorTransform;

    private bool canInteraction;
    private bool isOpen;

    private Coroutine doorCoroutine;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canInteraction && doorCoroutine == null)
        {
            doorCoroutine = StartCoroutine(DoorInteraction());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            canInteraction = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canInteraction = false;
    }

    private IEnumerator DoorInteraction()
    {
        Vector3 startRotate = doorTransform.localRotation.eulerAngles;
        Vector3 targetRotate = isOpen ? Vector3.zero : new Vector3(0, 90f, 0);
        float duration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Vector3 currentRot = Vector3.Lerp(startRotate, targetRotate, elapsedTime / duration);

            doorTransform.localRotation = Quaternion.Euler(currentRot);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isOpen = !isOpen;
        doorTransform.GetComponent<Collider>().enabled = !isOpen;
        doorCoroutine = null;
    }
}
