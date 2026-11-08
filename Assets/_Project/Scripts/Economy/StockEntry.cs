using System;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>One item a vendor sells and how many of it are on the shelf every morning (P11).</summary>
    [Serializable]
    public class StockEntry
    {
        public ItemData item;
        [Min(1)] public int dailyAmount = 3;
        [Tooltip("On sale from this day (P62: UV lamp from day 4, scanner from day 6). 1 = always.")]
        [Min(1)] public int fromDay = 1;
    }
}
