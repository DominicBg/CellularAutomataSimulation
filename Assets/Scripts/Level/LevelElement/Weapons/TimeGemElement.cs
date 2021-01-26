using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TimeGemElement : EquipableElement
{
    TimeGemScriptable settings => (TimeGemScriptable)baseSettings;
    bool timeStopped;

    NativeArray<Color32> previousColor;

    public override void OnInit()
    {
        base.OnInit();
        previousColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.Persistent);
    }

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
        if(timeStopped)
        {
            //add post process?
        }
    }

    protected override void OnEquip()
    {
    }

    protected override void OnUnequip()
    {
    }

    protected override void OnUse(int2 position, bool altButton, ref TickBlock tickBlock)
    {
        timeStopped = !timeStopped;
        int2 center = player.GetBound().center;

        if (timeStopped)
        {
            Debug.Log("Time shall never be altered");
            PostProcessManager.EnqueueScreenFlash(in settings.onEnableFlash);
            PostProcessManager.EnqueueShockwave(in settings.shockwave, center);
            PostProcessManager.EnqueueIllusion(in settings.illusionSettings);
        }
        else
        {
            Debug.Log("Release time.. from its chains");
            PostProcessManager.EnqueueScreenFlash(in settings.onDisableFlash);
            for (int i = 0; i < previousColor.Length; i++)
            {
                previousColor[i] = Color.clear;
            }
        }

        var worldLevel = FindObjectOfType<WorldLevel>();
        worldLevel.updatLevelElement = !timeStopped;
    }

    public override void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        if (!isEquiped)
            return;

        int2 center = player.GetBound().center;
        NativeArray<float> alphas = new NativeArray<float>(outputColors.Length, Allocator.TempJob);

        if (timeStopped)
        {
            for (int i = 0; i < alphas.Length; i++)
            {
                alphas[i] = 1;
            }
        }
        else
        {
           new BackgroundMaskJob()
           {
               alphas = alphas,
               center = center,
               tickBlock = tickBlock,
               settings = settings.backgroundMaskSettings
           }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        }

        new BackGroundEffectJob()
        {
            alphas = alphas,
            outputColors = outputColors,
            settings = settings.backgroundEffectSettings,
            tickBlock = tickBlock
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        alphas.Dispose();
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPosition)
    {
        if (!isEquiped)
            return;
        int2 center = player.GetBound().center;

        if (timeStopped)
        {
            NativeArray<Color32> input = new NativeArray<Color32>(previousColor, Allocator.TempJob);
            new DispersionBlurImageJob()
            {
                inputColors = input,
                dispersionFactor = settings.dispersion,
                fadeOff = settings.fadeoff,
                outputColors = previousColor,
                tickBlock = tickBlock
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

            //might break
            player.Render(ref previousColor, ref tickBlock, renderPosition);
            input.Dispose();

            new SlowTimeBlurEffectGemJob()
            {
                center = center,
                previousColor = previousColor,
                outputColors = outputColors,
                settings = settings.enabledSettings
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        }
    }


    public override void Dispose()
    {
        base.Dispose();
        previousColor.Dispose();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            position = player.position;
            player.EquipQ(this);
            isEquiped = true;
        }
    }

   
    [BurstCompile]
    public struct BackgroundMaskJob : IJobParallelFor
    {
        public int2 center;
        public Settings settings;
        public NativeArray<float> alphas;
        public TickBlock tickBlock;

        [System.Serializable]
        public struct Settings
        {
            public float fadeIn;
            public float fadeOut;

            public float minRadius;
            public float maxRadius;

            public float radialNoiseSpeed;
            public float radialNoiseScale;
            public float radialNoiseEvolution;
            
            public float offset;
            public float maxAlpha;
            public float alphaResolution;

            public float rotationSpeed;
        }

        public void Execute(int index)
        {
            alphas[index] = 0;

            int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

            float distancesq = math.distancesq(pos, center);
            if (distancesq < settings.maxRadius * settings.maxRadius)
            {
                float2 diff = pos - center;
                float distance = math.sqrt(distancesq);
                //float2 dir = diff / distance;

                float rotationalAngle = (tickBlock.tick * settings.rotationSpeed);
                ///float2 currentRotation = MathUtils.Rotate(new float2(0, 1), rotationalAngle);

                //float angle = math.acos(math.dot(dir, currentRotation));


                float2 fbmPos = (float2)pos * settings.radialNoiseScale;
                float2 fbmOffset = (float2)pos * tickBlock.tick * settings.radialNoiseEvolution;
                float noiseAlpha = MathUtils.unorm(NoiseXVII.fbm4r(fbmPos + NoiseXVII.fbm4r(fbmOffset + fbmPos + NoiseXVII.fbm4r(fbmPos))));

                //float finalLength = math.remap(0, 1, settings.minRadius, settings.maxRadius, noiseLength);

                if (distancesq < settings.maxRadius * settings.maxRadius)
                {
                    //float alpha = math.remap(0, 1, settings.minAlpha, 1, alphaSin);

                    float dist = math.sqrt(distancesq);
                    //float fadeIn = dist < settings.minRadius - settings.fadeIn ? 0 : math.remap(settings.minRadius - settings.fadeIn, settings.minRadius, 0, 1, dist);
                    //float fadeOut = dist < settings.maxRadius - settings.fadeOut ? 0 : math.remap(settings.maxRadius - settings.fadeOut, settings.maxRadius, 1, 0, dist);


                    //fadeIn = math.saturate(fadeIn);
                    //fadeOut = math.saturate(fadeOut);

                    //alpha *= alpha;// * fadeIn * fadeOut;
                    float alpha = math.remap(0, 1, 0, noiseAlpha * settings.maxAlpha, 1 - dist / settings.maxRadius);
                    alpha = MathUtils.ReduceResolution(alpha, settings.alphaResolution);
                    alphas[index] = alpha;
                }
            }
        }
    }

    [BurstCompile]
    public struct BackGroundEffectJob : IJobParallelFor
    {
        public Settings settings;

        public NativeArray<float> alphas;
        public NativeArray<Color32> outputColors;
        public TickBlock tickBlock;

        [System.Serializable]
        public struct Settings
        {
            public int radiusCell;
            public int radiusCircle;
            public float blending;
            public BlendingMode blendingMode;

            public Color pillarColor;
            public float fbmScale;
            public float fbmOffset;

            public Color4Dither backgroundColor;
        }

        public void Execute(int index)
        {
            if (alphas[index] == 0)
                return;

            int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);

            Color color = CalculateColor(pos);
            color.a = alphas[index];
            outputColors[index] = RenderingUtils.Blend(outputColors[index], color, settings.blendingMode);
        }

        public Color32 CalculateColor(int2 pos)
        {
            return IsCircle(pos) ? Nebula(pos) : settings.pillarColor;
        }

        bool IsCircle(int2 pos)
        {
            int2 repeatPosition = pos % (2 * settings.radiusCell);

            int2 centerPos = settings.radiusCell;
            int2 diff = repeatPosition - centerPos;
            return math.length(diff) < settings.radiusCircle + 1;
        }

        Color Nebula(int2 pos)
        {
            float2 offset = (float2)pos * settings.fbmScale + tickBlock.tick * settings.fbmOffset;
            float fbm = MathUtils.unorm(NoiseXVII.fbm4r(offset + NoiseXVII.fbm4r((float2)pos * settings.fbmScale)));
            return settings.backgroundColor.GetColorWitLightValue(fbm, pos);
        }
    }

    [BurstCompile]
    public struct SlowTimeBlurEffectGemJob : IJobParallelFor
    {
        public int2 center;
        public Settings settings;

        [ReadOnly] public NativeArray<Color32> previousColor;
        public NativeArray<Color32> outputColors;

        [System.Serializable]
        public struct Settings
        {
            public float blending;
            public BlendingMode blendingMode;
        }

        public void Execute(int index)
        {
            Color color = previousColor[index];
            color.a = math.min(settings.blending, color.a);
            outputColors[index] = RenderingUtils.Blend(outputColors[index], color, settings.blendingMode);
        }
    }

}
