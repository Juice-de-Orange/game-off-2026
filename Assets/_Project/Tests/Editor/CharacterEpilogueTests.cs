using System.Collections.Generic;
using GameOff.Characters;
using GameOff.EditorTools;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P85 rest: Nell's son is someone the player arrested, the Shifter's interrogation, the day 11 summary.</summary>
    public class CharacterEpilogueTests
    {
        private static List<RecurringCharacter> Story()
        {
            var list = new List<RecurringCharacter>();
            foreach (var g in AssetDatabase.FindAssets("t:RecurringCharacter", new[] { "Assets/_Project/Data/Characters/Story" }))
                list.Add(AssetDatabase.LoadAssetAtPath<RecurringCharacter>(AssetDatabase.GUIDToAssetPath(g)));
            Assert.IsNotEmpty(list, "no story characters - run GameOff/Build Story Characters");
            return list;
        }

        private static RecurringCharacter Nell() => Story().Find(c => c.Id == "nell");

        [Test]
        public void Kin_ArrestedSonComesFirst()
        {
            var nell = Nell();
            var m = new CharacterMemory();
            m.RememberArrest("RELL ASTA", "Vellani", 3);
            m.RememberArrest(StoryCharacterBuilder.NellSon, "Human", 4);
            m.RememberArrest("NESTOR KAI", "Human", 5);
            Assert.AreEqual((StoryCharacterBuilder.NellSon, true), m.Kin(nell, 6));
        }

        [Test]
        public void Kin_ElseTheLatestHumanArrestBeforeToday()
        {
            var nell = Nell();
            var m = new CharacterMemory();
            m.RememberArrest("KAEL DROSS", "Human", 3);
            m.RememberArrest("NESTOR KAI", "Human", 5);
            m.RememberArrest("RELL ASTA", "Vellani", 5);
            m.RememberArrest("DARO HESK", "Human", 6); // today: not yet in prison when she asks
            Assert.AreEqual(("NESTOR KAI", true), m.Kin(nell, 6));
            m.RememberArrest("CATO BRILL", "Human", 7);
            Assert.AreEqual(("NESTOR KAI", true), m.Kin(nell, 11), "fixed the first time it is asked");
        }

        [Test]
        public void Kin_NobodyArrested_SomeoneElseTookJonas()
        {
            var m = new CharacterMemory();
            m.RememberArrest("RELL ASTA", "Vellani", 3);
            Assert.AreEqual((StoryCharacterBuilder.NellSon, false), m.Kin(Nell(), 6));
        }

        [Test]
        public void NellsGreetingsNameTheSon()
        {
            var nell = Nell();
            var m = new CharacterMemory();
            m.RememberArrest(StoryCharacterBuilder.NellSon, "Human", 4);
            string first = CharacterDirector.Fill(nell, nell.Appearances[0].traveler.Greeting, m, 6);
            StringAssert.Contains("Jonas", first);
            StringAssert.Contains(nell.KinFate(true), first);
            Assert.IsFalse(first.Contains("{"), first);
            foreach (var a in nell.Appearances)
                foreach (var text in new[] { a.traveler.Greeting, a.greetingAfterHelped, a.greetingAfterBetrayed })
                    Assert.IsFalse(CharacterDirector.Fill(nell, text, m, a.dayFrom)?.Contains("{") ?? false, text);
        }

        [Test]
        public void JonasIsADay4ArrestInTheStory()
        {
            var jonas = AssetDatabase.LoadAssetAtPath<TravelerData>("Assets/_Project/Data/Travelers/Normal/T_Normal_Jonas.asset");
            Assert.IsNotNull(jonas, "Nell's son from P15 is missing");
            Assert.AreEqual(StoryCharacterBuilder.NellSon, jonas.DisplayName);
            Assert.AreEqual(Verdict.Arrest, jonas.IntendedVerdict);
            Assert.AreEqual("Human", jonas.Species);
        }

        [Test]
        public void ShifterArrestedOnDay3_InterrogationNextMorning()
        {
            var shifter = Story().Find(c => c.Id == "shifter");
            var day3 = shifter.Appearances[2];
            Assert.AreEqual(3, day3.dayFrom);
            var c = day3.consequences.Find(x => x.onlyIfArrested);
            Assert.IsNotNull(c, "no interrogation announcement");
            Assert.AreEqual(day3.OutcomeOf(Verdict.Arrest), c.when);
            Assert.AreEqual(1, c.afterDays);
            Assert.IsFalse(string.IsNullOrWhiteSpace(c.announcement));
        }

        [Test]
        public void Day11Summary_OneLinePerCharacterMet()
        {
            var chars = Story();
            var memory = new CharacterMemory();
            for (int d = 1; d <= 10; d++)
                foreach (var p in CharacterPlanner.Due(d, chars, memory, 3, () => 0f))
                {
                    var verdict = p.traveler.IntendedVerdict;
                    memory.Decide(p.character, p.appearance, d, p.character.Appearances[p.appearance].OutcomeOf(verdict), verdict); // P150: flags too
                }
            memory.RememberArrest(StoryCharacterBuilder.NellSon, "Human", 4);
            var lines = CharacterDirector.SummaryLines(chars, memory, 11);
            Assert.GreaterOrEqual(lines.Count, 5, string.Join("\n", lines));
            foreach (var l in lines) Assert.IsFalse(l.Contains("{"), l);
            Assert.IsTrue(lines.Exists(l => l.Contains("Nell Varga") && l.Contains("Jonas")), string.Join("\n", lines));
            Assert.IsTrue(lines.Exists(l => l.Contains("Ilsa Koval")), "P150: the Shifter's accomplice on the intended path\n" + string.Join("\n", lines));
        }

        [Test]
        public void SummaryByOutcome()
        {
            var c = ScriptableObject.CreateInstance<RecurringCharacter>();
            c.EditorSetSummary("h", "b", null, "d");
            Assert.AreEqual("h", c.SummaryFor(Outcome.Helped));
            Assert.AreEqual("b", c.SummaryFor(Outcome.Confiscated));
            Assert.IsNull(c.SummaryFor(Outcome.Neutral));
            Assert.AreEqual("d", c.SummaryFor(Outcome.Dead));
            var m = new CharacterMemory();
            Assert.IsEmpty(CharacterDirector.SummaryLines(new[] { c }, m, 11), "not met, not mentioned");
            m.Remember(c, 0, 2, Outcome.Dead);
            Assert.AreEqual(new List<string> { "· d" }, CharacterDirector.SummaryLines(new[] { c }, m, 11));
            Object.DestroyImmediate(c);
        }

        [Test]
        public void FirstName() => Assert.AreEqual("Jonas", CharacterDirector.FirstName("JONAS VARGA"));
    }
}
