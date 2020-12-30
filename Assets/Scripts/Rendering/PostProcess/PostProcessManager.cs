using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PostProcessManager
{
    public static PostProcessManager Instance;
    
    Animation<ShakeSettings> shakeAnimation;
    Animation<ScreenFlashSettings> screenFlashAnimation;
    Animation<ShockwaveSettings> shockwaveAnimation;
    Animation<BlackholeSettings> blackholeAnimation;
    Animation<IllusionEffectSettings> illusionAnimation;
    int currentTick;

    //FrameEffect<IllusionEffectSettings> illusionEffect;
    //Dictionary<System.Type, FrameEffect<IPostEffect>> frameDictionary = new Dictionary<System.Type, FrameEffect<IPostEffect>>();


    public static void EnqueueShake(in ShakeSettings settings)
    {
        Instance.shakeAnimation = new Animation<ShakeSettings>()
        {
            settings = settings,
            tick = Instance.currentTick,
            isActive = true
        };
    }

    public static void EnqueueScreenFlash(in ScreenFlashSettings settings)
    {
        Instance.screenFlashAnimation = new Animation<ScreenFlashSettings>()
        {
            tick = Instance.currentTick,
            settings = settings,
            isActive = true
        };
    }

    public static void EnqueueShockwave(in ShockwaveSettings settings, int2 position)
    {
        Instance.shockwaveAnimation = new Animation<ShockwaveSettings>()
        {
            tick = Instance.currentTick,
            settings = settings,
            isActive = true,
            position = position
        };
    }
    public static void EnqueueBlackHole(in BlackholeSettings settings, int2 position)
    {
        Instance.blackholeAnimation = new Animation<BlackholeSettings>()
        {
            settings = settings,
            tick = Instance.currentTick,
            isActive = true,
            position = position
        };
    }

    public static void EnqueueIllusion(in IllusionEffectSettings settings)
    {
        Instance.illusionAnimation = new Animation<IllusionEffectSettings>()
        {
            settings = settings,
            tick = Instance.currentTick,
            isActive = true
        };
    }

    public void Update(ref TickBlock tickBlock)
    {
        currentTick = tickBlock.tick;
    }

    public void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        RenderShake(ref outputColors, ref tickBlock);
        RenderScreenFlash(ref outputColors, ref tickBlock);
        RenderShockwave(ref outputColors, ref tickBlock);
        RenderBlackHole(ref outputColors, ref tickBlock);
        RenderIllusion(ref outputColors, ref tickBlock);

        //UpdateEffects(ref outputColors, ref tickBlock);

    }

    void RenderShake(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    {
        if (!ShouldUpdate(ref shakeAnimation, ref tickBlock, shakeAnimation.settings.duration, out float duration))
            return;

        float t = duration / shakeAnimation.settings.duration;

        NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);
        new ShakeScreenJob()
        {
            outputColors = outputColors,
            inputColors = inputColors,
            t = t,
            tickBlock = tickBlock,
            settings = shakeAnimation.settings
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

        Color32 color1 = screenFlashAnimation.settings.jobSettings.color1;
        Color32 color2 = screenFlashAnimation.settings.jobSettings.color2;
        MonochromeFilterJob.Settings jobSettings = screenFlashAnimation.settings.jobSettings;

        jobSettings.color1 = inverted ? color1 : color2;
        jobSettings.color2 = inverted ? color2 : color1;
        new MonochromeFilterJob()
        {
            settings = jobSettings,
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
           tickBlock = tickBlock,
           settings = shockwaveAnimation.settings,
           startTick = shockwaveAnimation.tick,
           position = shockwaveAnimation.position
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
            t = t,
            settings = blackholeAnimation.settings,
            position = blackholeAnimation.position
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

    //public PostProcessManager()
    //{
    //    frameDictionary.Add(typeof(IllusionEffectSettings), new FrameEffect<IPostEffect>());
    //}

    //public static void SetEffect(in IPostEffect settings)
    //{
    //    System.Type type = settings.GetType();
    //    FrameEffect<IPostEffect> data = Instance.frameDictionary[type];
    //    data.settings = settings;
    //    data.isEnabled = true;
    //    Instance.frameDictionary[type] = data;
    //}

    //private void UpdateEffects(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
    //{
    //    foreach(var frameEffect in frameDictionary.Values)
    //    {
    //        if(frameEffect.isEnabled)
    //        {
    //            var type = frameEffect.settings.GetType();
    //            if (type == typeof(IllusionEffectSettings))
    //            {
    //                NativeArray<Color32> inputColors = new NativeArray<Color32>(outputColors, Allocator.TempJob);
    //                new IllusionEffectJob()
    //                {
    //                    outputColors = outputColors,
    //                    settings = (IllusionEffectSettings)frameEffect.settings,
    //                    inputColors = inputColors,
    //                    t = tickBlock.tick,
                        
    //                }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    //                inputColors.Dispose();
    //            }

    //            //FrameEffect<IPostEffect> data = Instance.frameDictionary[type];
    //            //data.isEnabled = false;
    //            //Instance.frameDictionary[type] = data;
    //        }
    //    }
    //}



    //public struct FrameEffect<T>
    //{
    //    public T settings;
    //    public bool isEnabled;
    //}    

    public struct Animation<T>
    {
        public T settings;
        public int tick;
        public bool isActive;
        public int2 position;
    }

    [System.Serializable]
    public struct ShakeSettings 
    {
        public float intensity;
        public float duration;
        public float speed;
        public bool useFalloff;
        public float blendWithOriginal;
    }

    [System.Serializable]
    public struct ScreenFlashSettings
    {
        public MonochromeFilterJob.Settings jobSettings;
        public float duration;
        public int interval;
    }

    [System.Serializable]
    public struct ShockwaveSettings
    {
        public int radiusThickness;
        public float duration;
        public float waveSpeed;
        public float intensity;
    }



}
