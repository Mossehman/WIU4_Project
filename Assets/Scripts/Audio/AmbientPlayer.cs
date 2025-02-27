using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientPlayer : MonoBehaviour
{
    public string currentambience;
    [Range(0f, 1f)]
    public float volume = 1f;
    WeatherManager weatherManager;
    private void Start()
    {
        weatherManager = FindObjectOfType<WeatherManager>();
    }
    void Update()
    {
        // This code sucks
        if (TimeManager.Instance != null && AudioManager.Instance != null && PlayerStats.Instance != null && weatherManager != null)
        {
            bool cave = PlayerStats.Instance.transform.position.y <= -2f; 
            bool snow = weatherManager.currentWeather == WeatherManager.WeatherType.Snowstorm || weatherManager.currentWeather == WeatherManager.WeatherType.Sandstorm || weatherManager.currentWeather == WeatherManager.WeatherType.Blizzard && !cave; 
            bool rain = weatherManager.currentWeather == WeatherManager.WeatherType.AcidRain && !cave; 
            bool desert = TimeManager.Instance.IsWithinCurrentTimePeriod(TimeOfTheDay.Morning, TimeOfTheDay.Afternoon) && !cave && !snow && !rain;
            bool night = TimeManager.Instance.IsWithinCurrentTimePeriod(TimeOfTheDay.Night, TimeOfTheDay.Midnight) && !cave && !snow && !rain;
            if (cave && currentambience != nameof(cave))
            {
                currentambience = nameof(cave);
                AudioEventSystem.PlayAmbience(currentambience, volume);
            }
            else if (snow && currentambience != nameof(snow))
            {
                currentambience = nameof(snow);
                AudioEventSystem.PlayAmbience(currentambience, volume);
            }
            else if (rain && currentambience != nameof(rain))
            {
                currentambience = nameof(rain);
                AudioEventSystem.PlayAmbience(currentambience, volume);
            }
            else if (desert && currentambience != nameof(desert))
            {
                currentambience = nameof(desert);
                AudioEventSystem.PlayAmbience(currentambience, volume);
            }
            else if (night && currentambience != nameof(night))
            {
                currentambience = nameof(night);
                AudioEventSystem.PlayAmbience(currentambience, volume);
            }

        }
        
    }
}