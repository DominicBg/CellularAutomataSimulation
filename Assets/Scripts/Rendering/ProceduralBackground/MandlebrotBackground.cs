using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct MandlebrotBackground : IJobParallelFor
{
    [System.Serializable]
    public struct Settings
    {
        public float zoom;
        public float2 center;
        public int maxIteration;

        public Color color1;
        public Color color2;
        public Color color3;
        public Color color4;
    }

    public NativeArray<Color32> outputColor;
    public Settings settings;
    public int2 sizes;

    public static void Generate(ref NativeArray<Color32> outputColor, Settings settings, int2 sizes)
    {
        new MandlebrotBackground() 
        {
            outputColor = outputColor, settings = settings, sizes = sizes
        }.Schedule(sizes.x * sizes.y, GameManager.InnerLoopBatchCount).Complete();
    }

    public void Execute(int index)
    {
        //f_c(z) = z^2 + c
        //f_c(z) = (xz + yz*i)^2 + (xc + yc*i)
        //f_c(z) = (xz + yz*i)*(xz + yz*i) + (xc + yc*i)
        //f_c(z) = xz^2 + xz*yzi + xz*yzi + yzi^2 + xc + yc*i
        //f_c(z) = xz^2 + 2*xz*yz*i + yz*i^2 + xc + yc*i
        //f_c(z) = xz^2 + 2*xz*yz*i + -yz^2 + xc + yc*i
        //f_c(z) = (xz^2 - yz^2 + xc) +  (2*xz*yz*i + yc*i)
        //f_c(z) = (xz^2 - yz^2 + xc) +  (2*xz*yz + yc)i

        int2 pos = ArrayHelper.IndexToPos(index, sizes);
        const float smoll = 50f;
        float2 c = (pos - settings.center) / (settings.zoom * smoll);
        float2 z = 0;

        int i;
        for (i = 0; i < settings.maxIteration; i++)
        {
            float2 nextZ = 0;
            nextZ.x = z.x * z.x - z.y * z.y + c.x;
            nextZ.y = 2*z.x*z.y + c.y;

            z = nextZ;
            if(math.lengthsq(z) > 4)
            {
                break;
            }
        }

        //use i for color

        i = i * 4 / settings.maxIteration;

        switch(i)
        {
            case 0: outputColor[index] = settings.color1; break;
            case 1: outputColor[index] = settings.color2; break;
            case 2: outputColor[index] = settings.color3; break;
            case 3: outputColor[index] = settings.color4; break;
        }
        
        //if (i < settings.maxIteration - 1)
        //{
        //    outputColor[index] = Color.black;
        //}
        //else
        //{
        //    outputColor[index] = Color.white;
        //}
    }
}
