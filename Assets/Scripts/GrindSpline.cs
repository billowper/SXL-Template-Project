using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrindSpline : MonoBehaviour
{
    public enum Types
    {
        Concrete,
        Metal,
    }

    public Types GrindType;
    public bool IsRound;
    public List<Collider> GeneratedColliders = new List<Collider>();
    public float GeneratedColliderRadius = 0.1f;
    public float GeneratedColliderWidth = 0.1f;
    public float GeneratedColliderDepth = 0.05f;
    public bool IsEdge;
    public bool FlipEdgeSide;

    private Color gizmoColor = Color.green;

    private void OnValidate()
    {
        var proper_name = $"GrindSpline_{GrindType}{(IsRound ? "_Round" : "")}";

        if (gameObject.name.Contains(proper_name) == false || IsRound == false && gameObject.name.Contains("_Round"))
        {
            gameObject.name = proper_name;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            Gizmos.DrawWireSphere(child.transform.position, 0.05f);

            if (i + 1 < transform.childCount)
            {
                Gizmos.DrawLine(child.transform.position, transform.GetChild(i + 1).position);
            }
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

        for (int i = 0; i < transform.childCount - 1; i++)
        {
            var col = CreateColliderBetweenPoints(transform.GetChild(i).position, transform.GetChild(i + 1).position);

            GeneratedColliders.Add(col);
        }
    }

    private Collider CreateColliderBetweenPoints(Vector3 pointA, Vector3 pointB)
    {
        var go = new GameObject("Grind Cols")
        {
            layer = LayerMask.NameToLayer("Grindable")
        };

        go.transform.position = pointA;
        go.transform.LookAt(pointB);
        go.transform.SetParent(transform.parent);

        switch (GrindType)
        {
            case Types.Concrete:
                go.tag = "Grind_Concrete";
                break;
            case Types.Metal:
                go.tag = "Grind_Metal";
                break;
        }

        var length = Vector3.Distance(pointA, pointB);

        if (IsRound)
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
