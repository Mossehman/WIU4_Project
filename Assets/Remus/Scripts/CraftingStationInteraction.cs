using UnityEngine;

public class CraftingStationInteraction : InteractionObject
{
    public GameObject craftingUI; // Reference to the Crafting UI Canvas

    [SerializeField] private GameObject _backgroundPanel;
    [SerializeField] private GameObject _vitalsPanel;
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private GameObject _mapPanel;

    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject hotbar;
    [SerializeField] private GameObject inventoryIcon;
    [SerializeField] private GameObject inventoryText;

    public float interactionRange = 3f; // Range within which UI appears
    private GameObject player;
    private bool isNear = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Ensure the player has the "Player" tag
        if (craftingUI != null)
            craftingUI.SetActive(false); // Hide UI initially
    }

    private void Update()
    {
    }

    private void ShowCraftingUI()
    {
        if (craftingUI != null)
        {
            craftingUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None; // Unlock the cursor
            Cursor.visible = true; // Make the cursor visible
            isNear = true;

            _backgroundPanel.SetActive(false);
            _vitalsPanel.SetActive(false);
            _infoPanel.SetActive(false);
            _mapPanel.SetActive(false);

            crosshair.SetActive(false);
            hotbar.SetActive(false);
            inventoryIcon.SetActive(false);
            inventoryText.SetActive(false);
        }
    }

    private void HideCraftingUI()
    {
        if (craftingUI != null)
        {
            craftingUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked; // Lock the cursor back
            Cursor.visible = false; // Hide the cursor
            isNear = false;

            _backgroundPanel.SetActive(true);
            _vitalsPanel.SetActive(true);
            _infoPanel.SetActive(true);
            _mapPanel.SetActive(true);

            crosshair.SetActive(true);
            hotbar.SetActive(true);
            inventoryIcon.SetActive(true);
            inventoryText.SetActive(true);
        }
    }

    public override void Interact()
    {
        if (UIManager.IsInventoryOpen) return;

        if (craftingUI.activeSelf)
        {
            HideCraftingUI();
        }
        else
        {
            ShowCraftingUI();
        }
    }
}