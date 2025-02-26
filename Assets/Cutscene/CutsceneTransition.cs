using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Animations;
using UnityEngine.SceneManagement;

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

    void Start()
    {
        Application.targetFrameRate = targetFrames;
    }

    // Update is called once per frame
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
}
