using UnityEngine;

public class OrbFloating : MonoBehaviour
{
    [Header("References")]
    public Transform player;  // Reference to the player

    [Header("Floating Settings")]
    public float floatSpeed = 1f;  // Speed of floating
    public float floatAmplitude = 0.5f;  // Height of floating

    [Header("Position Settings")]
    public Vector3 offset = new Vector3(1.5f, 1.5f, 0); // Offset position relative to player
    public float followSpeed = 5f; // Speed at which the orb follows the player

    private float floatTimer;

    void Start()
    {
        if (player == null)
        {
            Debug.LogWarning("Player reference not set for Orb!");
            return;
        }
    }

    void Update()
    {
        if (player == null) return;

        // Target position beside the player with floating effect
        Vector3 targetPosition = player.position + offset;
        floatTimer += Time.deltaTime * floatSpeed;
        targetPosition.y += Mathf.Sin(floatTimer) * floatAmplitude;

        // Smoothly move to the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // Make the orb always face the player
        transform.LookAt(player);
    }
}