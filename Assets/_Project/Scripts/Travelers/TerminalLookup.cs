using System;
using System.Collections.Generic;
using System.Text;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>
    /// P34: the counter terminal's checks, kept apart from the screen so the tests can run them. No truth of its own: the wanted list is the
    /// day's (DayRules.Wanted, incl. the recurring characters' entries) matched exactly like RuleEvaluator does it (WantedEntry.MatchesName /
    /// MatchesId); the entry record is what the ID card says, expiry against the day's date like the standing orders.
    /// P112: only the ID card's name and number can be looked up, and only the card shows in the record: comparing the papers with each
    /// other stays the player's job, or the scanner's (P16).
    /// A later check (a gadget) is one more method here plus a row on the screen.
    /// </summary>
    public static class TerminalLookup
    {
        public enum Lane { Closed, Open, Over }

        public const string Green = "#9BE05A", Red = "#E5484D";

        /// <summary>The name and ID number on the traveler's ID card, as the terminal offers them to click; empty without a card.</summary>
        public static List<TerminalQuery> Queries(TravelerData traveler)
        {
            var list = new List<TerminalQuery>();
            var card = traveler != null ? traveler.Document(DocumentType.IdCard) : null;
            if (card == null) return list;
            Add(card.Get("name"), TerminalQuery.Kind.Name, list);
            Add(card.Get("id"), TerminalQuery.Kind.Id, list);
            return list;
        }

        private static void Add(string value, TerminalQuery.Kind kind, List<TerminalQuery> list)
        {
            if (!string.IsNullOrWhiteSpace(value)) list.Add(new TerminalQuery(kind, value.Trim()));
        }

        /// <summary>Looks one value up in today's wanted list and in the traveler's entry record.</summary>
        public static TerminalResult Check(TerminalQuery query, TravelerData traveler, DayRules day)
        {
            var wanted = new List<WantedEntry>();
            if (day != null)
                foreach (var w in day.Wanted)
                    if (query.Field == TerminalQuery.Kind.Name ? w.MatchesName(query.Value) : w.MatchesId(query.Value)) wanted.Add(w);

            var record = new List<string>();
            var card = traveler != null ? traveler.Document(DocumentType.IdCard) : null;
            if (card != null && TextMatch.Same(card.Get(query.Field == TerminalQuery.Kind.Name ? "name" : "id"), query.Value))
            {
                DateTime today = default;
                bool hasToday = day != null && DayRules.TryParseDate(day.Date, out today);
                record.Add(RecordLine(card, hasToday, today));
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
            return $"LANE 2 {state} · {served}/{total} · {clock}";
        }
    }
}
