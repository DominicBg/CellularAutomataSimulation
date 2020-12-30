﻿using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TimeGemElement : EquipableElement
{
    bool timeStopped;
    [SerializeField] PostProcessManager.ScreenFlashSettings onEnableFlash;
    [SerializeField] PostProcessManager.ScreenFlashSettings onDisableFlash;
    [SerializeField] PostProcessManager.ShockwaveSettings shockwave;

    [SerializeField] DisabledTimeGemJob.Settings disabledSettings;
    [SerializeField] EnabledTimeGemJob.Settings enabledSettings;
    [SerializeField] IllusionEffectSettings illusionSettings;
    [SerializeField] float dispersion;
    [SerializeField] float fadeoff;


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
        
        if(timeStopped)
        {
            Debug.Log("Time shall never be altered");
            PostProcessManager.EnqueueScreenFlash(in onEnableFlash);
            PostProcessManager.EnqueueShockwave(in shockwave, position);
            PostProcessManager.EnqueueIllusion(in illusionSettings);
        }
        else
        {
            Debug.Log("Release time.. from its chains");
            PostProcessManager.EnqueueScreenFlash(in onDisableFlash);
            for (int i = 0; i < previousColor.Length; i++)
            {
                previousColor[i] = Color.clear;
            }
        }

        var worldLevel = FindObjectOfType<WorldLevel>();
        worldLevel.updatLevelElement = !timeStopped;
    }

    public override void PreRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        if (timeStopped)
        {
            NativeArray<Color32> input = new NativeArray<Color32>(previousColor, Allocator.TempJob);
            new DispersionBlurImageJob()
            {
                inputColors = input,
                dispersionFactor = dispersion,
                fadeOff = fadeoff,
                outputColors = previousColor,
                tickBlock = tickBlock
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
            player.Render(ref previousColor, ref tickBlock);
            input.Dispose();
        }
        else
        {
            //reset previous color?
        }
    }

    public override void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!isEquiped)
            return;

        int2 center = player.GetBound().center;
        if (timeStopped)
        {
            //PostProcessManager.SetEffect(illusionSettings);
            new EnabledTimeGemJob()
            {
                center = center,
                previousColor = previousColor,
                outputColors = outputColors,
                settings = enabledSettings
            }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        }
        else
        {
            new DisabledTimeGemJob()
            {
                center = center,
                outputColors = outputColors,
                settings = disabledSettings,
                tickBlock = tickBlock
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
            PlayerElement player = FindObjectOfType<PlayerElement>();
            currentLevel = player.currentLevel;
            position = player.position;
            player.EquipQ(this);
            isEquiped = true;
        }
    }

    [BurstCompile]
    public struct DisabledTimeGemJob : IJobParallelFor
    {
        public int2 center;
        public Settings settings;
        public NativeArray<Color32> outputColors;
        public TickBlock tickBlock;

        [System.Serializable]
        public struct Settings
        {
            public int innerRadius;
            public int outerRadius;
            public Color32 innerColor;
            public Color32 outerColor;
            public Color32 radialColor;
            public BlendingMode blending;
            public float rotationSpeed;
            public float angleThreshold;
            public float spread;

        }

        public void Execute(int index)
        {
            int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
          
            float distancesq = math.distancesq(pos, center);
            if(distancesq < settings.outerRadius * settings.outerRadius && distancesq > settings.innerRadius * settings.innerRadius)
            {
                float2 diff = pos - center;
                float distance = math.sqrt(distancesq);
                float2 dir = diff / distance;

                float ratio = math.unlerp(settings.innerRadius, settings.outerRadius, distance);
                float rotationalAngle = (tickBlock.tick * settings.rotationSpeed + ratio * settings.spread);
                float2 currentRotation = MathUtils.Rotate(new float2(0, 1), rotationalAngle);
                float angle = math.acos(math.dot(dir, currentRotation));


                if(angle < settings.angleThreshold)
                {
                    Color color1 = Color.Lerp(settings.innerColor, settings.outerColor, ratio);
                    Color color2 = Color.Lerp(color1, settings.radialColor, math.unlerp(0, settings.angleThreshold, angle));
                    outputColors[index] = RenderingUtils.Blend(outputColors[index], color2, settings.blending);
                }
            }

        }
    }
    [BurstCompile]
    public struct EnabledTimeGemJob : IJobParallelFor
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
