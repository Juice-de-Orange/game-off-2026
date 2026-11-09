using System;
using System.Collections.Generic;
using System.Text;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>
    /// P34: the counter terminal's checks, kept apart from the screen so the tests can run them. No truth of its own: the wanted list is the
    /// day's (DayRules.Wanted, incl. the recurring characters' entries) matched exactly like RuleEvaluator does it (WantedEntry.MatchesName /
    /// MatchesId); the entry record is what the traveler's papers say, expiry against the day's date like the standing orders.
    /// A later check (a gadget) is one more method here plus a row on the screen.
    /// </summary>
    public static class TerminalLookup
    {
        public enum Lane { Closed, Open, Over }

        public const string Green = "#9BE05A", Red = "#E5484D";

        /// <summary>Every distinct name and ID number on the traveler's papers (names first), as the terminal offers them to click.</summary>
        public static List<TerminalQuery> Queries(TravelerData traveler)
        {
            var list = new List<TerminalQuery>();
            if (traveler == null) return list;
            Collect(traveler, TerminalQuery.Kind.Name, "name", list);
            Collect(traveler, TerminalQuery.Kind.Id, "id", list);
            return list;
        }

        private static void Collect(TravelerData traveler, TerminalQuery.Kind kind, string key, List<TerminalQuery> list)
        {
            foreach (var doc in traveler.Documents)
            {
                string value = doc != null ? doc.Get(key) : null;
                if (string.IsNullOrWhiteSpace(value)) continue;
                bool known = false;
                foreach (var q in list)
                    if (q.Field == kind && TextMatch.Same(q.Value, value)) known = true;
                if (!known) list.Add(new TerminalQuery(kind, value.Trim()));
            }
        }

        /// <summary>Looks one value up in today's wanted list and in the traveler's entry record.</summary>
        public static TerminalResult Check(TerminalQuery query, TravelerData traveler, DayRules day)
        {
            var wanted = new List<WantedEntry>();
            if (day != null)
                foreach (var w in day.Wanted)
                    if (query.Field == TerminalQuery.Kind.Name ? w.MatchesName(query.Value) : w.MatchesId(query.Value)) wanted.Add(w);

            var record = new List<string>();
            if (traveler != null)
            {
                DateTime today = default;
                bool hasToday = day != null && DayRules.TryParseDate(day.Date, out today);
                string key = query.Field == TerminalQuery.Kind.Name ? "name" : "id";
                foreach (var doc in traveler.Documents)
                    if (doc != null && TextMatch.Same(doc.Get(key), query.Value)) record.Add(RecordLine(doc, hasToday, today));
            }
            return new TerminalResult(query, wanted, record);
        }

        private static string RecordLine(DocumentData doc, bool hasToday, DateTime today)
        {
            var line = new StringBuilder(DocumentNames.Of(doc.Type).ToUpperInvariant());
            Append(line, doc.Get("name"));
            Append(line, doc.Get("id"));
            Append(line, doc.Get("species"));
            Append(line, doc.Get("origin"));
            string expires = doc.Get("expires");
            if (!string.IsNullOrWhiteSpace(expires))
            {
                line.Append(" · VALID ").Append(expires.Trim());
                if (hasToday && DayRules.TryParseDate(expires, out var until) && until < today)
                    line.Append(" <color=").Append(Red).Append(">EXPIRED</color>");
            }
            return line.ToString();
        }

        private static void Append(StringBuilder line, string value)
        {
            if (!string.IsNullOrWhiteSpace(value)) line.Append(" · ").Append(value.Trim());
        }

        /// <summary>The result as the terminal prints it (TextMeshPro rich text, monospace font).</summary>
        public static string Render(TerminalResult result)
        {
            var text = new StringBuilder();
            text.Append("> QUERY ").Append(result.Query.Label).Append(' ').Append(result.Query.Value).Append("\n\n");
            text.Append("WANTED LIST ......... ");
            if (!result.IsWanted) text.Append("<color=").Append(Green).Append(">NO MATCH</color>\n");
            else
            {
                text.Append("<color=").Append(Red).Append("><b>MATCH</b></color>\n");
                foreach (var w in result.Wanted)
                {
                    text.Append("<color=").Append(Red).Append(">  ").Append(w.name);
                    if (!string.IsNullOrWhiteSpace(w.id)) text.Append("  ID ").Append(w.id);
                    text.Append("\n  ORDER: ").Append(DayBulletin.Order(w.order)).Append("</color>\n");
                    if (!string.IsNullOrWhiteSpace(w.note)) text.Append("  ").Append(w.note.Trim()).Append('\n');
                }
            }
            text.Append("\nENTRY RECORD\n");
            if (result.Record.Count == 0) text.Append("  NO RECORD\n");
            foreach (string line in result.Record) text.Append("  ").Append(line).Append('\n');
            return text.ToString();
        }

        /// <summary>The terminal's status line: the counter lane, travelers dealt with today, the station clock.</summary>
        public static string StatusLine(Lane lane, bool inspecting, int served, int total, string clock)
        {
            string state = lane == Lane.Closed ? "CLOSED" : lane == Lane.Over ? "SHIFT OVER" : inspecting ? "OPEN · INSPECTION" : "OPEN · NEXT";
            return $"LANE 1 {state} · {served}/{total} · {clock}";
        }
    }
}
