using System.Collections;
using UnityEditor;
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
        public Group CurrentGroup { get; set; } // Track the group this creature belongs to

        private void Start()
        {
            fsm = GetComponent<FiniteStateMachine>();
        }

        private void Update()
        {
            if (hunger <= 0) Destroy(gameObject, 10f);

            if (hunger >= 90 && isSheltered)
            {
                hunger -= 10;
                Instantiate(segs).GetComponent<CreatureInfo>().hunger = 70;
            }

            transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
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