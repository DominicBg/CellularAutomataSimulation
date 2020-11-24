using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Overworld2", menuName = "Overworlds/Overworld 2", order = 1)]
public class Overworld2 : OverworldBase
{
    public Texture2D backgroundTexture;

    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock)
    {
        GetBackgroundFromTexture(out backgroundColors, backgroundTexture);
    } 
}
