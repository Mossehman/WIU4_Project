using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    /// <summary>
    /// A simple finite state machine component for our entities
    /// </summary>
    public sealed class FiniteStateMachine : MonoBehaviour
    {
        private Dictionary<string, BaseState> finiteStates = new Dictionary<string, BaseState>(); // Store key-pair values of strings to our states, allowing us to transition between the different finite states easier and efficiently
        [SerializeField] private FiniteStateData[] finiteStateData; // This should be set in the inspector
        private BaseState currentState = null;  // The currently active state that is defining the AI's behaviour
        private BaseState nextState = null;     // The queued state to swap to, this will allow the current state to finish it's code execution and only swap on the next frame

        [Header("Debugging")]   // note: since the active state only actually switches on the start of the next frame, this will technically be 1 frame early
        [SerializeField] private string currentStateName = string.Empty;
        [SerializeField] private string previousStateName = string.Empty;
        public string GetPreviousStateName() => previousStateName;
        public string GetCurrentStateName() => currentStateName;

        private void Awake()
        {
            nextState = null;
            if (finiteStateData == null)
            {
                Debug.LogError("Finite State Machine data was null! Did you forget to add state data into the component?");
                return;
            }

            foreach (var state in finiteStateData)
            {
                // if a state with the same ID already exists, do not add
                if (finiteStates.ContainsKey(state.stateID)) { Debug.LogError("Duplicate state name detected: " + state.stateID + ", unable to add to dictionary!"); continue; }

                // create a copy/instance of this scriptable object to avoid having all states reference the same instance
                BaseState stateInstance = Instantiate(state.stateData);
                stateInstance.OnInit(this);
                finiteStates.TryAdd(state.stateID, stateInstance);


            }
            if (nextState == null)
            {
                currentState = finiteStates[finiteStateData[0].stateID];
                finiteStates[finiteStateData[0].stateID].OnStateEnter(this);
            }
        }

        private void Update()
        {
            if (nextState != null)
            {
                // swap the current state with the next state if our next state isn't null
                currentState.OnStateLeave(this);
                currentState = nextState;
                currentState.OnStateEnter(this);
                nextState = null;
            }

            currentState.WhileStateActive(this);
        }

        private void LateUpdate()
        {
            currentState.WhileStateActiveLate(this);
        }

        private void FixedUpdate()
        {
            currentState.WhileStateActiveFixed(this);
        }

        public bool SwapState(string newStateID)
        {
            if (!finiteStates.ContainsKey(newStateID))
            {
                Debug.LogWarning("State ID cannot be found");
                return false;
            }
            previousStateName = currentStateName;
            nextState = finiteStates[newStateID];
            currentStateName = newStateID;
            return true;
        }

        public void ForceSwapState(string newStateID, GameObject newTarget)
        {
            if (!finiteStates.ContainsKey(newStateID))
            {
                Debug.LogWarning("State ID cannot be found: " + newStateID);
                return;
            }

            // Immediately transition
            currentState.OnStateLeave(this);
            currentState = finiteStates[newStateID];
            currentState.OnStateEnter(this);

            // Set new target if needed
            CreatureInfo info = GetComponent<CreatureInfo>();
            if (info != null)
            {
                info.target = newTarget;
            }

            // Update debug info
            previousStateName = currentStateName;
            currentStateName = newStateID;
        }

        /// <summary>
        /// Receives events from the Mediator and updates the state accordingly
        /// </summary>
        public void ReceiveEvent(string eventType, object[] data)
        {
            CreatureInfo info = gameObject.GetComponent<CreatureInfo>();
            if (info == null) return; // Safety check

            Group currentGroup = info.CurrentGroup;

            switch (eventType)
            {
                case "Im gonna kill you rahh":
                    if (currentStateName == "Resting" || gameObject.layer != LayerMask.NameToLayer("Passive")) return;

                    GameObject hunter = data[0] as GameObject;
                    if (hunter != null && Vector3.Distance(transform.position, hunter.transform.position) <= 55f)
                    {
                        if (currentGroup != null)
                        {
                            foreach (CreatureInfo member in currentGroup.Members)
                            {
                                if (member != null)
                                {
                                    member.fsm.ForceSwapState("Run", hunter);
                                }
                            }
                        }
                        else
                        {
                            ForceSwapState("Run", hunter);
                        }
                    }
                    break;

                case "Nvm Im not killing yall lol":
                    if (currentStateName == "Resting" || gameObject.layer != LayerMask.NameToLayer("Passive")) return;

                    GameObject previousThreat = data[0] as GameObject;
                    if (info.target == previousThreat)
                    {
                        if (currentGroup != null)
                        {
                            List<CreatureInfo> grp = currentGroup.Members;
                            foreach (CreatureInfo member in grp)
                            {
                                if (member != null)
                                {
                                    member.fsm.ForceSwapState("Idle", null);
                                }
                            }
                        }
                        else
                        {
                            ForceSwapState("Idle", null);
                        }
                    }
                    break;

                case "I found food":
                    if (currentStateName == "Resting" || gameObject.layer != LayerMask.NameToLayer("Passive")) return;
                    if (currentGroup == null) return;

                    CreatureInfo foodFinder = data[0] as CreatureInfo;
                    if (foodFinder != null && foodFinder.CurrentGroup == currentGroup)
                    {
                        foreach (CreatureInfo member in currentGroup.Members)
                        {
                            if (member != null)
                            {
                                member.fsm.ForceSwapState("Hunt", foodFinder.target);
                            }
                        }
                    }
                    break;
            }
        }

        private void OnDestroy()
        {
            AIBlackboardMediator.Instance.UnregisterFSM(gameObject);
        }
    }

    [System.Serializable]
    public class FiniteStateData
    {
        public string stateID;
        public BaseState stateData;
    }
}