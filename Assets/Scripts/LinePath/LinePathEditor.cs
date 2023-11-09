
#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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

        //save points to JSON file
        if(GUILayout.Button("Save Points"))
        {
            var path = EditorUtility.SaveFilePanel("Save Points", "", "Points", "json");
            File.WriteAllText(path, linePath.ToJSON());
        }

        if(GUILayout.Button("Load Points"))
        {
            var path = EditorUtility.OpenFilePanel("Load Points", "", "json");
            var jsontxt = File.ReadAllText(path);
            linePath.ParseJSON(jsontxt);
        }
    }

}

#endif