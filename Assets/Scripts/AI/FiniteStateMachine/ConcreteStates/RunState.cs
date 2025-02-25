using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "RunState", menuName = "AI/RunState")]
    public class RunState : BaseState
    {
        [SerializeField] float statetime = 30.0f; // Time before switching to another state
        [SerializeField] float hungerdrain = 1f; // Hunger drained per second while running
        [SerializeField] float speedmod = 1.2f; // Speed multiplier while running
        [SerializeField] float wallDetectionDistance = 2.0f; // Distance to detect walls
        [SerializeField] LayerMask wallLayer; // Layer for walls
        [SerializeField] float turnSmoothness = 0.1f; // Smoothness for turning
        [SerializeField] float runAwayRange = 40f; // Range at which the creature will stop running

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
                movedirection = Vector3.Slerp(movedirection, dir, turnSmoothness);

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

                
                if ((fsm.transform.position - stats.target.transform.position).sqrMagnitude <= runAwayRange * runAwayRange)
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
