using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine<T>
{
    private State<T> currentState = null;
    private Dictionary<T, State<T>> dictionary = new Dictionary<T, State<T>>();

    private T currentStateEnum;

    public void AddState(Action onStart, Action onUpdate, Action onEnd, T enumState)
    {
        State<T> state = new State<T>(onStart, onUpdate, onEnd, enumState);
        dictionary.Add(enumState, state);
    }

    public void Update()
    {
        currentState.OnUpdate?.Invoke();
    }

    public void SetState(T nextState)
    {
        SetState(dictionary[nextState]);
    }

    void SetState(State<T> nextState)
    {
        currentState?.OnEnd?.Invoke();
        currentState = nextState;
        currentState?.OnStart?.Invoke();

        currentStateEnum = nextState.enumState;
    }

    public void ResetState()
    {
        SetState(currentState);
    }
}

public class State<T>
{
    public State(Action onStart, Action onUpdate, Action onEnd, T enumState)
    {
        OnStart = onStart;
        OnUpdate = onUpdate;
        OnEnd = onEnd;
        this.enumState = enumState;
    }
    public Action OnStart;
    public Action OnUpdate;
    public Action OnEnd;
    public T enumState;
}

