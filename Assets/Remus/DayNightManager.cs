using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun;
    private TimeManager timeManager;

    private float currentSunRotation;
    private float targetSunRotation;

    void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        if (timeManager == null)
        {
            Debug.LogError("TimeManager not found in the scene!");
            return;
        }

        currentSunRotation = CalculateSunRotation(timeManager.hours, timeManager.minutes);
        sun.transform.rotation = Quaternion.Euler(currentSunRotation, 170, 0);
    }

    void Update()
    {
        if (timeManager == null) return;

        targetSunRotation = CalculateSunRotation(timeManager.hours, timeManager.minutes);

        currentSunRotation = Mathf.Lerp(currentSunRotation, targetSunRotation, Time.deltaTime * (1f / timeManager.secondsPerHour));
        sun.transform.rotation = Quaternion.Euler(currentSunRotation, 170, 0);
    }

    float CalculateSunRotation(int hour, int minute)
    {
        float totalMinutes = (hour * 60) + minute; // Convert current time to total minutes
        float dayProgress = totalMinutes / 1440f; // Normalize to 0-1 (full day cycle)

        // Properly map time to sun rotation:
        // - 6 AM = -30° (sunrise)
        // - 6 PM = 180° (sunset)
        // - 12 AM = 270° (night overhead)
        if (hour >= 6 && hour < 18) // Daytime: 6 AM to 6 PM (Sun moves upwards)
        {
            return Mathf.Lerp(-30f, 180f, (dayProgress - (6f / 24f)) * 2f);
        }
        else // Nighttime: 6 PM to 6 AM (Sun moves downwards and under)
        {
            return Mathf.Lerp(180f, 330f, (dayProgress < 0.25f ? dayProgress + 0.75f : dayProgress - 0.25f) * 2f);
        }
    }
}