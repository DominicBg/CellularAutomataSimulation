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

    ShakeAnimation shakeAnimation;
    ScreenFlashAnimation screenFlashAnimation;
    ShockwaveAnimation shockwaveAnimation;
    Animation<BlackholeSettings> blackholeAnimation;

    public static void EnqueueShake(in ShakeSettings settings, int tick)
    {
        Instance.shakeAnimation = new ShakeAnimation()
        {
            settings = settings,
            tick = tick,
            isActive = true
        };
    }

    public static void EnqueueScreenFlash(in ScreenFlashSettings settings, int tick)
    {
        Instance.screenFlashAnimation = new ScreenFlashAnimation()
        {
            tick = tick,
            settings = settings,
            isActive = true
        };
    }

    public static void EnqueueShockwave(in ShockwaveSettings settings, int tick)
    {
        Instance.shockwaveAnimation = new ShockwaveAnimation()
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

    public void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        RenderShake(ref outputColors, ref tickBlock);
        RenderScreenFlash(ref outputColors, ref tickBlock);
        RenderShockwave(ref outputColors, ref tickBlock);
        RenderBlackHole(ref outputColors, ref tickBlock);
    }

    void RenderShake(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!shakeAnimation.isActive)
            return;

        float duration = tickBlock.DurationSinceTick(shakeAnimation.tick);

        ref ShakeSettings settings = ref shakeAnimation.settings;
        if(duration > settings.duration)
        {
            shakeAnimation.isActive = false;
            return;
        }


        //float sin = math.sin(duration * shakeAnimation.settings.speed) * shakeAnimation.settings.intensity;
        float t = duration / settings.duration;
        float falloff = 1 - t * t * t;
        float p = tickBlock.tick * settings.speed;
        float x = noise.cnoise(new float2(p, 100)) * settings.intensity * falloff;
        float y = noise.cnoise(new float2(100, p)) * settings.intensity * falloff;

        int2 offset = new int2((int)x, (int)y);
        if (math.all(offset == 0))
            return;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        new ShakeScreenJobs()
        {
            outputColors = outputColors,
            inputColors = inputColors,
            offset = offset,
            blendWithOriginal = settings.blendWithOriginal
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
        inputColors.Dispose();
        return;
    }
    void RenderScreenFlash(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!screenFlashAnimation.isActive)
            return;

        float duration = tickBlock.DurationSinceTick(screenFlashAnimation.tick);

        if (duration > screenFlashAnimation.settings.duration)
        {
            screenFlashAnimation.isActive = false;
            return;
        }

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
        if (!shockwaveAnimation.isActive)
            return;

        float duration = tickBlock.DurationSinceTick(shockwaveAnimation.tick);

        if (duration > shockwaveAnimation.settings.duration)
        {
            shockwaveAnimation.isActive = false;
            return;
        };

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
        if (!ShouldUpdate(ref blackholeAnimation, ref tickBlock, blackholeAnimation.settings.duration))
            return;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);

        //to be able to controls the blackhole outside the job
        float t = tickBlock.DurationSinceTick(blackholeAnimation.tick) / blackholeAnimation.settings.duration;
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

    public bool ShouldUpdate<T>(ref Animation<T> animation, ref TickBlock tickBlock, float settingsDuration)
    {
        if (!animation.isActive)
            return false;

        float duration = tickBlock.DurationSinceTick(animation.tick);

        if (duration > settingsDuration)
        {
            animation.isActive = false;
            return false; 
        };

        return true;
    }


    //States
    struct ShakeAnimation
    {
        public ShakeSettings settings;
        public int tick;
        public bool isActive;
    }

    struct ScreenFlashAnimation
    {
        public ScreenFlashSettings settings;
        public int tick;
        public bool isActive;
    }

    struct ShockwaveAnimation
    {
        public ShockwaveSettings settings;
        public int tick;
        public bool isActive;
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

    [System.Serializable]
    public struct ShockwaveSettings
    {
        public int2 centerPoint;
        public int radiusThickness;
        public float duration;
        public float waveSpeed;
        public float intensity;
    }

    [System.Serializable]
    public struct BlackholeSettings
    {
        public int2 centerPoint;
        public float duration;
        public float waveSpeed;
        public float intensityMin;
        public float intensityMax;
    }
}
