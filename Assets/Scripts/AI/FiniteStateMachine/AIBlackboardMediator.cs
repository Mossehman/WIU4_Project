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

        public Dictionary<int, int> AITypeCounts = new Dictionary<int, int>();
        public int MaxAICountPerType = 12;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// Registers an FSM with the mediator, enforcing AI limits
        /// </summary>
        public void RegisterFSM(GameObject owner, FiniteStateMachine fsm)
        {
            int layer = owner.layer;

            if (AITypeCounts.ContainsKey(layer) && AITypeCounts[layer] >= MaxAICountPerType)
            {
                Debug.Log($"AI limit reached for layer {layer}. Cannot register more AIs.");
                Destroy(owner);
                return;
            }

            if (!registeredFSMs.ContainsKey(owner))
            {
                registeredFSMs.Add(owner, fsm);

                if (AITypeCounts.ContainsKey(layer))
                    AITypeCounts[layer]++;
                else
                    AITypeCounts[layer] = 1;
            }
        }

        /// <summary>
        /// Unregisters an FSM when it is destroyed
        /// </summary>
        public void UnregisterFSM(GameObject owner)
        {
            int layer = owner.layer;

            if (registeredFSMs.ContainsKey(owner))
            {
                registeredFSMs.Remove(owner);

                if (AITypeCounts.ContainsKey(layer) && AITypeCounts[layer] > 0)
                    AITypeCounts[layer]--;
            }
        }

        /// <summary>
        /// Notify FSMs of an event (e.g., AI has spotted an enemy)
        /// </summary>
        public void Notify(GameObject sender, string eventType, object[] data)
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
