using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "HuntState", menuName = "AI/HuntState")]
    public class HuntState : BaseState
    {
        [SerializeField] float statetime = 20.0f;
        [SerializeField] float attacktime = 0.7f;
        [SerializeField] float speedmod = 1.2f;
        private float currenttime;
        private float currentattacktime;
        CreatureInfo stats;

        Vector3 currentdirection;
        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
            currenttime = statetime;
            currentattacktime = 0f;
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
            currenttime = statetime;
            currentattacktime = 0f;
        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {

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
                stats.Move(speedmod * currentdirection);
                if (currentattacktime > 0) currentattacktime -= Time.deltaTime;
                if (dir.sqrMagnitude <= 2f && currentattacktime <= 0f)
                {
                    currentattacktime = attacktime;
                    if (stats.target.TryGetComponent<CreatureInfo>(out var creaturestats))
                    {
                        creaturestats.Health -= 30;
                        if (creaturestats.Health <= 0f)
                        {
                            stats.hunger += creaturestats.hunger * 0.25f;
                            stats.hunger += 15;
                            stats.CurrentGroup?.ShareFood(15);
                        }
                        return;
                    }
                    else
                    {
                        Destroy(stats.target);
                        stats.hunger += 15;
                        stats.CurrentGroup?.ShareFood(15);
                    }
                    //currenttime = 0;
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