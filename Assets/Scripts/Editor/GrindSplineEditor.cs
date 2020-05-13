using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrindSpline))]
public class GrindSplineEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Add Point"))
        {
            var p = ((GrindSpline) target).transform;
            var n = p.childCount;
            var go = new GameObject($"Point ({n + 1})");

            go.transform.SetParent(p);
            go.transform.localPosition = Vector3.zero;
        }

        serializedObject.UpdateIfRequiredOrScript();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("GrindType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("IsRound"));

        if (serializedObject.FindProperty("IsRound").boolValue)
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
            ((GrindSpline) target).GenerateColliders();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneratedColliders"), true);
        
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}