using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public GridRenderer gridRenderer;
    public GridPicker gridPicker;

    NativeArray<ParticleSpawner> nativeParticleSpawners;

    public ParticleBehaviourScriptable particleBehaviour;

    public Map map;
    TickBlock tickBlock;

    public LevelContainer currentLevelContainer;

    public void OnStart()
    {
        LoadLevel(GameManager.Instance.currentLevelContainer);
    }

    public void OnEnd()
    {
        Dispose();
    }

    void Dispose()
    {
        if (nativeParticleSpawners.IsCreated)
        {
            nativeParticleSpawners.Dispose();
            map.Dispose();
        }
    }

    public void LoadLevel(LevelContainer levelContainer)
    {
        Dispose();

        map = levelContainer.LoadMap();
        nativeParticleSpawners = levelContainer.GetParticleSpawner();

        currentLevelContainer = levelContainer;

        tickBlock.Init();
        currentLevelContainer.Init(this, map);
    }

    public void UpdateSimulation()
    {
        new CellularAutomataJob()
        {
            behaviour = particleBehaviour.particleBehaviour,
            map = map,
            nativeParticleSpawners = nativeParticleSpawners,
            tickBlock = tickBlock
        }.Run();
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
        currentLevelContainer.OnUpdate(ref tickBlock);
       

        //if(equipedWeapon != null && Input.GetMouseButton(0))
        //{
        //    //todo add middle position in bound
        //    //use map.TryFindEmptyPosition to spawn the particle around the player

        //    int2 aimPosition = gridPicker.GetGridPosition(GameManager.GridSizes) - 2;
        //    int2 startPosition = player.position + new int2(9, 3);
        //    float2 aimDirection = math.normalize(new float2(aimPosition - startPosition));
        //    equipedWeapon.OnShoot(startPosition, aimDirection, map);
        //}
    }

    public void OnRender()
    {
        var outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);

        GridRenderer.ApplyMapPixels(ref outputColor, map, tickBlock);
        currentLevelContainer.OnRender(ref outputColor, ref tickBlock);
        
        GridRenderer.RenderToScreen(outputColor);

        //if(m_levelPhase == LevelPhase.gameplay)
        //    GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.astronaut), player.position);

        //GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.shuttle), shuttlePosition);

        //for (int i = 0; i < pixelSortingSettings.Length; i++)
        //     GridPostProcess.ApplyPixelSorting(ref outputColor, ref pixelSortingSettings[i]);


        //if (debugBound)
        //    DebugAllPhysicBound(ref outputColor);

        //int2 aimPosition = gridPicker.GetGridPosition(GameManager.GridSizes) - 2;
        //GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.aimPosition), aimPosition);
        //GridRenderer.ApplySprite(ref outputColor, worldWeapon.pixelSprite, worldWeapon.worldPosition);


    }
}
