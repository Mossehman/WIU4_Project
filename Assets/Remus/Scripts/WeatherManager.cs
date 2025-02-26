using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType { None, Blizzard, Snowstorm, AcidRain, Heatwave, Sandstorm }
    public WeatherType currentWeather = WeatherType.None;

    public float baseTemperature = 45f;
    public float temperature;
    public float temperatureChangeSpeed = 20f;
    public int eventDuration = 0;

    [SerializeField] private TextMeshProUGUI WeatherText; // UI Text
    [SerializeField] private Material BlizzardMaterial;
    [SerializeField] private Material SnowstormMaterial;
    [SerializeField] private Material AcidRainMaterial;
    [SerializeField] private Material HeatwaveMaterial;
    [SerializeField] private Material SandstormMaterial;
    [SerializeField] private FullScreenPassRendererFeature FullScreenFeature; // URP Renderer Feature
    [SerializeField] private ParticleSystem AcidRainParticles; // Acid Rain Particle System

    private float targetIntensity = 0f;
    private float currentIntensity = 0f;
    private Material activeMaterial; // Track current material

    private PlayerController playerController;
    private PlayerStats playerStats;

    void Start()
    {
        playerController = FindObjectOfType<PlayerController>(); // Cache player once
        playerStats = FindObjectOfType<PlayerStats>();

        if (playerController == null)
        {
            Debug.LogError("[WeatherManager] PlayerController not found in the scene!");
        }

        EventManager.Connect("OnHourPassed", HandleHourlyUpdate);
        temperature = baseTemperature;
        ResetMaterial(); // Ensure the effect starts disabled
        UpdateWeatherText();
    }

    void HandleHourlyUpdate(object[] args)
    {
        if (playerController == null) return; // Ensure player is found
        bool isSheltered = playerController.IsUnderShelter();

        // Stop the event if the player is under shelter
        if (isSheltered)
        {
            Debug.Log("[WeatherManager] Player entered shelter. Stopping weather event.");
            currentWeather = WeatherType.None; // Clear weather
            eventDuration = 0; // Forcefully end the event
            targetIntensity = 0f; // Disable effects
            if (AcidRainParticles != null) AcidRainParticles.Stop();
        }
        else
        {
            if (eventDuration > 0)
            {
                eventDuration--;

                ApplyWeatherEffectsToPlayer();

                if (eventDuration == 0)
                {
                    currentWeather = WeatherType.None;
                    Debug.Log("[WeatherManager] Weather event ended.");
                    targetIntensity = 0f; // Fade out effect
                    if (AcidRainParticles != null) AcidRainParticles.Stop();
                }
            }
            else
            {
                TryStartWeatherEvent();
            }
        }

        AdjustTemperature();
        UpdateShaderEffect(); // Gradual fade-in/out effect
        UpdateWeatherText();
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
            return; // Stop further adjustments
        }

        float targetTemperature = baseTemperature; // Default normal temp (40-50°C)

        if (eventDuration > 0) // Weather event is active
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

    void ApplyWeatherEffectsToPlayer()
    {
        if (playerStats == null) return;

        float damageAmount = 0f;

        switch (currentWeather)
        {
            case WeatherType.Blizzard:
                damageAmount = 10f; // Blizzard slowly kills player
                playerStats.DecreaseStat(PlayerStats.StatType.Stamina, 5f); // Lower stamina too
                break;
            case WeatherType.Snowstorm:
                damageAmount = 5f;
                playerStats.DecreaseStat(PlayerStats.StatType.Stamina, 3f);
                break;
            case WeatherType.Heatwave:
                damageAmount = 15f;
                playerStats.DecreaseStat(PlayerStats.StatType.Water, 10f); // Increase dehydration
                break;
            case WeatherType.Sandstorm:
                damageAmount = 7f;
                playerStats.DecreaseStat(PlayerStats.StatType.Stamina, 4f);
                break;
            case WeatherType.AcidRain:
                damageAmount = 20f; // More deadly
                break;
            default:
                return; // No effect
        }

        Debug.Log($"[WeatherManager] Applying {damageAmount} damage due to {currentWeather}");
        playerStats.DecreaseStat(PlayerStats.StatType.Health, damageAmount);
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
            FullScreenFeature.passMaterial = null; // Remove shader effects
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

        // Gradually fade in/out the effect
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * 5f);
        activeMaterial.SetFloat("_VignetteIntensity", currentIntensity);

        // If targetIntensity is 0, fully remove the effect
        if (Mathf.Approximately(targetIntensity, 0f))
        {
            Debug.Log("[WeatherManager] Removing weather effect completely.");
            FullScreenFeature.passMaterial = null; // Remove the effect
            activeMaterial = null; // Reset activeMaterial to avoid lingering effects
        }

        //Debug.Log($"[WeatherManager] Updating Intensity: {currentIntensity}");
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