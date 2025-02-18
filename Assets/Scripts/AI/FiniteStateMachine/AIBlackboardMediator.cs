using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    /// <summary>
    /// The Mediator manages communication between multiple FSMs
    /// </summary>
    public class AIBlackboardMediator : MonoBehaviour
    {
        private Dictionary<GameObject, FiniteStateMachine> registeredFSMs = new();

        public static AIBlackboardMediator Instance { get; private set; }

        public Dictionary<LayerMask, int> AITypeCounts = new Dictionary<LayerMask, int>();
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Registers an FSM with the mediator
        /// </summary>
        public void RegisterFSM(GameObject owner, FiniteStateMachine fsm)
        {
            if (!registeredFSMs.ContainsKey(owner))
            {
                registeredFSMs.Add(owner, fsm);
                if (AITypeCounts.ContainsKey(owner.layer))
                    AITypeCounts[owner.layer]++;
                else
                    AITypeCounts.Add(owner.layer, 1);
            }
        }

        /// <summary>
        /// Unregisters an FSM when it is destroyed
        /// </summary>
        public void UnregisterFSM(GameObject owner)
        {
            if (registeredFSMs.ContainsKey(owner))
            {
                registeredFSMs.Remove(owner);
                AITypeCounts[owner.layer]--;
            }
        }

        /// <summary>
        /// Notify FSMs of an event (e.g., AI has spotted an enemy)
        /// </summary>
        public void Notify(GameObject sender, string eventType, object data)
        {
            foreach (var kvp in registeredFSMs)
            {
                if (kvp.Key != sender) // Prevent self-notification
                {
                    kvp.Value.ReceiveEvent(eventType, data);
                }
            }
        }
    }

}