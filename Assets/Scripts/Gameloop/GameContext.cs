using FiniteStateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameManager;

public abstract class GameContext: IGameState
{
    public virtual GameStateEnum ReturnState => GameStateEnum.Level;
    public abstract void OnStart();
    public abstract void OnUpdate();

    public abstract void OnRender();
    public abstract void OnEnd();

    protected void ExitContext()
    {
        GameManager.Instance.ExitContext();
    }
}
