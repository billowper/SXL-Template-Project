using UnityEditor;
using UnityEngine;

public static class GrindSplineUtils
{
    [MenuItem("GameObject/Create Other/Grind Spline")]   
    private static void CreateGrindSpline()
    {
        var go = new GameObject("GrindSpline", typeof(GrindSpline));

        var ray = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
        if (Physics.Raycast(ray, out var hit))
        {
            go.transform.position = hit.point + Vector3.up * 2.5f;
        }

        Selection.activeGameObject = go;
    }
}