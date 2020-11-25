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
    /// <summary>
    /// The resolution of the game
    /// </summary>
    public static readonly int2 GridSizes = new int2(100, 100);

    /// <summary>
    /// Correspond to the GridSizes.x * GridSizes.y
    /// </summary>
    public static readonly int GridLength = GridSizes.x * GridSizes.y;

    /// <summary>
    /// Correspond to the float size of one grid cell
    /// </summary>
    public static readonly float2 GridScale = 3;

    public static GameManager Instance;

    public enum GameStateEnum { MainMenu, Overworld, Level, LevelEditor }

    StateMachine<GameStateEnum> m_stateMachine;

    //Rename more manager stuff
    [SerializeField] GameLevelManager gameLevelManager;
    [SerializeField] GameLevelEditorManager gameLevelEditorManager;
    [SerializeField] GameOverworldManager gameOverworldManager;
    [SerializeField] GameMainMenuManager gameMainMenuManager;

    public int desiredFPS;
    public LevelDataScriptable currentLevel;

    float currentDeltaTime;
    float frameDuration;

    private void OnValidate()
    {
        frameDuration = 1f / desiredFPS;
    }

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
        m_stateMachine.AddState(gameMainMenuManager, GameStateEnum.MainMenu, EditorPingManager);

        m_stateMachine.SetState(GameStateEnum.Level);
    }

    private void Update()
    {
        currentDeltaTime += Time.deltaTime;
        while (currentDeltaTime >= frameDuration)
        {
            m_stateMachine.Update();
            m_stateMachine.Render();
            currentDeltaTime -= frameDuration;
        }
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
        SetState(GameStateEnum.MainMenu);
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
