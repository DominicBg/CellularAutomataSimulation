//using System.Collections;
//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Mathematics;
//using UnityEngine;

//public class WorldLevelContainer : MonoBehaviour, ILevelContainer
//{
//    public WorldObject[] worldObjects;
//    int2 levelPosition;


//    public void OnValidate()
//    {
//        worldObjects = GetComponentsInChildren<WorldObject>();
//    }

//    public void Init(Map map, LevelContainer currentLevel)
//    {
//        for (int i = 0; i < worldObjects.Length; i++)
//        {
//            worldObjects[i].Init(map, currentLevel);
//        }
//    }

//    public void UpdateLevelMap(int2 currentLevelPosition, Map map, LevelContainer currentLevel)
//    {
//        levelPosition = currentLevelPosition;
//        for (int i = 0; i < worldObjects.Length; i++)
//        {
//            worldObjects[i].UpdateLevelMap(currentLevelPosition, map, currentLevel);
//        }
//    }

//    public void Render(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
//    {
//        for (int i = 0; i < worldObjects.Length; i++)
//            if (math.all(worldObjects[i].currentLevel == levelPosition) && worldObjects[i].isVisible)
//                worldObjects[i].Render(ref outputColors, ref tickBlock, 0);

//    }
//    public void PreRender(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
//    {
//    }

//    public void PostRender (ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
//    {
//        for (int i = 0; i < worldObjects.Length; i++)
//            if (math.all(worldObjects[i].currentLevel == levelPosition) && worldObjects[i].isVisible)
//                worldObjects[i].PostRender(ref outputColors, ref tickBlock);
//    }

//    public void RenderUI(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
//    {
//        for (int i = 0; i < worldObjects.Length; i++)
//            if (math.all(worldObjects[i].currentLevel == levelPosition) && worldObjects[i].isVisible)
//                worldObjects[i].RenderUI(ref outputColors, ref tickBlock);
//    }

//    public void RenderDebug(ref NativeArray<Color32> outputColors, ref TickBlock tickBlock)
//    {
//        //for (int i = 0; i < worldObjects.Length; i++)
//        //    if (math.all(worldObjects[i].currentLevel == levelPosition) && worldObjects[i].isVisible)
//        //        worldObjects[i].RenderDebug(ref outputColors, ref tickBlock);
//    }

//    public void OnUpdate(ref TickBlock tickBlock)
//    {           
//        for (int i = 0; i < worldObjects.Length; i++)
//            if (worldObjects[i].isEnable)
//                worldObjects[i].OnUpdate(ref tickBlock);
//    }

//    public void OnLateUpdate(ref TickBlock tickBlock)
//    {
//        for (int i = 0; i < worldObjects.Length; i++)
//            if (worldObjects[i].isEnable)
//                worldObjects[i].OnLateUpdate(ref tickBlock);
//    }

//    public void Dispose()
//    {
//        for (int i = 0; i < worldObjects.Length; i++)
//        {
//            worldObjects[i].Dispose();
//        }
//    }
//}
