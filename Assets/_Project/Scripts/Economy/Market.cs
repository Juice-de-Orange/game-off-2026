using System.Collections.Generic;
using UnityEngine;

namespace GameOff.Economy
{
    /// <summary>
    /// The market of the day (P70 + P68, DECISIONS.md 06.11.). Everything multiplies, rounding only at the end:
    ///   market price = base × (1 + the item's fixed change for the day) × base factor of every label × the day's events per label
    ///   vendor price = market price × sell factor · buy price = market price × buy factor × events' buy factor · bed = market price × 1.35
    ///   shelf amount = daily amount × events' amount factor (rounded up, 0 = not there today) ± the events' assortment changes
    /// DayManager calls Begin every morning before the counter pins its notes; without a running game it is day 1 without events.
    /// </summary>
    public static class Market
    {
        private static readonly List<MarketEvent> Events = new List<MarketEvent>();

        public static int Day { get; set; } = 1;
        public static IReadOnlyList<MarketEvent> TodayEvents => Events;

        // P75: events triggered by recurring characters for a later day, added to that day's fixed events.
        private static readonly Dictionary<int, List<MarketEvent>> Scheduled = new Dictionary<int, List<MarketEvent>>();

        public static void Begin(int day, IEnumerable<MarketEvent> events)
        {
            Day = day;
            Events.Clear();
            if (day <= 1) Scheduled.Clear(); // a new run
            if (events != null)
                foreach (var e in events)
                    if (e != null && !Events.Contains(e)) Events.Add(e);
            if (Scheduled.TryGetValue(day, out var extra))
                foreach (var e in extra)
                    if (e != null && !Events.Contains(e)) Events.Add(e);
        }

        /// <summary>P75: an event happens on that day too (pinboard, announcement, rumour the evening before, station mood).</summary>
        public static void Schedule(int day, MarketEvent e)
        {
            if (e == null) return;
            if (!Scheduled.TryGetValue(day, out var list)) Scheduled[day] = list = new List<MarketEvent>();
            if (!list.Contains(e)) list.Add(e);
        }

        /// <summary>P128: every day something is scheduled for (the save keeps them).</summary>
        public static IEnumerable<int> ScheduledDays => Scheduled.Keys;

        /// <summary>P75: what is scheduled for that day so far (the barkeeper's rumour looks at tomorrow).</summary>
        public static IReadOnlyList<MarketEvent> ScheduledFor(int day) =>
            Scheduled.TryGetValue(day, out var list) ? list : (IReadOnlyList<MarketEvent>)System.Array.Empty<MarketEvent>();

        /// <summary>Unrounded market price today – round only at the end (vendor or bed price).</summary>
        public static float PriceOf(ItemData item)
        {
            if (item == null) return 0f;
            float price = item.BaseValue * (1f + item.DailyChange(Day));
            foreach (var tag in item.Tags)
                if (tag != null) price *= tag.PriceFactor;
            return price * EventFactor(item, e => e.price);
        }

        /// <summary>Extra factor on what vendors pay today (events only; the vendor's own buy factor comes on top).</summary>
        public static float BuyFactor(ItemData item) => EventFactor(item, e => e.buy);

        /// <summary>Today's shelf amount for a stock entry: daily amount × events, rounded up; 0 only if an event takes it away.</summary>
        public static int Amount(ItemData item, int dailyAmount)
        {
            float factor = EventFactor(item, e => e.amount);
            if (factor <= 0f || dailyAmount <= 0) return 0;
            return Mathf.Max(1, Mathf.CeilToInt(dailyAmount * factor - 0.0001f));
        }

        /// <summary>The events' assortment changes for one vendor today.</summary>
        public static IEnumerable<StockChange> StockChanges(VendorStock stock)
        {
            foreach (var e in Events)
                foreach (var change in e.stock)
                    if (change != null && change.stock == stock && change.item != null) yield return change;
        }

        /// <summary>Half rounds up (6.5 → 7), the way players expect it (P71).</summary>
        public static int Rounded(float price) => Mathf.Max(0, (int)System.Math.Round(price + 0.0001f, System.MidpointRounding.AwayFromZero)); // tiny nudge: 13.4999 from float math is 13.5

        private static float EventFactor(ItemData item, System.Func<LabelEffect, float> pick)
        {
            float factor = 1f;
            if (item == null) return factor;
            foreach (var e in Events)
                foreach (var effect in e.labels)
                    if (effect != null && item.HasTag(effect.tag)) factor *= pick(effect);
            return factor;
        }
    }
}
