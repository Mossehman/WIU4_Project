using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    // Singleton instance
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

    private float _health, _stamina, _oxygen, _water;

    [Header("Drain Rates")]
    public float oxygenDrainRate = 1f;  // Oxygen loss per second
    public float waterDrainRate = 0.5f; // Water loss per second

    [Header("Damage Overlay")]
    public Image overlay;
    public float overlayDuration = 0.5f;
    public float fadeSpeed = 2f;
    private float _durationTimer;

    //public Transform playerSpawn;

    void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        DontDestroyOnLoad(gameObject); // Keep PlayerStats across scenes
    }

    void Start()
    {
        LoadStats();
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);

        // Start draining oxygen and water over time
        StartCoroutine(DrainOxygen());
        StartCoroutine(DrainWater());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(DemoDrainHealthAndRestore());
            StartCoroutine(DemoDrainStaminaAndRestore());
        }

        if (_health <= 0)
        {
            //HandleDeath();
        }

        _health = Mathf.Clamp(_health, 0, maxHealth);
        _stamina = Mathf.Clamp(_stamina, 0, maxStamina);
        _oxygen = Mathf.Clamp(_oxygen, 0, maxOxygen);
        _water = Mathf.Clamp(_water, 0, maxWater);

        UpdateStatUI(StatType.Health, _health, maxHealth, healthBarFront, healthBarBack, healthText);
        UpdateStatUI(StatType.Stamina, _stamina, maxStamina, staminaBarFront, staminaBarBack, staminaText);
        UpdateStatUI(StatType.Oxygen, _oxygen, maxOxygen, oxygenBarFront, oxygenBarBack, oxygenText);
        UpdateStatUI(StatType.Water, _water, maxWater, waterBarFront, waterBarBack, waterText);

        HandleOverlayEffect();
    }

    //private void HandleDeath()
    //{
    //    ResetStats();
    //    transform.position = new Vector3(playerSpawn.transform.position.x, playerSpawn.transform.position.y + 2, playerSpawn.transform.position.z);
    //    SaveStats();
    //}

    private void HandleOverlayEffect()
    {
        if (overlay.color.a > 0)
        {
            if (_health < 30) return;
            _durationTimer += Time.deltaTime;
            if (_durationTimer > overlayDuration)
            {
                float tempAlpha = overlay.color.a;
                tempAlpha -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, tempAlpha);
            }
        }
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
            float percentComplete = _lerpTimer / chipSpeed;
            backBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }

        if (fillF < hFraction)
        {
            backBar.color = Color.green;
            backBar.fillAmount = hFraction;
            _lerpTimer += Time.deltaTime;
            float percentComplete = _lerpTimer / chipSpeed;
            frontBar.fillAmount = Mathf.Lerp(fillF, backBar.fillAmount, percentComplete);
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
            case StatType.Health:
                ApplyDamage(amount);
                break;
            case StatType.Stamina:
                _stamina -= amount;
                break;
            case StatType.Oxygen:
                _oxygen -= amount;
                break;
            case StatType.Water:
                _water -= amount;
                break;
        }
        _lerpTimer = 0f;
        SaveStats();
    }

    public void IncreaseStat(StatType type, float amount)
    {
        switch (type)
        {
            case StatType.Health:
                _health += amount;
                break;
            case StatType.Stamina:
                _stamina += amount;
                break;
            case StatType.Oxygen:
                _oxygen += amount;
                break;
            case StatType.Water:
                _water += amount;
                break;
        }
        _lerpTimer = 0f;
        SaveStats();
    }

    private void ApplyDamage(float damage)
    {
        _health -= damage;
        _durationTimer = 0;
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("TBC"))
        {
            DecreaseStat(StatType.Health, Random.Range(5, 10));
        }
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

    private void ResetStats()
    {
        _health = maxHealth;
        _stamina = maxStamina;
        _oxygen = maxOxygen;
        _water = maxWater;
        SaveStats();
    }

    // Oxygen drains over time
    private IEnumerator DrainOxygen()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            DecreaseStat(StatType.Oxygen, oxygenDrainRate);
        }
    }

    // Water drains over time
    private IEnumerator DrainWater()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            DecreaseStat(StatType.Water, waterDrainRate);
        }
    }

    // Demo function to drain and restore health
    public IEnumerator DemoDrainHealthAndRestore()
    {
        DecreaseStat(StatType.Health, maxHealth / 2);
        yield return new WaitForSeconds(2);
        IncreaseStat(StatType.Health, maxHealth / 2);
    }

    // Demo function to drain and restore stamina
    public IEnumerator DemoDrainStaminaAndRestore()
    {
        DecreaseStat(StatType.Stamina, maxStamina / 2);
        yield return new WaitForSeconds(2);
        IncreaseStat(StatType.Stamina, maxStamina / 2);
    }
}