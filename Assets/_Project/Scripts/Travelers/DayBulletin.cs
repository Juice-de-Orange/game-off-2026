using System.Text;
using GameOff.Economy;

namespace GameOff.Travelers
{
    /// <summary>
    /// The text of the two notes on the counter's pinboard (P41), built from the day's rules and wanted list.
    /// The standing orders are written by hand next to the code that checks them (RuleEvaluator.StandingRules).
    /// </summary>
    public static class DayBulletin
    {
        public static string Rules(DayRules day)
        {
            var text = new StringBuilder("<b>STANDING ORDERS</b>\n");
            foreach (string rule in RuleEvaluator.StandingRules) text.Append("- ").Append(rule).Append('\n');
            text.Append("\n<b>TODAY</b>\n");
            int count = 0;
            foreach (var rule in day.Rules)
            {
                if (rule == null) continue;
                text.Append("- ").Append(rule.BulletinText).Append('\n');
                count++;
            }
            if (count == 0) text.Append("- No special rules.\n");
            return text.ToString();
        }

        public static string Wanted(DayRules day)
        {
            if (day.Wanted.Count == 0) return "No names today.";
            var text = new StringBuilder();
            foreach (var entry in day.Wanted)
            {
                text.Append("<b>").Append(entry.name).Append("</b>");
                if (!string.IsNullOrWhiteSpace(entry.id)) text.Append("  ID ").Append(entry.id);
                text.Append('\n').Append(Order(entry.order)).Append('\n');
                if (!string.IsNullOrWhiteSpace(entry.note)) text.Append(entry.note).Append('\n');
                text.Append('\n');
            }
            return text.ToString();
        }

        /// <summary>P68: one line per market event of today (Market.TodayEvents); empty on days without events.</summary>
        public static string Market()
        {
            var text = new StringBuilder();
            foreach (var e in GameOff.Economy.Market.TodayEvents)
                if (!string.IsNullOrWhiteSpace(e.bulletinLine)) text.Append("- ").Append(e.bulletinLine).Append("\n\n");
            return text.ToString().TrimEnd();
        }

        public static string Order(WantedOrder order) => order == WantedOrder.Terminate ? "TERMINATE: AIRLOCK" : "ARREST";
    }
}
