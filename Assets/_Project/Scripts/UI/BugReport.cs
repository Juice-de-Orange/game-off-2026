using System;
using UnityEngine;

namespace GameOff.UI
{
    /// <summary>
    /// P84: what the in-game bug report sends to the backend (deploy/reports), which turns it into a GitHub issue.
    /// The player only writes the text; version (with commit, set by WebBuild), day, phase, scene and browser come from the game.
    /// </summary>
    public static class BugReport
    {
        public const int MinLength = 3;
        public const int MaxLength = 1000;

        [Serializable]
        private class Payload
        {
            public string text;
            public string version;
            public int day;
            public string phase;
            public string scene;
            public string browser;
        }

        [Serializable]
        public class Answer
        {
            public bool ok;
            public int issue;
            public string error;
        }

        public static bool CanSend(string text)
        {
            int length = string.IsNullOrWhiteSpace(text) ? 0 : text.Trim().Length;
            return length >= MinLength && length <= MaxLength;
        }

        /// <param name="day">1-based day, 0 when unknown (e.g. outside Main).</param>
        public static string Json(string text, string version, int day, string phase, string scene, string browser) =>
            JsonUtility.ToJson(new Payload
            {
                text = (text ?? "").Trim(),
                version = version,
                day = day,
                phase = phase,
                scene = scene,
                browser = browser,
            });

        private static Answer Parse(string body)
        {
            try { return string.IsNullOrEmpty(body) ? null : JsonUtility.FromJson<Answer>(body); }
            catch (ArgumentException) { return null; }
        }

        /// <summary>Only a 201 whose answer says ok with an issue number counts (a proxy page with 201 doesn't).</summary>
        public static bool Succeeded(long status, string body)
        {
            var answer = Parse(body);
            return status == 201 && answer != null && answer.ok && answer.issue > 0;
        }

        /// <summary>The line under the form after sending, from the HTTP status and the backend's answer.</summary>
        public static string Result(long status, string body)
        {
            var answer = Parse(body);
            if (Succeeded(status, body)) return $"Thanks! Your report is in (#{answer.issue}).";
            if (status == 400 && answer != null && answer.error == "text too short") return "Please write a little more about what happened.";
            if (status == 400 && answer != null && answer.error == "text too long") return "That's a bit long - please shorten it.";
            return "Couldn't send the report. Please try again later.";
        }
    }
}
