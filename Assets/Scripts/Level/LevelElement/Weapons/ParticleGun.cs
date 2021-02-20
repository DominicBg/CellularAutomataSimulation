using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ParticleGun : GunBaseElement
{
    public ParticleType type;
    public ParticleGunScriptable settings => (ParticleGunScriptable)baseSettings;

    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, ref TickBlock tickBlock)
    {
        Particle newParticle = new Particle()
        {
            type = type,
            velocity = aimDirection * settings.particleSpeed
        };

        if(map.IsFreePosition(aimStartPosition))
            map.SetParticle(aimStartPosition, newParticle, false, true);
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {
        //FIX render POS
        base.Render(ref outputcolor, ref tickBlock, renderPos, ref info);
        //int2 pos1 = isEquiped ? GetEquipOffset(renderPos, baseSettings.equipedOffset) + GetAjustedOffset(settings.particleOffset) : renderPos + settings.particleOffset;
        //pos1 += GetKickOffset();

        //ref ParticleRendering particleRendering = ref GridRenderer.Instance.particleRendering;
        //render 1 particles lol
        //outputcolor[ArrayHelper.PosToIndex(pos1, GameManager.GridSizes)] =
        //    ParticleRenderUtil.GetColorForType(pos1, type, ref particleRendering, ref tickBlock, ref map, levelContainer.lightSources);
    }
}
