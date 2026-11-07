using System;
using GameOff.UI;
using NUnit.Framework;

namespace GameOff.Tests
{
    /// <summary>P78: the bug report link opens a new issue in our repo, pre-filled and correctly escaped.</summary>
    public class BugReportTests
    {
        private static string Body(string url) => Uri.UnescapeDataString(url.Substring(url.IndexOf("&body=", StringComparison.Ordinal) + 6));
        private static string Title(string url)
        {
            int start = url.IndexOf("&title=", StringComparison.Ordinal) + 7;
            return Uri.UnescapeDataString(url.Substring(start, url.IndexOf("&body=", StringComparison.Ordinal) - start));
        }

        [Test]
        public void Url_points_at_new_issue_with_bug_label()
        {
            string url = BugReport.IssueUrl("0.1.0", 2, "Shift", "Main", "Firefox");
            StringAssert.StartsWith("https://github.com/Juice-de-Orange/game-off-2026/issues/new?labels=bug&title=", url);
        }

        [Test]
        public void Body_has_day_phase_scene_browser_and_commit_link()
        {
            string url = BugReport.IssueUrl("0.1.0+abc1234", 3, "Evening", "Main", "Mozilla/5.0 (X11; Linux) & more");
            string body = Body(url);
            StringAssert.Contains("Version: 0.1.0+abc1234 (https://github.com/Juice-de-Orange/game-off-2026/commit/abc1234)", body);
            StringAssert.Contains("Day: 3 · Phase: Evening", body);
            StringAssert.Contains("Scene: Main", body);
            StringAssert.Contains("Browser: Mozilla/5.0 (X11; Linux) & more", body);
            StringAssert.Contains("personal data", body);
            Assert.AreEqual("[Bug] Day 3, Evening: ", Title(url));
        }

        [Test]
        public void Special_characters_are_escaped()
        {
            string url = BugReport.IssueUrl("0.1.0", 1, "Shift", "Main", "a&b=c #d");
            string query = url.Substring(url.IndexOf('?') + 1);
            Assert.AreEqual(3, query.Split('&').Length, "only labels, title and body may be separated by &");
            Assert.IsFalse(query.Contains("#"), "a raw # would cut the link");
            Assert.IsFalse(query.Contains(" "));
        }

        [Test]
        public void Without_day_manager_the_scene_is_named()
        {
            string url = BugReport.IssueUrl("0.1.0", 0, "-", "Menu", "Chrome");
            Assert.AreEqual("[Bug] Menu: ", Title(url));
            StringAssert.Contains("Day: - · Phase: -", Body(url));
            StringAssert.Contains("Version: 0.1.0\n", Body(url));
        }
    }
}
