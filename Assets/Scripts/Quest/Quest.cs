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
    [SerializeField]    private string              _questID;
    [SerializeField]    private string              _questName;
    [SerializeField]    private List<Quest>         _requriedQuests;
    [SerializeField]    private List<SubQuest>      _subquests;
    [SerializeField]    private questStatus         _questStatus;

    private void OnValidate()
    {
        _questID = this.name;
    }

    public string GetID() { return _questID; }
    public string GetName() { return _questName; }
    public List<Quest> GetRequriedQuests() { return _requriedQuests; }
    public List <SubQuest> GetSubquests() { return _subquests; }
    public questStatus GetQuestStatus() { return _questStatus; }
}
