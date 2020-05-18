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
            GrindSplineUtils.AddPoint(grindSpline);
        }

        drawPoints = GUILayout.Toggle(drawPoints, "Draw Points", new GUIStyle("button"));
    }

    private bool drawPoints;
    private Vector3 nearestVert;

    private GrindSpline grindSpline => ((GrindSpline) target);

    private void OnSceneGUI()
    {
        if (drawPoints)
        {
            HandleUtility.AddDefaultControl(GetHashCode());

            nearestVert = GrindSplineUtils.PickNearestVertexToCursor(0.02f);
            
            HandleUtility.Repaint();

            Handles.color = Color.green;

            if (grindSpline.transform.childCount > 0)
            {
                Handles.DrawLine(grindSpline.transform.GetChild(grindSpline.transform.childCount - 1).position, nearestVert);
            }

            Handles.Label(nearestVert + Vector3.up * .5f, "Add Point");
            Handles.SphereHandleCap(0, nearestVert, Quaternion.identity, 0.25f, EventType.Repaint);

            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                GrindSplineUtils.AddPoint(grindSpline, nearestVert);
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                drawPoints = false;
                Repaint();
            }
        }
    }
}

