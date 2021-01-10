using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[BurstCompile]
public struct ASCIIRenderJob : IJobParallelFor
{
    public NativeArray<Color32> outputColors;
    public NativeArray<char> outputASCII;
    public void Execute(int index)
    {
		float intensity = RenderingUtils.Luminance(outputColors[index]);
		char c;
		if (intensity > 0.2f) c = ':';
		else if (intensity > 0.3f) c = '*';
		else if (intensity > 0.4f) c = 'o';
		else if (intensity > 0.5f) c = '&';
		else if (intensity > 0.6f) c = '8';
		else if (intensity > 0.7f) c = '@';
		else c = '#';

		outputASCII[index] = c;
	}
}
