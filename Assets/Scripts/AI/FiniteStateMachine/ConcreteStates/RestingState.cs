using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "RestingState", menuName = "AI/RestingState")]
    public class RestingState : BaseState
    {
        [SerializeField] MinMaxEnum<TimeOfTheDay> awaketime; // Time period when the creature is active
        [SerializeField] float hungerRecoveryRate = 0.5f; // Hunger recovery rate per second
        [SerializeField] float healthRecoveryRate = 0.5f; // Health recovery rate per second
        [SerializeField] float moveSpeedToShelter = 2f; // Speed when moving towards shelter
        [SerializeField] float shelterDetectionRange = 2.5f; // Range to detect shelter

        private CreatureInfo stats;

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
                if (dir.sqrMagnitude >= shelterDetectionRange * shelterDetectionRange)
                    stats.Move(dir.normalized * moveSpeedToShelter);
                else
                    AudioEventSystem.PlaySoundSmart(stats.rest, ref stats.voiceSource, default, default, true, true, 1);
                //AudioManager.Instance.PlayNonSpamAudio(stats.rest, ref stats.voiceSource, default, true, 1);
            }
            else
            {
                //AudioManager.Instance.PlayNonSpamAudio(stats.rest, ref stats.voiceSource, default, true, 1);
                AudioEventSystem.PlaySoundSmart(stats.rest, ref stats.voiceSource, default, default, true, true, 1);
                stats.hunger += Time.deltaTime * hungerRecoveryRate;
                stats.Health += Time.deltaTime * healthRecoveryRate;
            }
        }
    }
}