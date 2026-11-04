using System;
using System.Collections.Generic;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>Nobody from this origin may enter → deny.</summary>
    [Serializable]
    public class BannedOriginRule : DayRule
    {
        public string origin;

        public BannedOriginRule() { }
        public BannedOriginRule(string origin) => this.origin = origin;

        public override void Check(TravelerData traveler, DayRules day, List<Violation> violations)
        {
            var id = traveler.Document(DocumentType.IdCard);
            string from = id != null ? id.Get("origin") : null;
            if (TextMatch.Same(from, origin))
                violations.Add(new Violation(Verdict.Deny, $"Entry banned for travelers from {origin}"));
        }

        public override string BulletinText => $"No entry for travelers from {origin}.";
        public override string Problem => string.IsNullOrWhiteSpace(origin) ? "BannedOriginRule without origin" : null;
    }
}
