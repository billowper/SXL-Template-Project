using System.Collections.Generic;
using UnityEngine;

public class GrindSurface : MonoBehaviour
{
    public GrindSpline Spline;
    public List<Collider> GeneratedColliders = new List<Collider>();
    public float GeneratedColliderRadius = 0.1f;
    public float GeneratedColliderWidth = 0.1f;
    public float GeneratedColliderDepth = 0.05f;
    public bool IsEdge;
    public bool FlipEdgeSide;

    [SerializeField, HideInInspector] private int previousSplineLength;
    
    private void OnValidate()
    {
        if (Spline == null)
        {
            Spline = GetComponentInChildren<GrindSpline>();
        }
    }

    public void GenerateColliders()
    {
        foreach (var c in GeneratedColliders.ToArray())
        {
            if (c != null) 
                DestroyImmediate(c.gameObject);
        }
        
        GeneratedColliders.Clear();

        for (int i = 0; i < Spline.transform.childCount - 1; i++)
        {
            var col = CreateColliderBetweenPoints(Spline.transform.GetChild(i).position, Spline.transform.GetChild(i + 1).position);

            GeneratedColliders.Add(col);
        }

        previousSplineLength = Spline.transform.childCount;
    }

    private Collider CreateColliderBetweenPoints(Vector3 pointA, Vector3 pointB)
    {
        var go = new GameObject("Grind Cols")
        {
            layer = LayerMask.NameToLayer("Grindable")
        };

        go.transform.position = pointA;
        go.transform.LookAt(pointB);
        go.transform.SetParent(transform);

        switch (Spline.GrindType)
        {
            case GrindSpline.Types.Concrete:
                go.tag = "Grind_Concrete";
                break;
            case GrindSpline.Types.Metal:
                go.tag = "Grind_Metal";
                break;
        }

        var length = Vector3.Distance(pointA, pointB);

        if (Spline.IsRound)
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
            var offset = IsEdge ? new Vector3(FlipEdgeSide ? (GeneratedColliderWidth / 2f) * -1 : GeneratedColliderWidth / 2f, 0, 0) : Vector3.zero;
            box.center = offset + Vector3.forward * length / 2f + Vector3.down * GeneratedColliderDepth / 2f;
        }
        
        return go.GetComponent<Collider>();
    }
}