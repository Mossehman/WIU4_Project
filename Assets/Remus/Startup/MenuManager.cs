using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MenuManager : MonoBehaviour
{
    public ScriptableRendererFeature fullScreenRenderFeature1;
    public ScriptableRendererFeature fullScreenRenderFeature2;

    private void Start()
    {
        fullScreenRenderFeature1.SetActive(false);
        fullScreenRenderFeature2.SetActive(false);
    }

    public void StartGame()
    {
        LoadingManager.Instance.LoadScene("StartCutscene");
    }
}