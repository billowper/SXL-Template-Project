using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrindSurface))]
public class GrindSurfaceEditor : Editor
{
    private bool drawSplines;
    private Vector3 nearestVert;

    private GrindSurface grindSurface => ((GrindSurface) target);

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        if (grindSurface.GetComponent<GrindSpline>() != null)
        {
            EditorGUILayout.HelpBox("Found GrindSpline on this GameObject. This is not supported. Please remove the GrindSpline or this component.", MessageType.Error);

            GUI.enabled = false;
        }

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();
        
        // ---------------------------- Splines

        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        {
            EditorGUILayout.LabelField("Splines", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add GrindSpline"))
            {
                CreateSpline();

                serializedObject.UpdateIfRequiredOrScript();
            }

            drawSplines = GUILayout.Toggle(drawSplines, new GUIContent("Draw GrindSplines"), new GUIStyle("button"));
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Splines"), true);
            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndVertical();

        // ---------------------------- Colliders

        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        {
            EditorGUILayout.LabelField("Colliders", EditorStyles.boldLabel);

            if (GUILayout.Button("Generate Colliders"))
            {
                grindSurface.GenerateColliders();
            }

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderType"));

            if (grindSurface.ColliderType == GrindSurface.ColliderTypes.Capsule)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliderRadius"));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliderWidth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliderDepth"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsEdge"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderContainer"));

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliders"), true);
            EditorGUI.indentLevel--;

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndVertical();
        
        if (GUILayout.Button("Destroy & Reset"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
            {
                foreach (var c in grindSurface.GeneratedColliders)
                {
                    DestroyImmediate(c.gameObject);
                }

                foreach (var s in grindSurface.Splines)
                {
                    DestroyImmediate(s.gameObject);
                }

                grindSurface.GeneratedColliders.Clear();
                grindSurface.Splines.Clear();

                serializedObject.UpdateIfRequiredOrScript();

                return;
            }
        }

        GUI.enabled = true;
    }

    private GrindSpline CreateSpline()
    {
        var gs = new GameObject("GrindSpline", typeof(GrindSpline));

        gs.transform.SetParent(grindSurface.transform);
        gs.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(gs, "Created GrindSpline");

        var spline = gs.GetComponent<GrindSpline>();

        Undo.RecordObject(grindSurface, "Added GrindSpline");

        grindSurface.Splines.Add(spline);
        
        return spline;
    }

    private GrindSpline activeSpline;

    private void OnSceneGUI()
    {
        if (drawSplines)
        {
            Handles.BeginGUI();
            {
                var r = new Rect(10, SceneView.currentDrawingSceneView.camera.pixelHeight - 30*3 + 10, 400, 30*3);

                GUILayout.BeginArea(r);
                GUILayout.BeginVertical(new GUIStyle("box"));
                GUILayout.Label($"<color=white>LMB = Add Point/Create New Spline\nSPACE = Complete Spline\nESC = Stop Drawing</color>", new GUIStyle("label") {richText = true, fontSize = 14, fontStyle = FontStyle.Bold});
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            Handles.EndGUI();

            HandleUtility.AddDefaultControl(GetHashCode());

            nearestVert = GrindSplineUtils.PickNearestVertexToCursor(0.02f, grindSurface.transform);

            HandleUtility.Repaint();

            Handles.color = Color.green;

            if (activeSpline != null && activeSpline.transform.childCount > 0)
            {
                Handles.DrawLine(activeSpline.transform.GetChild(activeSpline.transform.childCount - 1).position, nearestVert);
            }

            Handles.Label(nearestVert + Vector3.up * .5f, activeSpline != null ? "Add Point" : "Create Grind", new GUIStyle("whiteLabel"));
            Handles.SphereHandleCap(0, nearestVert, Quaternion.identity, 0.25f, EventType.Repaint);

            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
            {
                if (activeSpline == null)
                {
                    activeSpline = CreateSpline();
                    activeSpline.transform.position = nearestVert;

                    Undo.RegisterCreatedObjectUndo(activeSpline.gameObject, "Create GrindSpline");

                    GrindSplineUtils.AddPoint(activeSpline);
                }
                else
                {
                    GrindSplineUtils.AddPoint(activeSpline, nearestVert);
                }
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            {
                activeSpline = null;
                grindSurface.GenerateColliders();
                Repaint();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                drawSplines = false;
                activeSpline = null;
                grindSurface.GenerateColliders();
                Repaint();
            }
        }
    }
}