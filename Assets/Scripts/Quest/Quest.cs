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
        [SerializeField]    private List<SubQuest>  _subquests;
        [SerializeField]    private questStatus     _questStatus;

        private void OnValidate()
        {
            _questID = this.name;
            _questStatus = questStatus.LOCKED;
        }

        public string GetID() { return _questID; }
        public string GetName() { return _questName; }
        public List<Quest> GetRequriedQuests() { return _requriedQuests; }
        public List<SubQuest> GetSubquests() { return _subquests; }
        public questStatus GetQuestStatus() { return _questStatus; }
        public void SetQuestStatus(questStatus status) { _questStatus = status; }
    }
}