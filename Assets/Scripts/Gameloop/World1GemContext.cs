using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class World1GemContext : GameContext
{
    TickBlock tickBlock;
    public System.Action OnEndCallBack;
    [SerializeField] World1GemContextScriptable settings;
    [SerializeField] World1GemRMJob.CameraTransform cameraTransform;
    //NativeArray<Color32> astroTexture;
    //int2 astroSizes;

    public override void OnEnd()
    {
        //astroTexture.Dispose();
        OnEndCallBack.Invoke();
    }

    public override void OnRender()
    {
        var outputColors = GridRenderer.GetBlankTexture();
        NativeArray<float3> normals = new NativeArray<float3>(outputColors.Length, Allocator.TempJob);
        NativeArray<Color32> edgeColor = new NativeArray<Color32>(outputColors.Length, Allocator.TempJob);

        new World1GemRMJob()
        {
            outputColor = outputColors,
            tickBlock = tickBlock,
            normals = normals,
            edgeColor = edgeColor,

            //astroTexture = astroTexture,
            //astroTextureSizes = astroSizes,

            render = settings.cameraSettings,
            diamond = settings.diamondSettings,
            astro = settings.astroSettings,
            pillar = settings.pillarSettings,
            light = settings.light,
            camera = cameraTransform,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        new RayMarchingEdgeDetectorJob()
        {
            normals = normals,
            outputColor = outputColors,
            edgeColor = edgeColor,
            settings = settings.edgeDetectionSettings,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        normals.Dispose();
        edgeColor.Dispose();
        GridRenderer.RenderToScreen(outputColors);
    }

    public override void OnStart()
    {
        Debug.Log("Start context");
        tickBlock.Init();
        //astroTexture = RenderingUtils.GetNativeArray(settings.astroTexture, Allocator.Persistent);
        //astroSizes = new int2(settings.astroTexture.width, settings.astroTexture.height);
        cameraTransform = settings.cameraTransform;
    }

    public override void OnUpdate()
    {
        tickBlock.UpdateTick();

        cameraTransform.pitchYawRoll += new float3(InputCommand.Direction.y, InputCommand.Direction.x, 0) * settings.speed * GameManager.DeltaTime;
        cameraTransform.pitchYawRoll.x = math.clamp(cameraTransform.pitchYawRoll.x, settings.minPitch, settings.maxPitch);

        if (InputCommand.IsButtonDown(KeyCode.Escape))
            ExitContext();
    }
}
