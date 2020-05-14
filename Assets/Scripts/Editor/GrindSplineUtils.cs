using UnityEditor;
using UnityEngine;

public static class GrindSplineUtils
{
    [MenuItem("GameObject/Create Other/Grind Spline")]   
    private static void CreateGrindSpline()
    {
        CreateObjectWithComponent<GrindSpline>("GrindSpline");
    }

    [MenuItem("GameObject/Create Other/Grind Surface")]   
    private static void CreateGrindSurface()
    {
        CreateObjectWithComponent<GrindSurface>("GrindSurface");
    }

    private static void CreateObjectWithComponent<T>(string name)
    {
        var go = new GameObject(name, typeof(T));

        var ray = SceneView.lastActiveSceneView.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1.0f));
        if (Physics.Raycast(ray, out var hit))
        {
            go.transform.position = hit.point + Vector3.up * 2.5f;
        }

        Selection.activeGameObject = go;
    }
}