using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class GameLevelEditorManager : MonoBehaviour, FiniteStateMachine.State
{
    [Header("Editing")]
    public bool isEditing;
    public ParticleType type;
    public int brushSize = 2;

    [Header("Managers")]
    public GameLevelManager gameLevelManager;
    public GridPicker gridPicker;
    public GridRenderer gridRenderer;

    [Header("LevelData")]
    public ParticleType[,] grid;

    public LevelDataScriptable levelData;
    public Texture2D debugTexture;

    LevelContainer currentLevelContainer;

    Stack<List<ParticleChange>> controlZ = new Stack<List<ParticleChange>>(50);
    List<ParticleChange> currentList;
    HashSet<int2> dirtyPixels = new HashSet<int2>();
    bool isRecording;

    private void OnValidate()
    {
        gridPicker = FindObjectOfType<GridPicker>();
        gameLevelManager = FindObjectOfType<GameLevelManager>();
        gridRenderer = FindObjectOfType<GridRenderer>();
    }

    public void OnStart()
    {
        levelData = GameManager.Instance.levelData;
        //tickBlock.Init();
        Load();
    }

    public void Load()
    {
        currentLevelContainer = levelData.LoadLevelContainer();
        grid = levelData.LoadGrid();

    }
    public void Save()
    {
        levelData.SaveGrid(grid);
    }

    public void OnUpdate()
    {
        if (!isRecording && Input.GetMouseButton(0))
        {
            currentList = new List<ParticleChange>(50);
            isRecording = true;
        }
        else if(isRecording && !Input.GetMouseButton(0))
        {
            controlZ.Push(currentList);
            dirtyPixels.Clear();
            isRecording = false;
        }

        if (isEditing && isRecording)
        {
            int2 sizes = GameManager.GridSizes;
            int2 pos = GridPicker.GetGridPosition(sizes);

            int halfSize = brushSize / 2;
            int extra = brushSize % 2 == 0 ? 0 : 1;

            for (int x = -halfSize; x < halfSize + extra; x++)
            {
                for (int y = -halfSize; y < halfSize + extra; y++)
                {
                    int2 pixelPos = new int2(pos.x + x, pos.y + y);
                    DrawPixel(sizes, pixelPos);
                }
            }
        }
        else if(InputCommand.IsButtonDown(KeyCode.Z))
        {
            var changes = controlZ.Pop();
            for (int i = changes.Count - 1; i >= 0; i--)
            {
                ParticleChange change = changes[i];
                grid[change.position.x, change.position.y] = change.previousType;
            }
        }
    }

    private void DrawPixel(int2 sizes, int2 pixelPos)
    {
        if (GridHelper.InBound(pixelPos, sizes))
        {
            ParticleChange particleChange = new ParticleChange()
            {
                position = pixelPos,
                previousType = grid[pixelPos.x, pixelPos.y],
            };

            if(!dirtyPixels.Contains(pixelPos))
            {
                dirtyPixels.Add(pixelPos);
                currentList.Add(particleChange);
            }
            grid[pixelPos.x, pixelPos.y] = type;
        }
    }

    public void OnRender()
    {
        Map map = new Map(grid, GameManager.GridSizes);
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        currentLevelContainer.Init(map);
        currentLevelContainer.OnRender(ref outputColors);

        //Color spawner
        var particleSpawner = currentLevelContainer.GetParticleSpawner();
        for (int i = 0; i < particleSpawner.Length; i++)
        {
            var spawner = particleSpawner[i];
            int index = ArrayHelper.PosToIndex(spawner.spawnPosition, GameManager.GridSizes);
            outputColors[index] = Color.white;
        }
        particleSpawner.Dispose();

        GridRenderer.RenderToScreen(outputColors);
        map.Dispose();
    }


    public void ResetLevelData()
    {
        int2 sizes = GameManager.GridSizes;
        grid = new ParticleType[sizes.x, sizes.y];

        if (currentLevelContainer != null)
            currentLevelContainer.Dispose();
        currentLevelContainer = null;
    }

    public void OnEnd()
    {
        if (currentLevelContainer != null)
            currentLevelContainer.Dispose();
        currentLevelContainer = null;
    }

    public struct ParticleChange
    {
        public int2 position;
        public ParticleType previousType;
    }
}

