using System.Collections.Generic;

namespace GameOff.Travelers
{
    /// <summary>P34: what the counter terminal found for one query: the wanted entries it hits and the papers that carry the value.</summary>
    public class TerminalResult
    {
        public TerminalQuery Query { get; }
        public IReadOnlyList<WantedEntry> Wanted { get; }
        /// <summary>The entry record: the ID card's line (name, ID, validity) if its name or number is the value, else empty (P112).</summary>
        public IReadOnlyList<string> Record { get; }

        public bool IsWanted => Wanted.Count > 0;

        public TerminalResult(TerminalQuery query, IReadOnlyList<WantedEntry> wanted, IReadOnlyList<string> record)
        {
            Query = query;
            Wanted = wanted;
            Record = record;
        }
    }
}
