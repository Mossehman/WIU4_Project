using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CutsceneTransition : MonoBehaviour
{
    public int targetFrames = 30;
    [Range(0f, 1f)]
    public float normalizedTimeTransition = 0.95f;
    public Animator animator;
    public bool sceneTransition = false;
    [ConditionalHide(nameof(sceneTransition), false)]
    public GameObject nextTransition;
    [ConditionalHide(nameof(sceneTransition), true)]
    public string nextScene;

    public Material defaultSkybox;
    public Material spaceSkybox;
    public Material planetSkybox;
    public int cutsceneNumber;

    public ScriptableRendererFeature fullScreenRenderFeature1;
    public ScriptableRendererFeature fullScreenRenderFeature2;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        Application.targetFrameRate = targetFrames;
        CheckSkyboxChange();
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= normalizedTimeTransition)
        {
            if (sceneTransition)
            {
                Application.targetFrameRate = 120;
                SceneManager.LoadScene(nextScene);
                return;
            }

            nextTransition.SetActive(true);
            gameObject.SetActive(false);
        }
    }

    private void CheckSkyboxChange()
    {
        if (cutsceneNumber == 2 || cutsceneNumber == 4)
        {
            RenderSettings.skybox = defaultSkybox;
        }
        else if (cutsceneNumber == 7 || cutsceneNumber == 8)
        {
            fullScreenRenderFeature1.SetActive(true);
            fullScreenRenderFeature2.SetActive(true);

            RenderSettings.skybox = planetSkybox;
        }
        else
        {
            RenderSettings.skybox = spaceSkybox;
        }

        DynamicGI.UpdateEnvironment();
    }
}