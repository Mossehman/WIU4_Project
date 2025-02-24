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
    }
}