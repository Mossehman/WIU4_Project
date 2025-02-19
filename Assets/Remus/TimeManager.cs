using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public int hours = 8; // Start at 08:00 AM
    public int minutes = 0;
    public int days = 1;
    public float secondsPerHour = 3f; // Each in-game hour lasts 3 real-world seconds

    private float timeAccumulator = 0f;
    private float hourAccumulator = 0;

    [SerializeField] private TextMeshProUGUI dayText;
    [SerializeField] private TextMeshProUGUI timeText;

    private void Start()
    {
        EventManager.CreateEvent("OnHourPassed");
    }

    void Update()
    {
        timeAccumulator += Time.deltaTime;

        while (timeAccumulator >= (secondsPerHour / 60f)) // Convert secondsPerHour to per-minute update
        {
            timeAccumulator -= (secondsPerHour / 60f);
            UpdateTime();
        }
    }

    void UpdateTime()
    {
        minutes++; // Increase minute by 1

        if (minutes >= 60)
        {
            minutes = 0;
            hours++;

            hourAccumulator++;
            if (hourAccumulator >= 3)
            {
                hourAccumulator = 0;
                EventManager.Fire("OnHourPassed");
            }

            if (hours >= 24)
            {
                hours = 0;
                days++;
            }
        }

        DisplayTime();
    }

    void DisplayTime()
    {
        string period = hours >= 12 ? "PM" : "AM";
        int displayHour = (hours % 12 == 0) ? 12 : (hours % 12);
        //Debug.Log($"Day {days}, Time: {displayHour:D2}:{minutes:D2} {period}");

        dayText.text = $"Day {days}";
        timeText.text = $"{displayHour:D2}:{minutes:D2} {period}";
    }
}