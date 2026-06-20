using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;

public class WindowsBuild
{
    public static void PerformBuild()
    {
        string[] scenes = FindEnabledEditorScenes();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/Windows/MyGame.exe",
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