using System.Collections;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "IdleState", menuName = "AI/IdleState")]
    public class IdleState : BaseState
    {
        [SerializeField] float statetime = 1.0f; // Time before making a decision
        [SerializeField] string[] food; // Layers or tags for food objects
        [SerializeField] float groupFormationRadius = 10f; // Radius for group formation
        [SerializeField] float shelterSearchRadius = 30f; // Radius for searching shelters
        [SerializeField] MinMaxEnum<TimeOfTheDay> awaketime; // Time period when the creature is active

        private float currenttime;
        private CreatureInfo stats;
        private Collider[] foodobjects;
        private int maxGroupSize = 5; // Maximum number of members in a group

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
            if (groupFormationRadius > 0)
            {
                if (stats.gameObject.layer == LayerMask.NameToLayer("Passive"))
                {
                    Collider[] nearbyCreatures = Physics.OverlapSphere(fsm.transform.position, groupFormationRadius, LayerMask.GetMask("Passive"));
                    foreach (Collider creature in nearbyCreatures)
                    {
                        CreatureInfo otherStats = creature.GetComponent<CreatureInfo>();

                        if (otherStats == null || otherStats == stats) continue;

                        if (otherStats.CurrentGroup != null)
                        {
                            // If the other creature has a group, try to merge
                            if (stats.CurrentGroup != null && stats.CurrentGroup.CanMerge(otherStats.CurrentGroup))
                            {
                                stats.CurrentGroup.Merge(otherStats.CurrentGroup);
                            }
                            else if (stats.CurrentGroup == null && otherStats.CurrentGroup.Members.Count < maxGroupSize)
                            {
                                otherStats.CurrentGroup.AddMember(stats);
                            }
                        }
                        else
                        {
                            // Neither has a group, form a new one
                            stats.CurrentGroup ??= new Group(stats);

                            stats.CurrentGroup.AddMember(otherStats);
                        }
                    }
                }
            }
            // Find a home if there is none
            if (shelterSearchRadius > 0)
            {
                if (stats.assignedHome == null)
                {
                    Collider[] nearbyShelters = Physics.OverlapSphere(fsm.transform.position, shelterSearchRadius, LayerMask.GetMask("Shelter"));
                    foreach (Collider c in nearbyShelters)
                    {
                        CreatureShelter shelter = c.GetComponent<CreatureShelter>();
                        if (shelter.numOfRegisteredCreatures >= shelter.maxHousingSpace) continue;
                        if (((1 << stats.gameObject.layer) & shelter.creatureHome) != 0)
                        {
                            stats.assignedHome = shelter;
                            shelter.numOfRegisteredCreatures++;
                        }
                    }
                }
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
                AudioEventSystem.PlaySoundSmart(stats.goes, ref stats.voiceSource, default, default, true, true, 1, true);
                if (!TimeManager.Instance.IsWithinCurrentTimePeriod(awaketime))
                {
                    fsm.SwapState("Resting");
                }
                else if (foodobjects.Length > 0)
                {
                    fsm.SwapState("Search");
                }
                else
                {
                    if (!fsm.SwapState("Lure"))
                    fsm.SwapState("Patrol");
                }
            }
        }
    }
}