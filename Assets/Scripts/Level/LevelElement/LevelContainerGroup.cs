using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class LevelContainerGroup : MonoBehaviour
{
    public LevelContainer[] levelContainers;
    public LevelBackground[] backgrounds;
    public LevelForeground[] foregrounds;

    public ILightSource[] lightSources; 

    public void OnValidate()
    {
        levelContainers = GetComponentsInChildren<LevelContainer>();
        backgrounds = GetComponents<LevelBackground>();
        foregrounds = GetComponents<LevelForeground>();
    }
    private void Awake()
    {
        lightSources = GetComponents<ILightSource>();
    }


    public void RenderBackground(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, float2 currentLevel)
    {
        for (int i = 0; i < backgrounds.Length; i++)
            backgrounds[i].Render(ref outputcolor, ref tickBlock, currentLevel);
    }

    public void RenderForeground(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, float2 currentLevel)
    {
        for (int i = 0; i < foregrounds.Length; i++)
            foregrounds[i].Render(ref outputcolor, ref tickBlock, currentLevel);
    }
}
