using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class VignetteEffect : MonoBehaviour
{
    public Volume postProcessVolume;
    private Vignette vignette;

    public float fadeDuration = 2f; // Time for the fade effect
    public float maxVignetteIntensity = 0.8f; // How dark it starts
    public float minVignetteIntensity = 0f; // Fully open eyes
    public Canvas uiCanvas; // UI Canvas to hide

    void Start()
    {
        if (postProcessVolume == null)
        {
            postProcessVolume = FindObjectOfType<Volume>();
        }

        if (uiCanvas != null)
        {
            uiCanvas.enabled = false; // Hide the UI at start
        }

        if (postProcessVolume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.Override(maxVignetteIntensity);
            StartCoroutine(FadeVignette(minVignetteIntensity));
        }
        else
        {
            Debug.LogError("Vignette effect not found in the post-processing volume.");
        }
    }

    private IEnumerator FadeVignette(float targetIntensity)
    {
        float elapsedTime = 0f;
        float startIntensity = vignette.intensity.value;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            vignette.intensity.Override(Mathf.Lerp(startIntensity, targetIntensity, t));
            yield return null;
        }

        vignette.intensity.Override(targetIntensity);

        // Show UI after fade effect is complete
        if (uiCanvas != null)
        {
            uiCanvas.enabled = true;
        }
    }
}