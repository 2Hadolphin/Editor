using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapEditor))]
public class Editor_MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapEditor editor = (MapEditor)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Edit Scene Flags"))
        {
            editor.BuildFlag();
        }

        if (GUILayout.Button("Save Scene Flags"))
        {
            editor.SaveFlag();
        }

    }
}
