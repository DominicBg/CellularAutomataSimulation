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
        if (GUILayout.Button("Select Current"))
        {

            Selection.activeObject = (MonoBehaviour)levelEditor.currentWorldLevel.levels[(int2)levelEditor.viewPosition];
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

    }
}