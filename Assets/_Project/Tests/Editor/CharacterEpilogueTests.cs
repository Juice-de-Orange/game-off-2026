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

        /// <summary>Jonas sent out of the airlock on day 4: Nell still asks for him, and hears he never left the station.</summary>
        [Test]
        public void Kin_JonasAirlocked_NellHearsHeNeverLeft()
        {
            var nell = Nell();
            var m = new CharacterMemory();
            m.RememberArrest("NESTOR KAI", "Human", 3);
            m.RememberAirlock(StoryCharacterBuilder.NellSon, "Human", 4);
            Assert.AreEqual((StoryCharacterBuilder.NellSon, true), m.Kin(nell, 6), "her own son, not the other arrest");
            string first = CharacterDirector.Fill(nell, nell.Appearances[0].traveler.Greeting, m, 6);
            StringAssert.Contains("Jonas", first);
            StringAssert.Contains("never left your station", first);
            StringAssert.DoesNotContain(nell.KinFate(true), first);
        }

        [Test]
        public void Kin_AirlockedStrangersAreNotInPrison()
        {
            var m = new CharacterMemory();
            m.RememberArrest("NESTOR KAI", "Human", 3);
            m.RememberAirlock("DARO HESK", "Human", 5);
            Assert.AreEqual(("NESTOR KAI", true), m.Kin(Nell(), 6));
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

        /// <summary>Plays days 1..lastDay with every character decided as intended (flags too, P150).</summary>
        private static CharacterMemory PlayIntended(List<RecurringCharacter> chars, int lastDay)
        {
            var memory = new CharacterMemory();
            for (int d = 1; d <= lastDay; d++)
                foreach (var p in CharacterPlanner.Due(d, chars, memory, 3, () => 0f))
                {
                    var verdict = p.traveler.IntendedVerdict;
                    memory.Decide(p.character, p.appearance, d, p.character.Appearances[p.appearance].OutcomeOf(verdict), verdict);
                }
            memory.RememberArrest(StoryCharacterBuilder.NellSon, "Human", 4);
            return memory;
        }

        /// <summary>
        /// The newsletter on the morning of day 11 only tells finished stories: Nell still comes that day and Oddo on day 12, so they
        /// are left out; everyone whose last appearance was day 10 or earlier is in it.
        /// </summary>
        [Test]
        public void Day11Newsletter_OnlyFinishedStories()
        {
            var chars = Story();
            var memory = PlayIntended(chars, 10);
            var lines = CharacterDirector.NewsletterLines(chars, memory, 11);
            string all = string.Join("\n", lines);
            Assert.GreaterOrEqual(lines.Count, 5, all);
            foreach (var l in lines) Assert.IsFalse(l.Contains("{"), l);
            Assert.IsFalse(lines.Exists(l => l.Contains("Nell")), "Nell comes today - her story is not over\n" + all);
            Assert.IsFalse(lines.Exists(l => l.Contains("Oddo")), "Oddo comes tomorrow\n" + all);
            Assert.IsFalse(lines.Exists(l => l.Contains("never learned")), "no open lines in the newsletter\n" + all);
            Assert.IsTrue(lines.Exists(l => l.Contains("Ilsa Koval")), "P150: the Shifter's accomplice on the intended path\n" + all);
            Assert.IsTrue(lines.Exists(l => l.Contains("Tobin")), "day 10 is over on the morning of day 11\n" + all);
        }

        /// <summary>The ticket ending tells finished stories and says which ones are still open.</summary>
        [Test]
        public void EndingLines_FinishedStoriesAndOpenOnes()
        {
            var chars = Story();
            var memory = PlayIntended(chars, 8);
            var lines = CharacterDirector.SummaryLines(chars, memory, 8);
            string all = string.Join("\n", lines);
            foreach (var l in lines) Assert.IsFalse(l.Contains("{"), l);
            Assert.IsTrue(lines.Contains("· You never learned what became of Nell Varga."), all);
            Assert.IsTrue(lines.Contains("· You never learned what became of Pim Oddo."), all);
            Assert.IsTrue(lines.Contains("· You never learned what became of Dr. Senn."), "own open line\n" + all);
            Assert.IsTrue(lines.Exists(l => l.Contains("Mira Talsk") && !l.Contains("never learned")), "her last day was today\n" + all);

            var after = PlayIntended(chars, 12);
            var final = CharacterDirector.SummaryLines(chars, after, 12);
            Assert.IsFalse(final.Exists(l => l.Contains("never learned") || l.Contains("never read")), string.Join("\n", final));
            Assert.IsTrue(final.Exists(l => l.Contains("Nell Varga") && l.Contains("Jonas")), string.Join("\n", final));
        }

        /// <summary>Every story character's story ends on (or after) their last appearance, and has an open line.</summary>
        [Test]
        public void StoryEnds_AfterTheLastAppearance()
        {
            foreach (var c in Story())
            {
                int last = 0;
                foreach (var a in c.Appearances) last = Mathf.Max(last, Mathf.Max(a.dayFrom, a.dayTo));
                Assert.GreaterOrEqual(c.StoryEndsOnDay, last, c.name);
                Assert.IsFalse(string.IsNullOrWhiteSpace(c.OpenLine), c.name);
                Assert.IsFalse(c.OpenLine.Contains("THE ") || c.OpenLine.Contains(c.DisplayName), $"{c.name}: title case, not \"{c.OpenLine}\"");
            }
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

        /// <summary>Before their last day the story is open (own line or the default from the name); the dead are always finished.</summary>
        [Test]
        public void OpenStories()
        {
            var c = ScriptableObject.CreateInstance<RecurringCharacter>();
            c.EditorSet("x", "PIM ODDO", "Quobb", new List<Appearance>());
            c.EditorSetSummary("h", "b", null, "d");
            c.EditorSetStoryEnd(12);
            var m = new CharacterMemory();
            m.Remember(c, 0, 8, Outcome.Helped);
            Assert.AreEqual(new List<string> { "· You never learned what became of Pim Oddo." }, CharacterDirector.SummaryLines(new[] { c }, m, 11));
            Assert.IsEmpty(CharacterDirector.SummaryLines(new[] { c }, m, 11, false), "the newsletter leaves open stories out");
            Assert.AreEqual(new List<string> { "· h" }, CharacterDirector.SummaryLines(new[] { c }, m, 12));
            Assert.IsEmpty(CharacterDirector.NewsletterLines(new[] { c }, m, 12), "on the morning of his last day his story isn't over");
            Assert.AreEqual(new List<string> { "· h" }, CharacterDirector.NewsletterLines(new[] { c }, m, 13));
            c.EditorSetStoryEnd(12, "Oddo is still out there.");
            Assert.AreEqual(new List<string> { "· Oddo is still out there." }, CharacterDirector.SummaryLines(new[] { c }, m, 11));
            m.Remember(c, 1, 10, Outcome.Dead);
            Assert.AreEqual(new List<string> { "· d" }, CharacterDirector.SummaryLines(new[] { c }, m, 11), "the dead are finished");
            Object.DestroyImmediate(c);
        }

        /// <summary>P77/P85: when a betrayal can come from a lost deposit, the summary doesn't speak of a refusal or a letter.</summary>
        [Test]
        public void LostDepositSummaries_DontClaimARefusal()
        {
            foreach (var c in Story())
            {
                bool entrusts = false;
                foreach (var a in c.Appearances) if (a.entrustItem != null) entrusts = true;
                if (!entrusts) continue;
                string line = c.SummaryFor(Outcome.Betrayed) ?? "";
                foreach (var word in new[] { "sent", "letter", "never came back", "ring is finished", "ran dry" })
                    StringAssert.DoesNotContain(word, line, c.name);
            }
        }

        [Test]
        public void FirstName() => Assert.AreEqual("Jonas", CharacterDirector.FirstName("JONAS VARGA"));
    }
}
