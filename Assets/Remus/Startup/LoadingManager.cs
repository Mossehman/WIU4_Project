using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image loadingObject;

    public float moveDistance = 800f; // How far the object moves across the screen
    public float hoverAmplitude = 10f; // Hovering effect height
    public float hoverSpeed = 2f; // Speed of hover
    public float moveSpeed = 0.05f; // Speed of movement across the screen

    private Vector3 startPosition;
    private Vector3 endPosition;
    private float fakeProgress = 0f; // Fake loading progress

    public void LoadScene(string sceneName)
    {
        loadingScreen.SetActive(true); // Show loading screen immediately

        // Set start and end positions
        startPosition = new Vector3(-moveDistance / 2, loadingObject.rectTransform.anchoredPosition.y, 0);
        endPosition = new Vector3(moveDistance / 2, loadingObject.rectTransform.anchoredPosition.y, 0);

        StartCoroutine(FakeLoading(sceneName));
    }

    private IEnumerator FakeLoading(string sceneName)
    {
        float totalDuration = 20f; // Fake loading time
        float elapsedTime = 0f;

        while (elapsedTime < totalDuration)
        {
            elapsedTime += Time.deltaTime;
            fakeProgress = Mathf.Clamp01(elapsedTime / totalDuration); // Normalize progress 0-1

            // Move object smoothly across the screen
            Vector3 newPosition = Vector3.Lerp(startPosition, endPosition, fakeProgress);

            // Add hover effect using sine wave
            float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmplitude;
            newPosition.y += hoverOffset;

            loadingObject.rectTransform.anchoredPosition = newPosition;

            yield return null;
        }

        // After fake loading, switch to the next scene
        SceneManager.LoadScene(sceneName);
    }
}