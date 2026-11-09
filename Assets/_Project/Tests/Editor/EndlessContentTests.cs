using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Hangar;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P126: what the days after the story have on top of the drawn travelers.</summary>
    public class EndlessContentTests
    {
        private static DaySchedule Schedule() => AssetDatabase.LoadAssetAtPath<DaySchedule>("Assets/_Project/Data/Days/DaySchedule.asset");

        [Test]
        public void FlawedTravelers_HaveAnAnswerAboutTheirFlaw()
        {
            var schedule = Schedule();
            var flawed = new List<TravelerData>();
            for (int day = schedule.Count + 1; day <= schedule.Count + 8; day++)
                flawed.AddRange(schedule.Get(day, 11).rules.Travelers.Where(t => t.IntendedVerdict != Verdict.Admit));
            Assert.IsNotEmpty(flawed);
            Assert.IsTrue(flawed.Any(t => t.Answers.Count > 0), "no flawed traveler has an authored answer");
            Assert.IsEmpty(schedule.Get(schedule.Count + 1, 11).rules.Travelers.Where(t => t.IntendedVerdict == Verdict.Admit && t.Answers.Count > 0), "clean travelers have nothing to explain");
        }

        [Test]
        public void Pool_HasTwentyLetters() => Assert.GreaterOrEqual(Schedule().Loop.letters.Count, 20);

        [Test]
        public void LoopCrate_OnlyEveryThirdDay_AndOwnerHasCleanPapers()
        {
            var tuning = ScriptableObject.CreateInstance<CargoCaseTuning>();
            tuning.loopGoods.Add(ScriptableObject.CreateInstance<ItemData>());
            var clean = TravelerData.CreateGenerated("A B", "Human", new List<GameOff.Documents.DocumentData>(), Verdict.Admit, "", "", null, false);
            var bad = TravelerData.CreateGenerated("C D", "Human", new List<GameOff.Documents.DocumentData>(), Verdict.Deny, "", "", null, false);
            var travelers = new[] { bad, clean };

            Assert.IsNull(tuning.ForLoopDay(13, travelers));
            var crate = tuning.ForLoopDay(15, travelers);
            Assert.AreEqual("A B", crate.traveler);
            Assert.AreEqual(crate.traveler, tuning.ForLoopDay(15, travelers).traveler);
            Assert.IsNull(tuning.ForLoopDay(15, new[] { bad }), "nobody with clean papers, no crate");
        }

        [Test]
        public void ShippedPool_HasGoodsForTheLoopCrates()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<CargoCaseTuning>("Assets/_Project/Data/Economy/CargoCases.asset");
            Assert.IsNotEmpty(tuning.loopGoods);
            Assert.IsTrue(tuning.loopGoods.All(i => i != null));
        }
    }
}
