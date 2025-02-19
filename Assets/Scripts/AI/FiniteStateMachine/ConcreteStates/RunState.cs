using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "RunState", menuName = "AI/RunState")]
    public class RunState : BaseState
    {
        [SerializeField] float statetime = 30.0f;
        [SerializeField] float hungerdrain = 1f;
        [SerializeField] float speedmod = 1.2f;
        [SerializeField] float wallDetectionDistance = 2.0f;
        [SerializeField] LayerMask wallLayer;

        private float currenttime;
        private CreatureInfo stats;
        private Vector3 movedirection;
        private bool isTurning;

        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
            currenttime = statetime + UnityEngine.Random.Range(0f, 3f);
            isTurning = false;
        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {
            currenttime = statetime;
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (stats.target != null)
            {
                Vector3 dir = (fsm.transform.position - stats.target.transform.position).normalized;
                if (isTurning) dir = Quaternion.Euler(0, 90, 0) * dir;
                movedirection = Vector3.Slerp(movedirection, dir, 0.1f);

                Vector3 left = Quaternion.Euler(0, -90, 0) * movedirection;
                // Check for wall
                if (Physics.Raycast(fsm.transform.position, movedirection, out RaycastHit hit, wallDetectionDistance, wallLayer))
                {
                    Debug.DrawRay(fsm.transform.position, movedirection * wallDetectionDistance, Color.red);

                    isTurning = true;
                }
                else
                {
                    Debug.DrawRay(fsm.transform.position, movedirection * wallDetectionDistance, Color.green);
                    Vector3 leftDirection = Quaternion.Euler(0, -90, 0) * movedirection;
                    if (Physics.Raycast(fsm.transform.position, leftDirection, out RaycastHit leftHit, wallDetectionDistance, wallLayer))
                    {
                        Debug.DrawRay(fsm.transform.position, leftDirection * wallDetectionDistance, Color.blue);
                        isTurning = true;
                    }
                    else
                    {
                        isTurning = false;
                    }
                }

                
                if ((fsm.transform.position - stats.target.transform.position).sqrMagnitude <= 55f * 55f)
                {
                    float moveSpeed = stats.hunger >= 50 ? speedmod : (stats.hunger <= 0 ? 0f : 1.0f);
                    stats.Move(moveSpeed * movedirection);

                    currenttime -= Time.deltaTime;
                    stats.hunger -= hungerdrain * Time.deltaTime;
                }
                else
                {
                    fsm.SwapState("Idle");
                }
            }
        }
    }
}
