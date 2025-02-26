using UnityEngine;

public class ShipEnterInteraction : InteractionObject
{
    public GameObject crashedShip;
    public GameObject shipInterior;
    public Transform shipInteriorSpawnPoint;
    public GameObject player;

    public override void Interact()
    {
        if (crashedShip != null)
            crashedShip.SetActive(false);

        if (shipInterior != null)
            shipInterior.SetActive(true);

        if (player != null && shipInteriorSpawnPoint != null)
            player.transform.position = shipInteriorSpawnPoint.position;
    }
}