using GameOff.Counter;
using GameOff.Day;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P97 and P100: the station clock over a day and the line after the stamp.</summary>
    public class GameClockAndReactionTests
    {
        [TestCase(DayManager.Phase.Morning, 0f, 0f, 6 * 60)]
        [TestCase(DayManager.Phase.Shift, 0f, 0f, 8 * 60)]
        [TestCase(DayManager.Phase.Shift, 0.5f, 0f, 13 * 60)]
        [TestCase(DayManager.Phase.Shift, 1f, 0f, 18 * 60)]
        [TestCase(DayManager.Phase.Evening, 0f, 0f, 18 * 60)]
        [TestCase(DayManager.Phase.Evening, 0f, 2f, 23 * 60)]
        [TestCase(DayManager.Phase.Night, 0f, 0f, 23 * 60)]
        public void Clock_FollowsThePhases(DayManager.Phase phase, float shift, float evening, int minutes) =>
            Assert.AreEqual(minutes, GameClock.Compute(phase, shift, evening));

        [Test]
        public void Clock_Formats() => Assert.AreEqual("08:05", GameClock.Format(8 * 60 + 5));

        private static TravelerData Traveler(string species, string admitted, string denied)
        {
            var t = ScriptableObject.CreateInstance<TravelerData>();
            t.EditorSet("X", species, new System.Collections.Generic.List<GameOff.Documents.DocumentData>(), Verdict.Admit, "", "", "", null, null);
            t.EditorSetReactions(admitted, denied);
            return t;
        }

        [Test]
        public void Reaction_OwnLineBeatsThePool_NothingForArrest()
        {
            var tuning = ScriptableObject.CreateInstance<DialogueTuning>();
            var t = Traveler("Human", null, "Mine.");
            Assert.AreEqual("Mine.", TravelerDialogue.Reaction(t, Verdict.Deny, tuning, 0.5f));
            CollectionAssert.Contains(tuning.speciesLines.Find(l => l.species == "Human").admitted, TravelerDialogue.Reaction(t, Verdict.Admit, tuning, 0.99f));
            Assert.IsNull(TravelerDialogue.Reaction(t, Verdict.Arrest, tuning, 0.5f));
            Object.DestroyImmediate(t);
            Object.DestroyImmediate(tuning);
        }

        [TestCase("Human")]
        [TestCase("Vellani")]
        [TestCase("Quobb")]
        [TestCase("Xirrik")]
        [TestCase("Sorvak")]
        public void Pool_CoversEverySpecies(string species)
        {
            var tuning = ScriptableObject.CreateInstance<DialogueTuning>();
            var t = Traveler(species, null, null);
            Assert.IsNotEmpty(TravelerDialogue.Reaction(t, Verdict.Deny, tuning, 0f));
            Assert.IsNotEmpty(TravelerDialogue.EveningLine(species, tuning, 0f));
            Object.DestroyImmediate(t);
            Object.DestroyImmediate(tuning);
        }
    }
}
