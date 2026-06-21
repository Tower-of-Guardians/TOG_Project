#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

public class WindowsBuild
{
    public static void PerformBuild()
    {
        BuildAddressables();

        string[] scenes = FindEnabledEditorScenes();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/TOG.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Build failed: {report.summary.result}");
        }

        Console.WriteLine("Build succeeded");
    }

    private static void BuildAddressables()
    {
        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

        if (settings == null)
        {
            throw new Exception("AddressableAssetSettings not found.");
        }

        Console.WriteLine("Cleaning Addressables content...");
        AddressableAssetSettings.CleanPlayerContent();

        Console.WriteLine("Building Addressables content...");
        AddressableAssetSettings.BuildPlayerContent();

        Console.WriteLine("Addressables build succeeded");
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> scenes = new List<string>();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled)
                continue;

            scenes.Add(scene.path);
        }

        return scenes.ToArray();
    }
}
#endif