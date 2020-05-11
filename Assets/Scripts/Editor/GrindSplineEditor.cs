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
            var p = ((GrindSpline) target).transform;
            var n = p.childCount;
            var go = new GameObject($"Point {n + 1}");

            go.transform.SetParent(p);
            go.transform.localPosition = Vector3.zero;
        }
    }
}