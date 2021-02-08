using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StatisGun : GunBaseElement
{
    protected override void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map)
    {
        Bound viewingBound = scene.pixelCamera.GetViewingBound();
        var positions = viewingBound.GetPositionsGrid();
        for (int i = 0; i < positions.Length; i++)
        {
            if(map.InBound(positions[i]) && !map.IsFreePosition(positions[i]) && map.CanPush(positions[i], GameManager.PhysiXVIISetings))
            {
                Particle particle = map.GetParticle(positions[i]);
                particle.tickStatis = 360;
                map.SetParticle(positions[i], particle);
            }
        }
        positions.Dispose();
    }
}
