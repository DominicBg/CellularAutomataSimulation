using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(GridPicker))]
public class LevelEditor : MonoBehaviour
{
    [Header("Editing")]
    public bool isEditing;
    public ParticleType type;
    public int brushSize = 2;

    [Header("Managers")]
    public CellularAutomata cellularAutomata;
    public GridPicker gridPicker;
    public GridRenderer gridRenderer;

    [Header("LevelData")]
    public LevelData levelData;
    public LevelDataScriptable levelDataScriptable;

    private Color32[] m_colors;

    private void OnValidate()
    {
        gridPicker = GetComponent<GridPicker>();
        cellularAutomata = FindObjectOfType<CellularAutomata>();
        gridRenderer = FindObjectOfType<GridRenderer>();
    }

    public void OnStart()
    {
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
        GetPixelSprite(out PixelSprite pixelSprite);
        GetMap(out Map map);

        gridRenderer.Init(cellularAutomata.sizes);
        //lol
        gridRenderer.FillColorArray(map, pixelSprite, 0, 1851936439u, ref m_colors);
        for (int i = 0; i < levelData.particleSpawners.Length; i++)
        {
            var spawner = levelData.particleSpawners[i];
            int index = ArrayHelper.PosToIndex(spawner.spawnPosition, levelData.sizes);
            m_colors[index] = Color.white;
        }
        gridRenderer.RenderToScreen(m_colors);


        pixelSprite.Dispose();
        map.Dispose();
    }

    void GetPixelSprite(out PixelSprite pixelSprite)
    {
        pixelSprite = new PixelSprite(levelData.playerPosition, levelData.playerTexture);
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
}

