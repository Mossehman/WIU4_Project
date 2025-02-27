using UnityEngine;

namespace QuestSystem
{
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

        public string GetTargetID() { return _targetItem.getID(); }
    }
}