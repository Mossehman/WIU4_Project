using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
            //stats.hunger = 80;
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (TimeManager.Instance.IsWithinCurrentTimePeriod(awaketime)) 
            {
                fsm.SwapState("Idle");
                return;
            }
            if (stats.assignedHome != null)
            {
                Vector3 dir = stats.assignedHome.transform.position - stats.transform.position;
                if (dir.sqrMagnitude >= 10f)
                    stats.Move(dir.normalized * 2f);
                else
                    AudioManager.Instance.PlayNonSpamAudio(stats.rest, ref stats.voiceSource, default, true, 1);
            }
            else
            {
                AudioManager.Instance.PlayNonSpamAudio(stats.rest, ref stats.voiceSource, default, true, 1);
                stats.hunger += Time.deltaTime * 0.5f;
            }
        }
    }
}