using Assets.Scripts.AI.FiniteStateMachine;
using UnityEngine;

/// <summary>
/// An abstract class for our finite state data, each finite state machine will store a list of them
/// </summary>
public abstract class BaseState : ScriptableObject
{
    /// <summary>
    /// This will run once at the start of every frame, even if the state is not currently active
    /// </summary>
    /// <param name="fsm">A reference to the state machine this state is attached, allowing us to directly modify the gameObject</param>
    public virtual void OnInit(FiniteStateMachine fsm) { return; }

    /// <summary>
    /// This will run once when this state is set as the new active state
    /// </summary>
    /// <param name="fsm">A reference to the state machine this state is attached, allowing us to directly modify the gameObject</param>
    public abstract void OnStateEnter(FiniteStateMachine fsm);

    /// <summary>
    /// This will run once when this state is swapped out for a new active state
    /// </summary>
    /// <param name="fsm">A reference to the state machine this state is attached, allowing us to directly modify the gameObject</param>
    public abstract void OnStateLeave(FiniteStateMachine fsm);

    /// <summary>
    /// This will run once per frame if this state is the currently active state in the FSM
    /// </summary>
    /// <param name="fsm">A reference to the state machine this state is attached, allowing us to directly modify the gameObject</param>
    public abstract void WhileStateActive(FiniteStateMachine fsm);

    /// <summary>
    /// This will run once per frame in the LateUpdate loop if this state is the currently active state in the FSM
    /// </summary>
    /// <param name="fsm">A reference to the state machine this state is attached, allowing us to directly modify the gameObject</param>
    public virtual void WhileStateActiveLate(FiniteStateMachine fsm) { return; }

    /// <summary>
    /// This will run once per frame in the FixedUpdate loop if this state is the currently active state in the FSM
    /// </summary>
    /// <param name="fsm">A reference to the state machine this state is attached, allowing us to directly modify the gameObject</param>
    public virtual void WhileStateActiveFixed(FiniteStateMachine fsm) { return; }

}
