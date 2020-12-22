using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FiniteStateMachine;
using Unity.Mathematics;
using Unity.Jobs;
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

    public static readonly int InnerLoopBatchCount = 100;
    public static int FPS => Instance.desiredFPS;
    public static float deltaTime => Instance.frameDuration;

    public static PhysiXVIISetings PhysiXVIISetings => Instance.physiXVIIScriptable.settings;
    public static ParticleBehaviour ParticleBehaviour => Instance.particleBehaviourScriptable.particleBehaviour;

    public static GameManager Instance;

    public enum GameStateEnum { MainMenu, Overworld, Level, LevelEditor }

    StateMachine<GameStateEnum> m_stateMachine;

    //Rename more manager stuff
    [SerializeField] GameLevelManager gameLevelManager = default;
    [SerializeField] GameLevelEditorManager gameLevelEditorManager = default;
    [SerializeField] GameOverworldManager gameOverworldManager = default;
    [SerializeField] GameMainMenuManager gameMainMenuManager = default;

    public GameStateEnum firstState;
    public int desiredFPS;
    //public LevelContainer currentLevelContainer;
    //public LevelDataScriptable levelData;
    public WorldLevel worldLevel;
    public WorldLevel GetWorldLevelInstance() => Instantiate(worldLevel);


    [SerializeField] PhysiXVIIScriptable physiXVIIScriptable = default;
    [SerializeField] ParticleBehaviourScriptable particleBehaviourScriptable = default;

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

        m_stateMachine.SetState(firstState);
    }

    private void Update()
    {
        currentDeltaTime += Time.deltaTime;
        if(currentDeltaTime >= frameDuration)
        {
            InputCommand.Update();
            m_stateMachine.Update();
            m_stateMachine.Render();
            currentDeltaTime -= frameDuration;

            //Safety
            if(currentDeltaTime >= frameDuration)
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

    public void EditorPingManager(State state)
    {
#if UNITY_EDITOR
        Selection.activeObject = (MonoBehaviour)state;
        EditorGUIUtility.PingObject(Selection.activeObject);
#endif
    }
}
