using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T> where T : MonoBehaviour
{
    private Dictionary<Type, State<T>> states = new Dictionary<Type, State<T>>();
    private State<T> currentState;
    public T Owner { get; private set; }
    public State<T> CurrentState => currentState;
    public State<T> PreviousState { get; private set; }
    public State<T> InitialState { get; private set; }

    public StateMachine(T owner)
    {
        Owner = owner;
    }

    public void AddState(State<T> state)
    {
        states[state.GetType()] = state;
        state.SetStateMachine(this);
    }

    public State<T> GetState<TState>() where TState : State<T>
    {
        return states[typeof(TState)];
    }

    public bool HasState<TState>() where TState : State<T>
    {
        return states.ContainsKey(typeof(TState));
    }

    public void AddTransition(State<T> fromState, State<T> toState, Func<bool> condition)
    {
        fromState.AddTransition(new Transition<T>(toState, condition));
    }

    public void AddTransition(State<T> fromState, Func<State<T>> condition)
    {
        fromState.AddTransition(new DynamicTransition<T>(condition));
    }

    public void SetInitialState<TState>() where TState : State<T>
    {
        InitialState = states[typeof(TState)];
        currentState = states[typeof(TState)];
        currentState.Enter();
        currentState.OnStateEnter?.Invoke();
    }

    public void ChangeState<TState>() where TState : State<T>
    {
        ChangeState(states[typeof(TState)]);
    }

    private void ChangeState(State<T> newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState.OnStateExit?.Invoke();
        }

        PreviousState = currentState;
        currentState = newState;
        currentState.Enter();
        currentState.OnStateEnter?.Invoke();
    }

    public void Update()
    {
        if (currentState != null)
        {
            currentState.Update();
            currentState.OnStateUpdate?.Invoke();
            CheckTransitions();
        }
    }

    private void CheckTransitions()
    {
        bool transitioned = false;
        foreach (var transition in currentState.Transitions)
        {
            if (transition.Condition())
            {
                ChangeState(transition.ToState);
                transitioned = true;
                break;
            }
        }

        if (!transitioned)
        {
            foreach (var transition in currentState.DynamicTransitions)
            {
                var toState = transition.Condition();
                if (toState != null)
                {
                    ChangeState(toState);
                    break;
                }
            }
        }
    }
}

public abstract class State<T> where T : MonoBehaviour
{
    protected StateMachine<T> stateMachine;
    protected T owner => stateMachine.Owner;
    protected List<Transition<T>> transitions = new List<Transition<T>>();
    protected List<DynamicTransition<T>> dynamicTransitions = new List<DynamicTransition<T>>();
    public Action OnStateEnter;
    public Action OnStateUpdate;
    public Action OnStateExit;

    public IReadOnlyList<Transition<T>> Transitions => transitions;
    public IReadOnlyList<DynamicTransition<T>> DynamicTransitions => dynamicTransitions;

    public void SetStateMachine(StateMachine<T> stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public void AddTransition(Transition<T> transition)
    {
        transitions.Add(transition);
    }

    public void AddTransition(DynamicTransition<T> transition)
    {
        dynamicTransitions.Add(transition);
    }

    public virtual void EndState()
    {
    }

    public virtual void Enter()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void Exit()
    {
    }
}

public class Transition<T> where T : MonoBehaviour
{
    public State<T> ToState { get; }
    public Func<bool> Condition { get; }

    public Transition(State<T> toState, Func<bool> condition)
    {
        ToState = toState;
        Condition = condition;
    }
}

public class DynamicTransition<T> where T : MonoBehaviour
{
    public Func<State<T>> Condition { get; }

    public DynamicTransition(Func<State<T>> condition)
    {
        Condition = condition;
    }
}