using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Return.CentreModule;
[CustomEditor(typeof(GDS))]
public class Data_BinarySaveEditor : Editor
{
    private void OnSceneGUI()
    {
        
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GDS saver = (GDS)target;
        if (GUILayout.Button("Save Object"))
        {
            saver.SaveData();
        }

        if (GUILayout.Button("Load Object"))
        {
            saver.LoadData();
        }
    }
}
