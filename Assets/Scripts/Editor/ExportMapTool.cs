using System;
using System.Collections.Generic;
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

    public static void ExportMap(string override_asset_bundle_name, bool use_version_numbering)
    {
        var scene = SceneManager.GetActiveScene();

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

        BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_BUILD_PATH, new []{ build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        var map_dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SkaterXL/Maps");
        var bundle_path = Path.Combine(Application.dataPath.Replace("/Assets", "/AssetBundles"), build.assetBundleName);
        var dest_path = Path.Combine(map_dir, build.assetBundleName);
      
        Debug.Log($"Copying {bundle_path} to {dest_path}");

        File.Copy(bundle_path, dest_path, overwrite: true);
        File.Delete(bundle_path);

        EditorSceneManager.OpenScene(scene.path);
    }

    private static void ProcessGrindsObjects(Scene scene)
    {
        var root_objects = scene.GetRootGameObjects();
        var grind_splines = Object.FindObjectsOfType<GrindSpline>();
        var grinds_root = root_objects.FirstOrDefault(o => o.name == "Grinds") ?? new GameObject("Grinds");

        /* TODO : leaving this off out for now since it fucks up prefabs and prevents user editing after generation which might be preferable
        var grind_surfaces = Object.FindObjectsOfType<GrindSurface>();
        foreach (var s in grind_surfaces)
        {
            s.GenerateColliders();
        }
        */
        
        foreach (var o in grind_splines)
        {
            var prefab_root = PrefabUtility.GetOutermostPrefabInstanceRoot(o);
            if (prefab_root != null)
            {
                PrefabUtility.UnpackPrefabInstance(prefab_root, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }

            o.transform.SetParent(grinds_root.transform);
        }
    }
}
