using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    // Temporary storage for entity stats
    public class CreatureInfo : MonoBehaviour
    {
        public float hunger = 80;
        public GameObject target;
        public GameObject segs;
        public FiniteStateMachine fsm;
        public bool isSheltered = false;

        public Group CurrentGroup; //{ get; set; }
        public Vector3 FlockDirection = Vector3.zero; //{ get; private set; }
        public float FlockSpeed = 2f;//{ get; private set; } = 2f;

        private CharacterController characterController;
        
        private void Start()
        {
            fsm = GetComponent<FiniteStateMachine>();
            characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (hunger <= 0) Destroy(gameObject, 10f);

            if (hunger >= 90 && isSheltered)
            {
                hunger -= 10;
                Instantiate(segs).GetComponent<CreatureInfo>().hunger = 70;
            }

            //transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);

            if (CurrentGroup != null)
            {
                UpdateFlockingBehavior();
            }
        }

        private void UpdateFlockingBehavior()
        {
            if (CurrentGroup.Leader == null || CurrentGroup.Leader == gameObject) return;

            // Cohesion: Move towards the leader
            Vector3 leaderPosition = CurrentGroup.Leader.transform.position;
            Vector3 cohesion = (leaderPosition - transform.position).normalized;

            // Separation: Avoid crowding local flockmates
            Vector3 separation = Vector3.zero;
            foreach (var member in CurrentGroup.Members)
            {
                if (member != gameObject && Vector3.Distance(transform.position, member.transform.position) < 2f)
                {
                    separation -= (member.transform.position - transform.position).normalized;
                }
            }

            // Combine forces and apply movement
            FlockDirection = (cohesion + separation).normalized;
        }

        public void Move(Vector3 dir)
        {
            characterController.Move(dir + FlockDirection + (Vector3.down * 9.8f));
        }

        private void OnDrawGizmos()
        {
            if (target != null)
            {
                if (fsm.GetCurrentStateName() == "Run")
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawLine(transform.position, target.transform.position);
                }
                else if (fsm.GetCurrentStateName() == "Hunt")
                {
                    Gizmos.color = new Color(0, 1, 1, 0.5f);
                    Gizmos.DrawLine(transform.position, target.transform.position);
                }
            }
            Gizmos.color = new Color(hunger <= 0f ? 0f : 1 - (hunger * 0.01f), hunger <= 0f ? 0f : hunger * 0.01f, 0, 0.5f);
            Gizmos.DrawCube(transform.position + new Vector3(0,5,0), new Vector3(1, 1, 1));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1, 1, 1, 0.2f);
            Gizmos.DrawSphere(transform.position, 30f);
        }
    }
}