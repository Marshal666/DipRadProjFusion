using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrackObject))]
[CanEditMultipleObjects]
public class TrackObjectEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TrackObject to = (TrackObject)target;

        if(GUILayout.Button("Init"))
        {
            to.Init();
        }

        if (GUILayout.Button("Clear"))
        {
            to.Clear();
        }
    }

}
