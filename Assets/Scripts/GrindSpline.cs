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

            Gizmos.DrawWireSphere(child.transform.position, 0.05f);

            if (i + 1 < transform.childCount)
            {
                Gizmos.DrawLine(child.transform.position, transform.GetChild(i + 1).position);
            }
        }
    }
}
