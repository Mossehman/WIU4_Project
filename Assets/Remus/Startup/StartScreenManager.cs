using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using TMPro;

public class StartScreenManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI companyText;

    [Header("Timings")]
    public float fadeDuration = 1.5f;
    public float logoDisplayDuration = 1.5f;

    [Header("Next Scene")]
    public string mainMenuSceneName = "Menu";

    private void Start()
    {
        StartCoroutine(PlayStartScreenSequence());
    }

    private IEnumerator PlayStartScreenSequence()
    {
        companyText.alpha = 0;
        companyText.DOFade(1, fadeDuration);
        yield return new WaitForSeconds(fadeDuration + logoDisplayDuration);

        companyText.DOFade(0, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        SceneManager.LoadScene("Menu");
    }
}