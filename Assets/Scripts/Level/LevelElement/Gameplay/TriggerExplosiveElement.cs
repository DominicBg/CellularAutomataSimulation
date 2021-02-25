using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class TriggerExplosiveElement : LevelObject
{
    [SerializeField] ExplosiveEffectScriptable settings = default;



    public override Bound GetBound()
    {
        return Bound.CenterAligned(position, settings.explosiveSettings.radius * 2);
    }
    public override void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock, int2 renderPos)
    {
        GridRenderer.DrawBound(ref outputColors, Bound.CenterAligned(renderPos, settings.explosiveSettings.radius * 2), Color.magenta * 0.75f);
    }

    public void OnTriggerExplosive()
    {
        Explosive.SetExplosive(position, in settings.explosiveSettings, map);
        pixelCamera.transform.ScreenShake(in settings.shakeSettings, scene.CurrentTick);
        PostProcessManager.EnqueueScreenFlash(in settings.screenFlashSettings);
        PostProcessManager.EnqueueShockwave(in settings.shockwaveSettings, position);
    }
}
