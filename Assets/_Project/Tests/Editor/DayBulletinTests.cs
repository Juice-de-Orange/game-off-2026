using System.Collections.Generic;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>The pinboard notes (P41) must name every rule and wanted person of a day: the player can't obey what isn't posted.</summary>
    public class DayBulletinTests
    {
        private static IEnumerable<DayRules> AllDays()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:DayRules"))
                yield return AssetDatabase.LoadAssetAtPath<DayRules>(AssetDatabase.GUIDToAssetPath(guid));
        }

        [Test]
        public void AllDaysInProject_PostEveryRuleAndName()
        {
            int days = 0;
            foreach (var day in AllDays())
            {
                days++;
                string rules = DayBulletin.Rules(day), wanted = DayBulletin.Wanted(day);
                foreach (string standing in RuleEvaluator.StandingRules) StringAssert.Contains(standing, rules, day.name);
                foreach (var rule in day.Rules)
                {
                    Assert.IsNotNull(rule, $"{day.name} has an empty rule slot");
                    StringAssert.Contains(rule.BulletinText, rules, day.name);
                }
                foreach (var entry in day.Wanted)
                {
                    StringAssert.Contains(entry.name, wanted, day.name);
                    StringAssert.Contains(DayBulletin.Order(entry.order), wanted, day.name);
                }
            }
            Assert.Greater(days, 0, "no days found");
        }

        [Test]
        public void EmptyDay_SaysSo()
        {
            var day = ScriptableObject.CreateInstance<DayRules>();
            try
            {
                StringAssert.Contains("No special rules", DayBulletin.Rules(day));
                StringAssert.Contains("No names", DayBulletin.Wanted(day));
            }
            finally
            {
                Object.DestroyImmediate(day);
            }
        }

        [Test]
        public void Orders_NameTheRightAction()
        {
            StringAssert.Contains("ARREST", DayBulletin.Order(WantedOrder.Arrest));
            StringAssert.Contains("AIRLOCK", DayBulletin.Order(WantedOrder.Terminate));
        }
    }
}
