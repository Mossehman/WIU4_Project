using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipEnterInteraction : InteractionObject
{
    public override void Interact()
    {
        SceneManager.LoadScene("Ship_Interior");
    }
}