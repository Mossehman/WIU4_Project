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

    public float moveDistance = 800f;
    public float hoverAmplitude = 10f;
    public float hoverSpeed = 2f;
    public float moveSpeed = 1.5f;

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
        loadingScreen.SetActive(true);

        startPosition = new Vector3(-moveDistance / 2, loadingObject.rectTransform.anchoredPosition.y, 0);
        endPosition = new Vector3(moveDistance / 2, loadingObject.rectTransform.anchoredPosition.y, 0);

        if (loadingFlavourText != null)
        {
            loadingFlavourText.text = flavourTexts[Random.Range(0, flavourTexts.Length)];
        }

        StartCoroutine(AnimateLoadingText());
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float progress = 0f;

        while (!operation.isDone)
        {
            progress = Mathf.Lerp(progress, operation.progress / 0.9f, Time.deltaTime * moveSpeed);
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, progress);

            float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            newPosition.y += hoverOffset;

            loadingObject.rectTransform.anchoredPosition = newPosition;

            if (operation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(1f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        loadingScreen.SetActive(false);
    }

    private IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (loadingScreen.activeSelf)
        {
            dotCount = (dotCount + 1) % 4;
            loadingText.text = baseText + new string('.', dotCount);
            yield return new WaitForSeconds(0.5f);
        }
    }
}