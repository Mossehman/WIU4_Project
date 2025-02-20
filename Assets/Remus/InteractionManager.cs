using UnityEngine;
using TMPro;

public class InteractionManager : MonoBehaviour
{
    public Camera PlayerCamera;
    public float InteractionDistance = 3f;
    public GameObject interactionText;
    private InteractionObject currentInteractable;

    void Update()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, InteractionDistance))
        {
            InteractionObject interactableObject = hit.collider.GetComponent<InteractionObject>();
            if (interactableObject != null && interactableObject != currentInteractable)
            {
                currentInteractable = interactableObject;
                interactionText.SetActive(true);

                TextMeshProUGUI textComponent = interactionText.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = currentInteractable.GetInteractionText();
                }
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                interactionText.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(); // Assuming GD_InteractObject has an Interact() method
        }
    }
}
