using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelEditorFunctions : MonoBehaviour
{
    GameLevelEditorManager editorManager;

    public int2 sizes;
    public int2 offset;

    public void Start()
    {
        editorManager = GetComponent<GameLevelEditorManager>();
    }

    [ContextMenu("Empty")]
    public void CreateEmpty()
    {
        editorManager.pixelSceneData.CreateEmpty(sizes);
        editorManager.Reload();
    }

    [ContextMenu("Randomize")]
    public void Randomize()
    {
        //Add param to that
        editorManager.pixelSceneData.CreateEmpty(sizes);
        for (int x = 0; x < sizes.x; x++)
        {
            for (int y = 0; y < sizes.y; y++)
            {
                float noiseValue = noise.cnoise(new float2(x, y) * 0.1f);
                editorManager.pixelSceneData.grid[x + y * sizes.x] = noiseValue > .25f ? ParticleType.Rock : (noiseValue > .15f) ? ParticleType.Sand : ParticleType.None;
            }
        }
        editorManager.Reload();
    }

    [ContextMenu("Rescale")]
    public void Rescale()
    {
        var grid = editorManager.pixelSceneData.grid;
        int2 oldSizes = editorManager.pixelSceneData.sizes;

        editorManager.pixelSceneData.CreateEmpty(sizes);
        var newGrid = editorManager.pixelSceneData.grid;

        //iterate over smallest
        int2 smallSizes = math.min(oldSizes, sizes);
        for (int x = 0; x < smallSizes.x; x++)
        {
            for (int y = 0; y < smallSizes.y; y++)
            {
                int index = x + y * oldSizes.x;
                int newIndex = x + y * sizes.x;
                if (newIndex < editorManager.pixelSceneData.grid.Length && index < grid.Length)
                    newGrid[newIndex] = grid[index];
            }
        }
        editorManager.pixelSceneData.grid = newGrid;
        editorManager.Reload();
    }

    [ContextMenu("OffsetMap")]
    public void OffsetMap()
    {
        var grid = editorManager.pixelSceneData.grid;
        int2 oldSizes = editorManager.pixelSceneData.sizes;

        int2 newSizes = oldSizes + offset;
        editorManager.pixelSceneData.CreateEmpty(newSizes);
        var newGrid = editorManager.pixelSceneData.grid;

        //iterate over smallest
        int2 smallSizes = math.min(oldSizes, newSizes);
        for (int x = 0; x < smallSizes.x; x++)
        {
            for (int y = 0; y < smallSizes.y; y++)
            {
                int index = x + y * oldSizes.x;
                int newIndex = (x - offset.x) + (y + offset.y) * newSizes.x;
                if (newIndex < editorManager.pixelSceneData.grid.Length && index < grid.Length)
                    newGrid[newIndex] = grid[index];
            }
        }
        editorManager.pixelSceneData.grid = newGrid;
        editorManager.Reload();
    }
}
