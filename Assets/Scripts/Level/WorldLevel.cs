using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    public PixelCamera pixelCamera;
    public PixelScene pixelScene;
    public PixelSceneData pixelSceneData;
    public PixelPartialScene currentPartialScene;

    public bool inDebug = false;

    public float transitionSpeed = 1;
    TransitionInfo transitionInfo;

    TickBlock tickBlock;
    TickBlock postProcessTickBlock;

    public bool updateWorldElement = true;
    public bool updatLevelElement = true;

    RenderPassRecorder renderPassRecorder;

    public void LoadLevel()
    {
        tickBlock.Init();
        postProcessTickBlock.Init();

        pixelCamera = new PixelCamera(pixelScene.GetComponentInChildren<PixelCameraTransform>(),GameManager.RenderSizes);
        pixelScene.Init(pixelSceneData.LoadMap(), pixelCamera);

        PostProcessManager.Instance = new PostProcessManager();

        renderPassRecorder = new RenderPassRecorder();

        CheatManager.AddCheat("Debug Mode", () => inDebug = !inDebug);
        CheatManager.AddCheat("Render Pass Recorder", () => renderPassRecorder.RecordRenderPass(pixelScene, ref tickBlock, GameManager.RenderSizes));
    }

    public void OnUpdate()
    {
        if (updatLevelElement)
            tickBlock.UpdateTick();

        postProcessTickBlock.UpdateTick();

        if (!transitionInfo.isInTransition)
        {
            pixelScene.OnUpdate(ref tickBlock, pixelCamera.position);
        }
        else
        {
            UpdateTransition();
        }
        PostProcessManager.Instance.Update(ref postProcessTickBlock);
    }

    public NativeArray<Color32> GetPixelCameraRender()
    {
        return pixelCamera.Render(pixelScene, ref tickBlock, inDebug);
    }


    public void OnRender()
    {
        //ADD transition rendering
        if(!transitionInfo.isInTransition)
        {
            var pixels = GetPixelCameraRender();
            PostProcessManager.Instance.Render(ref pixels, ref postProcessTickBlock);
            GridRenderer.RenderToScreen(pixels);
        }
        else
        {
            var outputs = GridRenderer.GetBlankTexture();

            transitionInfo.currentPartialScene.SetActive(true);
            transitionInfo.nextPartialScene.SetActive(false);
            var pixels1 = GetPixelCameraRender();

            int2 cameraPos = pixelCamera.position;

            //Align camera at second position and take a screenshot
            pixelCamera.position = transitionInfo.entrance.position;
            transitionInfo.currentPartialScene.SetActive(false);
            transitionInfo.nextPartialScene.SetActive(true);
            var pixels2 = GetPixelCameraRender();

            transitionInfo.transition.Transition(ref outputs, ref pixels1, ref pixels2, transitionInfo.transitionRatio);

            pixelCamera.position = cameraPos;
            GridRenderer.RenderToScreen(outputs);
            pixels1.Dispose();
            pixels2.Dispose();
        }
    }

   
    public void UpdateTransition()
    {
        transitionInfo.transitionRatio += GameManager.DeltaTime * transitionSpeed;

        if(!transitionInfo.changedPartialScene && transitionInfo.transitionRatio > 0.5f)
        {
            transitionInfo.changedPartialScene = true;
            currentPartialScene.SetActive(false);
            transitionInfo.nextPartialScene.SetActive(true);

            currentPartialScene = transitionInfo.nextPartialScene;

            //lol
            //PlayerElement player = GetComponentInChildren<PlayerElement>();

            pixelScene.player.SetPosition(transitionInfo.entrance.position);
            //pixelScene.player.currentEquipMouse?.OnUpdate(ref tickBlock);
            //pixelScene.player.currentEquipQ?.OnUpdate(ref tickBlock);
        }

        if (transitionInfo.transitionRatio >= 1)
        {
            transitionInfo.isInTransition = false;
            pixelCamera.position = transitionInfo.entrance.position;
        }
    }

    public void StartTransition(LevelEntrance entrance, TransitionBase transition, PixelPartialScene nextPartialScene)
    {
        transitionInfo = new TransitionInfo()
        {
            isInTransition = true,
            entrance = entrance,
            transition = transition,
            transitionRatio = 0,
            currentPartialScene = currentPartialScene,
            nextPartialScene = nextPartialScene,
            changedPartialScene = false
        };
    }

    public void Dispose()
    {
        pixelScene.Dispose();
        Destroy(gameObject);
    }

    public struct TransitionInfo
    {
        public float transitionRatio;
        public bool isInTransition;
        public LevelEntrance entrance;
        public TransitionBase transition;
        public PixelPartialScene currentPartialScene;
        public PixelPartialScene nextPartialScene;
        public bool changedPartialScene;
    }
}
