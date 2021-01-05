using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "Overworld1", menuName = "Overworlds/Overworld 1", order = 1)]
public class Overworld1 : OverworldBase
{
    public PlanetSettings planetSettings;
    public SandSettings sandSettings;
    public StarBackgroundRendering starSettings;
    public Color32 backgroundColor;

    [System.Serializable]
    public struct PlanetSettings
    {
        public int2 position;
        public int radius;
        public float spherizeIntensity;
        public float2 rotationSpeed;
        public float rockScaling;
    }

    [System.Serializable]
    public struct SandSettings
    {
        public int radius;
        public float minSandRatio;
        public float maxSandRatio;
        public float sandRatioRadiusOffset;
        public float scaling;
        public float2 rotationSpeed;
        public float spherizeIntensity;
        public float backShadow;

        //test
        public float2 periodicity;
        public float rotSpeed;
    }

    public override void GetBackgroundColors(out NativeArray<Color32> backgroundColors, ref TickBlock tickBlock)
    {
        GridRenderer.GetBlankTexture(out backgroundColors, backgroundColor);

        new ShiningStarBackgroundJob()
        {
            colors = backgroundColors,
            maxSizes = GameManager.GridSizes,
            tick = tickBlock.tick,
            settings = starSettings
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();


        new RenderPlanetJob()
        {
            outputColor = backgroundColors,
            planetSettings = planetSettings,
            sandSettings = sandSettings,
            rockRendering = GridRenderer.Instance.particleRendering.rockRendering,
            sandRendering = GridRenderer.Instance.particleRendering.sandRendering,
            sizes = GameManager.GridSizes,
            tickBlock = tickBlock
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }
 
    [BurstCompile]
    public struct RenderPlanetJob : IJobParallelFor
    {
        public NativeArray<Color32> outputColor;

        public PlanetSettings planetSettings;
        public SandSettings sandSettings;
        public int2 sizes;

        public RockRendering rockRendering;
        public SandRendering sandRendering;
        public TickBlock tickBlock;

        public void Execute(int index)
        {
            int2 pos = ArrayHelper.IndexToPos(index, sizes);
            GenerateFloatingSand(index, sizes - pos, sandSettings.backShadow);
            GenerateMainPlanet(index, pos);
            GenerateFloatingSand(index, pos, 1);
        }

        void GenerateMainPlanet(int index, int2 position)
        {
            //Outside of the sphere
            if (math.distancesq(position, planetSettings.position) > planetSettings.radius * planetSettings.radius)
                return;

            float2 spherizePosition = MathUtils.Spherize(planetSettings.rotationSpeed, position, planetSettings.radius);
            float2 finalPos = math.lerp(position, spherizePosition, planetSettings.spherizeIntensity);

            float2 rotation = planetSettings.rotationSpeed * tickBlock.tick;
            float2 noisePosition = finalPos * planetSettings.rockScaling + rotation;
            float noiseValue = MathUtils.unorm(noise.cnoise(noisePosition));

            outputColor[index] = rockRendering.color4Dither.GetColorWitLightValue(noiseValue, position);
        }

        void GenerateFloatingSand(int index, int2 position, float alpha)
        {
            int2 planetCenter = planetSettings.position;

            //Outside of the sphere
            if (math.distancesq(position, planetCenter) > sandSettings.radius * sandSettings.radius)
                return;

            float2 rotation = sandSettings.rotationSpeed * tickBlock.tick;
            float distance = math.distance(position, planetCenter);
            float ratio = distance / sandSettings.radius;
            float threshold = math.remap(0, sandSettings.sandRatioRadiusOffset, sandSettings.minSandRatio, sandSettings.maxSandRatio, ratio);

            float2 spherizePos = MathUtils.Spherize(planetCenter, position, sandSettings.radius);
            float2 finalPosition = math.lerp(position, spherizePos, sandSettings.spherizeIntensity);

            float2 noisePosition = finalPosition * sandSettings.scaling + rotation;
            float noiseValue = MathUtils.unorm(noise.snoise(noisePosition));

            //float noiseRot = sandSettings.rotSpeed * tickBlock.tick;
            //float noiseValue = MathUtils.unorm(noise.psrnoise(noisePosition, sandSettings.periodicity, noiseRot));
            bool canShow = noiseValue < threshold;

            if (!canShow)
                return;

            Color color = sandRendering.GetColor(position, ref tickBlock);
            color.a = alpha;
            outputColor[index] = color;
        }

    }
}
