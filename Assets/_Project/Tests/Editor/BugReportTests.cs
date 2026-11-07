using GameOff.UI;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P84: what the in-game bug report sends and how the form reads the backend's answer.</summary>
    public class BugReportTests
    {
        [System.Serializable]
        private class Sent
        {
            public string text, version, phase, scene, browser;
            public int day;
        }

        [Test]
        public void Json_carries_text_and_game_state()
        {
            var sent = JsonUtility.FromJson<Sent>(BugReport.Json("  Door stuck \"open\" & more  ", "0.1.0+abc1234", 3, "Evening", "Main", "Firefox"));
            Assert.AreEqual("Door stuck \"open\" & more", sent.text, "trimmed, quotes survive");
            Assert.AreEqual("0.1.0+abc1234", sent.version);
            Assert.AreEqual(3, sent.day);
            Assert.AreEqual("Evening", sent.phase);
            Assert.AreEqual("Main", sent.scene);
            Assert.AreEqual("Firefox", sent.browser);
        }

        [Test]
        public void Send_needs_a_few_characters_and_not_too_many()
        {
            Assert.IsFalse(BugReport.CanSend(null));
            Assert.IsFalse(BugReport.CanSend("   "));
            Assert.IsFalse(BugReport.CanSend("ab"));
            Assert.IsTrue(BugReport.CanSend("abc"));
            Assert.IsTrue(BugReport.CanSend(new string('x', BugReport.MaxLength)));
            Assert.IsFalse(BugReport.CanSend(new string('x', BugReport.MaxLength + 1)));
        }

        [Test]
        public void Answers_become_readable_lines()
        {
            Assert.AreEqual("Thanks! Your report is in (#42).", BugReport.Result(201, "{\"ok\": true, \"issue\": 42}"));
            Assert.IsTrue(BugReport.Succeeded(201, "{\"ok\": true, \"issue\": 42}"));
            Assert.IsFalse(BugReport.Succeeded(201, "<html>"), "a proxy page is not a success");
            StringAssert.Contains("Too many reports", BugReport.Result(429, "{\"ok\": false}"));
            StringAssert.Contains("write a little more", BugReport.Result(400, "{\"ok\": false, \"error\": \"text too short\"}"));
            StringAssert.Contains("shorten", BugReport.Result(400, "{\"ok\": false, \"error\": \"text too long\"}"));
            StringAssert.Contains("try again later", BugReport.Result(400, "{\"ok\": false, \"error\": \"not JSON\"}"));
            StringAssert.Contains("try again later", BugReport.Result(502, "{\"ok\": false, \"error\": \"github 401\"}"));
            StringAssert.Contains("try again later", BugReport.Result(0, null), "no connection");
            StringAssert.Contains("try again later", BugReport.Result(201, "<html>"), "garbage is not a success");
            Assert.IsFalse(BugReport.Succeeded(502, "{\"ok\": false}"));
        }
    }
}
