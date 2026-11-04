using System;
using System.Collections.Generic;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>A document is mandatory (optionally only for one species) → deny if missing.</summary>
    [Serializable]
    public class RequireDocumentRule : DayRule
    {
        public DocumentType document;
        public string onlyForSpecies;

        public RequireDocumentRule() { }
        public RequireDocumentRule(DocumentType document, string onlyForSpecies = "")
        {
            this.document = document;
            this.onlyForSpecies = onlyForSpecies;
        }

        public override void Check(TravelerData traveler, DayRules day, List<Violation> violations)
        {
            if (!string.IsNullOrWhiteSpace(onlyForSpecies) && !TextMatch.Same(traveler.Species, onlyForSpecies)) return;
            if (traveler.Document(document) == null)
                violations.Add(new Violation(Verdict.Deny, $"Missing {DocumentNames.Of(document)}"));
        }

        public override string BulletinText => string.IsNullOrWhiteSpace(onlyForSpecies)
            ? $"All travelers need a {DocumentNames.Of(document)}."
            : $"{onlyForSpecies} travelers need a {DocumentNames.Of(document)}.";

        public override string Problem => null;
    }
}
