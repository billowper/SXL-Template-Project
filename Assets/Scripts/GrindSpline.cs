using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class GrindSpline : MonoBehaviour
{
    public enum SurfaceTypes
    {
        Concrete,
        Metal,
    }

    [FormerlySerializedAs("GrindType")] public SurfaceTypes SurfaceType;
    public bool IsRound;
    public bool IsCoping;

    private Color gizmoColor = Color.green;

    private void OnValidate()
    {
        var proper_name = $"GrindSpline_Grind_{SurfaceType}{(IsRound ? "_Round" : "")}";

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

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(child.position, Vector3.one * 0.05f);
            
            if (i + 1 < transform.childCount)
            {
                if (Selection.activeGameObject == child.gameObject && i > 0)
                {
                    var current = child.position;
                    var next = transform.GetChild(i + 1).position;
                    var prev = transform.GetChild(i - 1).position;

                    var dir = next - current;

                    next.y = 0;
                    current.y = 0;
                    prev.y = 0;

                    var flat_dir = next - current;
                    var v_angle = Vector3.Angle(dir, flat_dir);
                    var prev_dir = current - prev;
                    var h_angle = Vector3.Angle(flat_dir, prev_dir);

                    Handles.Label(Vector3.Lerp(child.position, transform.GetChild(i + 1).position, 0.5f), $"h_angle = {h_angle} v_angle = {v_angle}");
                }

                Gizmos.color = gizmoColor;
                Gizmos.DrawLine(child.position, transform.GetChild(i + 1).position);
            }
        }
    }
}
