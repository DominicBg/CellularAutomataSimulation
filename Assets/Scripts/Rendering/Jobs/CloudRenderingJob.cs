using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CloudRenderingJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    public Settings settings;
    public TickBlock tickBlock;


    [System.Serializable]
    public struct Settings
    {
        public int steps;
        public float stepSize;
        public float scale;
        public float scaleNoiseWorly;
        public float scaleNoisePerlin;

        public float3 position;
        public float noiseRatio;

        public Color cloudColor; //new Color(1f, 0.95f, 0.8f)
        public Color cloudDenseColor; //0.75f

        public float3 sunDirection;
        public Color sunColor;
        public float diffuseRatio; // 0.25;

        public float exponent; // = -0.003f
        public float maxHeight;
        public float fadeDist;
        public float noiseAlpha;

        public int resolution;
        public float alphaMultiplier; // 0.5

        public float rotationSpeed;
        public float rotationAmp;
        public float forwardSpeed;
    }

    public void Execute(int index)
    {
        float2 pos = ((float2)ArrayHelper.IndexToPos(index, GameManager.GridSizes) / GameManager.GridSizes - 0.5f) * settings.scale;
        //ortho
        //float3 ro = (new float3(pos.x, pos.y, 0) + settings.offset) * settings.scale;
        //float3 rd = math.forward();

        float3 offset =
            math.forward() * tickBlock.tick * settings.forwardSpeed +
            new float3(
                settings.rotationAmp * math.sin(tickBlock.tick * settings.rotationSpeed * 2 * math.PI),
                settings.rotationAmp * math.cos(tickBlock.tick * settings.rotationSpeed * 2 * math.PI), 
                0);

        float3 ro = settings.position + offset;
        float3 rd = math.normalize(new float3(pos.x, pos.y, 1));

        Color backgroundColor = outputColors[index];
        outputColors[index] = RayMarching(ro, rd, backgroundColor).ReduceResolution(settings.resolution);
    }

    Color RayMarching(float3 ro, float3 rd, Color bgColor)
    {
        float t = 0;
        Color currentColor = Color.clear;
        for (int i = 0; i < settings.steps; i++)
        {
            if (currentColor.a > 0.99f)
            {
                t += math.max(0.1f, 0.02f * t);
                continue;
            }
            //float t = i * settings.stepSizes;

            float3 pos = ro + rd * t * settings.stepSize;

            float density = CloudNoise(pos);
            if(density > 0.01)
            {
                float offsetDensity = CloudNoise(pos + settings.sunDirection);
                float diffuse = math.saturate((density - offsetDensity));
                currentColor = Integrate(currentColor, diffuse, density, bgColor, t);
            }

            t += math.max(0.1f, 0.02f * t);

        }
        return currentColor;
    }

    Color Integrate(Color color, float diffuse, float density, Color bgcol, float t)
    {
        Color lighthing = settings.sunColor + Color.white * diffuse * settings.diffuseRatio;

        Color colrgb = Color.Lerp(settings.cloudColor, settings.cloudDenseColor, density);
        
        Color col = colrgb;
        float colAlpha = density;

        col *= lighthing;
        col = Color.Lerp(col, bgcol, 1f - math.exp(settings.exponent * t * t));

        colAlpha *= settings.alphaMultiplier;

        col *= colAlpha;
        col.a = colAlpha;
        return color + col * (1 - color.a);
    }


    float CloudNoise(float3 pos)
    {
        float worley = 1 - (MathUtils.unorm(NoiseXVII.fbm4_worly(pos * settings.scaleNoiseWorly).x));
        float fbm = MathUtils.unorm(NoiseXVII.fbm4(pos * settings.scaleNoisePerlin));
        //return NoiseProc(math.lerp(worley, fbm, settings.noiseRatio), pos);
        return NoiseProc(worley * fbm, pos);
    }

    float NoiseProc(float noise, float3 pos)
    {
        return settings.noiseAlpha * noise; // * math.saturate((settings.maxHeight - pos.y) / settings.fadeDist);
    }

}
