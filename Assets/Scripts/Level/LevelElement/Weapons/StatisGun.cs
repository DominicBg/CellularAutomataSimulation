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
    GolemController golemController;

    public override void OnInit()
    {
        base.OnInit();
        nativeBeamTexture = new NativeSprite(settings.beamTexture);

        spriteAnimator.returnToIdleAfterAnim = true;
        golemController = FindObjectOfType<GolemController>();
    }

    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, ref TickBlock tickBlock)
    {
        float2 viewDir = player.ViewDirection;
        lastRotationBound = GetRotationBound(viewDir);
        spriteAnimator.SetAnimation(1);
        new StatisParticles()
        {
            cameraHandle = pixelCamera.GetHandle(),
            map = map,
            rotationBound = lastRotationBound,
            statisTickMin = (int)(settings.statisMinDuration * GameManager.FPS),
            statisTickMax = (int)(settings.statisMaxDuration * GameManager.FPS),
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();



        if(golemController.isSummoned && lastRotationBound.IntersectWith(golemController.GetBound()))
        {
            golemController.ExploseGolem();
        }
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

    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        base.RenderDebug(ref outputColors, ref tickBlock, renderPos);
        var cameraHandle = scene.pixelCamera.GetHandle();
        if (InputCommand.HasInputDirection)
        {
            int2 startPos = cameraHandle.GetRenderPosition(player.GetBound().center);
            GridRenderer.DrawLine(ref outputColors, startPos, (int2)(startPos + InputCommand.Get8Direction * 25), Color.yellow);
            GridRenderer.DrawLine(ref outputColors, startPos, (int2)(startPos + InputCommand.Direction * 25), Color.yellow);

            var rotationBound = GetRotationBound(InputCommand.Get8Direction);
            using (var corners = rotationBound.GetCorners())
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    int2 cornerRenderPos = cameraHandle.GetRenderPosition(corners[i]);
                    GridRenderer.DrawEllipse(ref outputColors, cornerRenderPos, 3, Color.yellow, Color.clear);
                }
            }
        }
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
