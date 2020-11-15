using System.Collections;
using System.Collections.Generic;
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

    private Color32[] m_colors;
    private PixelSprite[] m_sprites;

    public LevelDataScriptable levelDataScriptable;

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
        Render();
    }

    public void OnUpdate()
    {
        if (isEditing && Input.GetMouseButton(0))
        {
            int2 sizes = cellularAutomata.sizes;
            int2 pos = gridPicker.GetGridPosition(sizes);

            int halfSize = brushSize / 2;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    int2 pixelPos = new int2(pos.x + x, pos.y + y);
                    if(ArrayHelper.InBound(pixelPos, sizes))
                    {
                        levelData.grid[pixelPos.x, pixelPos.y] = type;
                    }
                }
            }
            Render();
        }
    }

    public void Render()
    {
        GetPixelSprite(ref m_sprites);
        GetMap(out Map map);

        gridRenderer.Init(cellularAutomata.sizes);
        //lol
        gridRenderer.FillColorArray(map, m_sprites, 0, 1851936439u, ref m_colors);
        for (int i = 0; i < levelData.particleSpawners.Length; i++)
        {
            var spawner = levelData.particleSpawners[i];
            int index = ArrayHelper.PosToIndex(spawner.spawnPosition, levelData.sizes);
            m_colors[index] = Color.white;
        }
        gridRenderer.RenderToScreen(m_colors);

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
        map = new Map(levelData.grid, cellularAutomata.sizes);
    }

    public void ResetLevelData()
    {
        int2 sizes = cellularAutomata.sizes;

        //levelData = new LevelData();
        levelData.playerPosition = 0;
        levelData.particleSpawners = new ParticleSpawner[0];
        levelData.grid = new ParticleType[sizes.x, sizes.y];
        levelData.sizes = sizes;
    }

    public void OnEnd()
    {
    }
}

