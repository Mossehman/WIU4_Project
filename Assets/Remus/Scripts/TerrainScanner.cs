using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerrainScanner : MonoBehaviour
{
    [Header("VFX Scanner")]
    public GameObject TerrainScannerPrefab; // VFX prefab
    public float vfxDuration = 3f;
    public float vfxSize = 500f;

    [Header("Raycast Scanner")]
    public float scanDistance = 10f; // Max scan range
    public LayerMask scannableLayer; // Layer mask for scannable objects

    [Header("UI Setup")]
    public GameObject scanPanelPrefab; // UI Panel prefab
    public Transform worldCanvas; // World Space Canvas parent
    private GameObject currentPanel = null;
    private Transform currentTarget = null;
    private Coroutine hideCoroutine = null;

    private Camera playerCamera;
    public float panelDuration = 3f; // Time before panel disappears

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SpawnTerrainScannerVFX();
            StartCoroutine(DelayedScan());
        }

        // If panel is active, make it track the player's view
        if (currentPanel != null && currentTarget != null)
        {
            currentPanel.transform.position = currentTarget.position + Vector3.up * 2;
            currentPanel.transform.LookAt(playerCamera.transform);
            currentPanel.transform.Rotate(0, 180, 0); // Flip for correct readability
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
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, scanDistance, scannableLayer))
        {
            if (hit.collider.CompareTag("Scannable"))
            {
                ShowScanPanel(hit.transform);
            }
        }
    }

    void ShowScanPanel(Transform target)
    {
        // If panel already exists, reuse it
        if (currentPanel == null)
        {
            currentPanel = Instantiate(scanPanelPrefab, worldCanvas);
        }

        // Set the target object and update position
        currentTarget = target;

        TMP_Text titleText = currentPanel.transform.Find("TitleText").GetComponent<TMP_Text>();
        TMP_Text descText = currentPanel.transform.Find("DescText").GetComponent<TMP_Text>();

        if (titleText != null)
        {
            titleText.text = target.name;
        }

        if (descText != null)
        {
            descText.text = "• Object";
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
        if (currentPanel != null)
        {
            Destroy(currentPanel);
            currentPanel = null;
        }
        currentTarget = null;
    }
}