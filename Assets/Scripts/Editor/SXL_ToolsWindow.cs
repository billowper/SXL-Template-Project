using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SXL_ToolsWindow : EditorWindow
{
    [SerializeField] private GameObject scaleRef;

    [MenuItem("SXL/Tools Window")]
    private static void Init()
    {
        var window = GetWindow<SXL_ToolsWindow>();
        window.titleContent = new GUIContent("SXL Tools");
        window.Show();
    }

    private void OnEnable()
    {
        containerStyle = new GUIStyle() {padding = new RectOffset(10, 10, 10, 10)};
        UseVersionNumbering = EditorPrefs.GetBool("SXL_UseVersionNumbering", true);

        if (scaleRefInstance == null)
        {
            scaleRefInstance = Instantiate(scaleRef);
            scaleRefInstance.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            scaleRefInstance.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (scaleRefInstance != null)
        {
            DestroyImmediate(scaleRefInstance);
        }
    }

    private GUIStyle containerStyle;
    private bool UseVersionNumbering;
    private string OverrideAssetBundleName;
    private static GameObject scaleRefInstance;

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical(containerStyle);
        {
            EditorGUI.BeginChangeCheck();

            UseVersionNumbering = EditorGUILayout.Toggle(new GUIContent("Use Version Numbering", "If true, the exported AssetBundle will be appended with an incremental version number, e.g. Example Map v4"), UseVersionNumbering);
            OverrideAssetBundleName = EditorGUILayout.TextField(new GUIContent("Override Name", "Optionally override the AssetBundle name (by default we just use the scene name)"), OverrideAssetBundleName);

            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool("SXL_UseVersionNumbering", UseVersionNumbering);
            }

            if (GUILayout.Button("Export Map"))
            {
                ExportMapTool.ExportMap(OverrideAssetBundleName, UseVersionNumbering);
            }

            if (GUILayout.Button("Delete Previous Versions"))
            {
                var scene = SceneManager.GetActiveScene();

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
        }
        EditorGUILayout.EndVertical();
    }

    [MenuItem("SXL/Place Player Scale Reference at Cursor #g")]
    public static void PlaceScaleReference()
    {
        var mouse_pos = Event.current != null ? Event.current.mousePosition : Vector2.zero;
        mouse_pos.y = SceneView.lastActiveSceneView.camera.pixelHeight - mouse_pos.y;
        var ray = SceneView.lastActiveSceneView.camera.ScreenPointToRay(mouse_pos);

        if (Physics.Raycast(ray, out var hit))
        {
            scaleRefInstance.transform.position = hit.point;
            scaleRefInstance.SetActive(true);
        }
    }
}
