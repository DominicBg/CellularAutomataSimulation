using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public abstract class GunBaseElement : EquipableElement
{
    public enum GunAnim { Idle, Fire}

    public BaseGunScriptable baseSettings;
    protected SpriteAnimator spriteAnimator;

    bool isUsedThisFrame = false;

    public override void Init(Map map)
    {
        base.Init(map);
        spriteAnimator = new SpriteAnimator(baseSettings.spriteSheet);
        spriteAnimator.framePerImage = baseSettings.framePerImage;
    }
    protected abstract void OnShoot(int2 aimStartPosition, float2 aimDirection, Map map);

    public override void Use(int2 usePosition)
    {
        isUsedThisFrame = true;
        int2 aimPosition = GridPicker.GetGridPosition(GameManager.GridSizes) - 2;
        //int2 startPosition = usePosition + new int2(9, 3);
        int2 startPosition = player.position + GetAjustedOffset(baseSettings.shootOffset);

        float2 aimDirection = math.normalize(new float2(aimPosition - startPosition));
        OnShoot(startPosition, aimDirection, map);
    }

    public override void OnUpdate(ref TickBlock tickBlock)
    {
        spriteAnimator.Update();
        spriteAnimator.SetAnimation(isUsedThisFrame ? (int)GunAnim.Fire : (int)GunAnim.Idle);
        isUsedThisFrame = false;

        base.OnUpdate(ref tickBlock);
    }

    public override void OnRender(ref NativeArray<Color32> outputcolor, ref TickBlock tickBlock)
    {     
        int2 renderPos = isEquiped ? GetEquipOffset() : position;
        int2 kickOffset = GetKickOffset();
        bool isFlipped = isEquiped ? player.lookLeft : false;

        spriteAnimator.Render(ref outputcolor, renderPos + kickOffset, isFlipped);
    }

    protected int2 GetKickOffset()
    {
        int2 kickOffset = (spriteAnimator.currentAnim == (int)GunAnim.Fire && spriteAnimator.currentFrame == 1) ? baseSettings.kickDirection : 0;
        kickOffset.x = player.lookLeft ? -kickOffset.x : kickOffset.x;
        return kickOffset;
    }

    protected int2 GetEquipOffset()
    {
        int2 offset = baseSettings.equipedOffset;
        if (player.lookLeft)
            offset.x = -offset.x;

        offset -= spriteAnimator.nativeSpriteSheet.spriteSizes / 2;
        return player.GetBound().center + offset;
    }
    protected int2 GetAjustedOffset(int2 offset)
    {
        if (player.lookLeft)
        {
            offset.x = spriteAnimator.nativeSpriteSheet.spriteSizes.x - offset.x - 1;
        }

        return offset; 
    }
}
