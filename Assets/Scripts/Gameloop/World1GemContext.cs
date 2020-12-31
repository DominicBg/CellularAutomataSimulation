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

    public override void OnEnd()
    {
        OnEndCallBack.Invoke();
    }

    public override void OnRender()
    {
        var outputColors = GridRenderer.GetBlankTexture();
        NativeArray<float3> normals = new NativeArray<float3>(outputColors.Length, Allocator.TempJob);

        new World1GemRMJob()
        {
            outputColor = outputColors,
            tickBlock = tickBlock,
            normals = normals,
            csettings = settings.cameraSettings,
            dsettings = settings.diamondSettings
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        new RayMarchingEdgeDetectorJob()
        {
            normals = normals,
            outputColor = outputColors,
            settings = settings.edgeDetectionSettings,
        }.Schedule(GameManager.GridLength, GameManager.InnerLoopBatchCount).Complete();

        normals.Dispose();
        GridRenderer.RenderToScreen(outputColors);
    }

    public override void OnStart()
    {
        Debug.Log("Start context");
        tickBlock.Init();
    }

    public override void OnUpdate()
    {
        tickBlock.UpdateTick();

        if (InputCommand.IsButtonDown(KeyCode.Escape))
            ExitContext();
    }
}
