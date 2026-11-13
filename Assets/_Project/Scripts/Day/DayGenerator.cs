using System;
using System.Collections.Generic;
using System.Linq;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEngine;
using Random = System.Random;

namespace GameOff.Day
{
    /// <summary>
    /// Draws one endless day after the story (P73) from a DayPool: the bulletin rules, the wanted list, travelers with papers that
    /// fit the day's date, a market event, a bar wish, maybe a bribe, and a letter. The same seed and day always give the same
    /// day. Every traveler is checked with RuleEvaluator while it is made: it is only kept if the rules give the verdict the
    /// generator meant, so the validator can't find a wrong one. Days get slowly harder (more rules, travelers, tricky flaws).
    /// </summary>
    public static class DayGenerator
    {
        public enum Flaw { Clean, ExpiredPermit, ExpiredId, NameMismatch, IdMismatch, MissingPermit, ForgedSeal, Contraband, BannedOrigin, Quarantine, MissingManifest, Wanted }

        private const string Destination = "KESTREL-9";
        private const int Attempts = 25;

        // salts: every part of a day has its own random stream, so e.g. yesterday's rules can be drawn again without its travelers
        private const int SaltRules = 1, SaltTravelers = 2, SaltBribe = 3, SaltEvent = 4, SaltLetter = 5, SaltWish = 6, SaltWanted = 7;

        /// <summary>Cargo words for the same kind of goods: a day that forbids one forbids all of them (like story day 10), so a
        /// pistol is never legal on a weapons day. Rules match whole words only, WEAPON alone would let PISTOL through.</summary>
        private static readonly string[][] ForbiddenTogether = { new[] { "WEAPON", "PISTOL" } };

        public static DayEntry Generate(DayPool pool, int day, int firstDay, DateTime firstDate, int seed)
        {
            var date = firstDate.AddDays(day - firstDay);
            var rules = Rules(pool, day, firstDay, seed);
            var wanted = Wanted(pool, day, firstDay, seed, out var wantedSpecies);
            var travelers = new List<TravelerData>();
            var dayRules = DayRules.CreateGenerated($"Day{day:00}_Generated", date.ToString(DayRules.DateFormat), rules,
                pool.standingWanted.Concat(wanted).ToList(), travelers);
            travelers.AddRange(Travelers(pool, day, firstDay, seed, dayRules, wanted, wantedSpecies));

            var entry = new DayEntry { rules = dayRules };
            var letterRng = Rng(seed, day, SaltLetter);
            if (pool.letters.Count > 0)
            {
                var l = pool.letters[letterRng.Next(pool.letters.Count)];
                entry.letterTitle = l.title;
                entry.letter = l.body;
            }
            entry.newToday = NewToday(pool, day, firstDay, seed, rules);
            entry.marketEvents = Events(pool, day, seed);
            entry.barWish = Wish(pool, day, seed);
            entry.bribe = BribeFor(pool, day, seed, travelers);
            return entry;
        }

        /// <summary>0 on the first loop day, 1 after rampDays.</summary>
        public static float Difficulty(DayPool pool, int day, int firstDay) =>
            Mathf.Clamp01((day - firstDay) / (float)Mathf.Max(1, pool.rampDays));

        private static int Steps(int from, int to, float t) => Mathf.RoundToInt(Mathf.Lerp(from, to, t));

        public static Random Rng(int seed, int day, int salt)
        {
            unchecked
            {
                int h = 17;
                h = h * 486187739 + seed;
                h = h * 486187739 + day;
                h = h * 486187739 + salt;
                return new Random(h);
            }
        }

        private static T Pick<T>(Random rng, IList<T> list) => list[rng.Next(list.Count)];

        // ---------- Rules ----------

        public static List<DayRule> Rules(DayPool pool, int day, int firstDay, int seed)
        {
            var rng = Rng(seed, day, SaltRules);
            int n = Steps(pool.rulesStart, pool.rulesMax, Difficulty(pool, day, firstDay));
            var kinds = new List<int> { 0, 1, 2, 3 }.OrderBy(_ => rng.Next()).Take(Mathf.Min(n, 4)).OrderBy(k => k).ToList();
            var rules = new List<DayRule>();
            var origins = pool.species.SelectMany(s => s.origins).Distinct().ToList();
            var forbiddable = pool.cargo.Where(c => c.canBeForbidden && c.item != null && !string.IsNullOrEmpty(c.item.CargoWord)).ToList();
            foreach (var k in kinds)
            {
                switch (k)
                {
                    case 0 when origins.Count > 1: rules.Add(new BannedOriginRule(Pick(rng, origins))); break;
                    case 1 when forbiddable.Count > 0:
                        foreach (var word in ForbiddenWith(Pick(rng, forbiddable).item.CargoWord)) rules.Add(new ForbiddenGoodsRule(word));
                        break;
                    case 2 when pool.stops.Count > 1: rules.Add(new QuarantineRule(Pick(rng, pool.stops))); break;
                    case 3:
                        rules.Add(rng.NextDouble() < 0.5 && pool.species.Count > 0
                            ? new RequireDocumentRule(DocumentType.CargoManifest, Pick(rng, pool.species).name)
                            : new RequireDocumentRule(DocumentType.CargoManifest));
                        break;
                }
            }
            return rules;
        }

        /// <summary>The drawn cargo word first, then the words forbidden together with it (no extra random draw).</summary>
        public static List<string> ForbiddenWith(string word)
        {
            var words = new List<string> { word };
            foreach (var group in ForbiddenTogether)
                if (group.Contains(word))
                    words.AddRange(group.Where(w => w != word));
            return words;
        }

        private static string NewToday(DayPool pool, int day, int firstDay, int seed, List<DayRule> today)
        {
            var before = day > firstDay ? Rules(pool, day - 1, firstDay, seed).Select(r => r.BulletinText).ToList() : new List<string>();
            var fresh = today.FirstOrDefault(r => !before.Contains(r.BulletinText));
            return fresh != null ? "New today: " + fresh.BulletinText : "";
        }

        // ---------- Wanted ----------

        private static List<WantedEntry> Wanted(DayPool pool, int day, int firstDay, int seed, out List<DayPool.SpeciesEntry> species)
        {
            var rng = Rng(seed, day, SaltWanted);
            float t = Difficulty(pool, day, firstDay);
            int n = Mathf.Min(pool.wantedMax, Steps(0, pool.wantedMax, t) + (rng.NextDouble() < 0.5 ? 1 : 0));
            var list = new List<WantedEntry>();
            species = new List<DayPool.SpeciesEntry>();
            var taken = new HashSet<string>(pool.standingWanted.Select(w => w.name));
            for (int i = 0; i < n && pool.species.Count > 0; i++)
            {
                var sp = Pick(rng, pool.species);
                string name = NewName(rng, pool, taken);
                var order = rng.NextDouble() < pool.terminateChance ? WantedOrder.Terminate : WantedOrder.Arrest;
                string crime = pool.crimes.Count > 0 ? Pick(rng, pool.crimes) : "Wanted";
                list.Add(new WantedEntry(name, NewId(rng, sp), order, order == WantedOrder.Terminate ? crime + " Terminate on sight." : crime));
                species.Add(sp);
            }
            return list;
        }

        // ---------- Travelers ----------

        private class Context
        {
            public DayPool pool;
            public DayRules day;
            public DateTime date;
            public string banned, quarantine;
            public List<string> forbidden = new List<string>();
            public bool manifestRule;
            public string manifestSpecies; // empty = everybody
            public List<WantedEntry> wanted;
            public List<DayPool.SpeciesEntry> wantedSpecies;
            public HashSet<int> wantedUsed = new HashSet<int>();
            public HashSet<string> names = new HashSet<string>();
            public float tricky;
        }

        private static List<TravelerData> Travelers(DayPool pool, int day, int firstDay, int seed, DayRules dayRules,
            List<WantedEntry> wanted, List<DayPool.SpeciesEntry> wantedSpecies)
        {
            var rng = Rng(seed, day, SaltTravelers);
            float t = Difficulty(pool, day, firstDay);
            DayRules.TryParseDate(dayRules.Date, out var date);
            var c = new Context
            {
                pool = pool, day = dayRules, date = date, wanted = wanted, wantedSpecies = wantedSpecies,
                tricky = Mathf.Lerp(pool.trickyStart, pool.trickyMax, t),
            };
            foreach (var r in dayRules.Rules)
                switch (r)
                {
                    case BannedOriginRule b: c.banned = b.origin; break;
                    case ForbiddenGoodsRule f: c.forbidden.Add(f.keyword); break;
                    case QuarantineRule q: c.quarantine = q.station; break;
                    case RequireDocumentRule d when d.document == DocumentType.CargoManifest:
                        c.manifestRule = true;
                        c.manifestSpecies = d.onlyForSpecies ?? "";
                        break;
                }
            foreach (var w in dayRules.Wanted) c.names.Add(w.name);

            int count = Steps(pool.travelersStart, pool.travelersMax, t);
            float flawed = Mathf.Lerp(pool.flawedStart, pool.flawedMax, t);
            var list = new List<TravelerData>();
            for (int i = 0; i < count; i++)
            {
                for (int attempt = 0; attempt < Attempts; attempt++)
                {
                    var flaw = rng.NextDouble() < flawed && attempt < Attempts - 5 ? PickFlaw(rng, c) : Flaw.Clean;
                    var traveler = Make(rng, c, flaw, out int wantedIndex);
                    if (traveler == null) continue;
                    if (RuleEvaluator.Evaluate(traveler, dayRules).Correct == traveler.IntendedVerdict)
                    {
                        list.Add(traveler);
                        c.names.Add(traveler.DisplayName);
                        if (wantedIndex >= 0) c.wantedUsed.Add(wantedIndex);
                        break;
                    }
                    Discard(traveler);
                }
            }
            return list;
        }

        private static Flaw PickFlaw(Random rng, Context c)
        {
            var options = new List<(Flaw flaw, float weight)>
            {
                (Flaw.ExpiredPermit, 1f), (Flaw.ExpiredId, 0.7f), (Flaw.NameMismatch, 1f), (Flaw.IdMismatch, 0.8f),
                (Flaw.MissingPermit, 0.5f), (Flaw.ForgedSeal, 0.6f),
            };
            if (c.forbidden.Count > 0) options.Add((Flaw.Contraband, 1.6f));
            if (c.banned != null) options.Add((Flaw.BannedOrigin, 1.6f));
            if (c.quarantine != null) options.Add((Flaw.Quarantine, 0.8f));
            if (c.manifestRule) options.Add((Flaw.MissingManifest, 1.2f));
            if (c.wantedUsed.Count < c.wanted.Count) options.Add((Flaw.Wanted, 1.2f));
            float total = options.Sum(o => o.weight), roll = (float)rng.NextDouble() * total;
            foreach (var o in options)
            {
                roll -= o.weight;
                if (roll <= 0f) return o.flaw;
            }
            return options[options.Count - 1].flaw;
        }

        private static TravelerData Make(Random rng, Context c, Flaw flaw, out int wantedIndex)
        {
            var pool = c.pool;
            wantedIndex = -1;
            if (pool.species.Count == 0) return null;
            bool trick = rng.NextDouble() < c.tricky;

            DayPool.SpeciesEntry sp;
            string name, id, origin;
            if (flaw == Flaw.Wanted)
            {
                var free = Enumerable.Range(0, c.wanted.Count).Where(i => !c.wantedUsed.Contains(i)).ToList();
                if (free.Count == 0) return null;
                wantedIndex = Pick(rng, free);
                sp = c.wantedSpecies[wantedIndex];
                name = c.wanted[wantedIndex].name;
                id = c.wanted[wantedIndex].id;
            }
            else
            {
                sp = flaw == Flaw.BannedOrigin
                    ? pool.species.Where(s => s.origins.Contains(c.banned)).DefaultIfEmpty(Pick(rng, pool.species)).First()
                    : Pick(rng, pool.species);
                name = NewName(rng, pool, c.names);
                id = NewId(rng, sp);
            }
            var homes = sp.origins.Where(o => o != c.banned).ToList();
            if (flaw == Flaw.BannedOrigin) origin = c.banned;
            else if (homes.Count > 0) origin = Pick(rng, homes);
            else return null;

            // route: origin > maybe one stop > KESTREL-9 (a quarantined stop only on purpose)
            var stops = pool.stops.Where(s => s != c.quarantine).ToList();
            string route = flaw == Flaw.Quarantine ? $"{origin} > {c.quarantine} > {Destination}"
                : stops.Count > 0 && rng.NextDouble() < 0.6 ? $"{origin} > {Pick(rng, stops)} > {Destination}"
                : $"{origin} > {Destination}";

            var photo = pool.looks != null && pool.looks.PhotoFor(name, sp.name) != null ? pool.looks.PhotoFor(name, sp.name) : sp.photo; // P108
            string idExpires = Day(c.date.AddDays(rng.Next(200, 1100)));
            string permitExpires = Day(c.date.AddDays(rng.Next(2, 40)));
            string permitName = name, permitId = id;
            string reason = "Papers in order";
            Verdict verdict = Verdict.Admit;
            bool forged = false, permit = true;
            int ago = trick ? 1 : rng.Next(2, 90);
            // one forbidden word needs no draw: days with a single one stay exactly as they were
            string contraband = flaw != Flaw.Contraband ? null : c.forbidden.Count == 1 ? c.forbidden[0] : Pick(rng, c.forbidden);

            switch (flaw)
            {
                case Flaw.ExpiredPermit:
                    permitExpires = Day(c.date.AddDays(-ago));
                    verdict = Verdict.Deny; reason = $"Entry permit expired on {permitExpires}";
                    break;
                case Flaw.ExpiredId:
                    idExpires = Day(c.date.AddDays(-ago));
                    verdict = Verdict.Deny; reason = $"ID card expired on {idExpires}";
                    break;
                case Flaw.NameMismatch:
                    permitName = trick ? OneLetterOff(rng, name) : NewName(rng, pool, c.names);
                    verdict = Verdict.Deny; reason = $"Name on the entry permit ({permitName}) doesn't match the ID card";
                    break;
                case Flaw.IdMismatch:
                    permitId = trick ? OneDigitOff(rng, id) : NewId(rng, sp);
                    verdict = Verdict.Deny; reason = $"ID number on the entry permit ({permitId}) doesn't match the ID card";
                    break;
                case Flaw.MissingPermit:
                    permit = false;
                    verdict = Verdict.Deny; reason = "No entry permit";
                    break;
                case Flaw.ForgedSeal:
                    forged = true;
                    verdict = Verdict.Arrest; reason = "Forged seal on the entry permit";
                    break;
                case Flaw.Contraband:
                    verdict = Verdict.Arrest; reason = $"Declares {contraband}, forbidden today";
                    break;
                case Flaw.BannedOrigin:
                    verdict = Verdict.Deny; reason = $"From {c.banned}, entry banned today";
                    break;
                case Flaw.Quarantine:
                    verdict = Verdict.Airlock; reason = $"Route via quarantined {c.quarantine}";
                    break;
                case Flaw.MissingManifest:
                    verdict = Verdict.Deny; reason = "No cargo manifest, required today";
                    break;
                case Flaw.Wanted:
                    var w = c.wanted[wantedIndex];
                    verdict = w.order == WantedOrder.Terminate ? Verdict.Airlock : Verdict.Arrest;
                    reason = $"Wanted: {w.name}";
                    break;
            }

            bool manifestRequired = c.manifestRule && (string.IsNullOrEmpty(c.manifestSpecies) || c.manifestSpecies == sp.name);
            bool manifest = flaw == Flaw.Contraband || (flaw != Flaw.MissingManifest && (manifestRequired || rng.NextDouble() < 0.4));
            if (flaw == Flaw.MissingManifest && !manifestRequired) return null;

            var docs = new List<DocumentData>
            {
                DocumentData.CreateGenerated(DocumentType.IdCard, photo, Fields(("name", name), ("species", sp.name.ToUpperInvariant()),
                    ("origin", origin), ("id", id), ("expires", idExpires))),
            };
            if (permit)
                docs.Add(DocumentData.CreateGenerated(DocumentType.EntryPermit, photo, Fields(("name", permitName), ("id", permitId),
                    ("purpose", pool.purposes.Count > 0 ? Pick(rng, pool.purposes) : "TRANSIT"), ("duration", $"{rng.Next(1, 6)} CYCLES"),
                    ("expires", permitExpires), ("issued", origin + " CONSULATE"), ("route", route)), forged));
            if (manifest)
                docs.Add(DocumentData.CreateGenerated(DocumentType.CargoManifest, null, Fields(("name", name),
                    ("ship", pool.ships.Count > 0 ? Pick(rng, pool.ships) : "UNNAMED"), ("origin", origin), ("destination", Destination),
                    ("cargo", Cargo(rng, c, contraband)))));

            string greeting = pool.greetings.Count > 0 ? Pick(rng, pool.greetings) : "";
            bool trickIntended = trick && (flaw == Flaw.NameMismatch || flaw == Flaw.IdMismatch || flaw == Flaw.ExpiredPermit || flaw == Flaw.ExpiredId);
            return TravelerData.CreateGenerated(name, sp.name, docs, verdict, reason, greeting, sp.model, trickIntended, Answers(pool, flaw, name));
        }

        /// <summary>P126: one line about the flaw, chosen by the name: no extra random draw, so the days stay what they were.</summary>
        private static List<FieldAnswer> Answers(DayPool pool, Flaw flaw, string name)
        {
            var lines = pool.flawLines.Where(l => l.flaw == flaw).ToList();
            var answers = new List<FieldAnswer>();
            if (lines.Count == 0) return answers;
            var line = lines[name.Sum(ch => ch) % lines.Count];
            answers.Add(new FieldAnswer(line.key, line.line));
            return answers;
        }

        /// <summary>Manifest lines: legal goods only, plus the forbidden word <paramref name="contraband"/> if it is not null.</summary>
        private static string Cargo(Random rng, Context c, string contraband)
        {
            var legal = c.pool.cargo.Where(l => l.item != null && !c.forbidden.Any(w => TextMatch.ContainsWords(l.line, w))).ToList();
            var lines = new List<string>();
            int n = legal.Count == 0 ? 0 : rng.Next(1, 3);
            for (int i = 0; i < n; i++)
            {
                var l = Pick(rng, legal);
                string line = $"{rng.Next(1, 6)}x {l.line}";
                if (!lines.Contains(line)) lines.Add(line);
            }
            if (contraband != null)
            {
                var bad = c.pool.cargo.FirstOrDefault(l => l.item != null && TextMatch.ContainsWords(l.line, contraband));
                lines.Insert(rng.Next(lines.Count + 1), $"{rng.Next(1, 6)}x {(bad != null ? bad.line : contraband + " CRATE")}");
            }
            if (lines.Count == 0) lines.Add("1x PERSONAL LUGGAGE");
            return string.Join("\n", lines);
        }

        private static List<DocumentField> Fields(params (string key, string value)[] fields) =>
            fields.Select(f => new DocumentField(f.key, f.value)).ToList();

        private static string Day(DateTime d) => d.ToString(DayRules.DateFormat);

        private static string NewName(Random rng, DayPool pool, HashSet<string> taken)
        {
            for (int i = 0; i < 50; i++)
            {
                string n = $"{Pick(rng, pool.givenNames)} {Pick(rng, pool.familyNames)}".ToUpperInvariant();
                if (taken.Add(n)) return n;
            }
            return $"{Pick(rng, pool.givenNames)} {Pick(rng, pool.familyNames)}-{rng.Next(2, 9)}".ToUpperInvariant();
        }

        private static string NewId(Random rng, DayPool.SpeciesEntry sp) => $"{sp.idPrefix}-{rng.Next(1000, 10000)}-{rng.Next(10, 100)}";

        private const string Vowels = "AEIOU";

        /// <summary>The same name with one letter of the family name changed (a vowel for another vowel), e.g. DOLE → DALE.</summary>
        private static string OneLetterOff(Random rng, string name)
        {
            int space = name.IndexOf(' ');
            var chars = name.ToCharArray();
            var spots = Enumerable.Range(space + 1, name.Length - space - 1).Where(i => Vowels.IndexOf(chars[i]) >= 0).ToList();
            if (spots.Count == 0) return name + "E";
            int at = Pick(rng, spots);
            var others = Vowels.Where(v => v != chars[at]).ToList();
            chars[at] = Pick(rng, others);
            return new string(chars);
        }

        /// <summary>The same ID with one digit changed.</summary>
        private static string OneDigitOff(Random rng, string id)
        {
            var chars = id.ToCharArray();
            var spots = Enumerable.Range(0, chars.Length).Where(i => char.IsDigit(chars[i])).ToList();
            int at = Pick(rng, spots);
            chars[at] = (char)('0' + (chars[at] - '0' + rng.Next(1, 10)) % 10);
            return new string(chars);
        }

        private static void Discard(TravelerData t)
        {
            foreach (var d in t.Documents) Destroy(d);
            Destroy(t);
        }

        private static void Destroy(UnityEngine.Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(o);
            else UnityEngine.Object.DestroyImmediate(o);
        }

        // ---------- Evening ----------

        private static List<MarketEvent> Events(DayPool pool, int day, int seed)
        {
            var rng = Rng(seed, day, SaltEvent);
            var list = new List<MarketEvent>();
            var options = pool.marketEvents.Where(e => e.marketEvent != null && e.weight > 0f).ToList();
            if (options.Count == 0 || rng.NextDouble() >= pool.eventChance) return list;
            float roll = (float)rng.NextDouble() * options.Sum(e => e.weight);
            foreach (var e in options)
            {
                roll -= e.weight;
                if (roll > 0f) continue;
                list.Add(e.marketEvent);
                break;
            }
            if (list.Count == 0) list.Add(options[options.Count - 1].marketEvent);
            return list;
        }

        private static BarWish Wish(DayPool pool, int day, int seed)
        {
            var rng = Rng(seed, day, SaltWish);
            var options = pool.barWishes.Where(w => w.Active).ToList();
            if (options.Count == 0 || rng.NextDouble() >= pool.barWishChance) return new BarWish();
            var w = Pick(rng, options);
            return new BarWish { tag = w.tag, amount = w.amount, sign = w.sign };
        }

        private static Bribe BribeFor(DayPool pool, int day, int seed, List<TravelerData> travelers)
        {
            var rng = Rng(seed, day, SaltBribe);
            var bribable = travelers.Where(t => t.IntendedVerdict == Verdict.Deny || t.IntendedVerdict == Verdict.Arrest).ToList();
            if (bribable.Count == 0 || rng.NextDouble() >= pool.bribeChance) return new Bribe();
            int amount = Mathf.RoundToInt(rng.Next(pool.bribeMin, pool.bribeMax + 1) / 5f) * 5;
            return new Bribe { traveler = Pick(rng, bribable), amount = amount };
        }
    }
}
