using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class EscapeInShuttleRule : LevelRule
{
    public float openLightDuration = 1;
    
    [Header("Shake phase")]
    public float shakeDuration= 1;
    public float shakeSpeed = .01f;
    public float shakeIntensity;
    public CameraShakeSettings shakeSettings;
    public PostProcessManager.ShockwaveSettings shockWaveSettings;

    [Header("Fly phase")]
    public float acceleration = .1f;
    public float flightShuttleDuration = 2;
    public float sinSpeed;
    public float sinAmp;
    public CameraShakeSettings flyShakeSettings;


    public enum ShuttleAnim { Idle, RedLight, GreenLight, Fly};
    public enum Phase { Idle, OpenLight, Shake, Travel}
    Phase phase = Phase.Idle;

    float2 shuttlePosition;
    float velocity = 0;

    public override void OnLevelFinish()
    {
        shuttlePosition = goalElement.position;
        map.RemoveSpriteAtPosition(playerElement.position, ref playerElement.physicData.physicBound);
        playerElement.isEnable = false;
        playerElement.isVisible = false;
        
        ////hack
        //if(playerElement.currentEquipMouse != null)
        //    playerElement.currentEquipMouse.isVisible = false;

        //if (playerElement.currentEquipQ != null)
        //    playerElement.currentEquipQ.isVisible = false;
    }


    public override void OnLateUpdate(ref TickBlock tickBlock)
    {
        base.OnLateUpdate(ref tickBlock);
        SpriteSheetObject shuttleSpriteSheet = (SpriteSheetObject)goalElement;

        if(playerFinished)
        {
            switch (phase)
            {
                case Phase.Idle:
                    phase = Phase.OpenLight;
                    shuttleSpriteSheet.PlayAnimation((int)ShuttleAnim.RedLight);
                    break;
                case Phase.OpenLight:
                    //set anim
                    if(tickBlock.DurationSinceTick(tickFinished) > openLightDuration)
                    {
                        shuttleSpriteSheet.PlayAnimation((int)ShuttleAnim.GreenLight);

                        phase = Phase.Shake;
                        tickFinished = tickBlock.tick;
                        pixelCamera.transform.ScreenShake(in shakeSettings, scene.CurrentTick);
                        PostProcessManager.EnqueueShockwave(in shockWaveSettings, goalElement.GetBound().center);
                    }

                    break;
                case Phase.Shake:

                    //set anim
                    if (tickBlock.DurationSinceTick(tickFinished) > shakeDuration)
                    {
                        phase = Phase.Travel;
                        tickFinished = tickBlock.tick;
                        shuttleSpriteSheet.PlayAnimation((int)ShuttleAnim.Fly);
                        pixelCamera.transform.ScreenShake(in flyShakeSettings, scene.CurrentTick);
                    }
                    else
                    {
                        float shakeNoise = shakeIntensity * noise.cnoise(new float2(0, tickBlock.tick * GameManager.DeltaTime * shakeSpeed));
                        float2 noisePos = new float2(shakeNoise, 0);
                        goalElement.position = (int2)(shuttlePosition + noisePos);
                    }

                    break;
                case Phase.Travel:
                    //set anim
                    if (tickBlock.DurationSinceTick(tickFinished) > flightShuttleDuration)
                    {
                        GameManager.Instance.SetOverworld();
                    }
                    else
                    {
                        velocity += acceleration * GameManager.DeltaTime;
                        shuttlePosition += new float2(0, velocity * GameManager.DeltaTime);

                        float sinNoise = sinAmp * math.sin(tickBlock.tick * GameManager.DeltaTime * sinSpeed - tickFinished);
                        float2 sinPosition = new float2(sinNoise, 0);

                        goalElement.position = (int2)(shuttlePosition + sinPosition);
                    }
                    break;
            }
        }
    }
}
