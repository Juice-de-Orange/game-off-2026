using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;

namespace GameOff.Tests
{
    /// <summary>P73: endless days after the story, drawn from the day pool on the project's real data.</summary>
    public class EndlessDayTests
    {
        private const int Checked = 30;
        private static readonly int[] Seeds = { 1, 42, 7301, 123457, 2026 };

        private static DaySchedule Schedule()
        {
            var s = AssetDatabase.LoadAssetAtPath<DaySchedule>("Assets/_Project/Data/Days/DaySchedule.asset");
            Assert.IsNotNull(s, "DaySchedule missing");
            Assert.IsTrue(s.Endless, "DaySchedule has no loop pool - run GameOff/Build Day Pool");
            return s;
        }

        private static DayEntry Draw(DaySchedule s, int day, int seed) =>
            DayGenerator.Generate(s.Loop, day, s.Count + 1, s.FirstLoopDate(), seed);

        [Test]
        public void DrawnDaysPassTheValidator()
        {
            var s = Schedule();
            var problems = new List<string>();
            foreach (int seed in Seeds)
                for (int d = s.Count + 1; d <= s.Count + Checked; d++)
                {
                    var e = Draw(s, d, seed);
                    foreach (var p in TravelerValidator.Validate(e.rules)) problems.Add($"seed {seed} day {d}: {p}");
                    if (e.rules.Travelers.Count < s.Loop.travelersStart) problems.Add($"seed {seed} day {d}: only {e.rules.Travelers.Count} travelers");
                    if (e.bribe.Active && e.bribe.traveler.IntendedVerdict == Verdict.Admit) problems.Add($"seed {seed} day {d}: bribe for an admitted traveler");
                    if (string.IsNullOrWhiteSpace(e.letter)) problems.Add($"seed {seed} day {d}: no letter");
                }
            Assert.IsEmpty(problems, string.Join("\n", problems.Take(20)));
        }

        [Test]
        public void SameSeedSameDay_OtherSeedOrDayDiffers()
        {
            var s = Schedule();
            int d = s.Count + 3;
            string Sig(DayEntry e) => string.Join("|", e.rules.Rules.Select(r => r.BulletinText)) + "#" +
                                      string.Join("|", e.rules.Travelers.Select(t => t.DisplayName + ":" + t.IntendedVerdict));
            Assert.AreEqual(Sig(Draw(s, d, 42)), Sig(Draw(s, d, 42)), "same seed and day must give the same day");
            Assert.AreNotEqual(Sig(Draw(s, d, 42)), Sig(Draw(s, d, 43)), "another seed gives another day");
            Assert.AreNotEqual(Sig(Draw(s, d, 42)), Sig(Draw(s, d + 1, 42)), "the next day is another day");
        }

        [Test]
        public void DatesFollowTheStory()
        {
            var s = Schedule();
            DayRules.TryParseDate(s.Get(s.Count).rules.Date, out var last);
            for (int i = 1; i <= 5; i++)
            {
                DayRules.TryParseDate(s.Get(s.Count + i, 9).rules.Date, out var date);
                Assert.AreEqual(last.AddDays(i), date, $"day {s.Count + i}");
            }
        }

        [Test]
        public void DaysGetHarder()
        {
            var s = Schedule();
            int first = s.Count + 1, late = first + s.Loop.rampDays + 2;
            double Avg(int day, System.Func<DayEntry, int> f) => Seeds.Average(seed => f(Draw(s, day, seed)));
            Assert.AreEqual(s.Loop.travelersStart, Avg(first, e => e.rules.Travelers.Count), 0.01);
            Assert.AreEqual(s.Loop.travelersMax, Avg(late, e => e.rules.Travelers.Count), 0.01);
            Assert.Greater(Avg(late, e => e.rules.Rules.Count), Avg(first, e => e.rules.Rules.Count));
        }

        [Test]
        public void WrittenDaysStayAsTheyAre()
        {
            var s = Schedule();
            for (int d = 1; d <= s.Count; d++)
                Assert.AreSame(s.Days[d - 1], s.Get(d, 77), $"day {d}");
        }
    }
}
