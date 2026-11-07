using GameOff.Day;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;

namespace GameOff.Tests
{
    /// <summary>P36: the station's situation comes from the crisis events, calms down slowly and is capped at 1.</summary>
    public class StationMoodTests
    {
        [Test]
        public void Situation_AddsTodaysCrises_KeepsHalfOfYesterday_AndIsCapped()
        {
            Assert.AreEqual(0f, StationMood.Next(0f, 0f, 0.5f), 1e-5f, "calm day stays calm");
            Assert.AreEqual(0.4f, StationMood.Next(0f, 0.4f, 0.5f), 1e-5f, "a crisis raises it");
            Assert.AreEqual(0.8f, StationMood.Next(0.4f, 0.6f, 0.5f), 1e-5f, "yesterday's half stays");
            Assert.AreEqual(0.4f, StationMood.Next(0.8f, 0f, 0.5f), 1e-5f, "it halves on a calm day");
            Assert.AreEqual(1f, StationMood.Next(0.9f, 0.9f, 0.5f), 1e-5f, "capped at 1");
        }

        [Test]
        public void MarketEventsInProject_ShakeTheStation()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MarketEvent"))
            {
                var e = AssetDatabase.LoadAssetAtPath<MarketEvent>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.Greater(e.crisis, 0f, $"{e.name}: crisis is 0 - the station would not notice it");
            }
        }

        [Test]
        public void TuningInProject_GetsWorseWithTheSituation()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<StationMoodTuning>("Assets/_Project/Data/Days/StationMoodTuning.asset");
            Assert.NotNull(tuning, "StationMoodTuning missing - run GameOff/Build Day Flow into Main");
            Assert.Less(tuning.worstPause.y, tuning.calmPause.x, "lamps flicker more often when it is bad");
            Assert.Less(tuning.neonWorstPause.y, tuning.neonCalmPause.x, "neons drop out more often when it is bad");
            Assert.LessOrEqual(tuning.neonFrom, tuning.blackoutFrom, "neons go before the power does");
        }
    }
}
