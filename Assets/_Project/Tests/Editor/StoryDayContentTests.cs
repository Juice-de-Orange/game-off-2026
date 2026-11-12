using GameOff.Day;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;

namespace GameOff.Tests
{
    /// <summary>P15: the authored story days are free of placeholders, and the morning letter follows the family's state.</summary>
    public class StoryDayContentTests
    {
        private const int AuthoredDays = 12;

        private static DaySchedule Schedule()
        {
            var schedule = AssetDatabase.LoadAssetAtPath<DaySchedule>("Assets/_Project/Data/Days/DaySchedule.asset");
            Assert.IsNotNull(schedule, "no DaySchedule");
            return schedule;
        }

        [Test]
        public void AuthoredDays_HaveNoPlaceholderTravelers()
        {
            var schedule = Schedule();
            for (int d = 1; d <= AuthoredDays; d++)
                foreach (TravelerData t in schedule.Get(d).rules.Travelers)
                    StringAssert.Contains("/Travelers/Normal/", AssetDatabase.GetAssetPath(t), $"day {d}: {t.name}");
        }

        /// <summary>
        /// Six encounters a day on the intended path (ordinary travelers plus the characters who come), as the wage balance and the
        /// Economy Report assume (StoryCharacterBuilder warns otherwise). P150: the Shifter's accomplice counts on days 6 and 7.
        /// </summary>
        [Test]
        public void AuthoredDays_SixEncountersOnTheIntendedPath()
        {
            var schedule = Schedule();
            var chars = new System.Collections.Generic.List<GameOff.Characters.RecurringCharacter>();
            foreach (var g in AssetDatabase.FindAssets("t:RecurringCharacter", new[] { "Assets/_Project/Data/Characters/Story" }))
                chars.Add(AssetDatabase.LoadAssetAtPath<GameOff.Characters.RecurringCharacter>(AssetDatabase.GUIDToAssetPath(g)));
            var memory = new GameOff.Characters.CharacterMemory();
            for (int d = 1; d <= AuthoredDays; d++)
            {
                var due = GameOff.Characters.CharacterPlanner.Due(d, chars, memory, 3, () => 0f);
                foreach (var p in due)
                {
                    var verdict = p.traveler.IntendedVerdict;
                    memory.Decide(p.character, p.appearance, d, p.character.Appearances[p.appearance].OutcomeOf(verdict), verdict);
                }
                Assert.AreEqual(6, schedule.Get(d).rules.Travelers.Count + due.Count, $"day {d}: {due.Count} characters");
            }
        }

        [Test]
        public void AuthoredDays_HaveNoPlaceholderLetters()
        {
            var schedule = Schedule();
            for (int d = 1; d <= AuthoredDays; d++)
            {
                var entry = schedule.Get(d);
                StringAssert.DoesNotContain("placeholder", entry.letter.ToLowerInvariant(), $"day {d}");
                if (d == 1) continue;
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.letterIfTrouble), $"day {d} has no letter for a family in trouble");
                StringAssert.DoesNotContain("placeholder", entry.letterIfTrouble.ToLowerInvariant(), $"day {d}");
            }
        }

        [Test]
        public void LetterFor_UsesTroubleVersionOnlyWhenThereIsOne()
        {
            var entry = new DayEntry { letter = "fine", letterIfTrouble = "sick" };
            Assert.AreEqual("fine", entry.LetterFor(false));
            Assert.AreEqual("sick", entry.LetterFor(true));

            var single = new DayEntry { letter = "fine" };
            Assert.AreEqual("fine", single.LetterFor(true));
        }
    }
}
