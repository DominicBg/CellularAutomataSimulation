using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ParticleWeapon : WeaponBaseElement
{
    //to put in a scriptable?
    public ParticleType type;
    public float particleSpeed;


    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map)
    {
        Particle newParticle = new Particle()
        {
            type = type,
            velocity = aimDirection * particleSpeed
        };

        if(map.IsFreePosition(aimStartPosition))
            map.SetParticle(aimStartPosition, newParticle);
    }


    protected override void OnWeaponEquip()
    {
        Debug.Log("PARTICLE GUN EQUIPED");
    }

    protected override void OnWeaponUnequip()
    {
        Debug.Log("PARTICLE GUN UNEQUIPED");

    }
}
