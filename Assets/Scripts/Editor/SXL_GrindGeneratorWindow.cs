using UnityEditor;
using UnityEngine;

public class SXL_GrindGeneratorWindow : EditorWindow
{
    [MenuItem("SXL/Grind Surface Generator (Experimental)")]
    private static void Init()
    {
        var window = GetWindow<SXL_GrindGeneratorWindow>();
        window.titleContent = new GUIContent("Grind Surface Generator");
        window.Show();
    }

    private GUIStyle containerStyle;

    private bool gsDefault_IsEdge;
    private bool gsDefault_AutoDetectEdgeAlignment;
    private GrindSurface.ColliderTypes gsDefault_ColliderType = GrindSurface.ColliderTypes.Box;

    private float settings_PointTestOffset;
    private float settings_PointTestRadius;
    private float settings_MaxHorizontalAngle;
    private float settings_MaxSlope;

    private void OnEnable()
    {
        containerStyle = new GUIStyle() {padding = new RectOffset(10, 10, 10, 10)};

        settings_PointTestOffset = EditorPrefs.GetFloat(nameof(settings_PointTestOffset), GrindSplineGenerator.PointTestOffset);
        settings_PointTestRadius = EditorPrefs.GetFloat(nameof(settings_PointTestRadius), GrindSplineGenerator.PointTestRadius);
        settings_MaxHorizontalAngle = EditorPrefs.GetFloat(nameof(settings_MaxHorizontalAngle), GrindSplineGenerator.MaxHorizontalAngle);
        settings_MaxSlope = EditorPrefs.GetFloat(nameof(settings_MaxSlope), GrindSplineGenerator.MaxSlope);

        Selection.selectionChanged += SelectionChanged;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= SelectionChanged;
    }

    private void SelectionChanged()
    {
        Repaint();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(containerStyle);
        {
            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.LabelField("Default Surface Settings", EditorStyles.boldLabel);
                gsDefault_IsEdge = EditorGUILayout.Toggle("Is Edge", gsDefault_IsEdge);
                gsDefault_AutoDetectEdgeAlignment = EditorGUILayout.Toggle("Auto Edge Alignment", gsDefault_AutoDetectEdgeAlignment);
                gsDefault_ColliderType = (GrindSurface.ColliderTypes) EditorGUILayout.EnumPopup("Collider Type", gsDefault_ColliderType);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool(nameof(gsDefault_IsEdge), gsDefault_IsEdge);
                    EditorPrefs.SetBool(nameof(gsDefault_AutoDetectEdgeAlignment), gsDefault_AutoDetectEdgeAlignment);
                    EditorPrefs.SetInt(nameof(gsDefault_ColliderType), (int) gsDefault_ColliderType);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                settings_PointTestOffset = EditorGUILayout.FloatField("PointTestOffset", settings_PointTestOffset);
                settings_PointTestRadius = EditorGUILayout.FloatField("PointTestRadius", settings_PointTestRadius);
                settings_MaxHorizontalAngle = EditorGUILayout.FloatField("MaxHorizontalAngle", settings_MaxHorizontalAngle);
                settings_MaxSlope = EditorGUILayout.FloatField("MaxSlope", settings_MaxSlope);
                
                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetFloat(nameof(settings_PointTestOffset), settings_PointTestOffset);
                    EditorPrefs.SetFloat(nameof(settings_PointTestRadius), settings_PointTestRadius);
                    EditorPrefs.SetFloat(nameof(settings_MaxHorizontalAngle), settings_MaxHorizontalAngle);
                    EditorPrefs.SetFloat(nameof(settings_MaxSlope), settings_MaxSlope);

                    GrindSplineGenerator.PointTestOffset = settings_PointTestOffset;
                    GrindSplineGenerator.PointTestRadius = settings_PointTestRadius;
                    GrindSplineGenerator.MaxHorizontalAngle = settings_MaxHorizontalAngle;
                    GrindSplineGenerator.MaxSlope = settings_MaxSlope;
                }

            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                EditorGUILayout.LabelField("Selected Object", Selection.activeGameObject?.name);

                if (Selection.activeGameObject != null)
                {
                    var box_col = Selection.activeGameObject.GetComponent<BoxCollider>();
                    if (box_col != null && box_col.transform.parent.GetComponent<GrindSurface>())
                    {
                        if (GUILayout.Button("Flip Edge Collider Offset"))
                        {
                            var c = box_col.center;

                            c.x = c.x * -1f;

                            box_col.center = c;
                        }
                    }
                    else
                    {

                        var surface = Selection.activeGameObject.GetComponent<GrindSurface>() ?? Selection.activeGameObject.transform.GetComponentInParent<GrindSurface>();
                        if (surface == null)
                        {
                            EditorGUILayout.LabelField($"<i>No GrindSurface found</i>", new GUIStyle("label") {richText = true});

                            if (GUILayout.Button("Generate Grind Surface"))
                            {
                                surface = Selection.activeGameObject.AddComponent<GrindSurface>();

                                surface.IsEdge = gsDefault_IsEdge;
                                surface.AutoDetectEdgeAlignment = gsDefault_AutoDetectEdgeAlignment;
                                surface.ColliderType = gsDefault_ColliderType;

                                GrindSplineGenerator.Generate(surface);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField($"GrindSurface found!", new GUIStyle("label") {richText = true});
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }
}
