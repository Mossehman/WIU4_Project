using UnityEngine;

namespace QuestSystem
{
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