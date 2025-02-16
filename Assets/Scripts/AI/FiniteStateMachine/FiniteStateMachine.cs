using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple finite state machine component for our entities
/// </summary>
public sealed class FiniteStateMachine : MonoBehaviour
{
    private Dictionary<string, BaseState> finiteStates = new Dictionary<string, BaseState>(); // Store key-pair values of strings to our states, allowing us to transition between the different finite states easier and efficiently
    [SerializeField] private FiniteStateData[] finiteStateData; // This should be set in the inspector
    private BaseState currentState = null;  // The currently active state that is defining the AI's behaviour
    private BaseState nextState = null;     // The queued state to swap to, this will allow the current state to finish it's code execution and only swap on the next frame

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
        if (!finiteStates.ContainsKey(newStateID)) {  return false; }
        nextState = finiteStates[newStateID];
        return true;
    }

}

[System.Serializable]
public class FiniteStateData
{
    public string stateID;
    public BaseState stateData;
}
