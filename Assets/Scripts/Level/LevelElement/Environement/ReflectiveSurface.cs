using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class ReflectiveSurface : LevelObject
{
    public int2 sizes;
    public int2 offset;
    public Color32 baseColor;
    public BlendingMode mirrorBlending;
    public BlendingMode reflectionBlending;

    public float t;

    private NativeArray<Color32> reflectiveColors;

    private static readonly Color safetyColor = new Color(-1, -1, -1, -1);


    public override void OnUpdate(ref TickBlock tickBlock)
    {

    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock, int2 renderPos)
    {
        reflectiveColors = new NativeArray<Color32>(sizes.x * sizes.y, Allocator.TempJob);
        new PreReflectingJob()
        {
            bound = new Bound(renderPos, sizes),
            outputColors = outputColor,
            tickBlock = tickBlock,
            reflectiveColors = reflectiveColors
        }.Run();
    }

    public override void PostRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {

        new PostReflectingJob()
        {
            bound = GetBound(),
            reflectionBlending = reflectionBlending,
            mirrorBlending = mirrorBlending,
            offset = offset,
            t = t,
            outputColors = outputColors,
            reflectiveColors = reflectiveColors,
            tickBlock = tickBlock,
            baseColor = baseColor,
            iceRendering = GridRenderer.Instance.particleRendering.iceRendering,
        }.Run();
        reflectiveColors.Dispose();
    }

    public override Bound GetBound()
    {
        return new Bound(position, sizes);
    }

    [BurstCompile]
    public struct PreReflectingJob : IJob
    {
        public Bound bound;
        public NativeArray<Color32> outputColors;
        public NativeArray<Color32> reflectiveColors;
        public TickBlock tickBlock;

        public void Execute()
        {
            bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);

            //Two passes systems, if pixels have changed, don't override it
            for (int i = 0; i < positions.Length; i++)
            {
                int index = ArrayHelper.PosToIndex(positions[i], GameManager.GridSizes);
                reflectiveColors[i] = safetyColor;
                outputColors[index] = safetyColor;
            }
            positions.Dispose();
        }
    }

    [BurstCompile]
    public struct PostReflectingJob : IJob
    {
        public Bound bound;
        public NativeArray<Color32> outputColors;
        public NativeArray<Color32> reflectiveColors;
        public TickBlock tickBlock;
        public int2 offset;
        public float t;
        public BlendingMode mirrorBlending;
        public BlendingMode reflectionBlending;

        public IceRendering iceRendering;
        public Color baseColor;

        public void Execute()
        {
            bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
            NativeArray<bool> useReflectiveColors = new NativeArray<bool>(positions.Length, Allocator.Temp);

            for (int i = 0; i < positions.Length; i++)
            {
                int2 offsetPos = positions[i] + offset;

                if (!GridHelper.InBound(offsetPos, GameManager.GridSizes))
                    continue;

                //If the pixel didnt change, show reflection
                int index = ArrayHelper.PosToIndex(positions[i], GameManager.GridSizes);
                if (!RenderingUtils.Equals(reflectiveColors[i], outputColors[index]))
                    continue;

                int indexOffset = ArrayHelper.PosToIndex(offsetPos, GameManager.GridSizes);

                Color32 iceColor = iceRendering.GetColor(positions[i], ref tickBlock);
                Color color = RenderingUtils.Blend(iceColor, baseColor, mirrorBlending);
                color.a = t;

                useReflectiveColors[i] = true;

                //this will draw on top of stuff lol
                reflectiveColors[i] = RenderingUtils.Blend(outputColors[indexOffset], color, reflectionBlending);
            }

            //If we set the color, it does it recursivly
            for (int i = 0; i < positions.Length; i++)
            {
                int index = ArrayHelper.PosToIndex(positions[i], GameManager.GridSizes);
                if (!useReflectiveColors[i])
                    continue;

                outputColors[index] = reflectiveColors[i];
            }

            positions.Dispose();
            useReflectiveColors.Dispose();
        }
    }
}
