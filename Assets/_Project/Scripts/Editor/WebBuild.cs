using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// The WebGL build for the live version (P49, docs/DEPLOY.md). Called with -executeMethod, locally and by the GitHub workflow:
    /// -customBuildPath sets the output folder. Web template and hardware statistics apply to this build only and are put back
    /// afterwards, so the settings for the itch.io build (P19) stay as they are.
    /// </summary>
    public static class WebBuild
    {
        private const string LiveTemplate = "PROJECT:Kestrel9";
        private const string DefaultOutput = "Builds/WebGL";

        public static void Build()
        {
            var template = PlayerSettings.WebGL.template;
            bool statistics = HardwareStatistics;
            BuildResult result;
            string output = OutputPath();
            try
            {
                PlayerSettings.WebGL.template = LiveTemplate;
                HardwareStatistics = false; // the site promises not to pass visitor data on
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None,
                });
                result = report.summary.result;
                Debug.Log($"[WebBuild] {result}: {report.summary.totalSize / (1024 * 1024)} MB in {report.summary.totalTime:mm\\:ss}, {report.summary.totalErrors} errors → {output}");
            }
            finally
            {
                PlayerSettings.WebGL.template = template;
                HardwareStatistics = statistics;
                AssetDatabase.SaveAssets(); // the build wrote the temporary values to disk
            }
            if (Application.isBatchMode) EditorApplication.Exit(result == BuildResult.Succeeded ? 0 : 1);
        }

        /// <summary>Player Settings → Hardware Statistics; Unity has no public API for it.</summary>
        private static bool HardwareStatistics
        {
            get => PlayerSettingsAsset().FindProperty("submitAnalytics").boolValue;
            set
            {
                var settings = PlayerSettingsAsset();
                settings.FindProperty("submitAnalytics").boolValue = value;
                settings.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static SerializedObject PlayerSettingsAsset() =>
            new SerializedObject(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));

        private static string OutputPath()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-customBuildPath");
            return Path.GetFullPath(index >= 0 && index + 1 < args.Length ? args[index + 1] : DefaultOutput);
        }
    }
}
