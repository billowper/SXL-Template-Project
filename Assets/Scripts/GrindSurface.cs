using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates colliders from GrindSpline components
/// </summary>
public class GrindSurface : MonoBehaviour
{
    public List<GrindSpline> Splines = new List<GrindSpline>();
    public Transform ColliderContainer;
    public List<Collider> GeneratedColliders = new List<Collider>();

    public enum ColliderTypes
    {
        Box,
        Capsule
    }
    public ColliderTypes ColliderType;
    public float GeneratedColliderRadius = 0.1f;
    public float GeneratedColliderWidth = 0.1f;
    public float GeneratedColliderDepth = 0.05f;
    public bool IsEdge;
    public bool AutoDetectEdgeAlignment;
    public bool FlipEdge;

    private bool flipEdgeOffset;

    private void OnValidate()
    {
        if (Splines.Count == 0)
        {
            Splines.AddRange(GetComponentsInChildren<GrindSpline>());
        }
    }

    public void GenerateColliders()
    {
        if (GetComponent<GrindSpline>() != null)
        {
            Debug.LogError("GrindSurface cannot generate colliders as there is a GrindSpline component on the same GameObject");
            return;
        }

        foreach (var c in GeneratedColliders.ToArray())
        {
            if (c != null) 
                DestroyImmediate(c.gameObject);
        }
        
        GeneratedColliders.Clear();

        var test_cols = GetComponentsInChildren<Collider>();

        foreach (var spline in Splines)
        {
            if (spline == null || spline.transform.childCount < 2)
                return;

            flipEdgeOffset = ShouldFlipEdgeOffset(spline, test_cols);

            for (int i = 0; i < spline.transform.childCount - 1; i++)
            {
                var a = spline.transform.GetChild(i).position;
                var b = spline.transform.GetChild(i + 1).position;
                var col = CreateColliderBetweenPoints(spline, a, b);

                GeneratedColliders.Add(col);
            }
        }
    }

    private bool ShouldFlipEdgeOffset(GrindSpline spline, Collider[] test_cols)
    {
        if (IsEdge)
        {
            if (AutoDetectEdgeAlignment)
            {
                var left = false;

                var a = spline.transform.GetChild(0).position;
                var b = spline.transform.GetChild(1).position;

                var dir = a - b;
                var right = Vector3.Cross(dir.normalized, Vector3.up);
                var test_pos = a + (right * GeneratedColliderWidth);

                foreach (var t in test_cols)
                {
                    // if this ray doesnt hit anything then the ledge is to our left

                    if (t.Raycast(new Ray(test_pos + Vector3.up, Vector3.down), out var hit, 1f) == false)
                    {
                        left = true;
                    }
                }

                return left;
            }
            
            return FlipEdge;
        }

        return false;
    }

    private Collider CreateColliderBetweenPoints(GrindSpline spline, Vector3 pointA, Vector3 pointB)
    {
        var go = new GameObject("Grind Cols")
        {
            layer = LayerMask.NameToLayer(spline.IsCoping ? "Coping" : "Grindable")
        };

        go.transform.position = pointA;
        go.transform.LookAt(pointB);
        go.transform.SetParent(ColliderContainer != null ? ColliderContainer : transform);
        go.tag = $"Grind_{spline.SurfaceType}";

        var length = Vector3.Distance(pointA, pointB);

        if (spline.IsRound)
        {
            var cap = go.AddComponent<CapsuleCollider>();

            cap.direction = 2;
            cap.radius = GeneratedColliderRadius;
            cap.height = length + 2f * GeneratedColliderRadius;
            cap.center = Vector3.forward * length / 2f + Vector3.down * GeneratedColliderRadius;
        }
        else
        {
            var box = go.AddComponent<BoxCollider>();

            box.size = new Vector3(GeneratedColliderWidth, GeneratedColliderDepth, length);
            var offset = IsEdge ? new Vector3(flipEdgeOffset ? (GeneratedColliderWidth / 2f) * -1 : GeneratedColliderWidth / 2f, 0, 0) : Vector3.zero;
            box.center = offset + Vector3.forward * length / 2f + Vector3.down * GeneratedColliderDepth / 2f;
        }
        
        return go.GetComponent<Collider>();
    }

    public void DestroySplines()
    {
        foreach (var c in GeneratedColliders)
        {
            if (c != null) 
                DestroyImmediate(c.gameObject);
        }

        foreach (var s in Splines)
        {
            if (s != null) 
                DestroyImmediate(s.gameObject);
        }

        GeneratedColliders.Clear();
        Splines.Clear();
    }
}