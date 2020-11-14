using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    public enum GameState { Menu, Overworld, Level, LevelEditor}

    StateMachine<GameState> m_stateMachine;

    //Rename more manager stuff
    public CellularAutomata cellularAutomata;
    public LevelEditor levelEditor;
    public LevelDataScriptable currentLevel;

    private void Start()
    {
        m_stateMachine = new StateMachine<GameState>();
        m_stateMachine.AddState(StartLevel, UpdateLevel, EndLevel, GameState.Level);
        m_stateMachine.AddState(StartLevelEditor, UpdateLevelEditor, EndLevelEditor, GameState.LevelEditor);
        m_stateMachine.SetState(GameState.Level);
    }

    private void Update()
    {
        m_stateMachine.Update();
    }

    //API
    //for buttons lol
    public void SetLevel()
    {
        SetState(GameState.Level);
    }
    public void SetLevelEditor()
    {
        SetState(GameState.LevelEditor);
    }

    public void SetState(GameState state)
    {
        m_stateMachine.SetState(state);
    }


    //Behaviour
    void StartLevel()
    {
        cellularAutomata.enabled = true;
        cellularAutomata.LoadLevel(currentLevel);
    }
    void UpdateLevel()
    {
        cellularAutomata.OnUpdate();
    }
    void EndLevel()
    {
        cellularAutomata.enabled = false;
    }

    void StartLevelEditor()
    {
        levelEditor.enabled = true;
        levelEditor.OnStart();
    }
    void UpdateLevelEditor()
    {
        levelEditor.OnUpdate();
    }
    void EndLevelEditor()
    {
        levelEditor.enabled = false;
    }
}
