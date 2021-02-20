using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class GunBaseElement : EquipableElement
{
    public enum GunAnim { Idle, Fire}

    public BaseGunScriptable baseGunSettings => (BaseGunScriptable)baseSettings;

    int cooldownShoot = 0;

    protected int tickShoot;

    protected abstract void OnShoot(int2 aimStartPosition, float2 aimDirection, ref TickBlock tickBlock);

    protected override void OnUse(int2 position, bool _, ref TickBlock tickBlock)
    {
        int2 mousePosition = GridPicker.GetGridPosition(GameManager.RenderSizes);
        int2 aimPosition = scene.pixelCamera.GetGlobalPosition(mousePosition);
        int2 startPosition = player.GetBound().center;
        float2 aimDirection = math.normalize(new float2(aimPosition - startPosition));
        OnShoot(startPosition, aimDirection, ref tickBlock);
        tickShoot = tickBlock.tick;
    }

    public override void OnEquipableUpdate(ref TickBlock tickBlock)
    {
        cooldownShoot = math.max(cooldownShoot - 1, 0);
    }

    public override void Render(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock, int2 renderPos, ref EnvironementInfo info)
    {     
        int2 kickOffset = GetKickOffset();
        spriteAnimator.Render(ref outputcolor, renderPos + kickOffset);
    }

    protected int2 GetKickOffset()
    {
        int2 kickOffset = (spriteAnimator.currentAnim == (int)GunAnim.Fire && spriteAnimator.currentFrame == 1) ? baseGunSettings.kickDirection : 0;
        kickOffset.x = player.lookLeft ? -kickOffset.x : kickOffset.x;
        return kickOffset;
    }
}
