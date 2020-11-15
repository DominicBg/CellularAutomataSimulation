using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GameStateEnum { Menu, Overworld, Level, LevelEditor }

    StateMachine<GameStateEnum> m_stateMachine;

    //Rename more manager stuff
    [SerializeField] GameLevelManager gameLevelManager;
    [SerializeField] GameLevelEditorManager gameLevelEditorManager;


    public LevelDataScriptable currentLevel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_stateMachine = new StateMachine<GameStateEnum>();

        m_stateMachine.AddState(gameLevelManager, GameStateEnum.Level, EditorPingManager);
        m_stateMachine.AddState(gameLevelEditorManager, GameStateEnum.LevelEditor, EditorPingManager);

        m_stateMachine.SetState(GameStateEnum.Level);
    }

    private void Update()
    {
        m_stateMachine.Update();
    }

    //API
    //for buttons lol
    public void SetLevel()
    {
        SetState(GameStateEnum.Level);
    }
    public void SetLevelEditor()
    {
        SetState(GameStateEnum.LevelEditor);
    }

    public void SetState(GameStateEnum state)
    {
        m_stateMachine.SetState(state);
    }

    public void EditorPingManager(State state)
    {
#if UNITY_EDITOR
        Selection.activeObject = (MonoBehaviour)state;
        EditorGUIUtility.PingObject(Selection.activeObject);
#endif
    }
}
