using Player.Inventory;
using UnityEngine;

/// <summary>
/// This is a class for the 3D model gameObject of our Item. This will simplify certain operations like picking up/dropping items.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ItemModelScript : MonoBehaviour
{
    [SerializeField] /// TODO: Remove SerializeField once system has been verified to work
    private BaseItem itemData = null; // Stores a reference to our base item, that way we can instantiate it back into our list/inventory when picked up
    private BaseItem item = null; // Stores a reference to our base item, that way we can instantiate it back into our list/inventory when picked up

    public bool isDropped = true;

    [HideInInspector] public Rigidbody modelRB = null;    // Stores a reference to our Rigidbody for when we drop the item
    [HideInInspector] public Collider modelCollider = null;    // Stores a reference to our Collider for when we drop the item

    private void Awake()
    {
        modelRB = GetComponent<Rigidbody>();
        modelCollider = GetComponent<Collider>();
    }

    private void Start()
    {
        if (itemData != null)
        {
            BaseItem itemInstance = Instantiate(itemData);
            itemInstance.Init();
            item = itemInstance;
        }
    }

    /// <summary>
    /// Method for dropping our item from our inventory/item list
    /// </summary>
    /// <param name="item">The item data to copy into the MonoBehaviour for pickup later</param>
    /// <param name="dropDirection">The direction our item should drop towards</param>
    /// <param name="dropForce">The force our item should be dropped/thrown with</param>
    public void OnDropItem(BaseItem item, Vector3 dropDirection, float dropForce = 1.0f)
    {
        if (!item) { return; }
        this.item = item;
        modelRB.isKinematic = false;
        modelCollider.isTrigger = false;
        isDropped = true;

        modelRB.AddForce(dropDirection * dropForce);
    }

    /// <summary>
    /// Method for dropping our item from our inventory/item list
    /// </summary>
    /// <param name="item">The item data to copy into the MonoBehaviour for pickup later</param>
    public void OnDropItem(BaseItem item)
    {
        if (!item) { return; }
        this.item = item;
        modelRB.isKinematic = false;
        modelCollider.isTrigger = false;
        isDropped = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDropped) { return; }

        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory != null && item != null)
        {
            playerInventory.AddItem(item);
            Destroy(gameObject); // Remove the 3D model after adding it to inventory
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (!isDropped) { return; }
    //
    //    PlayerInventory playerInventory = collision.gameObject.GetComponent<PlayerInventory>();
    //    if (playerInventory != null && item != null)
    //    {
    //        playerInventory.AddItem(item);
    //        Destroy(gameObject); // Remove the 3D model after adding it to inventory
    //    }
    //}

    public BaseItem getSO()
    {
        return item;
    }
}
