using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class StatisGun : GunBaseElement
{
    [SerializeField] StatisGunScriptable settings => (StatisGunScriptable)baseSettings;

    RotationBound lastRotationBound;
    RotationBound.Anchor anchor = RotationBound.Anchor.CenterRight;

    NativeSprite nativeBeamTexture;

    public override void OnInit()
    {
        base.OnInit();
        nativeBeamTexture = new NativeSprite(settings.beamTexture);

        spriteAnimator.returnToIdleAfterAnim = true;
    }

    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, ref TickBlock tickBlock)
    {
        spriteAnimator.SetAnimation(1);
        lastRotationBound = GetRotationBound(aimDirection);
        new StatisParticles()
        {
            cameraHandle = pixelCamera.GetHandle(),
            map = map,
            rotationBound = lastRotationBound,
            statisTickMin = (int)(settings.statisMinDuration * GameManager.FPS),
            statisTickMax = (int)(settings.statisMaxDuration * GameManager.FPS),
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }

    public override void LateRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        float ratio = 1 - tickBlock.DurationSinceTick(tickShoot) / settings.flashDuration;
        ratio = math.saturate(ratio);
        if(ratio > 0)
            GridRenderer.DrawRotationSprite(ref outputColors, lastRotationBound, pixelCamera, nativeBeamTexture, settings.tint * ratio);
    }

    RotationBound GetRotationBound(float2 aimDirection)
    {
        float rotation = MathUtils.DirectionToAngle(aimDirection);
        return new RotationBound(Bound.CenterAligned(GetWorldPositionOffset(settings.beamOffset), settings.boundSizes), rotation, anchor);
    }

    public override void Dispose()
    {
        base.Dispose();
        nativeBeamTexture.Dispose();
    }

    [BurstCompile]
    public struct StatisParticles : IJobParallelFor
    {
        public RotationBound rotationBound;
        public PixelCamera.PixelCameraHandle cameraHandle;
        public Map map;
        public int statisTickMin;
        public int statisTickMax;

        public void Execute(int index)
        {
            int2 pos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
            int2 worldPos = cameraHandle.GetGlobalPosition(pos);

            if (rotationBound.PointInBound(worldPos) && map.InBound(worldPos))
            {
                Unity.Mathematics.Random random = Unity.Mathematics.Random.CreateFromIndex((uint)(index + math.abs(worldPos.x + worldPos.y)));
                Particle particle = map.GetParticle(worldPos);
                particle.tickStatis = random.NextInt(statisTickMin, statisTickMax + 1);
                map.SetParticle(worldPos, particle);
            }
        }
    }

    [BurstCompile]
    public struct RenderBeam : IJobParallelFor
    {
        public RotationBound rotationBound;
        public PixelCamera.PixelCameraHandle cameraHandle;
        public Color color;
        public NativeArray<Color32> outputColors;

        public void Execute(int index)
        {
            int2 pos = ArrayHelper.IndexToPos(index, GameManager.RenderSizes);
            int2 worldPos = cameraHandle.GetGlobalPosition(pos);

            if(rotationBound.PointInBound(worldPos))
                outputColors[index] = RenderingUtils.Blend(outputColors[index], color, BlendingMode.Normal);
        }
    }
}
