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

        /// <summary>
        /// Receives events from the Mediator
        /// </summary>
        public void ReceiveEvent(string eventType, object[] data)
        {
            if (eventType == "Im gonna kill you rahh")
            {
                if (currentStateName == "Resting") return;
                if (gameObject.layer != LayerMask.NameToLayer("Passive")) return;
                GameObject hunter = (GameObject)data[0];
                if (hunter != null || Vector3.Distance(transform.position, hunter.transform.position) <= 55f)
                {
                    SwapState("Run");
                    CreatureInfo info = gameObject.GetComponent<CreatureInfo>();
                    info.target = hunter;
                    info.CurrentGroup?.Disband();
                }
            }
            else if (eventType == "Nvm Im not killing yall lol")
            {
                if (currentStateName == "Resting") return;
                if (gameObject.layer != LayerMask.NameToLayer("Passive")) return;
                if (gameObject.GetComponent<CreatureInfo>().target == (GameObject)data[0])
                {
                    SwapState("Idle");
                    gameObject.GetComponent<CreatureInfo>().target = null;
                }
            }
            else if (eventType == "I found food")
            {
                if (currentStateName == "Resting") return;
                if (gameObject.layer != LayerMask.NameToLayer("Passive")) return;
                if (gameObject.GetComponent<CreatureInfo>().CurrentGroup == null) return;
                CreatureInfo info = (CreatureInfo)data[0];
                if (gameObject.GetComponent<CreatureInfo>().CurrentGroup == info.CurrentGroup)
                {
                    SwapState("Hunt");
                    gameObject.GetComponent<CreatureInfo>().target = info.target;
                }
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