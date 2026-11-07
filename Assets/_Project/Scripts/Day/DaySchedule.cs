using System;
using System.Collections.Generic;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Day
{
    /// <summary>
    /// The days of one run in order (P09): the written story days, then endless days drawn from a pool (P73). Edit in the inspector.
    /// </summary>
    [CreateAssetMenu(menuName = "GameOff/Day Schedule", fileName = "DaySchedule")]
    public class DaySchedule : ScriptableObject
    {
        [SerializeField] private List<DayEntry> days = new List<DayEntry>();
        [Tooltip("P73: after the last written day, every day is drawn from this pool. Empty = the run ends after the last day.")]
        [SerializeField] private DayPool loop;

        [NonSerialized] private Dictionary<int, DayEntry> _generated;
        [NonSerialized] private int _generatedSeed;

        /// <summary>Number of written (story) days.</summary>
        public int Count => days.Count;
        public IReadOnlyList<DayEntry> Days => days;
        public DayPool Loop => loop;
        /// <summary>Days go on after the written ones (P73).</summary>
        public bool Endless => loop != null && days.Count > 0;

        /// <summary>Day 1..Count; with a loop pool also every later day (seed 0).</summary>
        public DayEntry Get(int day) => Get(day, 0);

        /// <summary>
        /// Day n of a run: a written day, or (with a loop pool) the day drawn for this run's seed. Drawn days are kept for the
        /// current seed, so the barkeeper's look at tomorrow and the day itself are the same day.
        /// </summary>
        public DayEntry Get(int day, int seed)
        {
            if (!Endless || day <= days.Count) return days[Mathf.Clamp(day - 1, 0, days.Count - 1)];
            if (_generated == null || _generatedSeed != seed)
            {
                _generated = new Dictionary<int, DayEntry>();
                _generatedSeed = seed;
            }
            if (!_generated.TryGetValue(day, out var entry))
            {
                entry = DayGenerator.Generate(loop, day, days.Count + 1, FirstLoopDate(), seed);
                _generated[day] = entry;
            }
            return entry;
        }

        /// <summary>The date of the first drawn day: the day after the last written one.</summary>
        public DateTime FirstLoopDate()
        {
            var last = days[days.Count - 1].rules;
            if (last != null && DayRules.TryParseDate(last.Date, out var date)) return date.AddDays(1);
            return new DateTime(2290, 11, 5).AddDays(days.Count);
        }
    }
}
