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
    private string _questName;
    private subquestType _type;

    public virtual void Init()
    {
        _isCompleted = false;
    }

    public void Complete()
    {
        _isCompleted = true;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
