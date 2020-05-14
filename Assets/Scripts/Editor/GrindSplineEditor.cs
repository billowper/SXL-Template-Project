using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrindSpline))]
public class GrindSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Add Point"))
        {
            ((GrindSpline) target).AddPoint();
        }
    }
}