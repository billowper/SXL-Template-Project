using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(GrindSpline))]
public class GrindSplineEditor : Editor
{
    [SerializeField] private bool showPoints;

    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        {
            EditorGUILayout.LabelField("Grind Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SurfaceType"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsRound"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsCoping"));

            EditorGUILayout.HelpBox("These are used by the map importer to determine what kind of grind this is", MessageType.Info, true);
        }
        EditorGUILayout.EndVertical();

        if (targets.Length == 1)
        {
            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                EditorGUILayout.LabelField("Spline Tools", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Add Point"))
                {
                    GrindSplineUtils.AddPoint(grindSpline);
                }

                drawPoints = GUILayout.Toggle(drawPoints, "Draw Points", new GUIStyle("button"));

                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("Rename Points"))
                {
                    foreach (Transform x in grindSpline.transform)
                    {
                        x.gameObject.name = $"Point ({x.GetSiblingIndex() + 1})";
                    }
                }
                EditorGUI.indentLevel++;

                showPoints = EditorGUILayout.Foldout(showPoints, $"Points ({grindSpline.transform.childCount})");
                if (showPoints)
                {
                    foreach (Transform child in grindSpline.transform)
                    {
                        EditorGUILayout.ObjectField(child, typeof(Transform), true);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        {
            EditorGUILayout.LabelField("Colliders ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderContainer"));
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderGenerationSettings"), true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliders"), true);
            EditorGUI.indentLevel--;

            if (GUILayout.Button("Generate Colliders"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                {
                    foreach (var o in targets)
                    {
                        var t = (GrindSpline) o;

                        t.GenerateColliders();
                    }

                    serializedObject.UpdateIfRequiredOrScript();
                }
            }
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
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

