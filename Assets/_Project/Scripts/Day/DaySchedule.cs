using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Day
{
    /// <summary>The days of one run in order (P09; real content for 3 days comes with P15). Edit in the inspector.</summary>
    [CreateAssetMenu(menuName = "GameOff/Day Schedule", fileName = "DaySchedule")]
    public class DaySchedule : ScriptableObject
    {
        [SerializeField] private List<DayEntry> days = new List<DayEntry>();

        public int Count => days.Count;
        public IReadOnlyList<DayEntry> Days => days;

        /// <summary>Day 1..Count.</summary>
        public DayEntry Get(int day) => days[Mathf.Clamp(day - 1, 0, days.Count - 1)];
    }
}
