using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    static public TimeManager Instance { get; private set; }

    public int hours = 8; // Start at 08:00 AM
    public int minutes = 0;
    public int days = 1;
    public float secondsPerHour = 7.5f; // Each in-game hour lasts 3 real-world seconds

    private float timeAccumulator = 0f;
    private float hourAccumulator = 0;

    [SerializeField] private TextMeshProUGUI dayTimeText;

    public TimeOfTheDay timeOfTheDay;

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

        UpdateTimeOfDay();
        DisplayTime();
    }

    void DisplayTime()
    {
        string period = hours >= 12 ? "PM" : "AM";
        int displayHour = (hours % 12 == 0) ? 12 : (hours % 12);
        //Debug.Log($"Day {days}, Time: {displayHour:D2}:{minutes:D2} {period}");

        dayTimeText.text = $"DAY {days} {displayHour:D2}:{minutes:D2} {period}";
    }

    public bool IsWithinCurrentTimePeriod(TimeOfTheDay start, TimeOfTheDay end)
    {
        if (start <= end)
            return timeOfTheDay >= start && timeOfTheDay <= end;
        else
            return timeOfTheDay >= start || timeOfTheDay <= end;
    }

    public bool IsWithinCurrentTimePeriod(MinMaxEnum<TimeOfTheDay> range)
    {
        return IsWithinCurrentTimePeriod(range.start, range.end);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        float rad = -((360 * hours / 24f) - 90f) * Mathf.Deg2Rad;
        Gizmos.DrawLine(transform.position, transform.position + (new Vector3(50f * Mathf.Cos(rad), 50f * Mathf.Sin(rad), 0)));
    }

    void UpdateTimeOfDay()
    {
        if (hours >= 0 && hours < 6)
            timeOfTheDay = TimeOfTheDay.Midnight;
        else if (hours >= 6 && hours < 12)
            timeOfTheDay = TimeOfTheDay.Morning;
        else if (hours >= 12 && hours < 18)
            timeOfTheDay = TimeOfTheDay.Afternoon;
        else
            timeOfTheDay = TimeOfTheDay.Night;
    }
}

public enum TimeOfTheDay
{
    Midnight,
    Morning,
    Afternoon,
    Night,
    NumOfPeriods,
}