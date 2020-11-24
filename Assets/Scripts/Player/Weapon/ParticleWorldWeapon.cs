using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldParticleWeapon", menuName = "World Weapon/World Particle Weapons", order = 1)]
public class ParticleWorldWeapon : WorldWeapon
{
    public float particleSpeed;
    public ParticleType type;

    public override WeaponBase GetWeapon()
    {
        return new ParticleWeapon(this);
    }
}
