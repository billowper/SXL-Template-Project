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
            
            if (GUILayout.Button("Generate Splines"))
            {
                if (grindSurface.Splines.Count == 0 || EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                {
                    grindSurface.DestroySplines();

                    GrindSplineGenerator.Generate(grindSurface);

                    serializedObject.UpdateIfRequiredOrScript();
                    return;
                }
            }

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

                if (serializedObject.FindProperty("IsEdge").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AutoDetectEdgeAlignment"));

                    if (serializedObject.FindProperty("AutoDetectEdgeAlignment").boolValue == false)
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("FlipEdge"));
                    }
                }
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
                grindSurface.DestroySplines();

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
            HandleUtility.AddDefaultControl(GetHashCode());

            var pick_new_vert = GrindSplineUtils.PickNearestVertexToCursor(out var pos, 0, grindSurface.transform);
            
            if (pick_new_vert)
                nearestVert = pos;

            HandleUtility.Repaint();

            Handles.color = Color.green;

            if (activeSpline != null && activeSpline.transform.childCount > 0)
            {
                Handles.DrawAAPolyLine(3f, activeSpline.transform.GetChild(activeSpline.transform.childCount - 1).position, nearestVert);
            }

            var label = (activeSpline != null ? "Shift Click : Add Point\n" : "Shift + LMB : Create Grind\n") +
                        $"Space : Confirm\n" +
                        $"Escape : {(activeSpline == null ? "Exit Drawing Mode" : "Cancel")}";
            
            var offset = Vector3.up * Mathf.Lerp(0.1f, 1f, Mathf.Clamp01(SceneView.currentDrawingSceneView.cameraDistance / 4));

            Handles.Label(nearestVert + offset, label, new GUIStyle("whiteLabel") {richText = true, fontSize = 14, fontStyle = FontStyle.Bold});
            Handles.CircleHandleCap(0, nearestVert, Quaternion.LookRotation(SceneView.currentDrawingSceneView.camera.transform.forward), 0.02f, EventType.Repaint);

            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && Event.current.modifiers.HasFlag(EventModifiers.Shift))
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
                if (activeSpline != null && activeSpline.transform.childCount < 2)
                {
                    DestroyImmediate(activeSpline.gameObject);
                    grindSurface.Splines.Remove(activeSpline);
                }

                activeSpline = null;

                grindSurface.GenerateColliders();

                Repaint();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                drawSplines = false;

                if (activeSpline != null)
                {
                    DestroyImmediate(activeSpline.gameObject);
                    grindSurface.Splines.Remove(activeSpline);
                    activeSpline = null;
                }

                grindSurface.GenerateColliders();

                Repaint();
            }
        }
    }
}