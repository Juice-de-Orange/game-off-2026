using GameOff.Metrics;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P129: the play events the game queues and the request body deploy/reports reads.</summary>
    public class PlayMetricsTests
    {
        [System.Serializable]
        private class Head
        {
            public string session, version;
        }

        [System.Serializable]
        private class Beat
        {
            public string e, phase, msg;
            public float t, fps;
            public int day;
            public bool paused;
            public int[] family;
        }

        [SetUp]
        public void SetUp()
        {
            PlayMetrics.Reset();
            PlayMetrics.Enabled = true;
        }

        [TearDown]
        public void TearDown()
        {
            PlayMetrics.Reset();
            PlayMetrics.Enabled = false;
        }

        [Test]
        public void Event_is_valid_json_with_escaped_text_and_invariant_numbers()
        {
            var json = new MetricsEvent("beat", 12.345f)
                .Add("day", 3)
                .Add("phase", "Shift")
                .Add("fps", 1.5f)
                .Add("paused", true)
                .Add("family", new[] { 0, 2, 4 })
                .Add("msg", "He said \"no\"\\\n\tthen\u0001left")
                .ToJson();
            StringAssert.Contains("\"fps\":1.5", json, "a dot, never a comma, whatever the culture");
            var beat = JsonUtility.FromJson<Beat>(json);
            Assert.AreEqual("beat", beat.e);
            Assert.AreEqual(12.35f, beat.t, 0.001f, "two decimals");
            Assert.AreEqual(3, beat.day);
            Assert.AreEqual("Shift", beat.phase);
            Assert.IsTrue(beat.paused);
            CollectionAssert.AreEqual(new[] { 0, 2, 4 }, beat.family);
            Assert.AreEqual("He said \"no\"\\\n\tthen\u0001left", beat.msg);
        }

        [Test]
        public void Odd_numbers_become_zero()
        {
            Assert.AreEqual("0", MetricsEvent.Number(float.NaN));
            Assert.AreEqual("0", MetricsEvent.Number(float.PositiveInfinity));
        }

        [Test]
        public void Nothing_is_queued_while_off()
        {
            PlayMetrics.Enabled = false;
            PlayMetrics.Track(new MetricsEvent("beat", 1f));
            Assert.AreEqual(0, PlayMetrics.Queued);
            Assert.IsNull(PlayMetrics.TakeBatch("0.1.0"));
        }

        [Test]
        public void Batch_carries_session_and_version_and_at_most_100_events()
        {
            for (int i = 0; i < 130; i++) PlayMetrics.Track(new MetricsEvent("beat", i));
            string batch = PlayMetrics.TakeBatch("0.1.0+abc1234");
            var head = JsonUtility.FromJson<Head>(batch);
            Assert.AreEqual(PlayMetrics.SessionId, head.session);
            StringAssert.IsMatch("^[0-9a-f]{12}$", head.session, "the backend accepts 8-32 hex characters");
            Assert.AreEqual("0.1.0+abc1234", head.version);
            Assert.AreEqual(PlayMetrics.MaxPerBatch, CountEvents(batch));
            Assert.AreEqual(30, PlayMetrics.Queued, "the rest waits for the next request");
            Assert.AreEqual(30, CountEvents(PlayMetrics.TakeBatch("v")));
            Assert.IsNull(PlayMetrics.TakeBatch("v"));
        }

        [Test]
        public void Full_queue_drops_the_oldest()
        {
            for (int i = 0; i < PlayMetrics.MaxQueued + 5; i++) PlayMetrics.Track(new MetricsEvent("beat", i));
            Assert.AreEqual(PlayMetrics.MaxQueued, PlayMetrics.Queued);
            StringAssert.Contains("{\"e\":\"beat\",\"t\":5}", PlayMetrics.TakeBatch("v"), "events 0-4 are gone");
        }

        [Test]
        public void Page_session_is_taken_only_when_the_backend_would_accept_it()
        {
            PlayMetrics.UseSession("0a1b2c3d4e5f");
            Assert.AreEqual("0a1b2c3d4e5f", PlayMetrics.SessionId);
            foreach (var bad in new[] { null, "", "abc", "0A1B2C3D4E5F", "../../etc/x", new string('a', 33) })
            {
                PlayMetrics.UseSession(bad);
                Assert.AreEqual("0a1b2c3d4e5f", PlayMetrics.SessionId, $"'{bad}' is ignored");
            }
        }

        private static int CountEvents(string batch)
        {
            int count = 0, at = 0;
            while ((at = batch.IndexOf("{\"e\":", at, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                at++;
            }
            return count;
        }
    }
}
