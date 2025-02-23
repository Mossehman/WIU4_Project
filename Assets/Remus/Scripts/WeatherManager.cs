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

    void Start()
    {
        EventManager.Connect("OnHourPassed", HandleHourlyUpdate);
        temperature = baseTemperature;
        ResetMaterial(); // Ensure the effect starts disabled
        UpdateWeatherText();
    }

    void HandleHourlyUpdate(object[] args)
    {
        if (eventDuration > 0)
        {
            eventDuration--;
            if (eventDuration == 0)
            {
                currentWeather = WeatherType.None;
                Debug.Log("[WeatherManager] Weather event ended.");
                targetIntensity = 0f; // Start fading out effect
                if (AcidRainParticles != null) AcidRainParticles.Stop();
            }
        }
        else
        {
            TryStartWeatherEvent();
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
        float targetTemperature = baseTemperature; // Default normal temp (40-50°C)

        if (eventDuration > 0) // Weather event is active
        {
            switch (currentWeather)
            {
                case WeatherType.Blizzard:
                    targetTemperature = Random.Range(-150f, -200f); // Drastic drop
                    break;
                case WeatherType.Snowstorm:
                    targetTemperature = Random.Range(-50f, -100f);
                    break;
                case WeatherType.Heatwave:
                    targetTemperature = Random.Range(300f, 500f); // Drastic increase
                    break;
                case WeatherType.Sandstorm:
                    targetTemperature = Random.Range(100f, 250f);
                    break;
                case WeatherType.AcidRain:
                    targetTemperature = Random.Range(10f, 35f);
                    break;
            }
        }

        // If event ends, smoothly return to normal temp range (40-50°C)
        if (eventDuration == 0)
        {
            targetTemperature = Random.Range(40f, 50f);
        }

        float changeSpeed = (eventDuration > 0) ? 500f : 50f; // Faster change when event happens
        temperature = Mathf.MoveTowards(temperature, targetTemperature, changeSpeed * Time.deltaTime);
    }

    void UpdateWeatherEffects()
    {
        if (FullScreenFeature == null) return;

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
                if (AcidRainParticles != null) AcidRainParticles.Play(); // Start rain particles
                break;

            default:
                targetIntensity = 0f;
                if (AcidRainParticles != null) AcidRainParticles.Stop(); // Stop rain particles
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

        // Debugging output
        Debug.Log($"[WeatherManager] Updating Intensity: {currentIntensity}");
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
        string weatherStatus = currentWeather == WeatherType.None ? "Clear Skies" : currentWeather.ToString();
        if (currentWeather == WeatherType.AcidRain) { weatherStatus = "Acid Rain"; }
        WeatherText.text = $"{weatherStatus} {temperature:F1}°C";
    }

    bool IsPlayerUnderShelter()
    {
        Transform player = GameObject.FindWithTag("Player").transform;
        RaycastHit2D hit = Physics2D.Raycast(player.position, Vector2.up, 10f)
        return hit.collider != null;
    }
}