using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private bool isPaused = false;

    [SerializeField] private GameObject _pauseMenuUI;

    [SerializeField] private GameObject _backgroundPanel;
    [SerializeField] private GameObject _vitalsPanel;
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private GameObject _mapPanel;
    [SerializeField] private GameObject _hotbar;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _inventoryIcon;
    [SerializeField] private GameObject _inventoryText;
    [SerializeField] private GameObject _objectivePanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        _pauseMenuUI.SetActive(false);

        _backgroundPanel.SetActive(true);
        _vitalsPanel.SetActive(true);
        _infoPanel.SetActive(true);
        _mapPanel.SetActive(true);
        _hotbar.SetActive(true);
        _crosshair.SetActive(true);
        _inventoryIcon.SetActive(true);
        _inventoryText.SetActive(true);
        _objectivePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;

        _pauseMenuUI.SetActive(true);

        _backgroundPanel.SetActive(false);
        _vitalsPanel.SetActive(false);
        _infoPanel.SetActive(false);
        _mapPanel.SetActive(false);
        _hotbar.SetActive(false);
        _crosshair.SetActive(false);
        _inventoryIcon.SetActive(false);
        _inventoryText.SetActive(false);
        _objectivePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Startup");
    }
}