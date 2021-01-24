using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLevelEditorManager))]
public class LevelEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameLevelEditorManager levelEditor = (GameLevelEditorManager)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Save level"))
        {
            levelEditor.Save();
        }
        if (GUILayout.Button("Load level"))
        {
            levelEditor.Load();
        }

        GUILayout.Space(15);

        //show partial scene
        var partialScenes = FindObjectsOfType<PixelPartialScene>();
        for (int i = 0; i < partialScenes.Length; i++)
        {
            if (GUILayout.Button("Show " + partialScenes[i].name))
            {
                for (int j = 0; j < partialScenes.Length; j++)
                {
                    partialScenes[j].SetActive(i == j);
                }
            }
        }
    }
}