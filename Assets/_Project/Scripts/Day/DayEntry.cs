using System;
using GameOff.Travelers;
using UnityEngine;

namespace GameOff.Day
{
    /// <summary>One day of the run (P09): the counter rules plus what the player reads in the morning.</summary>
    [Serializable]
    public class DayEntry
    {
        [Tooltip("Travelers, rules and wanted list for this day's shift (CounterShift.BeginDay).")]
        public DayRules rules;
        public string letterTitle = "A letter from home";
        [TextArea(4, 10)] public string letter;
        [Tooltip("One line under the letter, e.g. \"New today: no entry from Vesk.\" The full bulletin hangs at the counter (P41). Empty = no line.")]
        public string newToday;
    }
}
