using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class StatisGun : GunBaseElement
{
    //public float boxRotation = 15;
    public int2 boundSizes = new int2(100, 50);
    public float statisDuration = 3;
    public int2 offset;
    RotationBound lastRotationBound;
    RotationBound.Anchor anchor = RotationBound.Anchor.CenterRight;

    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map)
    {
        int statisTick = (int)statisDuration * GameManager.FPS;
        lastRotationBound = GetRotationBound(aimDirection);
        new StatisParticles()
        {
            cameraHandle = pixelCamera.GetHandle(),
            map = map,
            rotationBound = lastRotationBound,
            statisTick = statisTick,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();
    }


    public override void LateRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        float ratio = 1 - tickBlock.DurationSinceTick(tickShoot) / statisDuration;
        math.saturate(ratio);
        if(ratio > 0)
            GridRenderer.DrawRotationBound(ref outputColors, lastRotationBound, pixelCamera, Color.yellow * ratio * 0.5f);
    }

    RotationBound GetRotationBound(float2 aimDirection)
    {
        float rotation = MathUtils.DirectionToAngle(aimDirection);
        //Debug.Log(aimDirection + " " + rotation);
        //float boxDirectionRotation = player.lookLeft ? -rotation : rotation;
        //RotationBound.Anchor anchor = player.lookLeft ? RotationBound.Anchor.CenterLeft : RotationBound.Anchor.CenterRight;
        return new RotationBound(Bound.CenterAligned(GetWorldPositionOffset(offset), boundSizes), rotation, anchor);
    }


    [BurstCompile]
    public struct StatisParticles : IJobParallelFor
    {
        public RotationBound rotationBound;
        public PixelCamera.PixelCameraHandle cameraHandle;
        public Map map;
        public int statisTick;

        public void Execute(int index)
        {
            int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
            int2 worldPos = cameraHandle.GetGlobalPosition(pos);

            if (rotationBound.PointInBound(worldPos) && map.InBound(worldPos))
            {
                Particle particle = map.GetParticle(worldPos);
                particle.tickStatis = statisTick;
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
            int2 pos = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
            int2 worldPos = cameraHandle.GetGlobalPosition(pos);

            if(rotationBound.PointInBound(worldPos))
                outputColors[index] = RenderingUtils.Blend(outputColors[index], color, BlendingMode.Normal);
        }
    }
}
