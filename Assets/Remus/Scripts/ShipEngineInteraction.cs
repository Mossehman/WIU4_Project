using Player.Inventory;
using UnityEngine;

public class ShipEngineInteraction : InteractionObject
{
    private PlayerInventory _playerInventory;
    public string requiredItemID = "ID18"; // Item ID for the repaired engine

    private void Start()
    {
        _playerInventory = GameObject.FindWithTag("Player").GetComponent<PlayerInventory>();
    }

    public override string GetCustomDescription()
    {
        int itemCount = _playerInventory.GetItemCount(requiredItemID);
        return $"Repaired Engine: {itemCount}/1";
    }

    public override void Interact()
    {
        if (_playerInventory.GetItemCount(requiredItemID) > 0)
        {
            Debug.Log("Ship engine repaired!");
            _playerInventory.RemoveItem(requiredItemID, 1);
            ShipRepairManager.Instance.MarkEngineRepaired(); //Notify repair manager
        }
        else
        {
            Debug.Log("You need a Repaired Engine to interact!");
        }
    }
}