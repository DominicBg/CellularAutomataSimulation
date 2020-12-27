using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelContainerGroup : MonoBehaviour
{
    public LevelContainer[] levelContainers;
    public LevelBackground[] backgrounds;
    //forgrounds

    public void OnValidate()
    {
        levelContainers = GetComponentsInChildren<LevelContainer>();
        backgrounds = GetComponents<LevelBackground>();
    }

    public void RenderBackground(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, float2 currentLevel)
    {
        for (int i = 0; i < backgrounds.Length; i++)
            backgrounds[i].Render(ref outputcolor, ref tickBlock, currentLevel);
    }

    public void RenderForeground(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, float2 currentLevel)
    {
        //for (int i = 0; i < backgrounds.Length; i++)
        //    backgrounds[i].Render(ref outputcolor, ref tickBlock, currentLevel);
    }
}
