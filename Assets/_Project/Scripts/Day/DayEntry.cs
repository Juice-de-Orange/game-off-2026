using System;
using System.Collections.Generic;
using GameOff.Economy;
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
        [Tooltip("Fixed market events of this day (P68): prices, amounts, assortment; announced at the pinboard and over the loudspeaker.")]
        public List<MarketEvent> marketEvents = new List<MarketEvent>();
        [Tooltip("What the barkeeper wants today for a rumour about tomorrow (P65). Amount 0 = nothing.")]
        public BarWish barWish = new BarWish();
        [Tooltip("The barkeeper's bribe note at shift start (P66). No traveler = no note.")]
        public Bribe bribe = new Bribe();
    }
}
