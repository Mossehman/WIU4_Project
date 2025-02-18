using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoA : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EventManager.CreateEvent("OnPlayerJoin");
        EventManager.Fire("OnPlayerJoin", "Player joined the game!");
    }

    // Update is called once per frame
    void Update()
    {
    }
}