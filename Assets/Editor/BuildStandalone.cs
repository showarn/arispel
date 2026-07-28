using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class BuildStandalone
{
    [MenuItem("Build/Build Linux Standalone")]
    public static void BuildLinux()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new InvalidOperationException("No enabled scenes were found in Build Settings.");

        const string outputPath = "Build/Linux/ArisMonstertrucks.x86_64";
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneLinux64,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException(
                $"Standalone build failed: {report.summary.result} " +
                $"({report.summary.totalErrors} errors).");
    }
}
