using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "IdleState", menuName = "AI/IdleState")]
    public class IdleState : BaseState
    {
        [SerializeField] float statetime = 1.0f;
        [SerializeField] string[] food;
        private float currenttime;
        CreatureInfo stats;
        Collider[] foodobjects;
        [SerializeField] private MinMaxEnum<TimeOfTheDay> awaketime;
        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
            currenttime = statetime;

            AIBlackboardMediator.Instance.RegisterFSM(fsm.gameObject, fsm);
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
            foodobjects = Physics.OverlapSphere(fsm.transform.position, 10f, LayerMask.GetMask(food));
            if (foodobjects.Length > 0)
            {
                foodobjects = foodobjects.OrderBy(c => Vector3.Distance(fsm.transform.position, c.transform.position)).ToArray();
                stats.target = foodobjects[0].gameObject;
            }

            // Check for nearby creatures to form a group
            Collider[] nearbyCreatures = Physics.OverlapSphere(fsm.transform.position, 10f, fsm.gameObject.layer);
            if (nearbyCreatures.Length > 1)
            {

                GameObject leader = nearbyCreatures[0].gameObject;
                //Group group;
                //if (leader.GetComponent<CreatureInfo>().CurrentGroup == null)
                //{
                //    group = new Group(leader);
                //    leader.GetComponent<CreatureInfo>().CurrentGroup = group;
                //}
                //else
                //    group = leader.GetComponent<CreatureInfo>().CurrentGroup;
                Group group = leader.GetComponent<CreatureInfo>().CurrentGroup ?? new Group(leader);
                group.AddMember(fsm.gameObject);
                stats.CurrentGroup = group;
            }
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
                if (!TimeManager.Instance.IsWithinCurrentTimePeriod(awaketime))
                {
                    fsm.SwapState("Resting");
                }
                else if (foodobjects.Length > 0)
                {
                    fsm.SwapState("Hunt");
                }
                else
                {
                    fsm.SwapState("Patrol");
                }
            }
        }
    }
}