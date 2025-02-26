using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Quest Logic")]
    [SerializeField] private List<Quest> _quests;
    private int _currentQuestIndex;
    

    [Header("Quest UI")]
    [SerializeField] private GameObject _questPanel;
    [SerializeField] private GameObject _subquestPrefab;

    // Start is called before the first frame update
    void Start()
    {
        _quests = new List<Quest>();

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

    private void OnCollectMission(object[] args)
    {

    }
    private void OnCraftMisson(object[] args)
    {

    }
    private void OnHuntMission(object[] args)
    {

    }
}
