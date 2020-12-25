using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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

    public WorldLevel currentWorldLevel;

    private bool inFreeView;
    public float2 viewPosition;
    public float movingSpeed = 5;
    public bool inDebugView;
    public void OnStart()
    {
        Load();
        viewPosition = currentWorldLevel.currentLevelPosition;
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
        if(inFreeView)
        {
            Debug.Log("Leave Freeview before saving");
            return;
        }

        WorldLevel prefab = GameManager.Instance.worldLevel;
        LevelContainer currentLevel = currentWorldLevel.levels[(int2)viewPosition];
        LevelContainerData data = currentLevel.GetComponent<LevelContainerData>();
        data.SaveGrid(grid);

        for (int i = 0; i < prefab.levelContainerPrefabList.Length; i++)
        {
            if (math.all(prefab.levelContainerPrefabList[i].levelPosition == (int2)viewPosition))
            {
                string path = AssetDatabase.GetAssetPath(prefab.levelContainerPrefabList[i]);               
                PrefabUtility.SaveAsPrefabAsset(data.gameObject, path);
                AssetDatabase.SaveAssets();
                Debug.Log("Asset saved");
            }
        }
    }

    public void OnUpdate()
    {
        viewPosition += (float2)InputCommand.Direction * movingSpeed * GameManager.deltaTime;

        if(Input.GetMouseButton(1))
        {
            viewPosition = (int2)math.round(viewPosition);
        }
        inFreeView = math.any(viewPosition % 1 != 0);
          

        if(!inFreeView)
        {
            FillGridWithCurrentMapParticle();
            DrawPixels();
            UpdateMapParticles();
        }
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
        LevelContainer levelContainer;
        if (!currentWorldLevel.levels.TryGetValue((int2)viewPosition, out levelContainer))
        {
            return;
        }

        Map map = levelContainer.map;
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
        LevelContainer levelContainer;
        if (!currentWorldLevel.levels.TryGetValue((int2)viewPosition, out levelContainer))
        {
            return;
        }

        Map map = levelContainer.map;
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
        if (inFreeView)
        {
            int2 min = new int2((int)math.floor(viewPosition.x), (int)math.floor(viewPosition.y));
            int2 max = new int2((int)math.ceil(viewPosition.x), (int)math.ceil(viewPosition.y));

            if(min.x == max.x)
            {
                max.x++;
            }
            if (min.y == max.y)
            {
                max.y++;
            }

            int2 pos1 = new int2(min.x, min.y);
            int2 pos2 = new int2(max.x, min.y);
            int2 pos3 = new int2(min.x, max.y);
            int2 pos4 = new int2(max.x, max.y);
            var colors1 = RenderLevelContainer(pos1);
            var colors2 = RenderLevelContainer(pos2);
            var colors3 = RenderLevelContainer(pos3);
            var colors4 = RenderLevelContainer(pos4);

            var horizontalOutput = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            new SlideTransitionJob()
            {
                firstImage = colors1,
                secondImage = colors2,
                outputColors = horizontalOutput,
                t = math.frac(viewPosition.x),
                isHorizontal = true
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

            var horizontal2Output = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            new SlideTransitionJob()
            {
                firstImage = colors3,
                secondImage = colors4,
                outputColors = horizontal2Output,
                t = math.frac(viewPosition.x),
                isHorizontal = true
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

            var finalOutput = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);
            new SlideTransitionJob()
            {
                firstImage = horizontalOutput,
                secondImage = horizontal2Output,
                outputColors = finalOutput,
                t = math.frac(viewPosition.y),
                isHorizontal = false
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

            colors1.Dispose();
            colors2.Dispose();
            colors3.Dispose();
            colors4.Dispose();
            horizontalOutput.Dispose();
            horizontal2Output.Dispose();
            GridRenderer.RenderToScreen(finalOutput);
        }
        else
        {
            var colors = RenderLevelContainer((int2)viewPosition);
            GridRenderer.RenderToScreen(colors);
        }
    }

    NativeArray<Color32> RenderLevelContainer(int2 position)
    {
        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);

        if(currentWorldLevel.levels.TryGetValue(position, out LevelContainer levelContainer))
        {
            levelContainer.OnRender(ref outputColors, inDebugView);

            //Color spawner
            var particleSpawner = levelContainer.GetParticleSpawner();
            for (int i = 0; i < particleSpawner.Length; i++)
            {
                var spawner = particleSpawner[i];
                int index = ArrayHelper.PosToIndex(spawner.spawnPosition, GameManager.GridSizes);
                outputColors[index] = Color.white;
            }
            particleSpawner.Dispose();
        }

        return outputColors;
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

