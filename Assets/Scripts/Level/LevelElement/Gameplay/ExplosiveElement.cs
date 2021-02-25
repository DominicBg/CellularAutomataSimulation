using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ExplosiveElement : LevelElement
{
    [SerializeField] LevelObject target = default;
    [SerializeField] ExplosiveEffectScriptable settings = default;


    public override void OnUpdate(ref TickBlock tickBlock)
    {
        Bound bound = target.GetBound();
        //if (InputCommand.IsButtonDown(KeyCode.Alpha2))
        //    Explode(bound.center);

        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.Temp);
        for (int i = 0; i < positions.Length; i++)
        {
            ParticleType type = map.GetParticleType(positions[i]);
            if (type ==  ParticleType.Cinder)
            {
                Explode(bound.center, tickBlock.tick);
                isEnable = false;
                target.isVisible = false;
                target.isEnable = false;
            }
        }
        positions.Dispose();
    }


    private void Explode(int2 position, int tick)
    {
        Explosive.SetExplosive(position, in settings.explosiveSettings, map);
        pixelCamera.transform.ScreenShake(in settings.shakeSettings, tick);
        PostProcessManager.EnqueueScreenFlash(in settings.screenFlashSettings);
        PostProcessManager.EnqueueShockwave(in settings.shockwaveSettings, position);
    }
}
