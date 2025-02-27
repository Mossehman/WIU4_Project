using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class EndCreditsManager : MonoBehaviour
{
    public CanvasGroup fadeCanvas; // UI Canvas Group for fading in/out
    public RectTransform creditsTransform; // The RectTransform of the text content
    public ScrollRect scrollRect; // ScrollRect component to control scrolling
    public TextMeshProUGUI creditsText; // The TextMeshProUGUI component for credits
    public float startDelay = 3.0f; // Time before the fade-in begins
    public float fadeDuration = 2.0f; // Time to fade in
    public float scrollSpeed = 50f; // Speed of scrolling credits
    public float delayBeforeFade = 3.0f; // Time before fading out
    public string mainMenuScene = "Menu"; // Name of the main menu scene
    public KeyCode skipKey = KeyCode.Space; // Key to skip credits

    private bool isSkipping = false;

    void Start()
    {
        fadeCanvas.alpha = 0; // Start fully invisible
        scrollRect.verticalNormalizedPosition = 1; // Ensure credits start at the top
        StartCoroutine(StartCredits());
    }

    IEnumerator StartCredits()
    {
        // Wait before fade-in
        yield return new WaitForSeconds(startDelay);

        // Fade In
        yield return StartCoroutine(FadeCanvas(0, 1, fadeDuration));

        // Wait for a moment before scrolling
        yield return new WaitForSeconds(1f);

        // Scroll credits
        yield return StartCoroutine(ScrollCredits());

        // Wait before fading out
        yield return new WaitForSeconds(delayBeforeFade);

        // Fade Out
        yield return StartCoroutine(FadeCanvas(1, 0, fadeDuration));

        // Load Main Menu
        SceneManager.LoadScene(mainMenuScene);
    }

    IEnumerator ScrollCredits()
    {
        float duration = creditsText.preferredHeight / scrollSpeed; // Calculate scroll time
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

        // Ensure it reaches the bottom
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