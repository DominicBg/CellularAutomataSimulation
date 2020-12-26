using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine
{
    public class StateMachine<T>
    {
        private State currentState = null;
        private State previousState = null;
        private Dictionary<T, State> dictionary = new Dictionary<T, State>();

        private T currentStateEnum;

        Action<State> m_onStartCallback;
        Action<State> m_onUpdateCallback;
        Action<State> m_onEndCallback; 

        public void AddState(State state, T enumState, Action<State> OnStartCallback = null, Action<State> OnUpdateCallback = null, Action<State> OnEndCallback = null)
        {
            dictionary.Add(enumState, state);
            m_onStartCallback = OnStartCallback;
            m_onUpdateCallback = OnUpdateCallback;
            m_onEndCallback = OnEndCallback;
        }

        public void Update()
        {
            currentState.OnUpdate();
            m_onUpdateCallback?.Invoke(currentState);
            previousState = currentState;
        }

        public void Render()
        {
            //If state changed in gameplay, don't render it
            if(currentState == previousState)
                currentState.OnRender();
        }

        public void SetState(T enumState)
        {
            State state = dictionary[enumState];

            currentState?.OnEnd();
            m_onEndCallback?.Invoke(currentState);

            currentState = state;

            currentState?.OnStart();
            m_onStartCallback?.Invoke(currentState);

            currentStateEnum = enumState;
        }

        public void ResetState()
        {
            SetState(currentStateEnum);
        }

        public void ForceClose()
        {
            currentState?.OnEnd();
        }
    }

    public interface State
    {
        void OnStart();
        void OnUpdate();
        void OnRender();
        void OnEnd();
    }
}
