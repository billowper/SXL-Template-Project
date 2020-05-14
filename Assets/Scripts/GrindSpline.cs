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

    public void AddPoint()
    {
        var p = transform;
        var n = p.childCount;
        var go = new GameObject($"Point ({n + 1})");

        go.transform.SetParent(p);
        go.transform.localPosition = Vector3.zero;
    }

}
