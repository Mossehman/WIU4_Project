using TMPro;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType { None, Blizzard, Snowstorm, AcidRain, Heatwave, Sandstorm }
    public WeatherType currentWeather = WeatherType.None;

    public float baseTemperature = 45f;
    public float temperature;
    public float temperatureChangeSpeed = 20f;
    public int eventDuration = 0;

    [SerializeField] private TextMeshProUGUI WeatherText;
    [SerializeField] private Material BlizzardMaterial;
    [SerializeField] private Material SnowstormMaterial;
    [SerializeField] private Material AcidRainMaterial;
    [SerializeField] private Material HeatwaveMaterial;
    [SerializeField] private Material SandstormMaterial;
    [SerializeField] private FullScreenPassRendererFeature FullScreenFeature;
    [SerializeField] private ParticleSystem AcidRainParticles;

    private float targetIntensity = 0f;
    private float currentIntensity = 0f;
    private Material activeMaterial;

    private PlayerController playerController;
    private PlayerStats playerStats;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
        playerStats = FindObjectOfType<PlayerStats>();

        if (playerController == null)
        {
            Debug.LogError("[WeatherManager] PlayerController not found in the scene!");
        }

        EventManager.Connect("OnHourPassed", HandleHourlyUpdate);
        temperature = baseTemperature;
        ResetMaterial();
        UpdateWeatherText();
    }

    void HandleHourlyUpdate(object[] args)
    {
        if (playerController == null || playerStats == null) return;

        bool isSheltered = playerController.IsUnderShelter();

        if (!isSheltered)
        {
            ApplyWeatherEffectsToPlayer();
        }

        if (eventDuration > 0)
        {
            eventDuration--;

            if (eventDuration == 0)
            {
                currentWeather = WeatherType.None;
                Debug.Log("[WeatherManager] Weather event ended.");
                targetIntensity = 0f;
                if (AcidRainParticles != null) AcidRainParticles.Stop();
            }
        }
        else
        {
            TryStartWeatherEvent();
        }

        AdjustTemperature();
        UpdateShaderEffect();
        UpdateWeatherText();
    }

    void ApplyWeatherEffectsToPlayer()
    {
        if (playerStats == null) return;

        float damageAmount = 0f;

        if (currentWeather == WeatherType.AcidRain)
        {
            damageAmount = Random.Range(5f, 10f);
            Debug.Log($"[WeatherManager] Acid Rain Damage: {damageAmount}");
        }
        else
        {
            if (temperature <= -30f)
            {
                damageAmount = Random.Range(1f, 3f);
                playerStats.DecreaseStat(PlayerStats.StatType.Stamina, 2f);
            }
            else if (temperature >= 70f)
            {
                damageAmount = Random.Range(1f, 3f);
                playerStats.DecreaseStat(PlayerStats.StatType.Water, 2f);
            }
        }

        if (damageAmount > 0)
        {
            playerStats.DecreaseStat(PlayerStats.StatType.Health, damageAmount);
        }
    }

    void TryStartWeatherEvent()
    {
        if (Random.value < 0.3f)
        {
            int currentHour = FindObjectOfType<TimeManager>().hours;

            if (currentHour >= 6 && currentHour < 18)
            {
                currentWeather = (Random.value > 0.5f) ? WeatherType.Heatwave : WeatherType.Sandstorm;
            }
            else
            {
                currentWeather = (Random.value > 0.5f) ? WeatherType.Blizzard : WeatherType.Snowstorm;
            }

            if (Random.value < 0.2f)
            {
                currentWeather = WeatherType.AcidRain;
            }

            eventDuration = Random.Range(10, 30);
            Debug.Log($"[WeatherManager] New Weather Event: {currentWeather}, Duration: {eventDuration} hours");
            UpdateWeatherEffects();
        }
    }

    void AdjustTemperature()
    {
        if (playerController != null && playerController.IsUnderShelter())
        {
            temperature = Mathf.MoveTowards(temperature, Random.Range(40f, 50f), 500f * Time.deltaTime);
            return;
        }

        float targetTemperature = baseTemperature;

        if (eventDuration > 0)
        {
            switch (currentWeather)
            {
                case WeatherType.Blizzard:
                    targetTemperature = Random.Range(-150f, -200f);
                    break;
                case WeatherType.Snowstorm:
                    targetTemperature = Random.Range(-50f, -100f);
                    break;
                case WeatherType.Heatwave:
                    targetTemperature = Random.Range(300f, 500f);
                    break;
                case WeatherType.Sandstorm:
                    targetTemperature = Random.Range(100f, 250f);
                    break;
                case WeatherType.AcidRain:
                    targetTemperature = Random.Range(10f, 35f);
                    break;
            }
        }

        float changeSpeed = (eventDuration > 0) ? 500f : 50f;
        temperature = Mathf.MoveTowards(temperature, targetTemperature, changeSpeed * Time.deltaTime);
    }

    void UpdateWeatherEffects()
    {
        if (FullScreenFeature == null || playerController == null) return;

        bool isSheltered = playerController.IsUnderShelter();

        if (isSheltered)
        {
            Debug.Log("[WeatherManager] Disabling weather effects because player is under shelter.");
            targetIntensity = 0f;
            if (AcidRainParticles != null) AcidRainParticles.Stop();
            FullScreenFeature.passMaterial = null;
            return;
        }

        Debug.Log($"[WeatherManager] Applying Weather Effect: {currentWeather}");

        switch (currentWeather)
        {
            case WeatherType.Blizzard:
                activeMaterial = BlizzardMaterial;
                targetIntensity = 4f;
                break;
            case WeatherType.Snowstorm:
                activeMaterial = SnowstormMaterial;
                targetIntensity = 2f;
                break;
            case WeatherType.Sandstorm:
                activeMaterial = SandstormMaterial;
                targetIntensity = 3f;
                break;
            case WeatherType.Heatwave:
                activeMaterial = HeatwaveMaterial;
                targetIntensity = 3.5f;
                break;
            case WeatherType.AcidRain:
                activeMaterial = AcidRainMaterial;
                targetIntensity = 2f;
                if (AcidRainParticles != null) AcidRainParticles.Play();
                break;
            default:
                targetIntensity = 0f;
                if (AcidRainParticles != null) AcidRainParticles.Stop();
                break;
        }

        FullScreenFeature.passMaterial = activeMaterial;
    }

    void UpdateShaderEffect()
    {
        if (activeMaterial == null) return;

        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 5f);
        activeMaterial.SetFloat("_VignetteIntensity", currentIntensity);

        if (Mathf.Approximately(targetIntensity, 0f))
        {
            Debug.Log("[WeatherManager] Removing weather effect completely.");
            FullScreenFeature.passMaterial = null;
            activeMaterial = null;
        }
    }

    void ResetMaterial()
    {
        if (BlizzardMaterial != null) BlizzardMaterial.SetFloat("_VignetteIntensity", 0f);
        if (SnowstormMaterial != null) SnowstormMaterial.SetFloat("_VignetteIntensity", 0f);
        if (SandstormMaterial != null) SandstormMaterial.SetFloat("_VignetteIntensity", 0f);
        if (HeatwaveMaterial != null) HeatwaveMaterial.SetFloat("_VignetteIntensity", 0f);
        if (AcidRainMaterial != null) AcidRainMaterial.SetFloat("_VignetteIntensity", 0f);
    }

    void UpdateWeatherText()
    {
        if (WeatherText == null) return; // Prevent errors

        string weatherStatus = currentWeather == WeatherType.None ? "Clear Skies" : currentWeather.ToString();
        if (currentWeather == WeatherType.AcidRain) { weatherStatus = "Acid Rain"; }
        WeatherText.text = $"{weatherStatus} {temperature:F1}°C";
    }
}