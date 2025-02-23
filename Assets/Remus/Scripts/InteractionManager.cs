using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    public Camera PlayerCamera;
    public float InteractionDistance = 3f;
    public GameObject interactionText; // On-screen UI text
    public GameObject scanPanelPrefab; // World-space UI panel
    public Transform worldCanvas; // Parent for world UI
    private IInteractable currentInteractable;
    private GameObject currentPanel = null;
    private Transform currentTarget = null;

    void Update()
    {
        CheckForInteractable();

        // Ensure floating panel tracks player view
        if (currentPanel != null && currentTarget != null)
        {
            PositionPanelInFront(currentTarget);
        }
    }

    void CheckForInteractable()
    {
        Ray ray = PlayerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, InteractionDistance))
        {
            IInteractable interactableObject = hit.collider.GetComponent<IInteractable>();
            if (interactableObject != null && interactableObject != currentInteractable)
            {
                currentInteractable = interactableObject;
                interactionText.SetActive(true);

                TextMeshProUGUI textComponent = interactionText.GetComponent<TextMeshProUGUI>();
                if (textComponent != null)
                {
                    textComponent.text = currentInteractable.GetInteractionText();
                }

                ShowScanPanel(hit.transform);
            }
        }
        else
        {
            if (currentInteractable != null)
            {
                currentInteractable = null;
                interactionText.SetActive(false);
                ClearPanel();
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void ShowScanPanel(Transform target)
    {
        if (currentPanel == null)
        {
            currentPanel = Instantiate(scanPanelPrefab, worldCanvas);
        }

        currentTarget = target;

        TMP_Text titleText = currentPanel.transform.Find("TitleText").GetComponent<TMP_Text>();
        TMP_Text descText = currentPanel.transform.Find("DescText").GetComponent<TMP_Text>();
        Image iconImage = currentPanel.transform.Find("Holder/Icon").GetComponent<Image>();

        IInteractable interactableObject = target.GetComponent<IInteractable>();

        if (titleText != null)
        {
            titleText.text = target.name;
        }

        if (descText != null)
        {
            descText.text = interactableObject.GetCustomDescription(); // Use dynamic descriptions
        }

        if (iconImage != null)
        {
            Sprite icon = interactableObject.GetInteractionIcon();
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        PositionPanelInFront(target);
    }

    void PositionPanelInFront(Transform target)
    {
        Vector3 directionToPlayer = (PlayerCamera.transform.position - target.position).normalized;
        Vector3 panelPosition = target.position + directionToPlayer * 1.5f; // Position slightly in front

        currentPanel.transform.position = panelPosition;
        currentPanel.transform.LookAt(PlayerCamera.transform);
        currentPanel.transform.Rotate(0, 180, 0); // Flip for readability
    }

    void ClearPanel()
    {
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
        currentTarget = null;
    }
}