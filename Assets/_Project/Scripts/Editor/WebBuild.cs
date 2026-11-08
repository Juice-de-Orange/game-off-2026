using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GameOff.EditorTools
{
    /// <summary>
    /// The WebGL build for the live version (P49, docs/DEPLOY.md). Called with -executeMethod, locally and by the GitHub workflow:
    /// -customBuildPath sets the output folder. Web template and hardware statistics apply to this build only and are put back
    /// afterwards, so the settings for the itch.io build (P19) stay as they are. The version becomes "&lt;bundleVersion&gt;+&lt;commit&gt;"
    /// for this build only (Application.version, P78 bug report).
    /// </summary>
    public static class WebBuild
    {
        private const string LiveTemplate = "PROJECT:Kestrel9";
        private const string ItchTemplate = "PROJECT:Kestrel9Itch"; // P19: without the imprint footer of the live page; -itch on the command line
        private const string DefaultOutput = "Builds/WebGL";

        public static void Build()
        {
            var template = PlayerSettings.WebGL.template;
            bool statistics = HardwareStatistics;
            string version = PlayerSettings.bundleVersion.Split('+')[0]; // a killed build may have left "+<commit>" behind
            BuildResult result;
            string output = OutputPath();
            try
            {
                PlayerSettings.WebGL.template = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-itch") >= 0 ? ItchTemplate : LiveTemplate;
                HardwareStatistics = false; // the site promises not to pass visitor data on
                string commit = Commit();
                if (commit != null) PlayerSettings.bundleVersion = version + "+" + commit;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                    locationPathName = output,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None,
                });
                result = report.summary.result;
                Debug.Log($"[WebBuild] {result} ({PlayerSettings.bundleVersion}): {report.summary.totalSize / (1024 * 1024)} MB in {report.summary.totalTime:mm\\:ss}, {report.summary.totalErrors} errors → {output}");
            }
            finally
            {
                PlayerSettings.WebGL.template = template;
                HardwareStatistics = statistics;
                PlayerSettings.bundleVersion = version;
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

        /// <summary>Short commit: GITHUB_SHA in the workflow, otherwise git; null when neither is there.</summary>
        private static string Commit()
        {
            string sha = Environment.GetEnvironmentVariable("GITHUB_SHA");
            if (string.IsNullOrEmpty(sha))
            {
                try
                {
                    var git = new ProcessStartInfo("git", "rev-parse HEAD")
                    {
                        RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true,
                    };
                    using (var process = Process.Start(git))
                    {
                        sha = process.StandardOutput.ReadToEnd().Trim();
                        process.WaitForExit();
                        if (process.ExitCode != 0) sha = null;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning("[WebBuild] no commit for the version: " + e.Message);
                    sha = null;
                }
            }
            if (string.IsNullOrEmpty(sha)) return null;
            return sha.Length > 7 ? sha.Substring(0, 7) : sha;
        }

        private static string OutputPath()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-customBuildPath");
            return Path.GetFullPath(index >= 0 && index + 1 < args.Length ? args[index + 1] : DefaultOutput);
        }
    }
}
