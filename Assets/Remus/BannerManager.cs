using System.Collections;
using UnityEngine;
using TMPro;

public class BannerManager : MonoBehaviour
{
    public static BannerManager Instance { get; private set; }

    [SerializeField] private GameObject bannerPanel;
    [SerializeField] private TextMeshProUGUI bannerText;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float displayTime = 2f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        HideBannerInstant();
    }

    public void ShowBanner(string message)
    {
        StopAllCoroutines();
        StartCoroutine(DisplayBanner(message));
    }

    private IEnumerator DisplayBanner(string message)
    {
        bannerText.text = message;
        bannerPanel.SetActive(true);
        bannerText.color = new Color(bannerText.color.r, bannerText.color.g, bannerText.color.b, 0);

        // Fade In
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            bannerText.color = new Color(bannerText.color.r, bannerText.color.g, bannerText.color.b, alpha);
            yield return null;
        }

        yield return new WaitForSeconds(displayTime);

        // Fade Out
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            bannerText.color = new Color(bannerText.color.r, bannerText.color.g, bannerText.color.b, alpha);
            yield return null;
        }

        bannerPanel.SetActive(false);
    }

    private void HideBannerInstant()
    {
        bannerPanel.SetActive(false);
    }
}