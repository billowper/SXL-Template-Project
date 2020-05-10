using UnityEngine;

public class GrindPreview : MonoBehaviour
{
    public Color GizmoColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);

            Gizmos.DrawSphere(child.transform.position, 0.05f);

            if (i + 1 < transform.childCount)
            {
                Gizmos.DrawLine(child.transform.position, transform.GetChild(i + 1).position);
            }
        }
    }
}
