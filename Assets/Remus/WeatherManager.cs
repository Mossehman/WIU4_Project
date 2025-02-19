using TMPro;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType { None, Blizzard, Snowstorm, AcidRain, Heatwave, Sandstorm }
    public WeatherType currentWeather = WeatherType.None;

    public float baseTemperature = 45f; // Normal temperature range (40-50°C)
    public float temperature; // Current temperature
    public float temperatureChangeSpeed = 0.05f; // Lower value for gradual transitions
    public int eventDuration = 0; // Duration of weather events

    [SerializeField] private TextMeshProUGUI WeatherText; // Assign in Inspector

    void Start()
    {
        EventManager.Connect("OnHourPassed", HandleHourlyUpdate);
        temperature = baseTemperature; // Set initial temperature

        AdjustTemperature();
        UpdateWeatherText(); // Update UI
    }

    void HandleHourlyUpdate(object[] args)
    {
        if (eventDuration > 0)
        {
            eventDuration--; // Reduce event duration each hour
            if (eventDuration == 0)
            {
                currentWeather = WeatherType.None; // Clear weather event
                Debug.Log("[WeatherManager] Weather event ended.");
            }
        }
        else
        {
            TryStartWeatherEvent();
        }

        AdjustTemperature();
        UpdateWeatherText(); // Update UI
    }

    void TryStartWeatherEvent()
    {
        if (Random.value < 0.3f) // 30% chance to start a new weather event
        {
            int currentHour = FindObjectOfType<TimeManager>().hours;

            if (currentHour >= 6 && currentHour < 18) // Daytime events
            {
                currentWeather = (Random.value > 0.5f) ? WeatherType.Heatwave : WeatherType.Sandstorm;
            }
            else // Nighttime events
            {
                currentWeather = (Random.value > 0.5f) ? WeatherType.Blizzard : WeatherType.Snowstorm;
            }

            if (Random.value < 0.2f) // 20% chance to override with Acid Rain
            {
                currentWeather = WeatherType.AcidRain;
            }

            eventDuration = Random.Range(10, 30); // Event lasts 10-30 in-game hours
            Debug.Log($"[WeatherManager] New Weather Event: {currentWeather}, Duration: {eventDuration} hours");
        }
    }

    void AdjustTemperature()
    {
        float targetTemperature = baseTemperature;

        switch (currentWeather)
        {
            case WeatherType.Blizzard:
                targetTemperature = Random.Range(-150f, -200f); // Blizzard can go as low as -200°C
                break;
            case WeatherType.Snowstorm:
                targetTemperature = Random.Range(-50f, -100f);
                break;
            case WeatherType.Heatwave:
                targetTemperature = Random.Range(300f, 500f); // Heatwave can go up to 500°C
                break;
            case WeatherType.Sandstorm:
                targetTemperature = Random.Range(100f, 250f);
                break;
            case WeatherType.AcidRain:
                targetTemperature = Random.Range(10f, 35f); // Acid Rain is not extreme
                break;
        }

        // Gradually adjust temperature instead of instant jumps
        temperature = Mathf.Lerp(temperature, targetTemperature, temperatureChangeSpeed * Time.deltaTime);
    }

    void UpdateWeatherText()
    {
        string weatherStatus = currentWeather == WeatherType.None ? "Clear Skies" : currentWeather.ToString();
        WeatherText.text = $"{weatherStatus} {temperature:F1}°C";
    }
}