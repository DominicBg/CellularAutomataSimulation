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


    [Header("LevelData")]
    public ParticleType[,] grid;


    Stack<List<ParticleChange>> controlZ = new Stack<List<ParticleChange>>(50);
    List<ParticleChange> currentList;
    HashSet<int2> dirtyPixels = new HashSet<int2>();
    bool isRecording;

    public int2 levelPosition;
    public WorldLevel currentWorldLevel;

    public void OnStart()
    {
        Load();
    }

    public void Load()
    {
        if (currentWorldLevel != null)
            currentWorldLevel.Dispose();

        currentWorldLevel = GameManager.Instance.GetWorldLevelInstance();
        currentWorldLevel.LoadLevel();
    }
    public void Save()
    {


        WorldLevel prefab = GameManager.Instance.worldLevel;
        LevelContainer currentLevel = currentWorldLevel.CurrentLevel;
        LevelContainerData data = currentLevel.GetComponent<LevelContainerData>();
        data.SaveGrid(grid);

        for (int i = 0; i < prefab.levelContainerPrefabList.Length; i++)
        {
            WorldLevel.LevelPosition levelPos = prefab.levelContainerPrefabList[i];
            if (math.all(levelPos.position == levelPosition))
            {
                string path = AssetDatabase.GetAssetPath(levelPos.levelContainerPrefab);               
                PrefabUtility.SaveAsPrefabAsset(data.gameObject, path);
                AssetDatabase.SaveAssets();
                Debug.Log("Asset saved");
            }
        }

        //LevelContainerData levelContainerData = worldLevelPrefab.CurrentLevel.GetComponent<LevelContainerData>();
        //levelContainerData.SaveGrid(grid);
        //REDO SAVE
        //levelData.SaveGrid(grid);
    }

    public void OnUpdate()
    {
        FillGridWithCurrentMapParticle();
        DrawPixels();
        UpdateMapParticles();
    }

    private void DrawPixels()
    {
        if (!isRecording && Input.GetMouseButton(0))
        {
            currentList = new List<ParticleChange>(50);
            isRecording = true;
        }
        else if (isRecording && !Input.GetMouseButton(0))
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
        else if (InputCommand.IsButtonDown(KeyCode.Z))
        {
            var changes = controlZ.Pop();
            for (int i = changes.Count - 1; i >= 0; i--)
            {
                ParticleChange change = changes[i];
                grid[change.position.x, change.position.y] = change.previousType;
            }
        }
    }

    void FillGridWithCurrentMapParticle()
    {
        Map map = currentWorldLevel.CurrentLevel.map;
        if (grid == null || grid.Length != map.ArrayLength)
            grid = new ParticleType[map.Sizes.x, map.Sizes.y];

        for (int x = 0; x < GameManager.GridSizes.x; x++)
        {
            for (int y = 0; y < GameManager.GridSizes.y; y++)
            {
                grid[x, y] = map.GetParticleType(new int2(x, y));
            }
        }
    }

    void UpdateMapParticles()
    {
        Map map = currentWorldLevel.CurrentLevel.map;
        for (int x = 0; x < GameManager.GridSizes.x; x++)
        {
            for (int y = 0; y < GameManager.GridSizes.y; y++)
            {
                map.SetParticleType(new int2(x, y), grid[x, y]);
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
        LevelContainer currentLevelContainer = currentWorldLevel.CurrentLevel;

        //Map map = new Map(grid, GameManager.GridSizes);
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        //currentLevelContainer.Init(map);
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
        //map.Dispose();
    }


    public void ResetLevelData()
    {
        int2 sizes = GameManager.GridSizes;
        grid = new ParticleType[sizes.x, sizes.y];

        if (currentWorldLevel != null)
            currentWorldLevel.Dispose();
        currentWorldLevel = null;
    }

    public void OnEnd()
    {
        if (currentWorldLevel != null)
            currentWorldLevel.Dispose();
        currentWorldLevel = null;
    }

    public struct ParticleChange
    {
        public int2 position;
        public ParticleType previousType;
    }
}

