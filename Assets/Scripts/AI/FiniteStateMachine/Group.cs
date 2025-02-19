using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    public class Group
    {
        public CreatureInfo Leader { get; private set; }
        public List<CreatureInfo> Members { get; private set; } = new List<CreatureInfo>();

        public Group(CreatureInfo leader)
        {
            Leader = leader;
            Members.Add(leader);
        }

        public void AddMember(CreatureInfo member)
        {
            if (!Members.Contains(member))
            {
                Members.Add(member);
            }
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

        /// <summary>
        /// Merges this group with another group.
        /// </summary>
        public void MergeWith(Group otherGroup)
        {
            if (otherGroup == null || otherGroup == this) return;

            // Add all members of the other group to this group
            foreach (var member in otherGroup.Members)
            {
                AddMember(member);
            }

            // Disband the other group
            otherGroup.Disband();
        }

        /// <summary>
        /// Disbands the group, setting CurrentGroup to null for all members.
        /// </summary>
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
    }
}