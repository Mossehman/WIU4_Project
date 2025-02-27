using Player.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

namespace QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Quest Logic")]
        [SerializeField]    private List<Quest>     _questsInit = new List<Quest>();
                            private List<Quest>     _quests = new List<Quest>();
                            private int             _currentQuestIndex = 0;

        [Header("Quest UI")]
        [SerializeField]    private GameObject      _questPanel;
        [SerializeField]    private GameObject      _subquestPrefab;

        // Start is called before the first frame update
        private void Awake()
        {
            foreach (var quest in _questsInit)
            {
                Quest newQuest = Instantiate(quest);
                newQuest.Init();
                _quests.Add(newQuest);
                
            }
        }

        void Start()
        {
            //_quests = new List<Quest>();
            RefreshQuestUI();

            EventManager.CreateEvent("OnCollectMission");
            EventManager.CreateEvent("OnCraftMisson");
            EventManager.CreateEvent("OnHuntMission");
        }

        private void OnEnable()
        {
            EventManager.Connect("OnCollectMission", OnCollectMission);
            EventManager.Connect("OnCraftMisson", OnCraftMisson);
            EventManager.Connect("OnHuntMission", OnHuntMission);
        }

        private void OnDisable()
        {
            EventManager.Disconnect("OnCollectMission", OnCollectMission);
            EventManager.Disconnect("OnCraftMisson", OnCraftMisson);
            EventManager.Disconnect("OnHuntMission", OnHuntMission);
        }

        // Update is called once per frame
        void Update()
        {
            foreach (Quest quest in _quests)
            {
                if (quest.GetQuestStatus() == questStatus.LOCKED && CheckRequirements(quest) == true)
                {
                    quest.SetQuestStatus(questStatus.UNLOCKED);
                    if (_quests[_currentQuestIndex].GetQuestStatus() == questStatus.UNLOCKED)
                    {
                        _quests[_currentQuestIndex].SetQuestStatus(questStatus.IN_PROGRESS);
                        EventManager.Fire("OnQuestStart", _quests[_currentQuestIndex]);
                    }
                }
            }

            if (CheckQuestCompleted() == true)
            {
                if (_quests.Count > 1)
                {
                    if (_quests[_currentQuestIndex + 1].GetQuestStatus() == questStatus.UNLOCKED)
                    {
                        EventManager.Fire("OnQuestComplete", _quests[_currentQuestIndex]);
                        _quests[_currentQuestIndex].SetQuestStatus(questStatus.COMPLETED);

                        _currentQuestIndex++;

                        EventManager.Fire("OnQuestStart", _quests[_currentQuestIndex]);
                        _quests[_currentQuestIndex].SetQuestStatus(questStatus.IN_PROGRESS);

                        RefreshQuestUI();
                    }
                }
                else
                {
                    EventManager.Fire("OnQuestComplete", _quests[_currentQuestIndex]);
                    _quests[_currentQuestIndex].SetQuestStatus(questStatus.COMPLETED);
                }
            }
        }

        public void CompleteQuest()
        {
            foreach (SubQuest goal in _quests[_currentQuestIndex].GetSubquests())
            {
                goal._isCompleted = true;
            }
        }

        private void RefreshQuestUI()
        {
            foreach (Transform child in _questPanel.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (SubQuest goal in _quests[_currentQuestIndex].GetSubquests())
            {
                GameObject questChild = Instantiate(_subquestPrefab, _questPanel.transform);
                string goalTitle = goal.GetSubQuestName();
                string goalProgress = goal._currentAmount.ToString() + " / " + goal.GetRequiredAmount().ToString();
                questChild.transform.Find("Info").GetComponent<TextMeshProUGUI>().text = goalTitle + " " + goalProgress;
            }
        }

        private Quest GetQuestByIndex(int index)
        {
            Quest selected = new Quest();

            if (index <= _quests.Count)
            {
                return _quests[index];
            }

            return selected;
        }

        private bool CheckRequirements(Quest currentQuest)
        {
            bool canUnlock = true;

            // Check if the required previous quests were completed
            foreach (Quest requiredQuest in currentQuest.GetRequriedQuests())
            {
                if (requiredQuest.GetQuestStatus() != questStatus.COMPLETED)
                {
                    canUnlock = false;
                }
            }

            return canUnlock;
        }

        private bool CheckQuestCompleted()
        {
            int numCompleted = 0;

            foreach (SubQuest goal in _quests[_currentQuestIndex].GetSubquests())
            {
                if (goal._isCompleted == true) { numCompleted++; }
            }

            return numCompleted == _quests[_currentQuestIndex].GetSubquests().Count;
        }

        // Pass in base item
        private void OnCollectMission(object[] args)
        {
            PlayerInventory inventory = (PlayerInventory)args[0];
            if (inventory == null) return;

            foreach (SubQuest goal in _quests[_currentQuestIndex].GetSubquests())
            {
                if (goal.GetSubQuestType() == subquestType.COLLECT && goal is Collect collectQuest)
                {
                    bool foundInHotbar = false;
                    foreach (var hotbarItem in inventory.GetHotbar())
                    {
                        if (hotbarItem == null) continue;
                        if (collectQuest.GetTargetID() == hotbarItem.getID())
                        {
                            foundInHotbar = true;
                            goal._currentAmount = hotbarItem._quantity;
                        }
                    }

                    foreach (var inventoryItem in inventory.GetInventory())
                    {
                        if (inventoryItem == null) continue;
                        if (collectQuest.GetTargetID() == inventoryItem.getID())
                        {
                            if (foundInHotbar)
                            {
                                goal._currentAmount += inventoryItem._quantity;
                            }
                            else
                            {
                                goal._currentAmount = inventoryItem._quantity;
                            }
                        }
                    }
                }
                RefreshQuestUI();

                if (goal._currentAmount >= goal.GetRequiredAmount()) { goal.Complete(); }
            }
        }
        // Pass in base item
        private void OnCraftMisson(object[] args)
        {
            BaseItem item = args[0] as BaseItem;


            foreach (SubQuest goal in _quests[_currentQuestIndex].GetSubquests())
            {
                if (goal.GetSubQuestType() == subquestType.CRAFT)
                {
                    Craft craftQuest = (Craft)goal;
                    if (craftQuest.GetTargetID() == item.getID())
                    {
                        goal._currentAmount++;
                    }
                }
                RefreshQuestUI();

                if (goal._currentAmount >= goal.GetRequiredAmount()) { 
                    goal.Complete(); 
                }
            }
        }
        // pass in gameobbject
        private void OnHuntMission(object[] args)
        {
            GameObject go = args[0] as GameObject;
            if (go == null) { return; }

            foreach (SubQuest goal in _quests[_currentQuestIndex].GetSubquests())
            {
                if (goal.GetSubQuestType() == subquestType.HUNT)
                {
                    goal._currentAmount++;
                }

                if (goal._currentAmount >= goal.GetRequiredAmount()) { goal.Complete(); }
            }
        }
    }
}