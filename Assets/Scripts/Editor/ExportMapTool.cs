using System;
using UnityEditor;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

/// <summary>
/// One-click export of the active scene to a unique asset bundle, with automatic version numbering
/// Asset bundle is exporter to the project root folder and copied into the Skater XL maps folder in MyDocuments
/// </summary>
public static class ExportMapTool
{
    private const string ASSET_BUNDLES_BUILD_PATH = "AssetBundles";

    [MenuItem("SXL/Quick Map Export")]
    public static void ExportMap()
    {
        ExportMap(null, EditorPrefs.GetBool("SXL_UseVersionNumbering", true));
    }

    public static Action<Scene> OnPreExport;

    public static void ExportMap(string override_asset_bundle_name, bool use_version_numbering)
    {
        var scene = SceneManager.GetActiveScene();

        var start_time = DateTime.Now;

        EditorSceneManager.SaveScene(scene);

        ProcessGrindsObjects(scene);

        var bundle_name = scene.name;

        if (use_version_numbering)
        {
            var version = EditorPrefs.GetInt($"{scene.name}_version", 1);

            version++;

            EditorPrefs.SetInt($"{scene.name}_version", version);

            bundle_name = $"{scene.name} v{version}";
        }

        if (string.IsNullOrEmpty(override_asset_bundle_name) == false)
        {
            bundle_name = override_asset_bundle_name;
        }

        var build = new AssetBundleBuild
        {
            assetBundleName = bundle_name,
            assetNames = new[] {scene.path}
        };

        if (!Directory.Exists(ASSET_BUNDLES_BUILD_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_BUILD_PATH);

        BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_BUILD_PATH, new []{ build }, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);

        var time_taken = start_time - DateTime.Now;

        Debug.Log($"BuildAssetBundles took {time_taken:mm\\:ss}");

        var map_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkaterXL/Maps");
        var bundle_path = Path.Combine(Application.dataPath.Replace("/Assets", "/AssetBundles"), build.assetBundleName);
        var dest_path = Path.Combine(map_dir, build.assetBundleName);
      
        Debug.Log($"Copying {bundle_path} to {dest_path}");

        File.Copy(bundle_path, dest_path, overwrite: true);
        File.Delete(bundle_path);

        EditorSceneManager.OpenScene(scene.path);
    }

    public static void ProcessGrindsObjects(Scene scene)
    {
        var grind_splines = Object.FindObjectsOfType<GrindSpline>();
        var grind_surfaces = Object.FindObjectsOfType<GrindSurface>();

        var grinds_root = scene.GetRootGameObjects().FirstOrDefault(o => o.name == "Grinds") ?? new GameObject("Grinds");

        foreach (var o in grind_splines)
        {
            var prefab_root = PrefabUtility.GetOutermostPrefabInstanceRoot(o);
            if (prefab_root != null)
            {
                PrefabUtility.UnpackPrefabInstance(prefab_root, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }

            o.transform.SetParent(grinds_root.transform);

            // remove points container transform, re-parent points to the spline root

            if (o.PointsContainer != o.transform)
            {
                var points = o.PointsContainer.GetComponentsInChildren<Transform>().Where(t => t != o.PointsContainer);
                foreach (var p in points)
                {
                    p.SetParent(o.transform);
                }

                Object.DestroyImmediate(o.PointsContainer.gameObject);
            }

            // move colliders out to scene root

            if (o.ColliderContainer == null)
            {
                foreach (var c in o.GeneratedColliders)
                {
                    c.transform.SetParent(null);
                }
            }
            else
            {
                o.ColliderContainer.SetParent(null);
            }
        }

        // strip components

        foreach (var gs in grind_surfaces)
        {
            Object.DestroyImmediate(gs);
        }

        foreach (var gs in grind_splines)
        {
            Object.DestroyImmediate(gs);
        }

        var missing_scripts = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Component>().Where(c => c == null)).ToArray();

        if (missing_scripts.Length > 0)
        {
            Debug.Log($"Found {missing_scripts.Length} missing scripts which will be removed.");

            foreach (var s in missing_scripts)
            {
                Object.DestroyImmediate(s);
            }
        }
    }
}
