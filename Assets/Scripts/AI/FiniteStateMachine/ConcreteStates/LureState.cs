using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "LureState", menuName = "AI/LureState")]
    public class LureState : BaseState
    {
        [SerializeField] float statetime = 1.0f;
        [SerializeField] GameObject baitprefab;
        GameObject bait;

        private float currenttime;
        private CreatureInfo stats;

        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            bait = null;
            stats = fsm.GetComponent<CreatureInfo>();
            currenttime = statetime;
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {

        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {
            currenttime = statetime + UnityEngine.Random.Range(0f, 2f);
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (stats.hunger <= 0f) return;
            if (currenttime > 0)
            {
                currenttime -= Time.deltaTime;
            }
            else
            {
                if (bait != null)
                {
                    fsm.SwapState("Patrol");
                }
                else
                {
                    bait = Instantiate(baitprefab, stats.transform.position, Quaternion.identity);
                }
            }
        }
    }
}