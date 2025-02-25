using UnityEngine;

public class ShipExitInteraction : InteractionObject
{
    public GameObject crashedShip;  // Assign the crashed ship in Inspector
    public GameObject shipInterior; // Assign the ship interior in Inspector
    public Transform outsideSpawnPoint; // Assign the spawn point outside the ship
    public GameObject player; // Assign the player GameObject

    public override void Interact()
    {
        if (shipInterior != null)
            shipInterior.SetActive(false);  // Hide the ship interior

        if (crashedShip != null)
            crashedShip.SetActive(true);  // Enable the crashed ship

        if (player != null && outsideSpawnPoint != null)
            player.transform.position = outsideSpawnPoint.position; // Teleport player outside
    }
}