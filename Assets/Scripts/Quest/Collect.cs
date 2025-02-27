using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "New Collect Subquest", menuName = "Quests/Collect Sub-Quest")]
    public class Collect : SubQuest
    {
        [SerializeField] private BaseItem _targetItem;

        public override void Init()
        {
            base.Init();
            _type = subquestType.COLLECT;
        }

        public override void CheckSubQuest()
        {
            base.CheckSubQuest();
        }

        public string GetTargetID() { return _targetItem.getID(); }
    }
}