using System.Collections.Generic;
using UnityEngine;
using Player.Inventory;

public class PickupSystem : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2.0f;
    [SerializeField] private LayerMask pickupLayer;

    private PlayerInventory _inventory;

    void Start()
    {
        _inventory = GetComponent<PlayerInventory>(); // Get the inventory reference
    }

    void Update()
    {
        DetectNearbyItems();

        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickupItem();
        }
    }

    private void DetectNearbyItems()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("PickupItem"))
            {
                // You can highlight or show UI indicator if needed
                Debug.Log($"Nearby item: {hit.gameObject.name}");
            }
        }
    }

    private void TryPickupItem()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, pickupLayer);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("PickupItem"))
            {
                _inventory.AddItem(hit.gameObject);
                Destroy(hit.gameObject); // Remove from world after pickup
                Debug.Log($"Picked up {hit.gameObject.name}");
                break; // Pick up only one item per press
            }
        }
    }
}