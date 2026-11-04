using System;
using System.Collections.Generic;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>Goods containing this keyword are contraband → arrest if declared on the cargo manifest.</summary>
    [Serializable]
    public class ForbiddenGoodsRule : DayRule
    {
        public string keyword;

        public ForbiddenGoodsRule() { }
        public ForbiddenGoodsRule(string keyword) => this.keyword = keyword;

        public override void Check(TravelerData traveler, DayRules day, List<Violation> violations)
        {
            var manifest = traveler.Document(DocumentType.CargoManifest);
            if (manifest != null && TextMatch.ContainsWords(manifest.Get("cargo"), keyword))
                violations.Add(new Violation(Verdict.Arrest, $"Contraband declared: {keyword}"));
        }

        public override string BulletinText => $"Import of {keyword} is forbidden. Arrest smugglers.";
        public override string Problem => string.IsNullOrWhiteSpace(keyword) ? "ForbiddenGoodsRule without keyword" : null;
    }
}
