using System.Collections.Generic;
using GameOff.Characters;
using GameOff.EditorTools;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P87: the Shifter's question on day 6 (join the resistance) and the ending AIRLOCKED on day 7.</summary>
    public class ShifterTests
    {
        private static List<RecurringCharacter> Story()
        {
            var list = new List<RecurringCharacter>();
            foreach (var g in AssetDatabase.FindAssets("t:RecurringCharacter", new[] { "Assets/_Project/Data/Characters/Story" }))
                list.Add(AssetDatabase.LoadAssetAtPath<RecurringCharacter>(AssetDatabase.GUIDToAssetPath(g)));
            Assert.IsNotEmpty(list, "no story characters - run GameOff/Build Story Characters");
            return list;
        }

        /// <summary>Plays days 1..lastDay on the intended path, except the Shifter on day 3 gets `day3` and his question `join`.</summary>
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
                    var verdict = p.character.Id == "shifter" && d == 3 ? day3 : p.traveler.IntendedVerdict;
                    memory.Remember(p.character, p.appearance, d, a.OutcomeOf(verdict));
                }
            }
            return due;
        }

        private static Appearance ShifterOn(List<CharacterPlanner.Planned> due)
        {
            foreach (var p in due)
                if (p.character.Id == "shifter") return p.character.Appearances[p.appearance];
            return null;
        }

        [Test]
        public void Day6_AsksToJoinTheResistance()
        {
            var a = ShifterOn(PlayTo(6, Verdict.Admit, false));
            Assert.IsNotNull(a, "the Shifter must come on day 6 when he was let through on day 3");
            Assert.IsTrue(a.HasQuestion);
            Assert.AreEqual(StoryCharacterBuilder.ResistanceFlag, a.flag);
            Assert.IsFalse(a.EndsRun);
        }

        [Test]
        public void Joined_Day7EndsAirlocked_FirstInQueue()
        {
            var a = ShifterOn(PlayTo(7, Verdict.Arrest, true));
            Assert.IsNotNull(a);
            Assert.IsTrue(a.EndsRun);
            Assert.AreEqual("AIRLOCKED", a.endingTitle);
            Assert.AreEqual(1, a.slot);
        }

        [Test]
        public void Refused_Day7GoesOn()
        {
            var a = ShifterOn(PlayTo(7, Verdict.Admit, false));
            Assert.IsNotNull(a, "the twist on day 7 without the ending");
            Assert.IsFalse(a.EndsRun);
            Assert.IsFalse(a.HasQuestion);
        }

        [Test]
        public void TerminatedOnDay3_NeverAsks()
        {
            Assert.IsNull(ShifterOn(PlayTo(6, Verdict.Airlock, true)));
            Assert.IsNull(ShifterOn(PlayTo(7, Verdict.Airlock, true)));
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
    }
}
