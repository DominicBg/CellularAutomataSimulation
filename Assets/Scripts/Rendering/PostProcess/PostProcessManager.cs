using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PostProcessManager
{
    static PostProcessManager _instance;
    public static PostProcessManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new PostProcessManager();

            return _instance;
        }
    }

    Animation<ShakeSettings> shakeAnimation;
    Animation<ScreenFlashSettings> screenFlashAnimation;
    Animation<ShockwaveSettings> shockwaveAnimation;
    Animation<BlackholeSettings> blackholeAnimation;
    Animation<IllusionEffectSettings> illusionAnimation;

    public static void EnqueueShake(in ShakeSettings settings, int tick)
    {
        Instance.shakeAnimation = new Animation<ShakeSettings>()
        {
            settings = settings,
            tick = tick,
            isActive = true
        };
    }

    public static void EnqueueScreenFlash(in ScreenFlashSettings settings, int tick)
    {
        Instance.screenFlashAnimation = new Animation<ScreenFlashSettings>()
        {
            tick = tick,
            settings = settings,
            isActive = true
        };
    }

    public static void EnqueueShockwave(in ShockwaveSettings settings, int tick)
    {
        Instance.shockwaveAnimation = new Animation<ShockwaveSettings>()
        {
            tick = tick,
            settings = settings,
            isActive = true
        };
    }
    public static void EnqueueBlackHole(in BlackholeSettings settings, int tick)
    {
        Instance.blackholeAnimation = new Animation<BlackholeSettings>()
        {
            settings = settings,
            tick = tick,
            isActive = true
        };
    }

    public static void EnqueuIllusion(in IllusionEffectSettings settings, int tick)
    {
        Instance.illusionAnimation = new Animation<IllusionEffectSettings>()
        {
            settings = settings,
            tick = tick,
            isActive = true
        };
    }


    public void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        RenderShake(ref outputColors, ref tickBlock);
        RenderScreenFlash(ref outputColors, ref tickBlock);
        RenderShockwave(ref outputColors, ref tickBlock);
        RenderBlackHole(ref outputColors, ref tickBlock);
        RenderIllusion(ref outputColors, ref tickBlock);
    }

    void RenderShake(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!ShouldUpdate(ref shakeAnimation, ref tickBlock, shakeAnimation.settings.duration, out float duration))
            return;

        //add in screenshake?
        float t = duration / shakeAnimation.settings.duration;
        float falloff = 1 - t * t * t;
        float p = tickBlock.tick * shakeAnimation.settings.speed;
        float x = noise.cnoise(new float2(p, 100)) * shakeAnimation.settings.intensity * falloff;
        float y = noise.cnoise(new float2(100, p)) * shakeAnimation.settings.intensity * falloff;

        int2 offset = new int2((int)x, (int)y);
        if (math.all(offset == 0))
            return;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        new ShakeScreenJobs()
        {
            outputColors = outputColors,
            inputColors = inputColors,
            offset = offset,
            blendWithOriginal = shakeAnimation.settings.blendWithOriginal
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
        return;
    }
    void RenderScreenFlash(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!ShouldUpdate(ref screenFlashAnimation, ref tickBlock, screenFlashAnimation.settings.duration, out float duration))
            return;

        float intervalDuration = screenFlashAnimation.settings.duration / screenFlashAnimation.settings.interval;

        bool inverted = (duration % intervalDuration * 2) < intervalDuration;

        Color32 color1 = screenFlashAnimation.settings.color1;
        Color32 color2 = screenFlashAnimation.settings.color2;

        new MonochromeFilterJob()
        {
            black = inverted ? color1 : color2,
            white = inverted ? color2 : color1,
            blendWithOriginal = screenFlashAnimation.settings.blendWithOriginal,
            threshold = screenFlashAnimation.settings.threshold,
            outputColors = outputColors,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    void RenderShockwave(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!ShouldUpdate(ref shockwaveAnimation, ref tickBlock, shockwaveAnimation.settings.duration, out float duration))
            return;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);

        new ShockwaveJob()
        {
           inputColors = inputColors,
           outputColors = outputColors,
           settings = shockwaveAnimation.settings,
           tickBlock = tickBlock,
           startTick = shockwaveAnimation.tick
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
    }

    void RenderBlackHole(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!ShouldUpdate(ref blackholeAnimation, ref tickBlock, blackholeAnimation.settings.duration, out float duration))
            return;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);

        //to be able to controls the blackhole outside the job
        float t = duration;
        t = math.sin(t * math.PI);

        new BlackholeJob()
        {
            inputColors = inputColors,
            outputColors = outputColors,
            settings = blackholeAnimation.settings,
            t = t
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
    }

    void RenderIllusion(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!ShouldUpdate(ref illusionAnimation, ref tickBlock, illusionAnimation.settings.duration, out float duration))
            return;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);

        new IllusionEffectJob()
        {
            inputColors = inputColors,
            outputColors = outputColors,
            settings = illusionAnimation.settings,
            t = duration
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
    }



    public bool ShouldUpdate<T>(ref Animation<T> animation, ref TickBlock tickBlock, float settingsDuration, out float duration)
    {
        duration = 0;
        if (!animation.isActive)
            return false;

        duration = tickBlock.DurationSinceTick(animation.tick);

        if (duration > settingsDuration)
        {
            animation.isActive = false;
            return false; 
        };

        return true;
    }


    public struct Animation<T>
    {
        public T settings;
        public int tick;
        public bool isActive;
    }

    //Settings
    [System.Serializable]
    public struct ShakeSettings 
    {
        public float intensity;
        public float duration;
        public float speed;
        public float blendWithOriginal;
    }

    [System.Serializable]
    public struct ScreenFlashSettings
    {
        public Color32 color1;
        public Color32 color2;
        public float threshold;
        public float duration;
        public int interval;
        public float blendWithOriginal;
    }

    //[System.Serializable]
    //public struct ShockwaveSettings
    //{
    //    public int2 centerPoint;
    //    public int radiusThickness;
    //    public float duration;
    //    public float waveSpeed;
    //    public float intensity;
    //}

   
}
