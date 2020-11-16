using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Overworld1", menuName = "Overworlds/Overworld 1", order = 1)]
public class Overworld1 : OverworldBase
{
    public Texture2D backgroundTexture;

    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors)
    {
        GetBackgroundFromTexture(out backgroundColors, backgroundTexture);
    }
}
