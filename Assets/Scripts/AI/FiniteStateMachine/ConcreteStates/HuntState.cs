using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "HuntState", menuName = "AI/HuntState")]
    public class HuntState : BaseState
    {
        [SerializeField] float statetime = 20.0f;
        [SerializeField] float speedmod = 1.2f;
        private float currenttime;
        CreatureInfo stats;

        Vector3 currentdirection;
        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
            currenttime = statetime;
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {
            currenttime = statetime;
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (stats.target == null)
            {
                fsm.SwapState("Idle");
                return;
            }
            if (currenttime > 0)
            {
                Vector3 dir = stats.target.transform.position - fsm.transform.position;
                currentdirection = Vector3.Slerp(currentdirection, dir.normalized, 0.1f);
                stats.Move(0.01f * speedmod * currentdirection);
                if (dir.sqrMagnitude <= 2f)
                {
                    Destroy(stats.target);
                    stats.hunger += 15;
                    currenttime = 0;
                }
                else if (dir.sqrMagnitude >= 55f * 55f)
                {
                    fsm.SwapState("Search");
                }
                currenttime -= Time.deltaTime;
            }
            else
            {
                fsm.SwapState("Patrol");
            }
        }
    }
}