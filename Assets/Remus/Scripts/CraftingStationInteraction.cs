using UnityEngine;

public class CraftingStationInteraction : InteractionObject
{
    public GameObject craftingUI;

    [SerializeField] private GameObject _backgroundPanel;
    [SerializeField] private GameObject _vitalsPanel;
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private GameObject _mapPanel;

    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject hotbar;
    [SerializeField] private GameObject inventoryIcon;
    [SerializeField] private GameObject inventoryText;

    [SerializeField] private GameObject _objectivePanel;

    public float interactionRange = 3f;
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (craftingUI != null)
            craftingUI.SetActive(false);
    }

    public override string GetCustomDescription()
    {
        return $"Craft Things!";
    }

    private void ShowCraftingUI()
    {
        if (craftingUI != null)
        {
            craftingUI.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            _backgroundPanel.SetActive(false);
            _vitalsPanel.SetActive(false);
            _infoPanel.SetActive(false);
            _mapPanel.SetActive(false);

            crosshair.SetActive(false);
            hotbar.SetActive(false);
            inventoryIcon.SetActive(false);
            inventoryText.SetActive(false);

            _objectivePanel.SetActive(false);

            Time.timeScale = 0f;
            UIManager.IsCraftingOpen = true;
        }
    }

    private void HideCraftingUI()
    {
        if (craftingUI != null)
        {
            craftingUI.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _backgroundPanel.SetActive(true);
            _vitalsPanel.SetActive(true);
            _infoPanel.SetActive(true);
            _mapPanel.SetActive(true);

            crosshair.SetActive(true);
            hotbar.SetActive(true);
            inventoryIcon.SetActive(true);
            inventoryText.SetActive(true);

            _objectivePanel.SetActive(true);

            Time.timeScale = 1f;
            UIManager.IsCraftingOpen = false;
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