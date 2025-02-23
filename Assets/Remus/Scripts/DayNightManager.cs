using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light sun; // Directional light for the sun
    public float dayLengthInSeconds = 180f; // Full 24-hour cycle in real-time seconds
    public float startTime = 8f; // 8 AM as the starting time
    public float currentTime; // Current time of day

    void Start()
    {
        // Initialize time to the starting time (8 AM)
        currentTime = startTime;
    }

    void Update()
    {
        // Rotate the sun naturally over time
        sun.transform.Rotate(Vector3.right * (360f / dayLengthInSeconds) * Time.deltaTime);

        // Convert sun rotation to time of day (0° = 8 AM, 360° = next 8 AM)
        float normalizedRotation = sun.transform.eulerAngles.x;
        currentTime = (normalizedRotation / 360f) * 24f + startTime;

        // Keep time within 0-24 hour range
        if (currentTime >= 24f) currentTime -= 24f;

        // Debug: Show time of day in console
        Debug.Log($"Time: {FormatTime(currentTime)}");
    }

    string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.FloorToInt((time - hours) * 60);
        string amPm = (hours >= 12) ? "PM" : "AM";

        if (hours > 12) hours -= 12;
        if (hours == 0) hours = 12;

        return $"{hours:D2}:{minutes:D2} {amPm}";
    }
}