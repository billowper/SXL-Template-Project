using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
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
            {
                if (GUILayout.Button("Add GrindSpline"))
                {
                    CreateSpline();

                    serializedObject.UpdateIfRequiredOrScript();
                }

                drawSplines = GUILayout.Toggle(drawSplines, new GUIContent("Draw GrindSplines"), new GUIStyle("button"));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Default Spline Settings:");

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SurfaceType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsRound"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsCoping"));

            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Separator();

            GUI.enabled = grindSurface.Splines.Count > 0;

            if (GUILayout.Button("Generate All Colliders"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(GenerateSplinesColliders());
                }
            }

            if (GUILayout.Button("Generate All Colliders (Use Generation Settings)"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(GenerateSplinesColliders(grindSurface.ColliderGenerationSettings));
                }
            }

            if (GUILayout.Button("Destroy All & Reset"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                {
                    grindSurface.DestroySplines();

                    serializedObject.UpdateIfRequiredOrScript();

                    return;
                }
            }

            GUI.enabled = true;

            var splines_arr = serializedObject.FindProperty("Splines");
            if (splines_arr.arraySize > 0)
            {
                EditorGUILayout.BeginVertical(new GUIStyle("box"));
                {
                    for (int i = 0; i < splines_arr.arraySize; i++)
                    {
                        var e = splines_arr.GetArrayElementAtIndex(i);
                        var spline = (GrindSpline) e.objectReferenceValue;
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.ObjectField(spline, typeof(GrindSpline), true);

                        if (GUILayout.Button("Generate Colliders"))
                        {
                            if (EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                            {
                                spline.GenerateColliders();
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                }
                EditorGUILayout.EndVertical();
            }
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(new GUIStyle("box"));
        {
            EditorGUILayout.LabelField("GrindSpline Generation (Experimental)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Spline generation is an experimental feature and not yet yielding good results for complex objects. It's pretty good at basic ledges, hubbas, etc.", MessageType.Info, true);

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderGenerationSettings"), true);
            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderContainer"), true);

            if (GUILayout.Button("Generate Splines"))
            {
                if (grindSurface.Splines.Count == 0 || EditorUtility.DisplayDialog("Confirm", "Are you sure? This cannot be undone", "Yes", "No!"))
                {
                    grindSurface.DestroySplines();

                    GrindSplineGenerator.Generate(grindSurface, grindSurface.ColliderGenerationSettings);

                    serializedObject.UpdateIfRequiredOrScript();
                    return;
                }
            }
        }
        EditorGUILayout.EndVertical();

        GUI.enabled = true;

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }

    private IEnumerator GenerateSplinesColliders(ColliderGenerationSettings settings = null)
    {
        var original_rotation = grindSurface.transform.rotation;
        grindSurface.transform.rotation = Quaternion.identity;

        yield return null;

        foreach (var s in grindSurface.Splines)
        {
            s.GenerateColliders(settings);
        }
        
        yield return null;

        grindSurface.transform.rotation = original_rotation;
    }

    private GrindSpline CreateSpline()
    {
        var gs = new GameObject("GrindSpline", typeof(GrindSpline)).GetComponent<GrindSpline>();

        gs.SurfaceType = grindSurface.SurfaceType;
        gs.IsRound = grindSurface.IsRound;
        gs.IsCoping = grindSurface.IsCoping;
        
        gs.PointsContainer = new GameObject("Points").transform;
        gs.PointsContainer.SetParent(gs.transform);
        gs.PointsContainer.localPosition = Vector3.zero;

        gs.transform.SetParent(grindSurface.transform);
        gs.transform.localPosition = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(gs.gameObject, "Created GrindSpline");

        Undo.RecordObject(grindSurface, "Added GrindSpline");

        grindSurface.Splines.Add(gs);
        
        return gs;
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

            if (activeSpline != null && activeSpline.PointsContainer.childCount > 0)
            {
                Handles.DrawAAPolyLine(3f, activeSpline.PointsContainer.GetChild(activeSpline.PointsContainer.childCount - 1).position, nearestVert);
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
                // destroy if invalid

                if (activeSpline != null && activeSpline.PointsContainer.childCount < 2)
                {
                    foreach (var c in activeSpline.GeneratedColliders)
                        DestroyImmediate(c.gameObject);

                    DestroyImmediate(activeSpline.gameObject);
                    
                    grindSurface.Splines.Remove(activeSpline);
                }
                
                activeSpline = null;

                Repaint();
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                drawSplines = false;

                if (activeSpline != null)
                {
                    foreach (var c in activeSpline.GeneratedColliders)
                        DestroyImmediate(c.gameObject);

                    DestroyImmediate(activeSpline.gameObject);
                    grindSurface.Splines.Remove(activeSpline);
                    activeSpline = null;
                }

                Repaint();
            }
        }
    }
}