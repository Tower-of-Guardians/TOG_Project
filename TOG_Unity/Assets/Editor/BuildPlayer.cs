using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPlayer : MonoBehaviour
{
    [MenuItem("Build/Build Windows")]
    public static void BuildWindow()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

        buildPlayerOptions.scenes = new[]
        {
            "Assets/01. Scenes/Lobby.unity",
            "Assets/01. Scenes/Game.unity"
        };

        buildPlayerOptions.locationPathName = $"Builds/Windows/TowerOfGuardians_{PlayerSettings.bundleVersion}.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Windows Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("Windows Build failed");
        }
    }
}