using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndCreditsManager : MonoBehaviour
{
    public CanvasGroup fadeCanvas;
    public RectTransform creditsTransform;
    public ScrollRect scrollRect;
    public TextMeshProUGUI creditsText;
    public float startDelay = 3.0f;
    public float fadeDuration = 2.0f;
    public float scrollSpeed = 50f;
    public float delayBeforeFade = 3.0f;
    public string mainMenuScene = "Startup";
    public KeyCode skipKey = KeyCode.Space;

    private bool isSkipping = false;

    void Start()
    {
        fadeCanvas.alpha = 0;
        scrollRect.verticalNormalizedPosition = 1;
        StartCoroutine(StartCredits());
    }

    IEnumerator StartCredits()
    {
        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(FadeCanvas(0, 1, fadeDuration));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ScrollCredits());
        SceneManager.LoadScene(mainMenuScene);
    }

    IEnumerator ScrollCredits()
    {
        float duration = creditsText.preferredHeight / scrollSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (Input.GetKey(skipKey))
            {
                isSkipping = true;
                break;
            }

            elapsed += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(1, 0, elapsed / duration);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = 0;
    }

    IEnumerator FadeCanvas(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeCanvas.alpha = endAlpha;
    }
}