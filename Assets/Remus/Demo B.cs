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

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPlayerJoin(object[] args)
    {
        Debug.Log((string)args[0]);
    }
}
