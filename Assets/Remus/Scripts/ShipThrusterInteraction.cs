using Player.Inventory;
using UnityEngine;

public class ShipThrusterInteraction : InteractionObject
{
    private PlayerInventory _playerInventory;
    public string requiredItemID = "ID19"; // Item ID for the repaired thruster

    private void Start()
    {
        _playerInventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
    }

    public override string GetCustomDescription()
    {
        int itemCount = _playerInventory.GetItemCount(requiredItemID);
        return $"Repaired Thruster: {itemCount}/1";
    }

    public override void Interact()
    {
        if (_playerInventory.GetItemCount(requiredItemID) > 0)
        {
            Debug.Log("Ship thruster repaired!");
            _playerInventory.RemoveItem(requiredItemID, 1);
            ShipRepairManager.Instance.MarkThrusterRepaired(); // Notify repair manager
        }
        else
        {
            Debug.Log("You need a Repaired Thruster to interact!");
        }
    }
}
