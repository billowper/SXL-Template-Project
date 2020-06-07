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

        if (GUILayout.Button("Rename Points"))
        {
            foreach (Transform x in grindSpline.transform)
            {
                x.gameObject.name = $"Point ({x.GetSiblingIndex() + 1})";
            }
        }
    }

    private bool drawPoints;
    private Vector3 nearestVert;

    private GrindSpline grindSpline => ((GrindSpline) target);

    private void OnSceneGUI()
    {
        if (drawPoints)
        {
            HandleUtility.AddDefaultControl(GetHashCode());

            var pick_new_vert = GrindSplineUtils.PickNearestVertexToCursor(out var pos);
            
            if (pick_new_vert)
                nearestVert = pos;
            
            HandleUtility.Repaint();

            Handles.color = Color.green;

            if (grindSpline.transform.childCount > 0)
            {
                Handles.DrawAAPolyLine(3f, grindSpline.transform.GetChild(grindSpline.transform.childCount - 1).position, nearestVert);
            }

            var label = "Shift Click : Add Point\n" +
                        $"Space : Confirm\n" +
                        $"Escape : Cancel";

            var offset = Vector3.up * Mathf.Lerp(0.1f, 1f, Mathf.Clamp01(SceneView.currentDrawingSceneView.cameraDistance / 4));

            Handles.Label(nearestVert + offset, label, new GUIStyle("whiteLabel") {richText = true, fontSize = 14, fontStyle = FontStyle.Bold});           
            Handles.CircleHandleCap(0, nearestVert, Quaternion.LookRotation(SceneView.currentDrawingSceneView.camera.transform.forward), 0.02f, EventType.Repaint);

            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && Event.current.modifiers.HasFlag(EventModifiers.Shift))
            {
                GrindSplineUtils.AddPoint(grindSpline, nearestVert);
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                drawPoints = false;
                Repaint();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                drawPoints = false;
                Repaint();
            }
        }
    }
}

