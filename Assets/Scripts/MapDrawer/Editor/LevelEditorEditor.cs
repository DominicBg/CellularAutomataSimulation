using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameLevelEditorManager))]
public class LevelEditorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GameLevelEditorManager levelEditor = (GameLevelEditorManager)target;

        DrawDefaultInspector();
        if (GUILayout.Button("Create new LevelDataScriptable"))
        {
            LevelDataScriptable levelDataScriptable = CreateInstance<LevelDataScriptable>();

            Debug.Log("Created new LevelData");
            AssetDatabase.CreateAsset(levelDataScriptable, "Assets/LevelDatas/levelDataScriptable.asset");
            AssetDatabase.SaveAssets();

            levelEditor.levelDataScriptable = levelDataScriptable;
        }
        if (GUILayout.Button("Save level"))
        {
            levelEditor.levelDataScriptable.SaveLevel(levelEditor.levelData);
        }
        if (GUILayout.Button("Load level"))
        {
            levelEditor.levelData = levelEditor.levelDataScriptable.LoadLevel();
        }
        if (GUILayout.Button("Reset level"))
        {
            levelEditor.ResetLevelData();
        }

    }
}