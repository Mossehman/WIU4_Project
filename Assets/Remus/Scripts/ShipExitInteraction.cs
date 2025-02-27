using UnityEngine;

public class ShipExitInteraction : InteractionObject
{
    public GameObject crashedShip;
    public GameObject shipInterior;
    public Transform outsideSpawnPoint;
    public GameObject player;

    public override string GetCustomDescription()
    {
        return $"Leave Ship";
    }

    public override void Interact()
    {
        if (shipInterior != null)
            shipInterior.SetActive(false);

        if (crashedShip != null)
            crashedShip.SetActive(true);

        if (player != null && outsideSpawnPoint != null)
        {
            player.GetComponent<CharacterController>().enabled = false;
            player.transform.position = outsideSpawnPoint.position;
            player.GetComponent<CharacterController>().enabled = true;
        }
    }
}