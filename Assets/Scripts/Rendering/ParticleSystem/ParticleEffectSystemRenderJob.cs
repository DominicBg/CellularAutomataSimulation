using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct ParticleEffectSystemRenderJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    [ReadOnly] public NativeList<ParticleEffectSystem.Particle> particles;
    public ParticleEffectSystemSettings settings;
    public PixelCamera.PixelCameraHandle cameraHandle;

    public void Execute(int index)
    {
        int2 position = ArrayHelper.IndexToPos(index, GameManager.GridSizes);
        Color color = Color.clear;
        for (int i = 0; i < particles.Length; i++)
        {
            ParticleEffectSystem.Particle particle = particles[i];
            int2 renderPos = cameraHandle.GetRenderPosition((int2)particle.position);

            float distSq = math.distancesq(position, renderPos);
            if(distSq < particle.radius)
            {
                //add fadeoff?
                color = RenderingUtils.Blend(color, particle.color, settings.colors.blendingMode);
            }
        }
        outputColors[index] = RenderingUtils.Blend(outputColors[index], color, settings.colors.blendingMode);
    }
}