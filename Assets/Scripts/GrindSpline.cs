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
            Gizmos.DrawWireCube(child.transform.position, Vector3.one * 0.05f);
            
            if (i + 1 < transform.childCount)
            {
                if (Selection.activeGameObject == child.gameObject && i > 0)
                {
                    var offset = Vector3.up * 0.05f;

                    Gizmos.color = Color.cyan;

                    var dir = (transform.GetChild(i + 1).position + offset) - (child.position + offset);
                    Gizmos.DrawRay(child.position + offset, dir);

                    var prev_dir = (child.position + offset) - (transform.GetChild(i - 1).position + offset);
                    Gizmos.DrawRay(child.position + offset, prev_dir);

                    var angle = Vector3.Angle(dir, prev_dir);

                    Handles.Label(Vector3.Lerp(child.position, transform.GetChild(i + 1).position, 0.5f), $"angle = {angle}");
                }
                else
                {

                    Gizmos.color = gizmoColor;
                    Gizmos.DrawLine(child.position, transform.GetChild(i + 1).position);

                }
            }
        }
    }
}
