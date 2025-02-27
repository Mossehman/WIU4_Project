using TMPro;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    static public TimeManager Instance { get; private set; }

    public int hours = 8; // Start at 08:00 AM
    public int minutes = 0;
    public int days = 1;
    public float secondsPerHour = 7.5f;

    private float timeAccumulator = 0f;
    private float hourAccumulator = 0;

    [SerializeField] private TextMeshProUGUI dayTimeText;

    public TimeOfTheDay timeOfTheDay;

    [SerializeField] private Material volumetricFogMaterial;
    private float targetFogDensity = 0.02f;
    private float fogTransitionSpeed = 1f;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
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

        UpdateFogDensity();
    }

    void UpdateTime()
    {
        minutes++;

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

        if (timeOfTheDay == TimeOfTheDay.Night || timeOfTheDay == TimeOfTheDay.Midnight)
        {
            targetFogDensity = 0f; // No fog at night
        }
        else if (timeOfTheDay == TimeOfTheDay.Morning)
        {
            targetFogDensity = Mathf.Lerp(0f, 0.02f, (hours - 6f) / 6f); // Morning transition (6 AM to 12 PM)
        }
        else
        {
            targetFogDensity = 0.02f;
        }
    }

    void UpdateFogDensity()
    {
        if (volumetricFogMaterial == null) return;

        float currentDensity = volumetricFogMaterial.GetFloat("_DensityMultiplier");
        float newDensity = Mathf.Lerp(currentDensity, targetFogDensity, Time.deltaTime * fogTransitionSpeed);
        volumetricFogMaterial.SetFloat("_DensityMultiplier", newDensity);
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