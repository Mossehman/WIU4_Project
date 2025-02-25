using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SubQuest : MonoBehaviour
{
    public enum subquestType
    {
        COLLECT,
        CRAFT,
        HUNT
    }

    private bool _isCompleted;
    private string _subquestID;
    private string _questDescription;
    private subquestType _type;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
