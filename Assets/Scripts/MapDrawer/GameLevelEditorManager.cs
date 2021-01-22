using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameLevelEditorManager : MonoBehaviour, FiniteStateMachine.IGameState
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

    public WorldLevel currentWorldLevel;
    public PixelSceneData pixelSceneData;

    public float movingSpeed = 5;
    public bool inDebugView;
    public void OnStart()
    {
        controlZ.Clear();
        dirtyPixels.Clear();
        Load();
    }

    public void Load()
    {
        if (currentWorldLevel != null)
            currentWorldLevel.Dispose();


        currentWorldLevel = GameManager.Instance.GetWorldLevelInstance();
        currentWorldLevel.LoadLevel();
        pixelSceneData = currentWorldLevel.pixelSceneData;
    }
    public void Reload()
    {
        currentWorldLevel.pixelScene.map.Dispose();
        Map map = pixelSceneData.LoadMap();
        currentWorldLevel.pixelScene.map = map;
    }

    public void Save()
    {
        WorldLevel prefab = GameManager.Instance.worldLevelPrefab;
        pixelSceneData.SaveMap(grid, currentWorldLevel.pixelScene.map.Sizes);
        currentWorldLevel.pixelSceneData = pixelSceneData;

#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(prefab);               
        PrefabUtility.SaveAsPrefabAsset(currentWorldLevel.gameObject, path);
        AssetDatabase.SaveAssets();
        Debug.Log(prefab + " Asset saved");
#endif

    }

    public void OnUpdate()
    {
        float multiplier = 1;
        if (InputCommand.IsButtonHeld(KeyCode.LeftShift))
            multiplier = 3;

        currentWorldLevel.pixelCameraPos += InputCommand.Direction * movingSpeed * GameManager.DeltaTime * multiplier;

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
            int2 pos = GridPicker.GetGridPosition(GameManager.GridSizes) + (int2)currentWorldLevel.pixelCameraPos - GameManager.GridSizes/2;

            int halfSize = brushSize / 2;
            int extra = brushSize % 2 == 0 ? 0 : 1;

            for (int x = -halfSize; x < halfSize + extra; x++)
            {
                for (int y = -halfSize; y < halfSize + extra; y++)
                {
                    int2 pixelPos = new int2(pos.x + x, pos.y + y);
                    DrawPixel(currentWorldLevel.pixelScene.map.Sizes, pixelPos);
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
        Map map = currentWorldLevel.pixelScene.map;
        if (grid == null || grid.Length != map.ArrayLength)
            grid = new ParticleType[map.Sizes.x, map.Sizes.y];

        Bound bound = currentWorldLevel.pixelCamera.GetViewingBound();
        int2 min = bound.bottomLeft;
        int2 max = bound.topRight;
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                int2 pos = new int2(x, y);
                if(map.InBound(pos))
                    grid[x, y] = map.GetParticleType(new int2(x, y));
            }
        }
    }

    void UpdateMapParticles()
    {
        Map map = currentWorldLevel.pixelScene.map;
        Bound bound = currentWorldLevel.pixelCamera.GetViewingBound();
        int2 min = bound.bottomLeft;
        int2 max = bound.topRight;

        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                int2 pos = new int2(x, y);
                if (map.InBound(pos))
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
        var pixels = currentWorldLevel.GetPixelCameraRender();
        DrawPreview(ref pixels);
        GridRenderer.RenderToScreen(pixels);
    }

    void DrawPreview(ref NativeArray<Color32> outputColors)
    {
        int2 pos = GridPicker.GetGridPosition(GameManager.GridSizes);

        int halfSize = brushSize / 2;
        int extra = brushSize % 2 == 0 ? 0 : 1;

        for (int x = -halfSize; x < halfSize + extra; x++)
        {
            for (int y = -halfSize; y < halfSize + extra; y++)
            {
                int2 pixelPos = new int2(pos.x + x, pos.y + y);
                if(GridHelper.InBound(pixelPos, GameManager.GridSizes))
                {
                    int index = ArrayHelper.PosToIndex(pixelPos, GameManager.GridSizes);
                    outputColors[index] = Color.gray;
                }
            }
        }
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

