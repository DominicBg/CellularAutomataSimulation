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
    LevelObject hoverObject;

    float2 fracPos;
    public float movingSpeed = 5;
    public int2 debugSize;

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
        currentWorldLevel.inDebug = true;
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
        if(GameManager.CurrentState != GameManager.GameStateEnum.LevelEditor)
        {
            Debug.LogError("You need to be in LevelEditor mode to save");
            return;
        }

        WorldLevel prefab = GameManager.Instance.worldLevelPrefab;
        pixelSceneData.SaveMap(grid, currentWorldLevel.pixelScene.map.Sizes);
        currentWorldLevel.pixelSceneData = pixelSceneData;
        currentWorldLevel.inDebug = false;
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
        if (InputCommand.IsButtonHeld(ButtonType.Action2))
            multiplier = 3;

        float2 movement = InputCommand.Direction * movingSpeed * GameManager.DeltaTime * multiplier + fracPos;
        fracPos = math.frac(movement);
        currentWorldLevel.pixelCamera.position += (int2)movement;

        FillGridWithCurrentMapParticle();
        if (isEditing)
            DrawPixels();
        else
            UpdateScenePicker();

        UpdateMapParticles();       
    }

    private void DrawPixels()
    {
        if (!isRecording && InputCommand.IsButtonHeld(ButtonType.Action1))
        {
            currentList = new List<ParticleChange>(50);
            isRecording = true;
        }
        else if (isRecording && !InputCommand.IsButtonHeld(ButtonType.Action1))
        {
            controlZ.Push(currentList);
            dirtyPixels.Clear();
            isRecording = false;
        }

        if (isRecording)
        {
            int2 pos = GridPicker.GetGridPosition(GameManager.RenderSizes) + currentWorldLevel.pixelCamera.position - GameManager.RenderSizes/2;

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
        else if (InputCommand.IsButtonDown(ButtonType.Action2Alt))
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
        {
            //copy whole map
            grid = new ParticleType[map.Sizes.x, map.Sizes.y];
            debugSize = map.Sizes;
            for (int x = 0; x < map.Sizes.x; x++)
            {
                for (int y = 0; y < map.Sizes.y; y++)
                {
                    grid[x, y] = map.GetParticleType(new int2(x, y));
                }
            }
        }

        Bound bound = currentWorldLevel.pixelCamera.GetViewingBound();
        int2 min = bound.bottomLeft;
        int2 max = bound.topRight;
        for (int x = min.x; x < max.x; x++)
        {
            for (int y = min.y; y < max.y; y++)
            {
                int2 pos = new int2(x, y);
                if(map.InBound(pos))
                    grid[x, y] = map.GetParticleType(pos);
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

        if (hoverObject != null)
            GridRenderer.DrawBound(ref pixels, hoverObject.GetBound(), currentWorldLevel.pixelCamera.position, Color.green * 0.75f);

        GridRenderer.RenderToScreen(pixels);
    }

    void DrawPreview(ref NativeArray<Color32> outputColors)
    {
        int2 pos = GridPicker.GetGridPosition(GameManager.RenderSizes);

        int halfSize = brushSize / 2;
        int extra = brushSize % 2 == 0 ? 0 : 1;

        for (int x = -halfSize; x < halfSize + extra; x++)
        {
            for (int y = -halfSize; y < halfSize + extra; y++)
            {
                int2 pixelPos = new int2(pos.x + x, pos.y + y);
                if(GridHelper.InBound(pixelPos, GameManager.RenderSizes))
                {
                    int index = ArrayHelper.PosToIndex(pixelPos, GameManager.RenderSizes);
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

    //add screen picker
    public void UpdateScenePicker()
    {
        var scene = currentWorldLevel.pixelScene;
        int2 mouseLocal = GridPicker.GetGridPosition(GameManager.RenderSizes);
        int2 pos = currentWorldLevel.pixelCamera.position - GameManager.RenderSizes/2 + mouseLocal;
        hoverObject = null;
        for (int i = 0; i < scene.levelObjects.Length; i++)
        {
            if(scene.levelObjects[i].GetBound().PointInBound(pos) && scene.levelObjects[i].GetType() != typeof(PixelCameraTransform))
            {
                hoverObject = scene.levelObjects[i];

#if UNITY_EDITOR
                //Set to mouse click? lol
                if(InputCommand.IsButtonDown(ButtonType.Action1Alt))
                {
                    Selection.activeObject = hoverObject;
                    EditorGUIUtility.PingObject(Selection.activeObject);
                }
#endif

                return;
            }
        }
        
    }
}

