using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class GameLevelManager : MonoBehaviour, FiniteStateMachine.State
{
    public GridRenderer gridRenderer;
    public PlayerCellularAutomata player;
    public GridPicker gridPicker;

    NativeArray<ParticleSpawner> nativeParticleSpawners;

    public ParticleBehaviourScriptable particleBehaviour;

    public Map map;
    TickBlock tickBlock;
    LevelData levelData;

    public enum LevelPhase { into, gameplay, ending}
    LevelPhase m_levelPhase;
    int tickAtPhase;

    //TEMP
    public int2 shuttlePosition;

    public PixelSortingSettings[] pixelSortingSettings;
    public Explosive.Settings explosiveSettings;
    public MandlebrotBackground.Settings mandlebrotSettings;
    public WorldWeapon worldWeapon;
    public WeaponBase equipedWeapon;

    [Header("Debug")]
    public bool debugBound;
    public PhysicBound.BoundFlag debugBoundFlag;

    InputCommand inputCommand = new InputCommand();

    public void OnStart()
    {
        m_levelPhase = LevelPhase.gameplay;
        tickAtPhase = 0;
        LoadLevel(GameManager.Instance.currentLevel);
        tickBlock.Init();

        inputCommand.CreateInput(KeyCode.X);
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

            //temp
            worldWeapon.Dispose();
        }
    }

    public void LoadLevel(LevelDataScriptable levelDataScriptable)
    {
        Dispose();

        levelData = levelDataScriptable.LoadLevel();
        map = new Map(levelData.grid, levelData.sizes);
        nativeParticleSpawners = new NativeArray<ParticleSpawner>(levelData.particleSpawners, Allocator.Persistent);

        player.Init(levelData.playerPosition, map);
        shuttlePosition = levelData.shuttlePosition;

        //TEMP
        worldWeapon.Init();
    }

    public void OnUpdate()
    {
        tickBlock.UpdateTick();
        tickAtPhase++;

        if (m_levelPhase == LevelPhase.gameplay)
        {
            player.OnUpdate(map);
        }

        new CellularAutomataJob()
        {
            behaviour = particleBehaviour.particleBehaviour,
            map = map,
            nativeParticleSpawners = nativeParticleSpawners,
            tickBlock = tickBlock
        }.Run();

        if (m_levelPhase == LevelPhase.gameplay && PlayerFinishedLevel())
        {
            Debug.Log("BRAVO");
            //map.RemoveSpriteAtPosition(playerSprite.position, ref player.physicBound);
            m_levelPhase = LevelPhase.ending;
            tickAtPhase = 0;
        }
        else if (m_levelPhase == LevelPhase.ending)
        {
            shuttlePosition += new int2(0, 1);
            if(tickAtPhase > 60)
            {
                GameManager.Instance.SetOverworld();
            }
        }

        ////TEMP
        //if (playerSprite.Bound.IntersectWith(worldWeapon.pixelSprite.Bound))
        //{
        //    equipedWeapon = worldWeapon.GetWeapon();
        //}

        if(equipedWeapon != null && Input.GetMouseButton(0))
        {
            //todo add middle position in bound
            //use map.TryFindEmptyPosition to spawn the particle around the player

            int2 aimPosition = gridPicker.GetGridPosition(GameManager.GridSizes) - 2;
            int2 startPosition = player.position + new int2(9, 3);
            float2 aimDirection = math.normalize(new float2(aimPosition - startPosition));
            equipedWeapon.OnShoot(startPosition, aimDirection, map);
        }


        inputCommand.Update();
        if (inputCommand.IsButtonDown(KeyCode.X))
        {
            Explosive.SetExplosive(player.position, ref explosiveSettings, map);
        }
    }

    public void OnRender()
    {
        var outputColor = new NativeArray<Color32>(GameManager.GridLength, Allocator.TempJob);

        GridRenderer.ApplyMapPixels(ref outputColor, map, tickBlock);

        if(m_levelPhase == LevelPhase.gameplay)
            GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.astronaut), player.position);

        GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.shuttle), shuttlePosition);

        for (int i = 0; i < pixelSortingSettings.Length; i++)
            GridPostProcess.ApplyPixelSorting(ref outputColor, ref pixelSortingSettings[i]);


        if (debugBound)
            DebugAllPhysicBound(ref outputColor);

        int2 aimPosition = gridPicker.GetGridPosition(GameManager.GridSizes) - 2;
        GridRenderer.ApplySprite(ref outputColor, SpriteRegistry.GetSprite(SpriteEnum.aimPosition), aimPosition);
        GridRenderer.ApplySprite(ref outputColor, worldWeapon.pixelSprite, worldWeapon.worldPosition);


        GridRenderer.RenderToScreen(outputColor);
    }

    void DebugAllPhysicBound(ref NativeArray<Color32> outputColor)
    {
        PhysicBound physicbound = player.physicBound;
        int2 position = player.position;

        if((debugBoundFlag & PhysicBound.BoundFlag.All) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetCollisionBound(position), Color.magenta);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Feet) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetFeetCollisionBound(position), Color.yellow);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Left) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetLeftCollisionBound(position), Color.red);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Right) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetRightCollisionBound(position), Color.blue);

        if ((debugBoundFlag & PhysicBound.BoundFlag.Top) > 0)
            DebugPhysicBound(ref outputColor, physicbound.GetTopCollisionBound(position), Color.cyan);
    }

    void DebugPhysicBound(ref NativeArray<Color32> outputColor, Bound bound, Color color)
    {
        bound.GetPositionsGrid(out NativeArray<int2> positions, Allocator.TempJob);
        NativeArray<Color32> colors = new NativeArray<Color32>(positions.Length, Allocator.TempJob);
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = color;
        }
        GridRenderer.ApplyPixels(ref outputColor, ref positions, ref colors);
        positions.Dispose();
        colors.Dispose();
    }


    bool PlayerFinishedLevel()
    {
        return false; // playerSprite.Bound.IntersectWith(shuttleSprite.Bound);
    }
}
