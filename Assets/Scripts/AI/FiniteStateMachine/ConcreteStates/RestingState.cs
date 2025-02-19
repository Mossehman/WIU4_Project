using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "RestingState", menuName = "AI/RestingState")]
    public class RestingState : BaseState
    {
        CreatureInfo stats;

        [SerializeField] private MinMaxEnum<TimeOfTheDay> awaketime;

        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {

        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {
            stats.hunger = 80;
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (TimeManager.Instance.IsWithinCurrentTimePeriod(awaketime)) 
            {
                fsm.SwapState("Idle");
                return;
            }
        }
    }
}