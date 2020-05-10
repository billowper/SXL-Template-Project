using System;
using UnityEditor;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click export of the active scene to a unique asset bundle, with automatic version numbering
/// Asset bundle is exporter to the project root folder and copied into the Skater XL maps folder in MyDocuments
/// </summary>
public static class ExportMapTool
{
    private const string ASSET_BUNDLES_BUILD_PATH = "AssetBundles";

    [MenuItem("SXL/Export Map")]
    public static void ExportMap()
    {
        var scene = SceneManager.GetActiveScene();

        var version = EditorPrefs.GetInt($"{scene.name}_version", 1);

        version++;

        EditorPrefs.SetInt($"{scene.name}_version", version);

        var build = new AssetBundleBuild
        {
            assetBundleName = $"{scene.name} v{version}",
            assetNames = new[] {scene.path}
        };

        if (!Directory.Exists(ASSET_BUNDLES_BUILD_PATH))
            Directory.CreateDirectory(ASSET_BUNDLES_BUILD_PATH);

        BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_BUILD_PATH, new []{ build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        var docs_dir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var map_dir = Path.Combine(docs_dir, "SkaterXL/Maps");
        var bundle_path = Path.Combine(Application.dataPath.Replace("/Assets", "/AssetBundles"), build.assetBundleName);
        var dest_path = Path.Combine(map_dir, build.assetBundleName);
      
        Debug.Log($"Copying {bundle_path} to {dest_path}");

        File.Copy(bundle_path, dest_path, overwrite: true);
    }
}
