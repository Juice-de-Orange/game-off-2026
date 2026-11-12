using System.Collections.Generic;
using GameOff.Characters;
using GameOff.EditorTools;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P87: the Shifter's question on day 6 (join the resistance) and the ending AIRLOCKED on day 7.
    /// P150: if he went out of the airlock or was arrested on day 3, his accomplice asks instead, and day 7 runs the same with her.
    /// </summary>
    public class ShifterTests
    {
        private const string Shifter = "shifter", Accomplice = "accomplice";

        private static List<RecurringCharacter> Story()
        {
            var list = new List<RecurringCharacter>();
            foreach (var g in AssetDatabase.FindAssets("t:RecurringCharacter", new[] { "Assets/_Project/Data/Characters/Story" }))
                list.Add(AssetDatabase.LoadAssetAtPath<RecurringCharacter>(AssetDatabase.GUIDToAssetPath(g)));
            Assert.IsNotEmpty(list, "no story characters - run GameOff/Build Story Characters");
            return list;
        }

        /// <summary>
        /// Plays days 1..lastDay on the intended path, except the Shifter on day 3 gets `day3` and every question is answered `join`.
        /// Decisions go through CharacterMemory.Decide like in the game, so the flags of their consequences are set too.
        /// </summary>
        private static List<CharacterPlanner.Planned> PlayTo(int lastDay, Verdict day3, bool join)
        {
            var chars = Story();
            var memory = new CharacterMemory();
            List<CharacterPlanner.Planned> due = null;
            for (int d = 1; d <= lastDay; d++)
            {
                due = CharacterPlanner.Due(d, chars, memory, 3, () => 0f);
                if (d == lastDay) break;
                foreach (var p in due)
                {
                    var a = p.character.Appearances[p.appearance];
                    if (a.HasQuestion && join) memory.SetFlag(a.flag);
                    var verdict = p.character.Id == Shifter && d == 3 ? day3 : p.traveler.IntendedVerdict;
                    memory.Decide(p.character, p.appearance, d, a.OutcomeOf(verdict), verdict);
                }
            }
            return due;
        }

        private static Appearance On(List<CharacterPlanner.Planned> due, string id)
        {
            foreach (var p in due)
                if (p.character.Id == id) return p.character.Appearances[p.appearance];
            return null;
        }

        private static Appearance ShifterOn(List<CharacterPlanner.Planned> due) => On(due, Shifter);
        private static Appearance AccompliceOn(List<CharacterPlanner.Planned> due) => On(due, Accomplice);

        [TestCase(Verdict.Admit)]
        [TestCase(Verdict.Deny)]
        public void Day6_AsksToJoinTheResistance(Verdict day3)
        {
            var due = PlayTo(6, day3, false);
            var a = ShifterOn(due);
            Assert.IsNotNull(a, $"the Shifter must come on day 6 when he was {day3} on day 3");
            Assert.IsTrue(a.HasQuestion);
            Assert.AreEqual(StoryCharacterBuilder.ResistanceFlag, a.flag);
            Assert.IsFalse(a.EndsRun);
            Assert.IsNull(AccompliceOn(due), "his accomplice only comes when he can't");
        }

        [TestCase(Verdict.Admit)]
        [TestCase(Verdict.Deny)]
        public void Joined_Day7EndsAirlocked_FirstInQueue(Verdict day3)
        {
            var due = PlayTo(7, day3, true);
            var a = ShifterOn(due);
            Assert.IsNotNull(a);
            Assert.IsTrue(a.EndsRun);
            Assert.AreEqual("AIRLOCKED", a.endingTitle);
            Assert.AreEqual(1, a.slot);
            Assert.IsNull(AccompliceOn(due));
        }

        [Test]
        public void Refused_Day7GoesOn()
        {
            var due = PlayTo(7, Verdict.Admit, false);
            var a = ShifterOn(due);
            Assert.IsNotNull(a, "the twist on day 7 without the ending");
            Assert.IsFalse(a.EndsRun);
            Assert.IsFalse(a.HasQuestion);
            Assert.IsNull(AccompliceOn(due));
        }

        [TestCase(Verdict.Airlock)]
        [TestCase(Verdict.Arrest)]
        public void GoneOnDay3_TheShifterNeverAsks(Verdict day3)
        {
            Assert.IsNull(ShifterOn(PlayTo(6, day3, true)));
            Assert.IsNull(ShifterOn(PlayTo(7, day3, true)));
            Assert.IsNull(ShifterOn(PlayTo(7, day3, false)));
        }

        // ---------- P150: his accomplice ----------

        [TestCase(Verdict.Airlock)]
        [TestCase(Verdict.Arrest)]
        public void GoneOnDay3_AccompliceAsksOnDay6(Verdict day3)
        {
            var a = AccompliceOn(PlayTo(6, day3, false));
            Assert.IsNotNull(a, $"his accomplice must come on day 6 when the Shifter was {day3} on day 3");
            Assert.IsTrue(a.HasQuestion);
            Assert.AreEqual("JOIN THE RESISTANCE?", a.questionTitle);
            Assert.AreEqual(StoryCharacterBuilder.ResistanceFlag, a.flag);
            Assert.IsFalse(a.EndsRun);
            Assert.AreEqual(Verdict.Admit, a.traveler.IntendedVerdict, "clean papers");
        }

        [Test]
        public void Accomplice_GreetsByWhatHappenedToHim()
        {
            var airlocked = AccompliceOn(PlayTo(6, Verdict.Airlock, false));
            var arrested = AccompliceOn(PlayTo(6, Verdict.Arrest, false));
            Assert.AreNotSame(airlocked.traveler, arrested.traveler);
            StringAssert.Contains("airlock", airlocked.traveler.Greeting);
            StringAssert.Contains("arrested", arrested.traveler.Greeting);
            Assert.AreEqual(airlocked.traveler.DisplayName, arrested.traveler.DisplayName, "the same woman");
        }

        [TestCase(Verdict.Airlock)]
        [TestCase(Verdict.Arrest)]
        public void GoneOnDay3_JoinedAccomplice_Day7EndsAirlocked_FirstInQueue(Verdict day3)
        {
            var due = PlayTo(7, day3, true);
            var a = AccompliceOn(due);
            Assert.IsNotNull(a);
            Assert.IsTrue(a.EndsRun);
            Assert.AreEqual("AIRLOCKED", a.endingTitle);
            Assert.AreEqual(1, a.slot);
            Assert.IsNull(ShifterOn(due), "only one ending on day 7");
        }

        [TestCase(Verdict.Airlock)]
        [TestCase(Verdict.Arrest)]
        public void GoneOnDay3_RefusedAccomplice_Day7GoesOn(Verdict day3)
        {
            var a = AccompliceOn(PlayTo(7, day3, false));
            Assert.IsNotNull(a, "the twist on day 7 without the ending");
            Assert.IsFalse(a.EndsRun);
            Assert.IsFalse(a.HasQuestion);
        }

        [Test]
        public void Accomplice_InTheEnding()
        {
            var accomplice = Story().Find(c => c.Id == Accomplice);
            Assert.IsNotNull(accomplice, "no accomplice - run GameOff/Build Story Characters");
            foreach (var outcome in new[] { Outcome.Helped, Outcome.Betrayed, Outcome.Neutral, Outcome.Dead })
                Assert.IsFalse(string.IsNullOrWhiteSpace(accomplice.SummaryFor(outcome)), outcome.ToString());
            var memory = new CharacterMemory();
            Assert.IsEmpty(CharacterDirector.SummaryLines(new[] { accomplice }, memory, 12), "not met, not mentioned");
            memory.Remember(accomplice, 3, 7, Outcome.Helped);
            Assert.AreEqual(1, CharacterDirector.SummaryLines(new[] { accomplice }, memory, 12).Count);
        }

        [Test]
        public void FlagConditions()
        {
            var c = ScriptableObject.CreateInstance<RecurringCharacter>();
            var memory = new CharacterMemory();
            Assert.IsFalse(memory.Meets(c, AppearanceCondition.FlagNotSet, "x"), "not met yet");
            memory.Remember(c, 0, 1, Outcome.Helped);
            Assert.IsTrue(memory.Meets(c, AppearanceCondition.FlagNotSet, "x"));
            Assert.IsFalse(memory.Meets(c, AppearanceCondition.FlagSet, "x"));
            memory.SetFlag("x");
            Assert.IsTrue(memory.Meets(c, AppearanceCondition.FlagSet, "x"));
            Assert.IsFalse(memory.Meets(c, AppearanceCondition.FlagNotSet, "x"));
            memory.Remember(c, 1, 2, Outcome.Dead);
            Assert.IsFalse(memory.Meets(c, AppearanceCondition.FlagSet, "x"), "the dead never come back");
            Object.DestroyImmediate(c);
        }

        /// <summary>P150: consequences set flags when the decision is made; appearances can wait for a flag or stay away because of one.</summary>
        [Test]
        public void ConsequenceFlags_AndFlagGates()
        {
            var c = ScriptableObject.CreateInstance<RecurringCharacter>();
            c.EditorSet("c", "C", "Human", new List<Appearance>
            {
                new Appearance
                {
                    onDeny = Outcome.Neutral, onArrest = Outcome.Neutral,
                    consequences = new List<Consequence>
                    {
                        new Consequence { when = Outcome.Dead, setFlag = "dead" },
                        new Consequence { when = Outcome.Neutral, onlyIfArrested = true, afterDays = 3, setFlag = "arrested" },
                    },
                },
            });
            var denied = new CharacterMemory();
            denied.Decide(c, 0, 3, Outcome.Neutral, Verdict.Deny);
            Assert.IsFalse(denied.HasFlag("arrested"), "only when the player chose arrest");
            var arrested = new CharacterMemory();
            arrested.Decide(c, 0, 3, Outcome.Neutral, Verdict.Arrest);
            Assert.IsTrue(arrested.HasFlag("arrested"), "set at once, not after afterDays");
            var dead = new CharacterMemory();
            dead.Decide(c, 0, 3, Outcome.Dead, Verdict.Airlock);
            Assert.IsTrue(dead.HasFlag("dead"));
            Assert.IsFalse(dead.HasFlag("arrested"));

            var gated = new Appearance { onlyIfFlag = "dead", unlessFlag = "arrested" };
            Assert.IsTrue(dead.Allows(gated));
            Assert.IsFalse(arrested.Allows(gated));
            Assert.IsFalse(denied.Allows(gated));
            Assert.IsTrue(denied.Allows(new Appearance()), "no gates");
            Object.DestroyImmediate(c);
        }
    }
}
