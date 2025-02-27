using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.TextCore.Text;
using Cinemachine;

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

    public float _health, _stamina, _oxygen, _water;

    [Header("Drain Rates")]
    public float oxygenDrainRate = 1f;
    public float waterDrainRate = 0.5f;

    [Header("Damage Overlay")]
    public Image overlay;
    public float overlayDuration = 0.5f;
    public float fadeSpeed = 2f;
    private float overlayTimer;

    [Header("Respawn System")]
    [SerializeField] private GameObject ragdoll;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Transform respawnPoint;
    private bool isDead = false;

    [SerializeField] private CinemachineVirtualCamera playerCamera;
    private CinemachineVirtualCamera ragdollCamera;

    [SerializeField] private GameObject _backgroundPanel;
    [SerializeField] private GameObject _vitalsPanel;
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private GameObject _mapPanel;
    [SerializeField] private GameObject _hotbar;
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _inventoryIcon;
    [SerializeField] private GameObject _inventoryText;

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
    }

    void Start()
    {
        overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 0);

        StartCoroutine(DrainOxygen());
        StartCoroutine(DrainWater());
    }

    void Update()
    {
        if (isDead && Input.GetMouseButtonDown(0))
        {
            Respawn();
        }

        if (_health <= 0 && !isDead)
        {
            Die();
        }

        _health = Mathf.Clamp(_health, 0, maxHealth);
        _stamina = Mathf.Clamp(_stamina, 0, maxStamina);
        _oxygen = Mathf.Clamp(_oxygen, 0, maxOxygen);
        _water = Mathf.Clamp(_water, 0, maxWater);

        UpdateStatUI(StatType.Health, _health, maxHealth, healthBarFront, healthBarBack, healthText);
        UpdateStatUI(StatType.Stamina, _stamina, maxStamina, staminaBarFront, staminaBarBack, staminaText);
        UpdateStatUI(StatType.Oxygen, _oxygen, maxOxygen, oxygenBarFront, oxygenBarBack, oxygenText);
        UpdateStatUI(StatType.Water, _water, maxWater, waterBarFront, waterBarBack, waterText);

        if (overlay.color.a > 0)
        {
            if (_health < 30) return;
            overlayTimer += Time.deltaTime;
            if (overlayTimer > overlayDuration)
            {
                float tempAlpha = overlay.color.a;
                tempAlpha -= Time.deltaTime * fadeSpeed;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, tempAlpha);
            }
        }
    }

    private void Die()
    {
        isDead = true;
        playerModel.SetActive(false);

        Destroy(Instantiate(ragdoll, transform.position, Quaternion.identity), 3f);

        ragdollCamera = ragdoll.GetComponentInChildren<CinemachineVirtualCamera>();

        if (ragdollCamera != null)
        {
            ragdollCamera.Priority = 11;
        }

        playerCamera.Priority = 9;

        _backgroundPanel.SetActive(false);
        _vitalsPanel.SetActive(false);
        _infoPanel.SetActive(false);
        _mapPanel.SetActive(false);
        _hotbar.SetActive(false);
        _crosshair.SetActive(false);
        _inventoryIcon.SetActive(false);
        _inventoryText.SetActive(false);

        BannerManager.Instance.ShowBanner("You Died");

        if (TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.enabled = false;
        }
    }

    private void Respawn()
    {
        isDead = false;
        playerModel.SetActive(true);

        _backgroundPanel.SetActive(true);
        _vitalsPanel.SetActive(true);
        _infoPanel.SetActive(true);
        _mapPanel.SetActive(true);
        _hotbar.SetActive(true);
        _crosshair.SetActive(true);
        _inventoryIcon.SetActive(true);
        _inventoryText.SetActive(true);

        _health = maxHealth;
        _stamina = maxStamina;
        _oxygen = maxOxygen;
        _water = maxWater;

        transform.position = new Vector3(respawnPoint.position.x, respawnPoint.position.y + 2, respawnPoint.position.z);

        playerCamera.Priority = 11;

        if (ragdollCamera != null)
        {
            ragdollCamera.Priority = 5;
        }


        if (TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.enabled = true;
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
            case StatType.Health:
                _health -= amount;

                overlayTimer = 0f;
                overlay.color = new Color(overlay.color.r, overlay.color.g, overlay.color.b, 1);

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