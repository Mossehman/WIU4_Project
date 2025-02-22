using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoB : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.Connect("OnPlayerJoin", OnPlayerJoin);
    }

    void OnPlayerJoin(object[] args)
    {
        // Extract GameObject, message, and bool flag
        GameObject player = args[0] as GameObject;
        string message = args[1] as string;
        bool isConfirmed = (bool)args[2];

        // Debug log the received data
        Debug.Log($"[DemoB] {message} | Player: {player.name} | Confirmed: {isConfirmed}");

        // Example usage: Check if the GameObject has a Rigidbody
        if (player.GetComponent<Rigidbody>() != null)
        {
            Debug.Log("[DemoB] The player has a Rigidbody component!");
        }
        else
        {
            Debug.Log("[DemoB] The player does not have a Rigidbody component.");
        }
    }
}