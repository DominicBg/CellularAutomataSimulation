using Unity.Mathematics;

public class ParticleWeapon : WeaponBase
{
    ParticleWorldWeapon weapon;

    public ParticleWeapon(ParticleWorldWeapon weapon)
    {
        this.weapon = weapon;
    }

    public override void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map)
    {
        Particle newParticle = new Particle()
        {
            type = weapon.type,
            velocity = aimDirection * weapon.particleSpeed
        };

        if(map.IsFreePosition(aimStartPosition))
            map.SetParticle(aimStartPosition, newParticle);
    }
}
