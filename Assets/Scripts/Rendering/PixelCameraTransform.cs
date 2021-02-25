using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PixelCameraTransform : LevelObject
{
    public LevelObject target;
    public int2 focusSizes = 25;
    public int snapMaxDistance = 2;
    public float elasticSmooth = 5;
    public int2 boundOffset;
    public int2 offset;
    public int2 shakeOffset;
    NativeList<Shake> shakeList;

    public override void OnInit()
    {
        target = FindObjectOfType<Player>();
        position = target.GetBound().center;
        shakeList = new NativeList<Shake>(Allocator.Persistent);
    }


    public override Bound GetBound()
    {
        return Bound.CenterAligned(position - boundOffset, focusSizes);
    }

    public override void OnLateUpdate(ref TickBlock tickBlock)
    {
        int2 targetCenter = target.GetBound().center;
        int2 closestPoint = GetBound().ProjectPointOnBound(targetCenter);
        if (math.any(closestPoint != 0))
        {
            int2 diff = targetCenter - closestPoint;

            if(math.any(math.abs(diff) > snapMaxDistance))
            {
                position = (int2)math.lerp(position, targetCenter + boundOffset, GameManager.DeltaTime * elasticSmooth);
            }
            else 
            {
                position += diff;
            }
        }
        UpdateShake(ref tickBlock);
    }

    void UpdateShake(ref TickBlock tickBlock)
    {
        new PrepassCameraShakeUpdateJob()
        {
            tickBlock = tickBlock,
            shakeList = shakeList
        }.Run();

        NativeArray<int2> offsets = new NativeArray<int2>(shakeList.Length, Allocator.TempJob);
        new CameraShakeUpdateJob()
        {
            offsets = offsets,
            shakeList = shakeList,
            tickBlock = tickBlock
        }.Schedule(shakeList.Length, 1).Complete();

        offset = 0;
        for (int i = 0; i < offsets.Length; i++)
        {
            offset += offsets[i];
        }

        offsets.Dispose();
    }

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(renderPos - boundOffset, focusSizes), Color.cyan * 0.35f, BlendingMode.Transparency);
    }

    public void ScreenShake(in CameraShakeSettings settings, int startTick)
    {
        shakeList.Add(new Shake() { settings = settings, startTick = startTick });
    }


    public override void Dispose()
    {
        base.Dispose();
        shakeList.Dispose();
    }

    public struct Shake
    {
        public CameraShakeSettings settings;
        public int startTick;
    }

    [BurstCompile]
    public struct PrepassCameraShakeUpdateJob : IJob
    {
        public NativeList<Shake> shakeList;
        public TickBlock tickBlock;
        public void Execute()
        {
            for (int i = shakeList.Length - 1; i >= 0; i--)
            {
                float duration = tickBlock.DurationSinceTick(shakeList[i].startTick);
                if (duration > shakeList[i].settings.duration)
                    shakeList.RemoveAt(i);
            }
        }
    }

    [BurstCompile]
    public struct CameraShakeUpdateJob : IJobParallelFor
    {
        [ReadOnly] public NativeList<Shake> shakeList;
        public NativeArray<int2> offsets;
        public TickBlock tickBlock;

        public void Execute(int index)
        {
            CameraShakeSettings settings = shakeList[index].settings;
            float t = 1f - math.saturate(tickBlock.DurationSinceTick(shakeList[index].startTick) / settings.duration);

            float intensity = EaseXVII.Evaluate(t, settings.intensityEase);
            float speed = EaseXVII.Evaluate(t, settings.speedEase);

            if (settings.inverseSpeedEase)
                speed = 1 - speed;

            float p = tickBlock.tick * settings.speed * speed;
            float2 offset = new float2(noise.cnoise(new float2(p, 100)), noise.cnoise(new float2(100, p)));
            offset *= settings.intensity * intensity;

            offsets[index] = (int2)offset;
        }
    }
}
