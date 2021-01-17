using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ParticleGun : GunBaseElement
{
    public ParticleType type;
    public ParticleGunScriptable settings => (ParticleGunScriptable)baseSettings;

    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map)
    {
        Particle newParticle = new Particle()
        {
            type = type,
            velocity = aimDirection * settings.particleSpeed
        };

        if(map.IsFreePosition(aimStartPosition))
            map.SetParticle(aimStartPosition, newParticle, false, true);
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, int2 renderPos)
    {
        //FIX render POS
        base.Render(ref outputcolor, ref tickBlock, renderPos);
        int2 pos1 = isEquiped ? GetEquipOffset(baseSettings.equipedOffset) + GetAjustedOffset(settings.particleOffset) : position + settings.particleOffset;
        pos1 += GetKickOffset();

        ref ParticleRendering particleRendering = ref GridRenderer.Instance.particleRendering;
        //render 1 particles lol
        outputcolor[ArrayHelper.PosToIndex(pos1, GameManager.GridSizes)] =
            ParticleRenderUtil.GetColorForType(pos1, type, ref particleRendering, ref tickBlock, ref map, levelContainer.lightSources);
    }

    protected override void OnEquip()
    {
        Debug.Log("PARTICLE GUN EQUIPED");
    }

    protected override void OnUnequip()
    {
        Debug.Log("PARTICLE GUN UNEQUIPED");

    }
}
