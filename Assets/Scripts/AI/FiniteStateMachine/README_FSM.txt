= FINITE STATE MACHINE = 

- I believe we're all aware of what an FSM is so I won't explain it

- To start, add the FiniteStateMachine component to the gameObject/AI entity
- Add all your states (they should derive from BaseState and should be ScriptableObjects) and ensure they each have unique IDs
- In your states, the functions should pass in the attached FSM as a parameter
- You can use the FSM to switch states by calling the state machine's SwapState() function and passing in the desired ID to switch to

- Feel free to ask me for any inqueries regarding this systm

- Aaron