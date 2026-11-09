using System.Collections.Generic;

namespace GameOff.Travelers
{
    /// <summary>P34: what the counter terminal found for one query: the wanted entries it hits and the papers that carry the value.</summary>
    public class TerminalResult
    {
        public TerminalQuery Query { get; }
        public IReadOnlyList<WantedEntry> Wanted { get; }
        /// <summary>The entry record: one line per paper carrying the value (name, ID, validity), in the order handed over.</summary>
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
