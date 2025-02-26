using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MenuManager : MonoBehaviour
{
    public ScriptableRendererFeature fullScreenRenderFeature1;
    public ScriptableRendererFeature fullScreenRenderFeature2;

    public LoadingManager loadingManager;

    private void Start()
    {
        fullScreenRenderFeature1.SetActive(false);
        fullScreenRenderFeature2.SetActive(false);
    }

    public void StartGame()
    {
        fullScreenRenderFeature1.SetActive(true);
        fullScreenRenderFeature2.SetActive(true);

        if (loadingManager != null)
        {
            loadingManager.LoadScene("Main Game Scene"); // Start loading screen
        }
        else
        {
            Debug.LogError("LoadingManager not assigned in MenuManager!");
        }
    }
}