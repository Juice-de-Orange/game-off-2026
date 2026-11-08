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

        [TestCase(0, "clean")]
        [TestCase(-1, "stained")]
        [TestCase(-4, "stained")]
        [TestCase(-5, "bloody")]
        public void MoraleLine_FollowsTheThresholds(int morale, string expected)
        {
            var texts = ScriptableObject.CreateInstance<DepartureTuning>();
            texts.moraleCleanLine = "clean";
            texts.moraleStainedLine = "stained";
            texts.moraleBloodyLine = "bloody";
            Assert.AreEqual(expected, TicketEnding.MoraleLine(morale, texts));
            Object.DestroyImmediate(texts);
        }
    }
}
