using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class WorldLevel : MonoBehaviour
{
    public LevelPosition[] levelContainerPrefabList = default;

    public Dictionary<int2, LevelContainer> levels;
    public int2 currentLevelPosition;

    public LevelContainer CurrentLevel => levels[currentLevelPosition];


    public void LoadLevel()
    {
        levels = new Dictionary<int2, LevelContainer>();
        for (int i = 0; i < levelContainerPrefabList.Length; i++)
        {
            LevelContainer instance = Instantiate(levelContainerPrefabList[i].levelContainerPrefab);
            levels.Add(levelContainerPrefabList[i].position, instance);

            LevelContainerData data = instance.GetComponent<LevelContainerData>();
            instance.Init(data.LoadMap());

            //todo, delete
            //Delete data
        }
    }

    public void OnUpdate()
    {
        levels[currentLevelPosition].OnUpdate();
    }
    public void OnRender()
    {
        //TODO add global effect in rendering here?

        GridRenderer.GetBlankTexture(out NativeArray<Color32> outputColors);
        levels[currentLevelPosition].OnRender(ref outputColors);
        GridRenderer.RenderToScreen(outputColors);
    }

    public void Dispose()
    {
        foreach(var level in levels.Values)
        {
            Destroy(level.gameObject);
        }
        levels.Clear();
        Destroy(gameObject);
    }

    [System.Serializable]
    public class LevelPosition
    {
        public LevelContainer levelContainerPrefab;
        public int2 position;
    }
}
