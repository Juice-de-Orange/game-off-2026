using GameOff.Economy;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P113: the airlock shows up the same evening and the next morning; without the airlock nothing does.</summary>
    public class MoraleEchoTests
    {
        private ConfiscationTuning _rules;
        private MoraleTuning _tuning;
        private Conduct _conduct;

        private static readonly Judgement Ordered = new Judgement(true, true, false, "", Verdict.Airlock, Verdict.Airlock);
        private static readonly Judgement Innocent = new Judgement(false, true, false, "x", Verdict.Admit, Verdict.Airlock);
        private static readonly Judgement Admitted = new Judgement(true, false, false, "", Verdict.Admit, Verdict.Admit);

        [SetUp]
        public void SetUp()
        {
            _rules = ScriptableObject.CreateInstance<ConfiscationTuning>();
            _tuning = ScriptableObject.CreateInstance<MoraleTuning>();
            _conduct = new Conduct();
        }

        [TearDown]
        public void CleanUp()
        {
            Object.DestroyImmediate(_rules);
            Object.DestroyImmediate(_tuning);
        }

        [TestCase("DELA MORN", "Dela Morn")]
        [TestCase("ZEV ZELL-3", "Zev Zell-3")]
        [TestCase("KESSLER'S LUCK", "Kessler's Luck")]
        [TestCase("Vesna", "Vesna")]
        [TestCase("", "")]
        public void TitleCase_MakesPaperNamesFitForProse(string name, string expected) => Assert.AreEqual(expected, TextMatch.TitleCase(name));

        [Test]
        public void WithoutTheAirlock_NothingAppears()
        {
            _conduct.Judge(Verdict.Admit, Admitted, _rules, "Vesna");
            Assert.IsNull(_tuning.Thought(_conduct, 3));
            Assert.IsNull(_tuning.Nightmare(_conduct, 3));
            Assert.IsNull(_tuning.CrowdLine(_conduct, 0.5f));
        }

        [Test]
        public void OrderedAirlock_IsFeltTheSameDay_FromTheOrderedLines()
        {
            _conduct.Judge(Verdict.Airlock, Ordered, _rules, "Vesna");
            CollectionAssert.Contains(Filled(_tuning.thoughtsOrdered), _tuning.Thought(_conduct, 1));
            CollectionAssert.Contains(Filled(_tuning.nightmaresOrdered), _tuning.Nightmare(_conduct, 1));
            CollectionAssert.Contains(Filled(_tuning.crowdOrdered), _tuning.CrowdLine(_conduct, 0.99f));
        }

        [Test]
        public void InnocentVictim_WinsOverAnOrderedOneThatDay_AndKeepsTheName()
        {
            _conduct.Judge(Verdict.Airlock, Innocent, _rules, "Vesna");
            _conduct.Judge(Verdict.Airlock, Ordered, _rules, "Sorvak");
            Assert.AreEqual("Vesna", _conduct.AirlockedName);
            StringAssert.Contains("Vesna", _tuning.Thought(_conduct, 0));
            CollectionAssert.Contains(Filled(_tuning.nightmaresInnocent), _tuning.Nightmare(_conduct, 1));
        }

        [Test]
        public void NextDay_OneOrderedAirlockFades()
        {
            _conduct.Judge(Verdict.Airlock, Ordered, _rules, "Vesna");
            _conduct.BeginDay();
            Assert.IsNull(_tuning.Thought(_conduct, 2), "-1 is above the lingering limit");
            Assert.IsNull(_tuning.Nightmare(_conduct, 2), "the nightmare belongs to the night after the airlock");
            Assert.IsNull(_tuning.CrowdLine(_conduct, 0f));
        }

        [Test]
        public void NextDay_AnInnocentVictimLingers_InThoughtsOnly()
        {
            _conduct.Judge(Verdict.Airlock, Innocent, _rules, "Vesna");
            _conduct.BeginDay();
            CollectionAssert.Contains(_tuning.thoughtsLingering, _tuning.Thought(_conduct, 2));
            Assert.IsNull(_tuning.Nightmare(_conduct, 2));
            Assert.IsNull(_tuning.CrowdLine(_conduct, 0f));
        }

        private string[] Filled(string[] lines)
        {
            var filled = new string[lines.Length];
            for (int i = 0; i < lines.Length; i++) filled[i] = lines[i].Replace("{name}", _conduct.AirlockedName);
            return filled;
        }
    }
}
