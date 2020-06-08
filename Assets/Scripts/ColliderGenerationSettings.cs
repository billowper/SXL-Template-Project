using System;

[Serializable]
public class ColliderGenerationSettings
{
    public enum ColliderTypes
    {
        Box,
        Capsule
    }

    public ColliderTypes ColliderType;
    public float Radius = 0.1f;
    public float Width = 0.1f;
    public float Depth = 0.05f;
    public bool IsEdge;
    public bool AutoDetectEdgeAlignment;
    public bool FlipEdge;
}