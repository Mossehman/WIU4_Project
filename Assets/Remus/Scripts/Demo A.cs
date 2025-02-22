using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoA : MonoBehaviour
{
    [SerializeField] private GameObject testGameObject;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.CreateEvent("OnPlayerJoin");

        // Fire the event, passing the GameObject and a message
        EventManager.Fire("OnPlayerJoin", testGameObject, "Player joined the game!", true);
    }
}