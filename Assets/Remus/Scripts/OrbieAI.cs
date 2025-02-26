using UnityEngine;
using Cinemachine;

public class OrbAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;          
    public Transform cameraTransform; 
    public Transform chargingBay;     
    public Renderer orbRenderer;      
    public CinemachineVirtualCamera thirdPersonCam; 

    [Header("Movement Settings")]
    public Vector3 baseOffset = new Vector3(1.5f, 1.8f, -1f); 
    public float followSpeed = 5f;      
    public float rotationSpeed = 2f;    
    public float floatSpeed = 2f;       
    public float floatAmplitude = 0.2f; 
    public float cameraFollowWeight = 0.8f; 

    [Header("Battery Settings")]
    public float maxBattery = 100f;       
    public float batteryDrainRate = 5f;   
    public float chargeRate = 20f;        
    public float lowBatteryThreshold = 20f; 
    public float mediumBatteryThreshold = 50f; 

    private float battery;  
    private float floatTimer;
    private bool isCharging = false;  
    private Vector3 dynamicOffset; 
    private bool isThirdPersonMode = false; 

    void Start()
    {
        battery = maxBattery; 
        UpdateOrbEmissiveColor();
    }

    void Update()
    {
        if (player == null || cameraTransform == null || chargingBay == null) return;

        if (isThirdPersonMode)
        {
            FacePlayerFromCamera();
        }
        else if (isCharging)
        {
            RechargeBattery();
        }
        else
        {
            battery -= batteryDrainRate * Time.deltaTime;
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

    void FollowPlayerWithDynamicOffset()
    {
        Vector3 cameraOffset = cameraTransform.forward * cameraFollowWeight;
        Vector3 localOffset = player.transform.localRotation * baseOffset;

        dynamicOffset = new Vector3(
            localOffset.x + cameraOffset.x,
            localOffset.y,
            localOffset.z + cameraOffset.z
        );

        Vector3 targetPosition = player.position + dynamicOffset;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        Vector3 lookDirection = (player.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        floatTimer += Time.deltaTime * floatSpeed;
        transform.position += new Vector3(0, Mathf.Sin(floatTimer) * floatAmplitude * Time.deltaTime, 0);
    }

    void FacePlayerFromCamera()
    {
        if (thirdPersonCam == null) return;

        Vector3 camPosition = thirdPersonCam.transform.position;

        Vector3 newOrbPosition = camPosition + thirdPersonCam.transform.forward * -1.5f;
        newOrbPosition.y = player.position.y + 1.5f;

        transform.position = Vector3.Lerp(transform.position, newOrbPosition, Time.deltaTime * followSpeed);

        Vector3 lookDirection = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);
    }

    void GoToChargingBay()
    {
        Vector3 targetPosition = chargingBay.position;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        Vector3 direction = (chargingBay.position - transform.position).normalized;
        if (direction.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotationSpeed);
        }

        if (Vector3.Distance(transform.position, chargingBay.position) < 0.5f)
        {
            isCharging = true;
        }
    }

    void RechargeBattery()
    {
        battery += chargeRate * Time.deltaTime;
        if (battery >= maxBattery)
        {
            battery = maxBattery;
            isCharging = false;
        }
    }

    void UpdateOrbEmissiveColor()
    {
        if (orbRenderer == null) return;

        Color emissionColor = Color.green; 

        if (battery < mediumBatteryThreshold && battery > lowBatteryThreshold)
            emissionColor = Color.yellow;
        else if (battery <= lowBatteryThreshold)
            emissionColor = Color.red;

        orbRenderer.material.SetColor("_EmissionColor", emissionColor * 2f);
        DynamicGI.SetEmissive(orbRenderer, emissionColor);
    }

    public float GetBatteryLevel()
    {
        return battery;
    }

    public void ActivateThirdPersonMode()
    {
        isThirdPersonMode = true;
    }

    public void DeactivateThirdPersonMode()
    {
        isThirdPersonMode = false;
    }
}