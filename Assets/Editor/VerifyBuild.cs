using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class VerifyBuild
{
    // Invoke with -batchmode -quit -executeMethod VerifyBuild.Perform.
    public static void Perform()
    {
        var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Builds/SushiShakedown.app",
            target = BuildTarget.StandaloneOSX,
            options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception($"Player verification failed: {report.summary.result}, errors={report.summary.totalErrors}");
        UnityEngine.Debug.Log($"PLAYER_BUILD verified scenes={scenes.Length} errors={report.summary.totalErrors}");
    }
}
