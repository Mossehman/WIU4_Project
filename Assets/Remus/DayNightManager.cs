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
        float adjustedMinutes = totalMinutes - (8 * 60); // Shift time so 8 AM starts at 0
        float dayProgress = adjustedMinutes / 1440f; // Normalize time to 0-1 (full day cycle)

        // Ensure we stay in a 0-360° range while keeping 8 AM as the start
        return (Mathf.Lerp(180f, 540f, dayProgress) % 360f);
    }
}