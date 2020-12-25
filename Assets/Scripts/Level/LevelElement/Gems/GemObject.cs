using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static FogElement;

public class GemObject : LevelObject, ILightSource
{
    public Color32 color;
    public GemShine[] gemShines;
    public LightSourceScriptable lightSource;

    public override Bound GetBound()
    {
        return new Bound(position, 5);
    }

    public override void Render(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        int pos1 = ArrayHelper.PosToIndex(position, GameManager.GridSizes);
        int pos2 = ArrayHelper.PosToIndex(position + 1, GameManager.GridSizes);

        outputColor[pos1] = color;
        outputColor[pos2] = color;
    }

    public override void PostRender(ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
    {
        for (int i = 0; i < gemShines.Length; i++)
        {
            gemShines[i].Render(position, ref outputColor, ref tickBlock);
        }
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
    }

    public LightSource GetLightSource(out int2 position)
    {
        position = this.position;
        return lightSource.lightSource;
    }

    [System.Serializable]
    public struct GemShine
    {
        public float flashSpeed;
        public int flashRadiusMin;
        public int flashRadiusMax;
        public Color32 color;
        public BlendingMode blend;
        public float offsynch;
        public void Render(int2 position, ref NativeArray<Color32> outputColor, ref TickBlock tickBlock)
        {
            float t = MathUtils.unorm(math.sin(tickBlock.tick * flashSpeed + offsynch));
            int radius = (int)math.lerp(flashRadiusMin, flashRadiusMax, t);
            GridRenderer.DrawEllipse(ref outputColor, position, radius, color, Color.clear, blend);
        }
    }
}
