using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ApplyParticleRenderToTextureJob : IJobParallelFor
{
    public NativeArray<Color32> colorArray;
    public NativeArray<Color32> textureColor;
    public Map map;
    public ParticleRendering particleRendering;
    public TickBlock tickBlock;
    public BlendingMode blending;
    public ParticleType particleType;

    public ApplyParticleRenderToTextureJob(
        NativeArray<Color32> colorArray,
        NativeArray<Color32> textureColor,
        Map map,
        ParticleRendering particleRendering,
        TickBlock tickBlock,
        BlendingMode blending,
        ParticleType particleType)
    {
        this.colorArray = colorArray;
        this.textureColor = textureColor;
        this.map = map;
        this.particleRendering = particleRendering;
        this.tickBlock = tickBlock;
        this.blending = blending;
        this.particleType = particleType;
    }

    public void Execute(int i)
    {
        int2 pos = ArrayHelper.IndexToPos(i, map.Sizes);
        if (textureColor[i].a > 0)
        {
            Color32 color = ParticleRenderUtil.GetColorForType(pos, particleType, ref particleRendering, ref tickBlock);
            colorArray[i] = RenderingUtils.Blend(colorArray[i], color, blending);
        }
    }
}
