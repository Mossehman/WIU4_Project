using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "PatrolState", menuName = "AI/PatrolState")]
    public class PatrolState : BaseState
    {
        [SerializeField] float statetime = 1.0f;
        [SerializeField] float hungerdrain = 10f;
        private float currenttime;
        [SerializeField] int numOfWalkDirections = 8;
        CreatureInfo stats;

        Vector3 movedirection;
        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
            if (fsm.GetPreviousStateName() == "Search" || fsm.GetPreviousStateName() == "Hunt")
                AIBlackboardMediator.Instance.Notify(fsm.gameObject, "Nvm Im not killing yall lol", new object[] { fsm.gameObject });

            Vector3[] dir = new Vector3[numOfWalkDirections];
            float incrementangle = 360f / numOfWalkDirections;
            for (int i = 0; i < numOfWalkDirections; i++)
            {
                float angle = incrementangle * i;
                dir[i] = new Vector3(Mathf.Sin(Mathf.Deg2Rad * angle), 0, Mathf.Cos(Mathf.Deg2Rad * angle));
            }

            // Check if the creature has an assigned home
            if (stats.assignedHome == null)
            {
                // Look for the nearest home within a 50-unit radius
                Collider[] nearbyShelters = Physics.OverlapSphere(fsm.transform.position, 50f, LayerMask.GetMask("Shelter"));
                Collider closestShelter = null;
                float minDistance = float.MaxValue;

                foreach (Collider c in nearbyShelters)
                {
                    float distance = Vector3.Distance(fsm.transform.position, c.transform.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestShelter = c;
                    }
                }

                if (closestShelter != null)
                {
                    // Bias movement towards the nearest shelter (60% chance)
                    if (Random.value < 0.6f)
                    {
                        movedirection = (closestShelter.transform.position - fsm.transform.position).normalized;
                        return;
                    }
                }
            }
            else
            {
                // If the creature has a home, bias movement AWAY from it (70% chance)
                if (Random.value < 0.7f)
                {
                    movedirection = (fsm.transform.position - stats.assignedHome.transform.position).normalized;
                    return;
                }
            }

            // If no shelter is found or if random chance dictates, pick a fully random direction
            movedirection = dir[UnityEngine.Random.Range(0, numOfWalkDirections)];
        }


        public override void OnStateLeave(FiniteStateMachine fsm)
        {
            float totalhungerdrain = hungerdrain;
            if (stats.CurrentGroup != null) totalhungerdrain *= 0.4f;
            stats.hunger -= totalhungerdrain;
            currenttime = statetime + UnityEngine.Random.Range(0f, 3f);
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (currenttime > 0)
            {
                float moveSpeed = stats.hunger <= 0 ? 0f : 1.0f;
                stats.Move(moveSpeed * movedirection.normalized);
                currenttime -= Time.deltaTime;
            }
            else if (stats.hunger <= 50)
            {
                fsm.SwapState("Search");
            }
            else
            {
                fsm.SwapState("Idle");
            }
        }
    }
}