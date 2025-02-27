using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    public enum subquestType
    {
        COLLECT,
        CRAFT,
        HUNT
    }

    public abstract class SubQuest : ScriptableObject
    {
        [SerializeField]    private string              _subquestID;
        [SerializeField]    private string              _subquestName;
        [SerializeField]    public int                  _currentAmount;
        [SerializeField]    protected int               _requiredAmount;
                            public bool                 _isCompleted;
                            protected subquestType      _type;

        public virtual void Init()
        {
            _isCompleted = false;
        }

        public virtual void CheckSubQuest() 
        {
            if (_currentAmount != _requiredAmount) { Complete(); }
        }

        public void Complete()
        {
            _isCompleted = true;
        }

        public string GetSubQuestName() { return _subquestName; }
        public string GetSubQuestID() { return _subquestID; }
        public subquestType GetSubQuestType() { return _type; }
        public int GetRequiredAmount() { return _requiredAmount; }
    }
}