using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum questStatus
{
    LOCKED,
    IN_PROGRESS,
    COMPLETED
}

public class Quest : ScriptableObject
{
    [SerializeField] private string _questID;
    [SerializeField] private string _questName;
    [SerializeField] private List<Quest> _requriedQuests;
    [SerializeField] private List<SubQuest> _subquests;
    [SerializeField] private questStatus questStatus;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
