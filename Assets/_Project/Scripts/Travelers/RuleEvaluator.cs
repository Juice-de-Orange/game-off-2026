using System.Collections.Generic;
using System.Linq;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>
    /// Computes the correct decision for a traveler from their papers and the day (P07). Always-on base rules (papers agree,
    /// nothing expired, ID card and entry permit present, seals genuine), then the day's bulletin rules and wanted list.
    /// The most severe violation decides: mandatory airlock &gt; arrest &gt; deny &gt; admit.
    /// </summary>
    public static class RuleEvaluator
    {
        public static EvaluationResult Evaluate(TravelerData traveler, DayRules day)
        {
            var violations = new List<Violation>();
            CheckBaseRules(traveler, day, violations);
            foreach (var rule in day.Rules)
                rule?.Check(traveler, day, violations);
            CheckWanted(traveler, day, violations);

            // Most severe first; OrderBy is stable, so for equal severity the first-found reason stays first (same citation text every time)
            var sorted = violations.OrderByDescending(v => v.Requires).ToList();
            var correct = sorted.Count > 0 ? sorted[0].Requires : Verdict.Admit;
            return new EvaluationResult(correct, sorted);
        }

        private static void CheckBaseRules(TravelerData traveler, DayRules day, List<Violation> violations)
        {
            var id = traveler.Document(DocumentType.IdCard);
            if (id == null) violations.Add(new Violation(Verdict.Deny, "Missing ID card"));
            if (traveler.Document(DocumentType.EntryPermit) == null) violations.Add(new Violation(Verdict.Deny, "Missing entry permit"));

            // Every paper must name the same person (name and ID number as printed on the ID card)
            string name = id?.Get("name"), number = id?.Get("id");
            foreach (var doc in traveler.Documents)
            {
                if (doc == null || doc == id) continue;
                string docName = doc.Get("name"), docNumber = doc.Get("id");
                if (name != null && docName != null && !TextMatch.Same(name, docName))
                    violations.Add(new Violation(Verdict.Deny, $"Name on {DocumentNames.Of(doc.Type)} doesn't match ID card"));
                if (number != null && docNumber != null && !TextMatch.Same(number, docNumber))
                    violations.Add(new Violation(Verdict.Deny, $"ID number on {DocumentNames.Of(doc.Type)} doesn't match ID card"));
            }

            DayRules.TryParseDate(day.Date, out var today);
            foreach (var doc in traveler.Documents)
            {
                if (doc == null) continue;
                if (DayRules.TryParseDate(doc.Get("expires"), out var expires) && expires < today)
                    violations.Add(new Violation(Verdict.Deny, $"Expired {DocumentNames.Of(doc.Type)}"));
                if (doc.ForgedSeal)
                    violations.Add(new Violation(Verdict.Arrest, $"Forged seal on {DocumentNames.Of(doc.Type)}"));
            }
        }

        private static void CheckWanted(TravelerData traveler, DayRules day, List<Violation> violations)
        {
            foreach (var w in day.Wanted)
            {
                bool match = false;
                foreach (var doc in traveler.Documents)
                {
                    if (doc == null) continue;
                    if (TextMatch.Same(doc.Get("name"), w.name) || TextMatch.Same(doc.Get("id"), w.id)) match = true;
                }
                if (!match) continue;
                violations.Add(w.order == WantedOrder.Terminate
                    ? new Violation(Verdict.Airlock, $"Wanted, terminate on sight: {w.name}")
                    : new Violation(Verdict.Arrest, $"Wanted: {w.name}"));
            }
        }
    }
}
