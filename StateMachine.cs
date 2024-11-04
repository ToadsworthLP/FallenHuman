using System;
using System.Collections.Generic;

namespace FallenHuman;

public class StateMachine<T> where T : class, IStateMachineTarget
{
    private IDictionary<IState<T>, int> stateToIdMap = new Dictionary<IState<T>, int>();
    private IDictionary<int, IState<T>> idToStateMap = new Dictionary<int, IState<T>>();
    private int nextFreeStateId = 0;

    public void RegisterStates(params IState<T>[] states)
    {
        foreach (IState<T> state in states)
        {
            RegisterState(state);
        }
    }

    public void RegisterState(IState<T> state)
    {
        stateToIdMap.Add(state, nextFreeStateId);
        idToStateMap.Add(nextFreeStateId, state);
        nextFreeStateId++;
    }

    public void ChangeState(IState<T> state, T target)
    {
        if (!stateToIdMap.ContainsKey(state))
        {
            throw new ArgumentException("Failed to change state: The given state is not registered with this state machine.");
        }

        target.CurrentStateId = stateToIdMap[state];
    }

    public void Update(T target)
    {
        if(target.LastStateId != target.CurrentStateId) ChangeState(target.CurrentStateId, target);
        target.LastStateId = target.CurrentStateId;
        
        if(target.CurrentStateId < 0) return;
        
        IState<T> currentState = idToStateMap[target.CurrentStateId];
        currentState.Update(new StateUpdateContext<T>(this, target));

        target.StateTimer += 1f;
    }

    private void ChangeState(int newStateId, T target)
    {
        IState<T> nextState = idToStateMap[newStateId];
        IState<T> previousState = idToStateMap[target.CurrentStateId];

        if (target.CurrentStateId >= 0)
        {
            previousState.Exit(new StateExitContext<T>(this, target, nextState));
        }
        
        target.StateTimer = 0;
        target.LastStateId = target.CurrentStateId;
        target.CurrentStateId = newStateId;
        
        nextState.Enter(new StateEnterContext<T>(this, target, previousState));
    }
}

public interface IStateMachineTarget
{
    int CurrentStateId { get; set; }
    int LastStateId { get; set; }
    float StateTimer { get; set; }
}

public interface IState<T> where T : class, IStateMachineTarget
{
    void Enter(StateEnterContext<T> context);
    void Update(StateUpdateContext<T> context);
    void Exit(StateExitContext<T> context);
}

public struct StateUpdateContext<T> where T : class, IStateMachineTarget
{
    public StateMachine<T> StateMachine { get; set; }
    public T Target { get; set; }

    public StateUpdateContext(StateMachine<T> stateMachine, T target)
    {
        StateMachine = stateMachine;
        Target = target;
    }
}

public struct StateEnterContext<T> where T : class, IStateMachineTarget
{
    public StateMachine<T> StateMachine { get; set; }
    public T Target { get; set; }
    public IState<T> PreviousState { get; set; }

    public StateEnterContext(StateMachine<T> stateMachine, T target, IState<T> previousState)
    {
        StateMachine = stateMachine;
        Target = target;
        PreviousState = previousState;
    }
}

public struct StateExitContext<T> where T : class, IStateMachineTarget
{
    public StateMachine<T> StateMachine { get; set; }
    public T Target { get; set; }
    public IState<T> NextState { get; set; }

    public StateExitContext(StateMachine<T> stateMachine, T target, IState<T> nextState)
    {
        StateMachine = stateMachine;
        Target = target;
        NextState = nextState;
    }
}