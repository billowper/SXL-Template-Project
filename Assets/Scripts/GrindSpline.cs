using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

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

    [FormerlySerializedAs("GrindType")] public SurfaceTypes SurfaceType;
    public bool IsRound;
    public bool IsCoping;

#if UNITY_EDITOR

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
        var selected = Selection.gameObjects.Contains(gameObject);

        gizmoColor.a = selected ? 1f : 0.5f;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            Gizmos.DrawWireCube(child.position, Vector3.one * 0.05f);
            
            if (i + 1 < transform.childCount)
            {
                if (selected)
                {
                    Handles.color = gizmoColor;
                    Handles.DrawAAPolyLine(5f, child.position, transform.GetChild(i + 1).position);
                }
                else
                {
                    Gizmos.DrawLine(child.position, transform.GetChild(i + 1).position);
                }
            }
        }
    }

#endif

}
