using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// The market price of an item today (P70, foundation for P68): base market price × (1 + the item's fixed change for the day).
    /// P68 adds the label base factors and the day's events on top. Vendors (sell/buy factor) and the bed (× 1.35) start from this.
    /// The day is set by PlayerEconomy every morning; without a running game it stays on day 1.
    /// </summary>
    public static class Market
    {
        public static int Day { get; set; } = 1;

        /// <summary>Unrounded market price today – round only at the end (vendor or bed price).</summary>
        public static float PriceOf(ItemData item) => item == null ? 0f : item.BaseValue * (1f + item.DailyChange(Day));

        public static int Rounded(float price) => Mathf.Max(0, Mathf.RoundToInt(price));
    }
}
