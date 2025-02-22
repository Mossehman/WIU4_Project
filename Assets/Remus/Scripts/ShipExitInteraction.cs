using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipExitInteraction : InteractionObject
{
    public override void Interact()
    {
        SceneManager.LoadScene("Test_Remus");
    }
}