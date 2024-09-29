using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Mathematics;
using Unity.Jobs;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The resolution of the game
    /// </summary>
    public static readonly int2 RenderSizes = new int2(128, 128);

    /// <summary>
    /// Correspond to the GridSizes.x * GridSizes.y
    /// </summary>
    public static readonly int GridLength = RenderSizes.x * RenderSizes.y;

    public static readonly int InnerLoopBatchCount = 100;
    public static readonly int FPS = 60;
    public static readonly float DeltaTime = 1f / FPS;

    public static PhysiXVIISetings PhysiXVIISetings => Instance.physiXVIIScriptable.settings;
    public static ParticleBehaviour ParticleBehaviour => Instance.particleBehaviourScriptable.particleBehaviour;

    public static GameManager Instance;

    public enum GameStateEnum { MainMenu, Overworld, Level, LevelEditor, Context }

    StateMachine<GameStateEnum> m_stateMachine;


    //Rename more manager stuff
    [SerializeField] GameLevelManager gameLevelManager = default;
    [SerializeField] GameLevelEditorManager gameLevelEditorManager = default;
    [SerializeField] GameOverworldManager gameOverworldManager = default;
    [SerializeField] GameMainMenuManager gameMainMenuManager = default;

    public GameStateEnum firstState;
    public WorldLevel worldLevelPrefab;
    public WorldLevel GetWorldLevelInstance() => Instantiate(worldLevelPrefab);

    public Action OnGameModeChange;
    public Action OnGameModeEnd;

    [SerializeField] PhysiXVIIScriptable physiXVIIScriptable = default;
    [SerializeField] ParticleBehaviourScriptable particleBehaviourScriptable = default;

    float currentDeltaTime;
    GameContext currentContext;

    private void Awake()
    {
        Instance = this;
    }

    void OnApplicationQuit()
    {
        if (currentContext != null)
            currentContext.OnEnd();

        m_stateMachine.ForceClose();
    }

    private void Start()
    {
        m_stateMachine = new StateMachine<GameStateEnum>();

        m_stateMachine.AddState(gameLevelManager, GameStateEnum.Level, OnStateChange, null , (state) => OnGameModeEnd.Invoke());
        m_stateMachine.AddState(gameLevelEditorManager, GameStateEnum.LevelEditor, OnStateChange, null, (state) => OnGameModeEnd.Invoke());
        m_stateMachine.AddState(gameOverworldManager, GameStateEnum.Overworld, OnStateChange, null, (state) => OnGameModeEnd.Invoke());
        m_stateMachine.AddState(gameMainMenuManager, GameStateEnum.MainMenu, OnStateChange, null, (state) => OnGameModeEnd.Invoke());

        m_stateMachine.SetState(firstState);
    }

    private void Update()
    {
        currentDeltaTime += Time.deltaTime;
        if(currentDeltaTime >= DeltaTime)
        {
            InputCommand.Update();

            if(currentContext != null)
            {
                currentContext.OnUpdate();
                currentContext?.OnRender();
            }
            else
            { 
                m_stateMachine.Update();
                m_stateMachine.Render();
            }

            currentDeltaTime -= DeltaTime;

            //Safety
            if(currentDeltaTime >= DeltaTime)
            {
                currentDeltaTime = 0;
            }
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

    public void SetContext(GameContext context)
    {
        currentContext = context;
        currentContext.OnStart();
    }
    public void ExitContext()
    {
        currentContext.OnEnd();
        currentContext = null;
    }

    public static GameStateEnum CurrentState => Instance.m_stateMachine.GetCurrentState();


    void OnStateChange(IGameState state)
    {
        OnGameModeChange?.Invoke();
        EditorPingManager(state);
    }

    void EditorPingManager(IGameState state)
    {
#if UNITY_EDITOR
        Selection.activeObject = (MonoBehaviour)state;
        EditorGUIUtility.PingObject(Selection.activeObject);
#endif
    }
}
