using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Overworld1", menuName = "Overworlds/Overworld 1", order = 1)]
public class Overworld1 : OverworldBase
{
    public Texture2D backgroundTexture;

    public int2 planetMiddle = 50;
    public int planetRadius = 10;
    public float fishEyeIntensity;
    public float2 rotationSpeed;
    public float rockScaling;
    public float2 noiseRep;

    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock)
    {
        GridRenderer.GetBlankTexture(out backgroundColors);

        var circlePositions = GridHelper.GetCircleAtPosition(planetMiddle, planetRadius, GameManager.GridSizes, Allocator.TempJob);
        NativeArray<Color32> circleColors = new NativeArray<Color32>(circlePositions.Length, Allocator.TempJob);
        RockRendering rockRendering = GridRenderer.Instance.particleRendering.rockRendering;

        for (int i = 0; i < circlePositions.Length; i++)
        {
            float2 rotation = rotationSpeed * tickBlock.tick;

            float2 position = MathUtils.Spherize(planetMiddle, circlePositions[i], planetRadius);
            float2 finalPos = math.lerp(circlePositions[i], position, fishEyeIntensity);

            float2 noisePosition = finalPos * rockScaling + rotation;
            float noiseValue = MathUtils.unorm(noise.cnoise(noisePosition));

            circleColors[i] = rockRendering.GetColorWithNoiseValue(noiseValue);
        }

        GridRenderer.ApplyPixels(ref backgroundColors, ref circlePositions, ref circleColors);

        circlePositions.Dispose();
        circleColors.Dispose();
    }
}
