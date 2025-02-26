using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance { get; private set; }

    public GameObject loadingScreen;
    public Image loadingObject;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI loadingFlavourText;

    public float moveDistance = 800f; // How far the object moves across the screen
    public float hoverAmplitude = 10f; // Hovering effect height
    public float hoverSpeed = 2f; // Speed of hover
    public float moveSpeed = 1.5f; // Speed of movement across the screen

    private Vector3 startPosition;
    private Vector3 endPosition;

    private readonly string[] flavourTexts =
    {
        "Calibrating warp drive...",
        "Assembling nanites...",
        "Booting up AI systems...",
        "Scanning cosmic anomalies...",
        "Optimizing planetary terrain...",
        "Refueling starship...",
        "Compiling planetary data...",
        "Connecting to deep space network..."
    };

    void Awake()
    {
        // Singleton (no DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadScene(string sceneName)
    {
        loadingScreen.SetActive(true); // Show loading screen immediately

        // Set start and end positions
        startPosition = new Vector3(-moveDistance / 2, loadingObject.rectTransform.anchoredPosition.y, 0);
        endPosition = new Vector3(moveDistance / 2, loadingObject.rectTransform.anchoredPosition.y, 0);

        // Choose a random loading flavour text
        if (loadingFlavourText != null)
        {
            loadingFlavourText.text = flavourTexts[Random.Range(0, flavourTexts.Length)];
        }

        // Start animations and loading
        StartCoroutine(AnimateLoadingText());
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false; // Prevents immediate scene switch

        float progress = 0f;

        while (!operation.isDone)
        {
            // Smoothly interpolate progress
            progress = Mathf.Lerp(progress, operation.progress / 0.9f, Time.deltaTime * moveSpeed);
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, progress);

            // Hover effect using sine wave
            float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            newPosition.y += hoverOffset;

            loadingObject.rectTransform.anchoredPosition = newPosition;

            // If loading is done, delay activation slightly
            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f); // Optional delay before switching
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingScreen.SetActive(false); // Hide loading screen after loading
    }

    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (loadingScreen.activeSelf)
        {
            dotCount = (dotCount + 1) % 4; // Cycles 0-3 dots
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.5f); // Adjust speed of animation
        }
    }
}