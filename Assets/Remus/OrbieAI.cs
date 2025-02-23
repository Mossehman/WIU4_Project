using UnityEngine;

public class OrbAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;          // The player to follow
    public Transform cameraTransform; // The player's camera
    public Transform chargingBay;     // The charging station
    public Renderer orbRenderer;      // Orb's material for emissive glow

    [Header("Movement Settings")]
    public Vector3 baseOffset = new Vector3(1.5f, 1.8f, -1f); // Default offset from the player
    public float followSpeed = 5f;      // Speed when following the player
    public float rotationSpeed = 2f;    // How quickly it rotates toward the player
    public float floatSpeed = 2f;       // Speed of floating motion
    public float floatAmplitude = 0.2f; // Range of floating motion
    public float cameraFollowWeight = 0.8f; // Influence of the camera's facing direction

    [Header("Battery Settings")]
    public float maxBattery = 100f;       // Maximum battery level
    public float batteryDrainRate = 5f;   // Battery drain per second
    public float chargeRate = 20f;        // Recharge speed when in charging bay
    public float lowBatteryThreshold = 20f; // Turns red when battery is 20% or lower
    public float mediumBatteryThreshold = 50f; // Turns yellow when battery is below 50%

    private float battery;  // Current battery level
    private float floatTimer;
    private bool isCharging = false;  // Is the orb at the charging bay?
    private Vector3 dynamicOffset; // Adjusted offset considering camera movement

    void Start()
    {
        battery = maxBattery; // Start with full battery
        UpdateOrbEmissiveColor(); // Set initial color
    }

    void Update()
    {
        if (player == null || cameraTransform == null || chargingBay == null) return;

        if (isCharging)
        {
            RechargeBattery();
        }
        else
        {
            battery -= batteryDrainRate * Time.deltaTime; // Battery drains over time
            if (battery <= lowBatteryThreshold)
            {
                GoToChargingBay();
            }
            else
            {
                FollowPlayerWithDynamicOffset();
            }
        }

        UpdateOrbEmissiveColor();
    }

    // Orb Follows the Player, Adjusting Based on Camera
    void FollowPlayerWithDynamicOffset()
    {
        // Compute camera-based influence on position
        Vector3 cameraOffset = cameraTransform.right * cameraFollowWeight; // Shift left/right with the camera

        // Calculate final offset by blending camera influence with the base offset
        dynamicOffset = new Vector3(
            baseOffset.x + cameraOffset.x,
            baseOffset.y,
            baseOffset.z + cameraOffset.z
        );

        // Target position considering player position + dynamic offset
        Vector3 targetPosition = player.position + dynamicOffset;

        // Smooth movement toward the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Rotate to face the player in a natural way
        Vector3 lookDirection = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // Floating animation
        floatTimer += Time.deltaTime * floatSpeed;
        transform.position += new Vector3(0, Mathf.Sin(floatTimer) * floatAmplitude * Time.deltaTime, 0);
    }

    // Orb Returns to Charging Bay
    void GoToChargingBay()
    {
        Vector3 targetPosition = chargingBay.position;

        // Move towards charging station
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Rotate to face the charging station
        Vector3 direction = (chargingBay.position - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
        }

        // Check if orb reached the charging bay
        if (Vector3.Distance(transform.position, chargingBay.position) < 0.5f)
        {
            isCharging = true;
        }
    }

    // Recharge when in Charging Bay
    void RechargeBattery()
    {
        battery += chargeRate * Time.deltaTime;
        if (battery >= maxBattery)
        {
            battery = maxBattery;
            isCharging = false; // Resume following the player
        }
    }

    // Orb Changes Color Based on Battery Level
    void UpdateOrbEmissiveColor()
    {
        if (orbRenderer == null) return;

        Color emissionColor = Color.green; // Default: fully charged (green)

        if (battery < mediumBatteryThreshold && battery > lowBatteryThreshold)
            emissionColor = Color.yellow; // Medium battery (yellow)
        else if (battery <= lowBatteryThreshold)
            emissionColor = Color.red; // Low battery (red)

        // Apply emission color
        orbRenderer.material.SetColor("_EmissionColor", emissionColor * 2f);
        DynamicGI.SetEmissive(orbRenderer, emissionColor);
    }

    // Get Battery Status
    public float GetBatteryLevel()
    {
        return battery;
    }
}