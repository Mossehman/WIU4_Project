using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public enum StatType { Health, Stamina, Oxygen, Water }

    private float _lerpTimer;

    [Header("Stat Bars")]
    public float maxHealth = 125f;
    public float maxStamina = 100f;
    public float maxOxygen = 100f;
    public float maxWater = 100f;
    public float chipSpeed = 2f;

    public Image healthBarFront, healthBarBack;
    public Image staminaBarFront, staminaBarBack;
    public Image oxygenBarFront, oxygenBarBack;
    public Image waterBarFront, waterBarBack;

    public TextMeshProUGUI healthText;
    public TextMeshProUGUI staminaText;
    public TextMeshProUGUI oxygenText;
    public TextMeshProUGUI waterText;

    [SerializeField] private float _health, _stamina, _oxygen, _water;

    [Header("Drain Rates")]
    public float oxygenDrainRate = 1f;
    public float waterDrainRate = 0.5f;

    [Header("Damage Overlay")]
    public Image overlay;
    public float overlayDuration = 0.5f;
    public float fadeSpeed = 2f;
    private float _durationTimer;

    [Header("Player Death & Respawn")]
    public Transform respawnPoint;
    public float respawnTime = 5f;
    private bool isDead = false;

    private Rigidbody[] ragdollRigidbodies;
    private Animator playerAnimator;
    private PlayerController playerController;

    void Awake()
    {
        // Singleton (no DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        // Get references for ragdoll system
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();

        DisableRagdoll();
    }

    void Start()
    {
        LoadStats(); // Load stats when the scene starts
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);

        StartCoroutine(DrainOxygen());
        StartCoroutine(DrainWater());
    }

    void Update()
    {
        if (isDead) return;

        _health = Mathf.Clamp(_health, 0, maxHealth);
        _stamina = Mathf.Clamp(_stamina, 0, maxStamina);
        _oxygen = Mathf.Clamp(_oxygen, 0, maxOxygen);
        _water = Mathf.Clamp(_water, 0, maxWater);

        UpdateStatUI(StatType.Health, _health, maxHealth, healthBarFront, healthBarBack, healthText);
        UpdateStatUI(StatType.Stamina, _stamina, maxStamina, staminaBarFront, staminaBarBack, staminaText);
        UpdateStatUI(StatType.Oxygen, _oxygen, maxOxygen, oxygenBarFront, oxygenBarBack, oxygenText);
        UpdateStatUI(StatType.Water, _water, maxWater, waterBarFront, waterBarBack, waterText);

        if (_health <= 0 && !isDead)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        isDead = true;
        EnableRagdoll();
        Debug.Log("[PlayerStats] Player has died. Respawning in " + respawnTime + " seconds...");

        // Wait a few seconds, then respawn
        StartCoroutine(RespawnPlayer());
    }

    private IEnumerator RespawnPlayer()
    {
        yield return new WaitForSeconds(respawnTime);

        DisableRagdoll();
        ResetStats();
        transform.position = respawnPoint.position;
        isDead = false;
    }

    private void EnableRagdoll()
    {
        playerAnimator.enabled = false; // Disable normal movement animation
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = false; // Enable ragdoll
        }
        if (playerController != null)
        {
            playerController.enabled = false; // Disable player controls
        }
    }

    private void DisableRagdoll()
    {
        playerAnimator.enabled = true; // Re-enable normal animation
        foreach (var rb in ragdollRigidbodies)
        {
            rb.isKinematic = true; // Disable ragdoll physics
        }
        if (playerController != null)
        {
            playerController.enabled = true; // Re-enable controls
        }
    }

    private void ResetStats()
    {
        _health = maxHealth;
        _stamina = maxStamina;
        _oxygen = maxOxygen;
        _water = maxWater;
        SaveStats();
    }

    private void UpdateStatUI(StatType type, float value, float maxValue, Image frontBar, Image backBar, TextMeshProUGUI text)
    {
        float fillF = frontBar.fillAmount;
        float fillB = backBar.fillAmount;
        float hFraction = value / maxValue;

        if (fillB > hFraction)
        {
            frontBar.fillAmount = hFraction;
            backBar.color = Color.red;
            _lerpTimer += Time.deltaTime;
            backBar.fillAmount = Mathf.Lerp(fillB, hFraction, _lerpTimer / chipSpeed);
        }

        if (fillF < hFraction)
        {
            backBar.color = Color.green;
            backBar.fillAmount = hFraction;
            _lerpTimer += Time.deltaTime;
            frontBar.fillAmount = Mathf.Lerp(fillF, backBar.fillAmount, _lerpTimer / chipSpeed);
        }

        switch (type)
        {
            case StatType.Health:
                text.text = Mathf.Ceil(value) + " HP";
                break;
            case StatType.Stamina:
                text.text = Mathf.Ceil(value) + " SP";
                break;
            case StatType.Oxygen:
                text.text = Mathf.Ceil(value) + "ml O2";
                break;
            case StatType.Water:
                text.text = Mathf.Ceil(value) + "ml H2";
                break;
        }
    }

    public void DecreaseStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Health: _health -= amount; break;
            case StatType.Stamina: _stamina -= amount; break;
            case StatType.Oxygen: _oxygen -= amount; break;
            case StatType.Water: _water -= amount; break;
        }
        SaveStats(); // Save the updated values
    }

    public void IncreaseStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Health: _health += amount; break;
            case StatType.Stamina: _stamina += amount; break;
            case StatType.Oxygen: _oxygen += amount; break;
            case StatType.Water: _water += amount; break;
        }
        SaveStats(); // Save the updated values
    }

    private void SaveStats()
    {
        PlayerPrefs.SetFloat("Health", _health);
        PlayerPrefs.SetFloat("Stamina", _stamina);
        PlayerPrefs.SetFloat("Oxygen", _oxygen);
        PlayerPrefs.SetFloat("Water", _water);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        _health = PlayerPrefs.GetFloat("Health", maxHealth);
        _stamina = PlayerPrefs.GetFloat("Stamina", maxStamina);
        _oxygen = PlayerPrefs.GetFloat("Oxygen", maxOxygen);
        _water = PlayerPrefs.GetFloat("Water", maxWater);
    }

    private IEnumerator DrainOxygen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            DecreaseStat(StatType.Oxygen, oxygenDrainRate);
        }
    }

    private IEnumerator DrainWater()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            DecreaseStat(StatType.Water, waterDrainRate);
        }
    }
}