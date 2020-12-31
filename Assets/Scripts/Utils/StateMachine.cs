using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FiniteStateMachine
{
    public class StateMachine<T>
    {
        private IGameState currentState = null;
        private IGameState previousState = null;
        private Dictionary<T, IGameState> dictionary = new Dictionary<T, IGameState>();

        private T currentStateEnum;

        Action<IGameState> m_onStartCallback;
        Action<IGameState> m_onUpdateCallback;
        Action<IGameState> m_onEndCallback; 

        public void AddState(IGameState state, T enumState, Action<IGameState> OnStartCallback = null, Action<IGameState> OnUpdateCallback = null, Action<IGameState> OnEndCallback = null)
        {
            if (!dictionary.ContainsKey(enumState))
                dictionary.Add(enumState, state);
            else
                dictionary[enumState] = state;

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
            IGameState state = dictionary[enumState];

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

    public interface IGameState
    {
        void OnStart();
        void OnUpdate();
        void OnRender();
        void OnEnd();
    }
}
