using System;

namespace GameOff.Travelers
{
    /// <summary>One person on the day's wanted list. Matches by name or ID number (either is enough).</summary>
    [Serializable]
    public struct WantedEntry
    {
        public string name;
        public string id;
        public WantedOrder order;
        public string note;

        public WantedEntry(string name, string id, WantedOrder order, string note = "")
        {
            this.name = name;
            this.id = id;
            this.order = order;
            this.note = note;
        }

        /// <summary>The wanted list's matching (RuleEvaluator and the counter terminal, P34): same name, case and spaces ignored.</summary>
        public bool MatchesName(string value) => TextMatch.Same(value, name);

        /// <summary>Same ID number, case and spaces ignored.</summary>
        public bool MatchesId(string value) => TextMatch.Same(value, id);
    }
}
