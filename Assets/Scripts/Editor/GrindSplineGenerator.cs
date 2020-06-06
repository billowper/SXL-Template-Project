using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Object = UnityEngine.Object;

public static class GrindSplineGenerator
{
    private static List<Vector3> vertices = new List<Vector3>();
    private static List<Vector3> endPoints = new List<Vector3>();
    private static List<Vector3> searchList = new List<Vector3>();
    private static List<Vector3> blockedPoints = new List<Vector3>();
    private static List<Vector3> activeSplinePoints = new List<Vector3>();

    public static void Generate(GrindSurface surface)
    {
        // build a list of vertexes from child objects that are valid potential grindable surfaces
        // do a set offset sphere checks to see if we have open space in any cardinal directions

        vertices.Clear();
        endPoints.Clear();
        blockedPoints.Clear();
        activeSplinePoints.Clear();

        foreach (var m in surface.GetComponentsInChildren<MeshFilter>())
        {
            foreach (var v in m.sharedMesh.vertices.Distinct())
            {
                var w = m.transform.TransformPoint(v);

                if (IsValidPotentialVertex(w))
                {
                    vertices.Add(w);
                }
            }
        }

        bool GetValidStartPoint(out Vector3 pt)
        {
            RefreshSearchList();

            var filter = new List<Vector3>();

            if (searchList.Count > 1)
            {
                foreach (var p in searchList)
                {
                    foreach (var other in searchList)
                    {
                        if (other == p)
                            continue;
                        
                        if (surface.Splines.Any(s =>
                        {
                            var pts = s.GetComponentsInChildren<Transform>().Select(t => t.position).ToArray();
                            return pts.Contains(p) && pts.Contains(other);
                        }))
                        {
                            continue;
                        }

                        filter.Add(p);
                    }
                }
            }

            if (filter.Count > 0)
            {
                pt = filter[0];
                return true;
            }

            pt = default;
            return false;
        }

        Debug.Log($"Found {vertices.Count} potential valid vertices in child meshes.");

        // start a grind spline by picking a valid vertex
        // find the nearest valid vert and add that, repeat until there are no valid verts left

        var start = vertices[0];
        var active_spline = CreateSpline(surface, start);
        var current_point = start;
        var current_index = 0;

        vertices.RemoveAt(0);

        endPoints.Add(start);
        activeSplinePoints.Add(start);
        
        AddSplinePoint(active_spline, start);
        
        while (active_spline != null)
        {
            RefreshSearchList();

            // if ran out of verts to use, we're done

            if (searchList.Count == 0 || (searchList.Count == 1 && searchList[0] == current_point))
            {
                break;
            }

            // find nearest vert for our next spline point

            var nearst_vert = GrindSplineUtils.GetNearestVertex(searchList, current_point);
            var previous_point = current_index > 0 ? active_spline.transform.GetChild(current_index - 1) : null;

            if (previous_point == null || CheckAngle(previous_point.position, current_point, nearst_vert) && CheckMidPoint(current_point, nearst_vert))
            {
                if (vertices.Contains(nearst_vert))
                    vertices.Remove(nearst_vert);

                AddSplinePoint(active_spline, nearst_vert);

                activeSplinePoints.Add(nearst_vert);

                current_point = nearst_vert;
                current_index++;

                Debug.Log($"Add spline point at {nearst_vert}");
            }

            // if we failed to find a valid next point, but still have verts left, lets create a new spline

            else
            {
                var last = active_spline.transform.GetChild(active_spline.transform.childCount - 1).position;

                if (endPoints.Contains(last) == false)
                    endPoints.Add(last);
                else
                    blockedPoints.Add(last);

                activeSplinePoints.Clear();

                if (GetValidStartPoint(out var pt))
                {
                    current_point = pt;

                    if (vertices.Contains(current_point))
                        vertices.Remove(current_point);

                    if (endPoints.Contains(current_point) == false)
                        endPoints.Add(current_point);
                    else
                        blockedPoints.Add(current_point);

                    current_index = 0;

                    active_spline = CreateSpline(surface, current_point);

                    activeSplinePoints.Clear();
                    activeSplinePoints.Add(current_point);

                    AddSplinePoint(active_spline, current_point);

                    Debug.Log($"Creating new spline at {current_point}, searchList size is = {searchList.Count}");
                }
                else
                {
                    if (active_spline.transform.childCount < 2)
                    {
                        surface.Splines.RemoveAt(surface.Splines.Count - 1);

                        Object.DestroyImmediate(active_spline.gameObject);
                    }

                    active_spline = null;
                }
            }
        }
    }

    private static void RefreshSearchList()
    {
        searchList.Clear();
        searchList.AddRange(vertices);
        searchList.AddRange(endPoints);

        foreach (var b in blockedPoints)
            searchList.Remove(b);
        
        foreach (var p in activeSplinePoints)
            searchList.Remove(p);
    }

    private static bool CheckMidPoint(Vector3 current, Vector3 next)
    {
        var m = Vector3.Lerp(current, next, 0.5f);

        return IsValidPotentialVertex(m);
    }
    
    private static bool CheckAngle(Vector3 previous, Vector3 current, Vector3 next)
    {
        // "valid" means within a reasonable angle threshold in any direction, 
        // relative to the direction from our previous point

        const float max_angle = 15f;
        
        var dir = next - current;
        var prev_dir = current - previous;
        var angle = Vector3.Angle(dir, prev_dir);

        return angle < max_angle;
    }

    private static void AddSplinePoint(GrindSpline spline, Vector3 world_position)
    {
        var p = spline.transform;
        var n = p.childCount;
        var go = new GameObject($"Point ({n + 1})");

        go.transform.position = world_position;
        go.transform.SetParent(spline.transform);
    }

    private static GrindSpline CreateSpline(GrindSurface surface, Vector3 world_position)
    {
        var gs = new GameObject("GrindSpline", typeof(GrindSpline));

        gs.transform.position = world_position;
        gs.transform.SetParent(surface.transform);

        var spline = gs.GetComponent<GrindSpline>();

        surface.Splines.Add(spline);
        
        return spline;
    }

    private static bool IsValidPotentialVertex(Vector3 v)
    {
        var offset = 0.1f;
        var radius = 0.05f;

        bool test_dir(Vector3 dir)
        {
            var t = v + (dir * offset);

            if (Physics.CheckBox(t, Vector3.one * radius))
            {
                Debug.DrawLine(v, t, Color.red, 1f);
                return false;
            }
            
            Debug.DrawLine(v, t, Color.green, 1f);
            return true;
        }

        if (test_dir(Vector3.up))
        {
            var passed = test_dir(Vector3.forward);

            if (test_dir(Vector3.back) && passed == false)
                passed = true;

            if (test_dir(Vector3.left) && passed == false)
                passed = true;

            if (test_dir(Vector3.right) && passed == false)
                passed = true;

            return passed;
        }

        return false;
    }
}