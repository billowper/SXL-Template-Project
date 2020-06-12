﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GrindSpline : MonoBehaviour
{
    public enum SurfaceTypes
    {
        Concrete,
        Metal,
    }

    public SurfaceTypes SurfaceType;
    public bool IsRound;
    public bool IsCoping;
    public ColliderGenerationSettings ColliderGenerationSettings = new ColliderGenerationSettings();
    public Transform PointsContainer;
    public Transform ColliderContainer;
    public List<Collider> GeneratedColliders = new List<Collider>();

    private bool flipEdgeOffset;

#if UNITY_EDITOR

    private Color gizmoColor = Color.green;

    private void OnValidate()
    {
        var proper_name = $"GrindSpline_Grind_{SurfaceType}{(IsRound ? "_Round" : "")}";

        if (gameObject.name.Contains(proper_name) == false || IsRound == false && gameObject.name.Contains("_Round"))
        {
            gameObject.name = proper_name;
        }

        if (IsCoping && GeneratedColliders.Any(c => c.gameObject.layer != LayerMask.NameToLayer("Coping")))
        {
            foreach (var c in GeneratedColliders)
            {
                c.gameObject.layer = LayerMask.NameToLayer("Coping");
            }
        }

        if (PointsContainer == null)
        {
            PointsContainer = transform;
        }
    }

    private void OnDrawGizmos()
    {
        var selected = Selection.gameObjects.Contains(gameObject);

        gizmoColor.a = selected ? 1f : 0.5f;

        Gizmos.color = gizmoColor;

        if (PointsContainer == null) 
            return;

        for (int i = 0; i < PointsContainer.childCount; i++)
        {
            var child = PointsContainer.GetChild(i);

            Gizmos.DrawWireCube(child.position, Vector3.one * 0.05f);

            if (i + 1 < PointsContainer.childCount)
            {
                if (selected)
                {
                    Handles.color = gizmoColor;
                    Handles.DrawAAPolyLine(5f, child.position, PointsContainer.GetChild(i + 1).position);
                }
                else
                {
                    Gizmos.DrawLine(child.position, PointsContainer.GetChild(i + 1).position);
                }
            }
        }
    }
    
    public void GenerateColliders(ColliderGenerationSettings settings = null)
    {
        if (settings == null)
            settings = ColliderGenerationSettings;

        foreach (var c in GeneratedColliders.ToArray())
        {
            if (c != null) 
                DestroyImmediate(c.gameObject);
        }
        
        GeneratedColliders.Clear();

        List<Collider> test_cols = null;

        /*
        if (transform.parent != null)
        {
            var sibling_splines = transform.parent.GetComponentsInChildren<GrindSpline>();

            test_cols = transform.parent.GetComponentsInChildren<Collider>()
                .Where(c => sibling_splines.All(s => s.GeneratedColliders.Contains(c) == false))
                .ToList();
        }
        */

        if (PointsContainer.childCount < 2)
            return;

        flipEdgeOffset = ShouldFlipEdgeOffset(settings);

        for (int i = 0; i < PointsContainer.childCount - 1; i++)
        {
            var a = PointsContainer.GetChild(i).position;
            var b = PointsContainer.GetChild(i + 1).position;
            var col = CreateColliderBetweenPoints(settings, a, b);

            GeneratedColliders.Add(col);
        }
    }

    private bool ShouldFlipEdgeOffset(ColliderGenerationSettings settings)
    {
        if (settings.IsEdge)
        {
            if (settings.AutoDetectEdgeAlignment)
            {
                var left = false;

                var a = PointsContainer.GetChild(0).position;
                var b = PointsContainer.GetChild(1).position;

                var dir = a - b;
                var right = Vector3.Cross(dir.normalized, Vector3.up);
                var test_pos = a + (right * settings.Width);

                Debug.DrawLine(test_pos, a + right, Color.green, 1f);
                Debug.DrawLine(test_pos, test_pos + Vector3.down, Color.cyan, 1f);
                
                if (Physics.SphereCast(new Ray(test_pos + Vector3.up, Vector3.down), 0.01f, out var hit, 1) == false || 
                    hit.transform.gameObject.layer == LayerMask.NameToLayer("Grindable") || 
                    settings.SkipExternalCollisionChecks && hit.transform.parent == transform.parent)
                {
                    left = true;
                }

                return left;
            }
            
            return settings.FlipEdge;
        }

        return false;
    }

    private Collider CreateColliderBetweenPoints(ColliderGenerationSettings settings, Vector3 pointA, Vector3 pointB)
    {
        var go = new GameObject("Grind Cols")
        {
            layer = LayerMask.NameToLayer(IsCoping ? "Coping" : "Grindable")
        };

        go.transform.position = pointA;
        go.transform.SetParent(ColliderContainer != null ? ColliderContainer : transform);
        go.tag = $"Grind_{SurfaceType}";

        var length = Vector3.Distance(pointA, pointB);

        if (IsRound)
        {
            go.transform.LookAt(pointB);

            var cap = go.AddComponent<CapsuleCollider>();

            cap.direction = 2;
            cap.radius = settings.Radius;
            cap.height = length + 2f * settings.Radius;
            cap.transform.localPosition -= Vector3.forward * length / 2f + Vector3.down * settings.Radius;
        }
        else
        {
            var box = go.AddComponent<BoxCollider>();

            box.size = new Vector3(settings.Width, settings.Depth, length);

            var inset = flipEdgeOffset ? (settings.Width / 2f) * -1 : settings.Width / 2f;
            var offset = settings.IsEdge ? new Vector3(inset, 0, length / 2f) : Vector3.zero;

            var angle = Vector3.Angle(Vector3.forward, pointA - pointB);
            box.transform.localPosition = Quaternion.LookRotation(pointB - pointA, Vector3.up * angle) * offset;
            box.transform.localRotation = Quaternion.Euler(0, angle, 0);

            var p = box.transform.localPosition;
            p.y = -(settings.Depth / 2f);

            box.transform.localPosition = p;
        }
        
        return go.GetComponent<Collider>();
    }

#endif

}
