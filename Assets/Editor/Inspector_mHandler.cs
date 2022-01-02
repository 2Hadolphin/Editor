using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(m_Handler))]
public class Inspector_mHandler : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        return;
        var it = (m_Handler)target;

        //it.Type=(m_Handler.mValueType)EditorGUILayout.EnumPopup(it.Type);
    }
}
