using System.IO;
using UnityEditor;
using UnityEngine;

public class AppExporter : EditorWindow
{
    private GameObject parentObject, AppIconParent, AppScreenParent;
    private string AppName = "";
    private string AuthorName = "";

    [MenuItem("GorillaPad/App Exporter")]
    public static void ShowWindow()
    {
        var window = GetWindow<AppExporter>("App Exporter");
        window.minSize = new Vector2(300, 150);
    }

    private void OnGUI()
    {
        GUILayout.Label("Export Your Custom App For GorillaPad!", EditorStyles.boldLabel);

        AppName = EditorGUILayout.TextField("App Name", AppName);
        EditorGUILayout.HelpBox("This is the name that your App will be displayed with.", MessageType.None);

        AuthorName = EditorGUILayout.TextField("Author Name", AuthorName);
        EditorGUILayout.HelpBox("This is where you assign your own name.", MessageType.None);

        parentObject = (GameObject)EditorGUILayout.ObjectField("App Parent", parentObject, typeof(GameObject), true);
        EditorGUILayout.HelpBox("Assign your app parent from the inspector here!", MessageType.None);

        AppIconParent = (GameObject)EditorGUILayout.ObjectField("App Icon ", AppIconParent, typeof(GameObject), true);
        EditorGUILayout.HelpBox("This is your own unique Icon that will be displayed on the home screen.", MessageType.None);

        AppScreenParent = (GameObject)EditorGUILayout.ObjectField("App Screen ", AppScreenParent, typeof(GameObject), true);
        EditorGUILayout.HelpBox("This is your own unique App that can be opened by clicking your app Icon.", MessageType.None);

        GUILayout.Space(10);

        if (GUILayout.Button("Export"))
        {
            if (string.IsNullOrWhiteSpace(AppName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter an App Name!", "OK");
                return;
            }

            if (parentObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a the Custom App Parent!", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Export App", "", AppName + ".app", "app");
            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Export Cancelled", "No location selected.", "OK");
                return;
            }
            BuildAppBundle(parentObject, AppIconParent, AppScreenParent, AppName, path, AuthorName);
            EditorUtility.DisplayDialog("Export Success", $"Exported {AppName}!", "OK");
            EditorUtility.RevealInFinder(path);
            Close();
        }
    }

    static void BuildAppBundle(GameObject appObject, GameObject AppIcon, GameObject AppScreen, string appName, string savePath, string AuthName)
    {
        GameObject AppInfo = new GameObject($"{AuthName}");
        AppInfo.transform.parent = appObject.transform;

        AppIcon.gameObject.name = appName + "Icon";
        AppScreen.gameObject.name = appName + "App";

        string ExportedFolder = "Assets/ExportedApps";
        if (!Directory.Exists(ExportedFolder))
            Directory.CreateDirectory(ExportedFolder);

        string prefabPath = Path.Combine(ExportedFolder, appName + ".prefab");
        PrefabUtility.SaveAsPrefabAsset(appObject, prefabPath);

        AssetImporter importer = AssetImporter.GetAtPath(prefabPath);
        importer.assetBundleName = appName;

        BuildPipeline.BuildAssetBundles(ExportedFolder,
            BuildAssetBundleOptions.None,
            BuildTarget.StandaloneWindows);

        string BundleFileName = appName.ToLower();
        string bundlePath = Path.Combine(ExportedFolder, BundleFileName);
        if (!File.Exists(bundlePath))
        {
            Debug.LogError("Bundle not found: " + bundlePath);
            return;
        }
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            string metaPath = savePath + ".meta";
            if (File.Exists(metaPath))
                File.Delete(metaPath);
        }
        File.Move(bundlePath, savePath);

        string manifestPath = bundlePath + ".manifest";
        if (File.Exists(manifestPath))
            File.Delete(manifestPath);

        Debug.Log($"App exported to: {savePath}");

        DestroyImmediate(AppInfo);
    }

}