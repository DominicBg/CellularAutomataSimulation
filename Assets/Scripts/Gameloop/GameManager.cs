using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    public static readonly int2 GridSizes = new int2(100, 100);
    public static readonly int GridLength = GridSizes.x * GridSizes.y;

    public static GameManager Instance;

    public enum GameStateEnum { Menu, Overworld, Level, LevelEditor }

    StateMachine<GameStateEnum> m_stateMachine;

    //Rename more manager stuff
    [SerializeField] GameLevelManager gameLevelManager;
    [SerializeField] GameLevelEditorManager gameLevelEditorManager;
    [SerializeField] GameOverworldManager gameOverworldManager;

    public LevelDataScriptable currentLevel;

    private void Awake()
    {
        Instance = this;
    }
    private void OnDestroy()
    {
        m_stateMachine.ForceClose();
    }
    private void Start()
    {
        m_stateMachine = new StateMachine<GameStateEnum>();

        m_stateMachine.AddState(gameLevelManager, GameStateEnum.Level, EditorPingManager);
        m_stateMachine.AddState(gameLevelEditorManager, GameStateEnum.LevelEditor, EditorPingManager);
        m_stateMachine.AddState(gameOverworldManager, GameStateEnum.Overworld, EditorPingManager);

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
    public void SetOverworld()
    {
        SetState(GameStateEnum.Overworld);
    }
    public void SetMainMenu()
    {
        SetState(GameStateEnum.Menu);
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
