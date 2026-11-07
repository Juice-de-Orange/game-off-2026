using System.Linq;
using GameOff.Day;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P66: the bribe note's text, the barkeeper's sulking window, and that every bribe names a traveler of its day.</summary>
    public class BribeTests
    {
        [Test]
        public void Sulking_LastsTheRefusalDayAndTheNext()
        {
            var go = new GameObject("economy");
            try
            {
                var economy = go.AddComponent<PlayerEconomy>();
                Assert.IsFalse(economy.BarkeeperSulks(1), "never refused");
                economy.BarkeeperSulksFrom = 3;
                Assert.IsFalse(economy.BarkeeperSulks(2));
                Assert.IsTrue(economy.BarkeeperSulks(3));
                Assert.IsTrue(economy.BarkeeperSulks(4));
                Assert.IsFalse(economy.BarkeeperSulks(5));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void StandardNote_NamesTravelerAndAmount()
        {
            var traveler = ScriptableObject.CreateInstance<GameOff.Travelers.TravelerData>();
            try
            {
                var so = new SerializedObject(traveler);
                so.FindProperty("displayName").stringValue = "QUILL BARRAGH";
                so.ApplyModifiedPropertiesWithoutUndo();
                var bribe = new Bribe { traveler = traveler, amount = 60 };
                Assert.IsTrue(bribe.Active);
                StringAssert.Contains("QUILL BARRAGH", bribe.Text);
                StringAssert.Contains("60 CR", bribe.Text);
                Assert.IsFalse(new Bribe { traveler = traveler, amount = 0 }.Active);
            }
            finally { Object.DestroyImmediate(traveler); }
        }

        [Test]
        public void EveryBribeInProject_NamesATravelerOfItsDay()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:DaySchedule"))
            {
                var schedule = AssetDatabase.LoadAssetAtPath<DaySchedule>(AssetDatabase.GUIDToAssetPath(guid));
                for (int d = 1; d <= schedule.Count; d++)
                {
                    var entry = schedule.Get(d);
                    if (entry.bribe == null || !entry.bribe.Active) continue;
                    // P76: the traveler may also come as a recurring character's appearance on that day
                    bool character = false;
                    foreach (var g in AssetDatabase.FindAssets("t:RecurringCharacter"))
                        foreach (var ap in AssetDatabase.LoadAssetAtPath<GameOff.Characters.RecurringCharacter>(AssetDatabase.GUIDToAssetPath(g)).Appearances)
                            if (ap.traveler == entry.bribe.traveler && ap.InWindow(d)) character = true;
                    Assert.IsTrue(character || (entry.rules != null && entry.rules.Travelers.Contains(entry.bribe.traveler)),
                        $"{schedule.name} day {d}: {entry.bribe.traveler.DisplayName} is neither among the day's travelers nor a character appearance that day");
                }
            }
        }
    }
}
