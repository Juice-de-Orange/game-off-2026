using System.Collections.Generic;
using GameOff.Departure;
using GameOff.Economy;
using NUnit.Framework;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P69 and P12: dying after two nights on critical, the warning before, and the endings that follow.</summary>
    public class FamilyFateTests
    {
        private static Household Critical(int limit)
        {
            var h = new Household(new List<bool> { true }, limit);
            h.Settle(false, false, false); // sick
            h.Settle(false, false, false); // critical
            Assert.AreEqual(Household.Critical, h.Condition(0));
            return h;
        }

        [Test]
        public void DiesAfterTwoFurtherNightsOnCritical_WarnedTheNightBefore()
        {
            var h = Critical(2);
            Assert.IsFalse(h.WillDieNextNight(0));
            h.Settle(true, true, false);
            Assert.AreEqual(Household.Critical, h.Condition(0));
            Assert.IsTrue(h.WillDieNextNight(0));
            h.Settle(true, true, false);
            Assert.IsTrue(h.IsDead(0));
            CollectionAssert.AreEqual(new[] { 0 }, h.DiedTonight);
            Assert.AreEqual(0, h.Alive);
        }

        [Test]
        public void MedicineOnTheLastNightSaves()
        {
            var h = Critical(2);
            h.Settle(true, true, false);
            Assert.IsTrue(h.WillDieNextNight(0));
            h.Settle(true, true, true);
            Assert.AreEqual(Household.Sick, h.Condition(0));
            Assert.IsFalse(h.WillDieNextNight(0));
        }

        [Test]
        public void PreviewShowsTheDeathWithoutCausingIt()
        {
            var h = Critical(2);
            h.Settle(true, true, false);
            Assert.AreEqual(Household.Dead, h.Preview(true, true, false)[0]);
            Assert.AreEqual(Household.Critical, h.Condition(0));
        }

        [Test]
        public void TheDeadStayDead_AndAreNotSick()
        {
            var h = Critical(1);
            h.Settle(true, true, false);
            Assert.IsTrue(h.IsDead(0));
            Assert.IsFalse(h.AnySick);
            h.Settle(true, true, true);
            Assert.IsTrue(h.IsDead(0));
            Assert.AreEqual(0, h.DiedTonight.Count, "only counted on the night it happened");
        }

        [Test]
        public void WithoutALimitNobodyDies()
        {
            var h = Critical(0);
            for (int i = 0; i < 10; i++) h.Settle(false, false, false);
            Assert.AreEqual(Household.Critical, h.Condition(0));
        }

        [Test]
        public void TicketEnding_SomebodyDead_IsTheLostEnding()
        {
            var texts = ScriptableObject.CreateInstance<DepartureTuning>();
            var (title, body) = TicketEnding.Pick(new[] { Household.Well, Household.Critical, Household.Dead }, new[] { "Lena", "Mom", "Pip" }, texts);
            Assert.AreEqual(texts.lostTitle, title);
            StringAssert.Contains("Pip", body);
            StringAssert.DoesNotContain("Mom", body);
            Object.DestroyImmediate(texts);
        }

        [TestCase(0, 0, "clean")]
        [TestCase(1, 0, "stained")]
        [TestCase(7, 0, "stained")]
        [TestCase(1, 1, "bloody")]
        [TestCase(8, 1, "bloody")]
        public void AirlockLine_FollowsTheAirlockCounts(int airlocks, int innocent, string expected)
        {
            var texts = ScriptableObject.CreateInstance<DepartureTuning>();
            texts.moraleCleanLine = "clean";
            texts.moraleStainedLine = "stained";
            texts.moraleBloodyLine = "bloody";
            Assert.AreEqual(expected, TicketEnding.AirlockLine(airlocks, innocent, texts));
            Object.DestroyImmediate(texts);
        }

        /// <summary>The story orders seven airlocks: an obedient run gets the "orders" line, one innocent is enough for the other.</summary>
        [Test]
        public void ConductLine_OrderedAirlocks_AreNotTheInnocentLine()
        {
            var texts = ScriptableObject.CreateInstance<DepartureTuning>();
            var rules = ScriptableObject.CreateInstance<ConfiscationTuning>();
            var ordered = new GameOff.Travelers.Judgement(true, true, false, "", GameOff.Travelers.Verdict.Airlock, GameOff.Travelers.Verdict.Airlock);
            var arrestable = new GameOff.Travelers.Judgement(false, true, false, "x", GameOff.Travelers.Verdict.Arrest, GameOff.Travelers.Verdict.Airlock);
            var innocent = new GameOff.Travelers.Judgement(false, true, false, "x", GameOff.Travelers.Verdict.Admit, GameOff.Travelers.Verdict.Airlock);
            var conduct = new Conduct();
            Assert.AreEqual(texts.moraleCleanLine, TicketEnding.ConductLine(conduct, texts));
            for (int i = 0; i < 7; i++) conduct.Judge(GameOff.Travelers.Verdict.Airlock, ordered, rules);
            conduct.Judge(GameOff.Travelers.Verdict.Airlock, arrestable, rules);
            Assert.AreEqual(8, conduct.Airlocks);
            Assert.AreEqual(0, conduct.InnocentAirlocks);
            Assert.AreEqual(texts.moraleStainedLine, TicketEnding.ConductLine(conduct, texts), "orders alone, however many");
            conduct.BeginDay();
            conduct.Judge(GameOff.Travelers.Verdict.Airlock, innocent, rules);
            Assert.AreEqual(1, conduct.InnocentAirlocks, "the run count survives the new day");
            Assert.AreEqual(texts.moraleBloodyLine, TicketEnding.ConductLine(conduct, texts));
            Object.DestroyImmediate(rules);
            Object.DestroyImmediate(texts);
        }
    }
}
