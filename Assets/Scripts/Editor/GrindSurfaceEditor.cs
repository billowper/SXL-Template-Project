using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrindSurface))]
public class GrindSurfaceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.UpdateIfRequiredOrScript();

        var creator = (GrindSurface) target;

        if (creator.GetComponent<GrindSpline>() != null)
        {
            EditorGUILayout.HelpBox("Found GrindSpline on this GameObject. This is not supported. Please remove the GrindSpline or this component.", MessageType.Error);

            GUI.enabled = false;
        }

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Spline"));
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        if (serializedObject.FindProperty("Spline").objectReferenceValue == null)
        {
            if (GUILayout.Button("Create GrindSpline"))
            {
                var gs = new GameObject("GrindSpline", typeof(GrindSpline));

                gs.transform.SetParent(creator.transform);
                gs.transform.localPosition = Vector3.zero;
                
                creator.Spline = gs.GetComponent<GrindSpline>();
            }

            return;
        }

        
        EditorGUI.BeginChangeCheck();

        if (creator.Spline.IsRound)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliderRadius"));
        }
        else
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliderWidth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliderDepth"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IsEdge"));
            
            if (serializedObject.FindProperty("IsEdge").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FlipEdgeSide"));
        }

        if (GUILayout.Button("Generate Colliders"))
        {
            creator.GenerateColliders();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliders"), true);
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }

        GUI.enabled = true;
    }
}