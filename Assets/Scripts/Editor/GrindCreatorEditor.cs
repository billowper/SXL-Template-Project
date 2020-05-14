using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrindSurface))]
public class GrindCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var creator = (GrindSurface) target;

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Spline"));

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

        serializedObject.UpdateIfRequiredOrScript();
        
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
    }
}