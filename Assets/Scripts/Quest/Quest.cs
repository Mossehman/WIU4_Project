using DialogueSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    public enum questStatus
    {
        LOCKED,
        UNLOCKED,
        IN_PROGRESS,
        COMPLETED
    }
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
    public class Quest : ScriptableObject
    {
        [SerializeField]    private string          _questID;
        [SerializeField]    private string          _questName;
        [SerializeField]    private List<Quest>     _requriedQuests;
        [SerializeField]    private List<SubQuest>  subQuests;
                            public List<SubQuest>   _subquests = new List<SubQuest>();
        [SerializeField]    private questStatus     _questStatus;
        [SerializeField]    public Dialogue         _dialogueUponCompletion, _dialogueUponStart;

        private void OnValidate()
        {
            _questID = this.name;
            _questStatus = questStatus.LOCKED;
        }

        public void Init()
        {
            foreach (var subQuest in subQuests)
            {
                SubQuest q = Instantiate(subQuest);
                q.Init();
                _subquests.Add(q);
            }
        }


        public string GetID() { return _questID; }
        public string GetName() { return _questName; }
        public List<Quest> GetRequriedQuests() { return _requriedQuests; }
        public List<SubQuest> GetSubquests() { return _subquests; }
        public questStatus GetQuestStatus() { return _questStatus; }
        public void SetQuestStatus(questStatus status) { _questStatus = status; }
    }
}