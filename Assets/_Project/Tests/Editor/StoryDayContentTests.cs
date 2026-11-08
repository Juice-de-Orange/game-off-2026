using GameOff.Day;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;

namespace GameOff.Tests
{
    /// <summary>P15: the authored story days are free of placeholders, and the morning letter follows the family's state.</summary>
    public class StoryDayContentTests
    {
        private const int AuthoredDays = 4;

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
