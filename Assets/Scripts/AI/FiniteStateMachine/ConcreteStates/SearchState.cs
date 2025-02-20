using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "SearchState", menuName = "AI/SearchState")]
    public class SearchState : BaseState
    {
        [SerializeField] float statetime = 1.0f;
        [SerializeField] float searchradius = 1.0f;
        [SerializeField] string[] food;
        private float currenttime;
        Collider[] foodobjects;
        CreatureInfo stats;
        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
            foodobjects = Physics.OverlapSphere(fsm.transform.position, searchradius, LayerMask.GetMask(food));
            if (foodobjects.Length > 0)
            {
                foodobjects = foodobjects.OrderBy(c => Vector3.Distance(fsm.transform.position, c.transform.position)).ToArray();
            }
        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {
            currenttime = statetime;
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (currenttime > 0)
            {
                currenttime -= Time.deltaTime;
                return;
            }

            if (foodobjects.Length > 0)
            {
                Collider target = null;
                foreach (var food in foodobjects)
                {
                    if (food != null)
                    {
                        if (food.TryGetComponent<CreatureInfo>(out var info))
                        {
                            if (!info.isSheltered)
                            {
                                target = food;
                                break;
                            }
                        }
                        else
                        {
                            target = food; // Closest target
                            break;
                        }
                    }
                }
                if (target != null)
                {
                    stats.target = target.gameObject;

                    if (target.gameObject.layer == LayerMask.NameToLayer("Passive"))
                    {
                        AIBlackboardMediator.Instance.Notify(fsm.gameObject, "Im gonna kill you rahh", new object[]{ fsm.gameObject });
                    }
                    if (stats.CurrentGroup != null)
                    {
                        AIBlackboardMediator.Instance.Notify(fsm.gameObject, "I found food", new object[] { stats });
                    }
                    fsm.SwapState("Hunt");
                    return;
                }
            }

            fsm.SwapState("Patrol");
        }
    }
}