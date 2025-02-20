using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    public class Group
    {
        public CreatureInfo Leader { get; private set; }
        public List<CreatureInfo> Members { get; private set; } = new List<CreatureInfo>();
        private const int MaxGroupSize = 5;

        public Group(CreatureInfo leader)
        {
            Leader = leader;
            Members.Add(leader);
        }

        public bool AddMember(CreatureInfo member)
        {
            // Reject if group is full
            if (Members.Count >= MaxGroupSize) return false; 

            CreatureShelter leaderHome = Leader.assignedHome;

            if (leaderHome != null)
            {
                // Reject if leader's home is full
                if (leaderHome.numOfRegisteredCreatures >= leaderHome.maxHousingSpace)
                {
                    return false;
                }

                // Relocate the member to the leader's home
                if (member.assignedHome != null)
                {
                    member.assignedHome.numOfRegisteredCreatures--;
                }

                member.assignedHome = leaderHome;
                leaderHome.numOfRegisteredCreatures++;
            }

            if (!Members.Contains(member))
            {
                Members.Add(member);
                member.CurrentGroup = this;
                return true;
            }

            return false;
        }


        public void RemoveMember(CreatureInfo member)
        {
            Members.Remove(member);
            if (member == Leader && Members.Count > 0)
            {
                Leader = Members[0];
            }
        }

        public void ShareFood(float hungerres)
        {
            float multiplied = hungerres * 0.6f;// * Members.Count;
            foreach (CreatureInfo member in Members)
            {
                member.hunger += multiplied;

            }
        }

        public void Disband()
        {
            foreach (var member in Members)
            {
                if (member != null)
                {
                    member.GetComponent<CreatureInfo>().CurrentGroup = null;
                }
            }

            Members.Clear();
            Leader = null;
        }

        public bool CanMerge(Group otherGroup)
        {
            return (Members.Count + otherGroup.Members.Count) <= MaxGroupSize;
        }

        public void Merge(Group otherGroup)
        {
            if (!CanMerge(otherGroup)) return;

            foreach (var member in otherGroup.Members)
            {
                if (Members.Count < MaxGroupSize)
                {
                    AddMember(member);
                }
            }
            otherGroup.Disband();
        }
    }
}