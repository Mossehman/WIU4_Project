using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    public class Group
    {
        public GameObject Leader { get; private set; }
        public List<GameObject> Members { get; private set; } = new List<GameObject>();

        public Group(GameObject leader)
        {
            Leader = leader;
            Members.Add(leader);
        }

        public void AddMember(GameObject member)
        {
            if (!Members.Contains(member))
            {
                Members.Add(member);
            }
        }

        public void RemoveMember(GameObject member)
        {
            Members.Remove(member);
            if (member == Leader && Members.Count > 0)
            {
                Leader = Members[0]; // Assign a new leader if the current leader leaves
            }
        }
    }
}