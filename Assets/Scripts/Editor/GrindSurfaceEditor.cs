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
            if (GUILayout.Button("Add GrindSpline"))
            {
                CreateSpline();

                serializedObject.UpdateIfRequiredOrScript();
            }

            drawSplines = GUILayout.Toggle(drawSplines, "Draw GrindSplines", new GUIStyle("button"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Splines"), true);
        }
        EditorGUILayout.EndVertical();


        // ---------------------------- Colliders

        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        {
            if (GUILayout.Button("Generate Colliders"))
            {
                grindSurface.GenerateColliders();
            }

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


            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliders"), true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
        
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
                var r = new Rect(10, SceneView.currentDrawingSceneView.camera.pixelHeight - 40, 600, 30);

                GUILayout.BeginArea(r);
                GUILayout.BeginVertical(new GUIStyle("box"));
                GUILayout.Label($"<color=white>LMB = Add Point/Create New Spline, Esc = Stop Drawing</color>", new GUIStyle("label") { richText = true, fontSize = 14, fontStyle =  FontStyle.Bold});
                GUILayout.EndVertical();
                GUILayout.EndArea();
            }
            Handles.EndGUI();

            HandleUtility.AddDefaultControl(GetHashCode());

            nearestVert = GrindSplineUtils.PickNearestVertexToCursor(0.02f, grindSurface.transform);

            HandleUtility.Repaint();

            Handles.color = Color.green;

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