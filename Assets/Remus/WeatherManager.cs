using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum WeatherType { None, Blizzard, Snowstorm, AcidRain, Heatwave, Sandstorm }
    public WeatherType currentWeather = WeatherType.None;

    public float baseTemperature = 45f; // Default temperature (40-50°C)
    public float temperature; // Current temperature
    public float temperatureChangeSpeed = 0.5f; // How fast temperature changes
    public int eventDuration = 0; // How many in-game hours the event lasts

    void Start()
    {
        EventManager.Connect("OnHourPassed", GachaWeather);
        temperature = baseTemperature; // Start at base temp
    }

    void GachaWeather(object[] args)
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
                targetTemperature -= 20f;
                break;
            case WeatherType.Snowstorm:
                targetTemperature -= 15f;
                break;
            case WeatherType.Heatwave:
                targetTemperature += 15f;
                break;
            case WeatherType.Sandstorm:
                targetTemperature += 10f;
                break;
            case WeatherType.AcidRain:
                targetTemperature -= 5f;
                break;
        }

        // Smooth temperature transition
        temperature = Mathf.Lerp(temperature, targetTemperature, temperatureChangeSpeed * Time.deltaTime);

        Debug.Log(Mathf.FloorToInt(temperature) + "°C");
    }
}