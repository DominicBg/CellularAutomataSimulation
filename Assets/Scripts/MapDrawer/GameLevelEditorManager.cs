﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(GridPicker))]
public class GameLevelEditorManager : MonoBehaviour, FiniteStateMachine.State
{
    [Header("Editing")]
    public bool isEditing;
    public ParticleType type;
    public int brushSize = 2;

    [Header("Managers")]
    public GameLevelManager cellularAutomata;
    public GridPicker gridPicker;
    public GridRenderer gridRenderer;

    [Header("LevelData")]
    public LevelData levelData;

    private PixelSprite[] m_sprites;

    public LevelDataScriptable levelDataScriptable;
    public Texture2D debugTexture;

    InputCommand input = new InputCommand();

    TickBlock tickBlock;

    Stack<List<ParticleChange>> controlZ = new Stack<List<ParticleChange>>(50);
    List<ParticleChange> currentList;
    HashSet<int2> dirtyPixels = new HashSet<int2>();
    bool isRecording;

    private void OnValidate()
    {
        gridPicker = GetComponent<GridPicker>();
        cellularAutomata = FindObjectOfType<GameLevelManager>();
        gridRenderer = FindObjectOfType<GridRenderer>();
    }

    public void OnStart()
    {
        GameManager.Instance.currentLevel = levelDataScriptable;
        levelData = levelDataScriptable.LoadLevel();
        tickBlock.Init();
        input.CreateInput(KeyCode.Z);
    }

    public void OnUpdate()
    {
        input.Update();

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
            int2 pos = gridPicker.GetGridPosition(sizes);

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
        else if(input.IsButtonDown(KeyCode.Z))
        {
            var changes = controlZ.Pop();
            for (int i = changes.Count - 1; i >= 0; i--)
            {
                ParticleChange change = changes[i];
                levelData.grid[change.position.x, change.position.y] = change.previousType;
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
                previousType = levelData.grid[pixelPos.x, pixelPos.y],
            };

            if(!dirtyPixels.Contains(pixelPos))
            {
                dirtyPixels.Add(pixelPos);
                currentList.Add(particleChange);
            }
            levelData.grid[pixelPos.x, pixelPos.y] = type;
        }
    }

    public void OnRender()
    {
        tickBlock.UpdateTick();

        GetPixelSprite(ref m_sprites);
        GetMap(out Map map);

        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColor);
        GridRenderer.ApplyMapPixels(ref outputColor, map, tickBlock);
        GridRenderer.ApplySprites(ref outputColor, m_sprites);

        if (debugTexture != null)
            GridRenderer.ApplyTextureToColor(ref outputColor, debugTexture);

        for (int i = 0; i < levelData.particleSpawners.Length; i++)
        {
            var spawner = levelData.particleSpawners[i];
            int index = ArrayHelper.PosToIndex(spawner.spawnPosition, levelData.sizes);
            outputColor[index] = Color.white;
        }

        GridRenderer.RenderToScreen(outputColor);

        for (int i = 0; i < m_sprites.Length; i++)
        {
            m_sprites[i].Dispose();
        }
        map.Dispose();
    }

    void GetPixelSprite(ref PixelSprite[] pixelSprite)
    {
        if (pixelSprite == null || pixelSprite.Length != 2)
            pixelSprite = new PixelSprite[2];

        pixelSprite[0] = new PixelSprite(levelData.playerPosition, levelData.playerTexture);
        pixelSprite[1] = new PixelSprite(levelData.shuttlePosition, levelData.shuttleTexture);
    }

    void GetMap(out Map map)
    {
        map = new Map(levelData.grid, GameManager.GridSizes);
    }

    public void ResetLevelData()
    {
        int2 sizes = GameManager.GridSizes;
        levelData.playerPosition = 0;
        levelData.particleSpawners = new ParticleSpawner[0];
        levelData.grid = new ParticleType[sizes.x, sizes.y];
        levelData.sizes = sizes;
    }

    public void OnEnd()
    {
    }

    public struct ParticleChange
    {
        public int2 position;
        public ParticleType previousType;
    }
}

