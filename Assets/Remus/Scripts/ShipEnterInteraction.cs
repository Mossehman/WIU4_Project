using UnityEngine;

public class ShipEnterInteraction : InteractionObject
{
    public GameObject crashedShip;  // Assign the crashed ship in Inspector
    public GameObject shipInterior; // Assign the ship interior in Inspector
    public Transform shipInteriorSpawnPoint; // Assign the spawn point inside the ship
    public GameObject player; // Assign the player GameObject

    public override void Interact()
    {
        if (crashedShip != null)
            crashedShip.SetActive(false);  // Hide the crashed ship

        if (shipInterior != null)
            shipInterior.SetActive(true);  // Enable the ship interior

        if (player != null && shipInteriorSpawnPoint != null)
            player.transform.position = shipInteriorSpawnPoint.position; // Teleport player inside
    }
}