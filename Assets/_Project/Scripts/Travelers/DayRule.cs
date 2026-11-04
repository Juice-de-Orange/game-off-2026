using System;
using System.Collections.Generic;

namespace GameOff.Travelers
{
    /// <summary>
    /// One rule from the day's bulletin. Subclasses are stored with [SerializeReference] in DayRules, so a day can mix any rules.
    /// Check adds a Violation if the traveler breaks the rule; BulletinText is what the pinboard shows (P41).
    /// </summary>
    [Serializable]
    public abstract class DayRule
    {
        public abstract void Check(TravelerData traveler, DayRules day, List<Violation> violations);
        public abstract string BulletinText { get; }
        /// <summary>A problem with the rule itself (missing parameter), or null. Used by the validator.</summary>
        public abstract string Problem { get; }
    }
}
