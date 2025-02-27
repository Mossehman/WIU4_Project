using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainScanner : MonoBehaviour
{
    public static bool isHoldingScanner = false;

    [Header("VFX Scanner")]
    public GameObject TerrainScannerPrefab; // VFX prefab
    public float vfxDuration = 3f;
    public float vfxSize = 500f;

    public float scanCooldown = 5.0f;
    private float scanTimer = 0.0f;

    [Header("SFX Scanner")]
    public string sfxname;

    [Header("Raycast Scanner")]
    public float scanDistance = 10f; // Max scan range
    public LayerMask scannableLayer; // Layer mask for scannable objects

    [Header("UI Setup")]
    public GameObject scanPanelPrefab; // UI Panel prefab
    public Transform worldCanvas; // World Space Canvas parent
    private Coroutine hideCoroutine = null;

    private List<Transform> currentTargets = new List<Transform>();
    private List<GameObject> activePanels = new List<GameObject>();
    private List<float> offsets = new List<float>();

    private Camera playerCamera;
    public float panelDuration = 3f; // Time before panel disappears

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Q) && isHoldingScanner && scanTimer <= 0)
        {
            scanTimer = scanCooldown;
            AudioEventSystem.PlaySound(sfxname, default, default, transform.position, true);
            SpawnTerrainScannerVFX();
            StartCoroutine(DelayedScan());
        }

        // If panel is active, make it track the player's view
        for (int i = 0; i < activePanels.Count; i++)
        {
            if (activePanels[i] != null && currentTargets[i] != null)
            {
                activePanels[i].transform.position = currentTargets[i].position + new Vector3(0, offsets[i], 0);
                activePanels[i].transform.LookAt(playerCamera.transform);
                activePanels[i].transform.Rotate(0, 180, 0); // Flip for correct readability
            }
        }
    }

    void SpawnTerrainScannerVFX()
    {
        GameObject terrainScanner = Instantiate(TerrainScannerPrefab, transform.position, Quaternion.identity);
        ParticleSystem terrainScannerPS = terrainScanner.transform.GetChild(0).GetComponent<ParticleSystem>();

        if (terrainScannerPS != null)
        {
            var main = terrainScannerPS.main;
            main.startLifetime = vfxDuration;
            main.startSize = vfxSize;
        }
        else
        {
            Debug.LogWarning("The first child doesn't have a Particle System.");
        }

        Destroy(terrainScanner, vfxDuration + 1);
    }

    IEnumerator DelayedScan()
    {
        yield return new WaitForSeconds(1f); // Delay for VFX effect
        ScanForObject();
    }

    void ScanForObject()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, scanDistance);

        foreach (var col in colliders) {
            if (col.TryGetComponent(out ScannableObject scanData))
            {
                ShowScanPanel(col.transform, scanData);
            }
        }
    }

    void ShowScanPanel(Transform target, ScannableObject scanData)
    {
        GameObject currentPanel = Instantiate(scanPanelPrefab, worldCanvas);


        // Set the target object and update position
        currentTargets.Add(target);
        activePanels.Add(currentPanel);
        offsets.Add(scanData.yOffset);

        TMP_Text titleText = currentPanel.transform.Find("TitleText").GetComponent<TMP_Text>();
        TMP_Text descText = currentPanel.transform.Find("DescText").GetComponent<TMP_Text>();
        Image iconImage = currentPanel.transform.Find("Holder/Icon").GetComponent<Image>();

        iconImage.sprite = scanData.sprite;
        currentPanel.transform.localScale = new Vector3(3, 3, 1);
        if (titleText != null)
        {
            titleText.text = scanData.displayName;
        }

        if (descText != null)
        {
            descText.text = scanData.description;
        }

        // Restart the hide timer
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }
        hideCoroutine = StartCoroutine(HidePanelAfterTime(panelDuration));
    }

    IEnumerator HidePanelAfterTime(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClearPanel();
        }

    void ClearPanel()
    {
        for (int i = 0; i < activePanels.Count; i++)
        {
            if (activePanels[i] != null)
            {
                Destroy(activePanels[i]);
            }
        }
        offsets.Clear();
        activePanels.Clear();
        currentTargets.Clear();
    }
}