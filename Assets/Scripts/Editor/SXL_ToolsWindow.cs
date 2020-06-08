using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SXL_ToolsWindow : EditorWindow
{
    [MenuItem("SXL/Tools Window")]
    private static void Init()
    {
        var window = GetWindow<SXL_ToolsWindow>();
        window.titleContent = new GUIContent("SXL Tools");
        window.Show();
    }

    private GUIStyle containerStyle;
    private bool UseVersionNumbering;
    private bool StripComponents;
    private string OverrideAssetBundleName;

    [SerializeField] private bool showGenerationSettings;

    private bool gsDefault_IsEdge;
    private bool gsDefault_AutoDetectEdgeAlignment;
    private ColliderGenerationSettings.ColliderTypes gsDefault_ColliderType = ColliderGenerationSettings.ColliderTypes.Box;

    private float settings_PointTestOffset;
    private float settings_PointTestRadius;
    private float settings_MaxHorizontalAngle;
    private float settings_MaxSlope;
    private bool canFlipBoxCollider;

    private void OnEnable()
    {
        containerStyle = new GUIStyle() {padding = new RectOffset(10, 10, 10, 10)};

        UseVersionNumbering = EditorPrefs.GetBool("SXL_UseVersionNumbering", true);

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
        canFlipBoxCollider = false;

        var box_col = Selection.activeGameObject?.GetComponent<BoxCollider>();
        if (box_col != null && FindObjectsOfType<GrindSpline>().Any(s => s.GeneratedColliders.Contains(box_col)))
            canFlipBoxCollider = true;

        Repaint();
    }

    private void OnGUI()
    {
	    var scene = SceneManager.GetActiveScene();

        EditorGUILayout.BeginVertical(containerStyle);
        {
            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                UseVersionNumbering = EditorGUILayout.Toggle(new GUIContent("Use Version Numbering", "If true, the exported AssetBundle will be appended with an incremental version number, e.g. Example Map v4"), UseVersionNumbering);
                StripComponents = EditorGUILayout.Toggle("StripComponents", StripComponents);
                OverrideAssetBundleName = EditorGUILayout.TextField(new GUIContent("Override Name", "Optionally override the AssetBundle name (by default we just use the scene name)"), OverrideAssetBundleName);

                if (EditorGUI.EndChangeCheck())
                {
                    EditorPrefs.SetBool("SXL_UseVersionNumbering", UseVersionNumbering);
                }

	            if (EditorPrefs.HasKey($"{scene.name}_version"))
	            {
		            EditorGUILayout.LabelField($"Version Number", EditorPrefs.GetInt($"{scene.name}_version", 1).ToString());
	            }

                if (GUILayout.Button("Export Map"))
                {
                    ExportMapTool.ExportMap(OverrideAssetBundleName, UseVersionNumbering, StripComponents);
                }

                if (GUILayout.Button("Delete Previous Versions"))
                {
                    if (EditorUtility.DisplayDialog("Are you sure?", $"This will delete all previously exported maps containing the name '{scene.name}'", "Yes", "Cancel"))
                    {
                        var map_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkaterXL/Maps");
                        var paths = Directory.GetFiles(map_dir).Where(p => Path.GetFileName(p).StartsWith(scene.name)).ToArray();

                        foreach (var p in paths)
                        {
                            Debug.Log($"Deleting '{p}'");
                            File.Delete(p);
                        }
                    } 
                }

                if (GUILayout.Button("Reset Version Number"))
                {
	                if (EditorPrefs.HasKey($"{scene.name}_version"))
	                {
			            EditorPrefs.DeleteKey($"{scene.name}_version");
	                }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                EditorGUILayout.LabelField("Selected Object", Selection.activeGameObject?.name, EditorStyles.boldLabel);

                if (Selection.activeGameObject != null)
                {
                    var box_col = Selection.activeGameObject.GetComponent<BoxCollider>();
                    if (box_col != null && canFlipBoxCollider)
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

                                GrindSplineGenerator.Generate(surface, new ColliderGenerationSettings()
                                {
                                    IsEdge = gsDefault_IsEdge,
                                    AutoDetectEdgeAlignment = gsDefault_AutoDetectEdgeAlignment,
                                    ColliderType = gsDefault_ColliderType
                                });
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

            EditorGUILayout.BeginVertical(new GUIStyle("box"));
            {
                showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings, "Grind Spline Generation (Experimental)", new GUIStyle("foldout") { fontStyle = FontStyle.Bold });

                if (showGenerationSettings)
                {
                    EditorGUI.indentLevel++;

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.LabelField("Default Surface Settings", EditorStyles.boldLabel);
                    gsDefault_IsEdge = EditorGUILayout.Toggle("Is Edge", gsDefault_IsEdge);
                    gsDefault_AutoDetectEdgeAlignment = EditorGUILayout.Toggle("Auto Edge Alignment", gsDefault_AutoDetectEdgeAlignment);
                    gsDefault_ColliderType = (ColliderGenerationSettings.ColliderTypes) EditorGUILayout.EnumPopup("Collider Type", gsDefault_ColliderType);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool(nameof(gsDefault_IsEdge), gsDefault_IsEdge);
                        EditorPrefs.SetBool(nameof(gsDefault_AutoDetectEdgeAlignment), gsDefault_AutoDetectEdgeAlignment);
                        EditorPrefs.SetInt(nameof(gsDefault_ColliderType), (int) gsDefault_ColliderType);
                    }

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.Separator();
                    EditorGUILayout.LabelField("Grindable Vertex Settings", EditorStyles.boldLabel);

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

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }
}
