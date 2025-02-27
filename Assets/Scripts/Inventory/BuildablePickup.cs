using Player.Inventory;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerInventory))]
public class BuildablePickup : MonoBehaviour
{
    PlayerInventory inventory;

    public Sprite handSprite;
    public Transform worldCanvas;

    public GameObject worldPanel;
    private GameObject worldPickupText;
    public LayerMask pickupItemsLayer;

    public float pickupRange = 3.0f;
    private GameObject itemToPickup;

    TMP_Text titleText;

    void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        worldPickupText = Instantiate(worldPanel, worldCanvas);
        Image iconImage = worldPickupText.transform.Find("Holder/Icon").GetComponent<Image>();
        TMP_Text descText = worldPickupText.transform.Find("DescText").GetComponent<TMP_Text>();
        iconImage.sprite = handSprite;
        descText.SetText("Press F to pick up");
        titleText = worldPickupText.transform.Find("TitleText").GetComponent<TMP_Text>();

        worldPickupText.SetActive(false);
    }

    private void Update()
    {
        Collider[] itemsInRange = Physics.OverlapSphere(transform.position, pickupRange, pickupItemsLayer);

        if (itemsInRange.Length == 0 ) { 
            itemToPickup = null; 
            worldPickupText.SetActive(false); 
            return; 
        }

        float nearestItemDistance = float.MaxValue;
        foreach (var col in itemsInRange)
        {
            float distanceSq = Vector3.SqrMagnitude(col.transform.position - transform.position);
            if (distanceSq < nearestItemDistance)
            {
                itemToPickup = col.gameObject;
                nearestItemDistance = distanceSq;
            }

            Debug.Log("Found Item!!");
        }

        worldPickupText.SetActive(true);
        worldPickupText.transform.position = itemToPickup.transform.position + new Vector3(0, 2.0f, 0);
        worldPickupText.transform.LookAt(Camera.main.transform);
        worldPickupText.transform.Rotate(0, 180, 0);
        titleText.SetText(itemToPickup.GetComponent<PlaceableObjectScript>().item.getDisplayName());
        HandlePickup();
    }


    void HandlePickup()
    {
        if (Input.GetKeyDown(KeyCode.F) && itemToPickup != null)
        {
            inventory.AddItem(itemToPickup.GetComponent<PlaceableObjectScript>().item);
            Destroy(itemToPickup);
            itemToPickup = null;
        }
    }

}
