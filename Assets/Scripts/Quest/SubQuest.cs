using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    public abstract class SubQuest : ScriptableObject
    {
        public enum subquestType
        {
            COLLECT,
            CRAFT,
            HUNT
        }

        [SerializeField] public int _currentAmount;
        [SerializeField] protected int _requiredAmount;

        public bool _isCompleted;
        private string _subquestID;
        private string _subquestName;
        protected subquestType _type;

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

    [CreateAssetMenu(fileName = "New Collect Subquest", menuName = "Quests/Collect Sub-Quest")]
    public class Collect : SubQuest
    {
        public override void Init()
        {
            base.Init();
            _type = subquestType.COLLECT;
        }

        public override void CheckSubQuest()
        {
            base.CheckSubQuest();
        }
    }

    [CreateAssetMenu(fileName = "New Craft Subquest", menuName = "Quests/Craft Sub-Quest")]
    public class Craft : SubQuest
    {
        [SerializeField] private BaseItem _targetItem;
        public override void Init()
        {
            base.Init();
            _type = subquestType.CRAFT;
        }
        public override void CheckSubQuest()
        {
            base.CheckSubQuest();
        }
    }

    [CreateAssetMenu(fileName = "New Hunt Subquest", menuName = "Quests/Hunt Sub-Quest")]
    public class Hunt : SubQuest
    {
        public override void Init()
        {
            base.Init();
            _type = subquestType.HUNT;
        }
        public override void CheckSubQuest()
        {
            base.CheckSubQuest();
        }
    }
}