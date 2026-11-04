using System;
using System.Collections.Generic;
using GameOff.Documents;

namespace GameOff.Travelers
{
    /// <summary>
    /// Checks content before it reaches the game (P07): every traveler's computed decision must equal the author's intended
    /// one, papers must be complete and well-formed, rules must have parameters, and near misses (a rule that almost but not
    /// quite matches, e.g. "STIMULANT" vs "STIMULANTS") are reported because they usually mean the author expected a match.
    /// Used by the menu GameOff/Validate Travelers and by the EditMode tests.
    /// </summary>
    public static class TravelerValidator
    {
        private static readonly string[] KnownSpecies = { "Human", "Vellani", "Quobb", "Xirrik", "Sorvak" };

        /// <summary>Fields every document of a type must have (keys of the "Field_" texts in its prefab).</summary>
        public static readonly IReadOnlyDictionary<DocumentType, string[]> RequiredFields = new Dictionary<DocumentType, string[]>
        {
            { DocumentType.IdCard, new[] { "name", "species", "origin", "id", "expires" } },
            { DocumentType.EntryPermit, new[] { "name", "id", "purpose", "duration", "expires", "issued", "route" } },
            { DocumentType.CargoManifest, new[] { "name", "ship", "origin", "destination", "cargo" } },
        };

        public static List<string> Validate(DayRules day)
        {
            var problems = new List<string>();
            string d = day != null ? day.name : "(null day)";
            if (day == null) { problems.Add("Day is null"); return problems; }
            if (!DayRules.TryParseDate(day.Date, out _)) problems.Add($"{d}: date \"{day.Date}\" is not {DayRules.DateFormat}");
            if (day.Travelers.Count == 0) problems.Add($"{d}: no travelers");
            foreach (var rule in day.Rules)
            {
                if (rule == null) problems.Add($"{d}: empty rule slot");
                else if (rule.Problem != null) problems.Add($"{d}: {rule.Problem}");
            }
            foreach (var w in day.Wanted)
                if (string.IsNullOrWhiteSpace(w.name) && string.IsNullOrWhiteSpace(w.id)) problems.Add($"{d}: wanted entry without name and ID");

            foreach (var t in day.Travelers)
            {
                if (t == null) { problems.Add($"{d}: empty traveler slot"); continue; }
                string who = $"{d}/{t.name}";
                if (string.IsNullOrWhiteSpace(t.DisplayName)) problems.Add($"{who}: no name");
                if (Array.FindIndex(KnownSpecies, s => TextMatch.Same(s, t.Species)) < 0) problems.Add($"{who}: unknown species \"{t.Species}\"");
                CheckDocuments(t, who, problems);
                if (!t.TrickIntended) NearMisses(t, day, who, problems);

                var result = RuleEvaluator.Evaluate(t, day);
                if (result.Correct != t.IntendedVerdict)
                    problems.Add($"{who}: rules say {result.Correct} ({result.MainReason}) but the author intended {t.IntendedVerdict}" +
                                 (string.IsNullOrWhiteSpace(t.IntendedReason) ? "" : $" ({t.IntendedReason})"));
            }
            return problems;
        }

        private static void CheckDocuments(TravelerData t, string who, List<string> problems)
        {
            foreach (var doc in t.Documents)
            {
                if (doc == null) { problems.Add($"{who}: empty document slot"); continue; }
                if (doc.Type == DocumentType.Citation) { problems.Add($"{who}: a citation is not a traveler document"); continue; }
                if (RequiredFields.TryGetValue(doc.Type, out var keys))
                    foreach (var key in keys)
                        if (string.IsNullOrWhiteSpace(doc.Get(key))) problems.Add($"{who}: {DocumentNames.Of(doc.Type)} has no \"{key}\"");
                string expires = doc.Get("expires");
                if (!string.IsNullOrWhiteSpace(expires) && !DayRules.TryParseDate(expires, out _))
                    problems.Add($"{who}: {DocumentNames.Of(doc.Type)} expiry \"{expires}\" is not {DayRules.DateFormat}");
            }
        }

        /// <summary>Text that almost matches a rule is nearly always an authoring slip: report it.</summary>
        private static void NearMisses(TravelerData t, DayRules day, string who, List<string> problems)
        {
            var id = t.Document(DocumentType.IdCard);
            var permit = t.Document(DocumentType.EntryPermit);
            var manifest = t.Document(DocumentType.CargoManifest);
            foreach (var rule in day.Rules)
            {
                switch (rule)
                {
                    case ForbiddenGoodsRule goods when manifest != null:
                        string cargo = manifest.Get("cargo");
                        if (Loose(cargo, goods.keyword) && !TextMatch.ContainsWords(cargo, goods.keyword))
                            problems.Add($"{who}: cargo nearly matches forbidden \"{goods.keyword}\" but not as a whole word - intended?");
                        break;
                    case BannedOriginRule banned when id != null:
                        string origin = id.Get("origin");
                        if ((Loose(origin, banned.origin) || Loose(banned.origin, origin)) && !TextMatch.Same(origin, banned.origin))
                            problems.Add($"{who}: origin \"{origin}\" nearly matches banned \"{banned.origin}\" - intended?");
                        break;
                    case QuarantineRule quarantine when permit != null:
                        string route = permit.Get("route");
                        if (Loose(route, quarantine.station) && !TextMatch.RouteHasStop(route, quarantine.station))
                            problems.Add($"{who}: route nearly passes quarantined \"{quarantine.station}\" - intended?");
                        break;
                }
            }
            foreach (var w in day.Wanted)
                foreach (var doc in t.Documents)
                {
                    if (doc == null) continue;
                    string n = doc.Get("name");
                    if (Loose(n, w.name) && !TextMatch.Same(n, w.name))
                        problems.Add($"{who}: name \"{n}\" nearly matches wanted \"{w.name}\" - intended?");
                }
        }

        private static bool Loose(string text, string part) =>
            !string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(part)
            && text.IndexOf(part.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
