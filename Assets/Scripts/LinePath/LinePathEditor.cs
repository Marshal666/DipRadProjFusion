
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LinePath))]
[CanEditMultipleObjects]
public class LinePathEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        LinePath linePath = (LinePath)target;
        GUILayout.Label($"Length: {linePath.Length}");
        if(GUILayout.Button("Reinit"))
        {
            linePath.Reinit();
        }
    }

}

#endif