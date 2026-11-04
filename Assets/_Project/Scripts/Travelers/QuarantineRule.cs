using System;
using System.Collections.Generic;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>Outbreak at a station: anyone whose route passed through it must go out the airlock (mandatory).</summary>
    [Serializable]
    public class QuarantineRule : DayRule
    {
        public string station;

        public QuarantineRule() { }
        public QuarantineRule(string station) => this.station = station;

        public override void Check(TravelerData traveler, DayRules day, List<Violation> violations)
        {
            var permit = traveler.Document(DocumentType.EntryPermit);
            if (permit != null && TextMatch.RouteHasStop(permit.Get("route"), station))
                violations.Add(new Violation(Verdict.Airlock, $"Quarantine: route via {station}"));
        }

        public override string BulletinText => $"OUTBREAK at {station}. Anyone who passed through it goes out the airlock. No exceptions.";
        public override string Problem => string.IsNullOrWhiteSpace(station) ? "QuarantineRule without station" : null;
    }
}
