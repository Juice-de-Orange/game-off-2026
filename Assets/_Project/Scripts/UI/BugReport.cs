using System;

namespace GameOff.UI
{
    /// <summary>
    /// P78: link to a new GitHub issue, pre-filled with what the game knows (version with commit, day, phase, scene, browser).
    /// The player only describes the bug. No server: the issue is written on GitHub under the player's own account.
    /// </summary>
    public static class BugReport
    {
        public const string RepoUrl = "https://github.com/Juice-de-Orange/game-off-2026";

        /// <param name="version">Application.version; WebBuild sets it to "&lt;bundleVersion&gt;+&lt;commit&gt;".</param>
        /// <param name="day">1-based day, 0 when unknown.</param>
        public static string IssueUrl(string version, int day, string phase, string scene, string browser)
        {
            string where = day > 0 ? $"Day {day}, {phase}" : scene;
            string title = $"[Bug] {where}: ";
            string body =
                "**What happened?**\n\n\n" +
                "**What did you expect?**\n\n\n" +
                "**How can we make it happen again?**\n\n\n" +
                "---\n" +
                "_Filled in by the game. Please don't add personal data (name, e-mail, address)._\n" +
                $"- Version: {VersionLine(version)}\n" +
                $"- Day: {(day > 0 ? day.ToString() : "-")} · Phase: {(day > 0 ? phase : "-")}\n" +
                $"- Scene: {scene}\n" +
                $"- Browser: {browser}\n";
            return $"{RepoUrl}/issues/new?labels=bug&title={Uri.EscapeDataString(title)}&body={Uri.EscapeDataString(body)}";
        }

        /// <summary>"0.1.0+abc1234" → "0.1.0+abc1234 (commit link)"; a version without commit stays as it is.</summary>
        private static string VersionLine(string version)
        {
            int plus = version.IndexOf('+');
            if (plus < 0 || plus == version.Length - 1) return version;
            return $"{version} ({RepoUrl}/commit/{version.Substring(plus + 1)})";
        }
    }
}
