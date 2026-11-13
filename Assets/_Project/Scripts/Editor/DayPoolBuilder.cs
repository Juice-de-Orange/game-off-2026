using System.Collections.Generic;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Travelers;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P73: fills Data/Days/DayPool.asset (what the endless days after the story are drawn from) and hangs it into the DaySchedule
    /// as its loop. Menu GameOff/Build Day Pool; also runs in Build Day Flow. Values tweaked in the inspector are overwritten:
    /// change them here.
    /// </summary>
    public static class DayPoolBuilder
    {
        public const string PoolPath = "Assets/_Project/Data/Days/DayPool.asset";
        private const string SchedulePath = "Assets/_Project/Data/Days/DaySchedule.asset";
        private const string PhotoFolder = "Assets/_Project/Art/Textures/Documents/";
        private const string ItemFolder = "Assets/_Project/Data/Items/";
        private const string TagFolder = "Assets/_Project/Data/Items/Tags/";
        private const string MarketFolder = "Assets/_Project/Data/Market/";

        // Origins and stops must not contain each other (the validator reports near misses). Place names and ID prefixes as in
        // the story days (SORV DEEP, QUOB ..., SK-/QU- IDs).
        private static readonly (string species, string prefix, string[] origins)[] Species =
        {
            ("Human", "HM", new[] { "TORVIK COLONY", "ASKA-3", "NEW HADRA" }),
            ("Vellani", "VL", new[] { "VELL PRIME", "SELUNE" }),
            ("Quobb", "QU", new[] { "QUOB MARSH", "ORVAN" }),
            ("Xirrik", "XR", new[] { "LUMEN REACH", "XIR HOLLOW" }),
            ("Sorvak", "SK", new[] { "SORV DEEP", "KHARN" }),
        };

        private static readonly string[] Stops = { "KETRA STATION", "DUNMOR RELAY", "PIKE GATE", "HALDEN DOCKS", "MERIDIAN HUB", "OSSA BEACON" };

        // Not the names of story characters or story fillers.
        private static readonly string[] Given =
        {
            "ADA", "BRAM", "CEL", "DASH", "EDO", "FENN", "GALE", "HOLT", "IVO", "JUNA", "KAI", "LIO",
            "MAREK", "NIA", "ORIN", "PIA", "RUNE", "SAL", "TAVI", "UMA", "VAREK", "WREN", "YARA", "ZEV",
        };

        private static readonly string[] Family =
        {
            "ASHBY", "BRENN", "CALDER", "DUVAL", "ESKER", "FARO", "GRIMM", "HALVER", "IRRO", "JASK", "KOVAC", "LUND",
            "MORROW", "NAVIK", "ODELL", "PRAX", "ROOK", "SAVO", "TARN", "ULLER", "VANCE", "WESTA", "YORIN", "ZELL",
        };

        private static readonly string[] Purposes = { "WORK", "TRADE", "TRANSIT", "FAMILY VISIT", "MEDICAL", "PILGRIMAGE" };
        private static readonly string[] Ships = { "RUSTWING", "DAWN OF ASKA", "TALLOW 9", "GREY HERON", "LOW TIDE", "KESSLER'S LUCK", "IRON PSALM", "SLOW BLOOM" };

        // item asset → manifest line (contains the item's cargo word), may be forbidden by a day rule
        private static readonly (string item, string line, bool forbiddable)[] Cargo =
        {
            ("RationPack", "RATION PACK CRATE", false), ("ProteinBar", "PROTEIN BAR BOX", false), ("CannedStew", "CANNED STEW CASE", false),
            ("FreshFruit", "FRESH FRUIT CRATE", false), ("NovaCola", "NOVA COLA CRATE", false), ("BatteryPack", "BATTERY PACK CASE", false),
            ("FusionCell", "FUSION CELL", false), ("SpareFuse", "SPARE FUSE BOX", false), ("CoughSyrup", "COUGH SYRUP CASE", false),
            ("Painkillers", "PAINKILLERS BOX", true), ("Antibiotics", "ANTIBIOTICS CASE", true), ("StimulantVials", "STIMULANT VIALS", true),
            ("Spice", "SPICE SACK", true), ("DataChip", "DATA CHIP CASE", true), ("Pistol", "PISTOL", true),
            ("WeaponParts", "WEAPON PARTS CRATE", true), ("PocketWatch", "POCKET WATCH", false), ("SilverRing", "SILVER RING", false),
            ("AlienTrinket", "ALIEN TRINKET BOX", true), ("CashBundle", "CASH BUNDLE", true), ("Liquor", "LIQUOR BOTTLE", true),
            ("AlienSpecimen", "ALIEN SPECIMEN JAR", true),
        };

        private static readonly string[] Greetings =
        {
            "Long trip. Please make it quick.",
            "Good evening, officer. Or is it morning here?",
            "Everything should be in order.",
            "I've been in that queue for three hours.",
            "My cousin said this station is friendly.",
            "Is the bar still open after this?",
            "Don't look at me like that. I'm just tired.",
            "Here. All of it.",
            "First time on Kestrel. Is it always this cold?",
            "They told me to talk to you.",
            "I have a connecting shuttle. Please.",
            "You look like you need a drink more than I do.",
            "Same as last time, officer.",
            "Mind the papers, they got wet.",
            "I'm only passing through.",
            "Work is work. Where do I go?",
        };

        private static readonly string[] Crimes =
        {
            "Armed robbery.", "Smuggling, three counts.", "Escaped from a labor camp.", "Desertion.", "Forgery of transit papers.",
            "Sabotage at a fuel depot.", "Fraud, Meridian Hub.", "Assault on an officer.",
        };

        // P126: what a traveler with a flaw says when asked about that field.
        private static readonly (DayGenerator.Flaw flaw, string key, string line)[] FlawLines =
        {
            (DayGenerator.Flaw.ExpiredPermit, "expires", "It ran out? They told me I had until the end of the cycle."),
            (DayGenerator.Flaw.ExpiredPermit, "expires", "It's only a little late, officer. The queue for renewals is months long."),
            (DayGenerator.Flaw.ExpiredId, "expires", "I've been meaning to renew it. The office has been shut for weeks."),
            (DayGenerator.Flaw.ExpiredId, "expires", "It still looks like me, doesn't it?"),
            (DayGenerator.Flaw.NameMismatch, "name", "A clerk's mistake. Everybody spells it differently."),
            (DayGenerator.Flaw.NameMismatch, "name", "That's my name. More or less. They rushed the form."),
            (DayGenerator.Flaw.IdMismatch, "id", "The numbers? I never look at the numbers."),
            (DayGenerator.Flaw.IdMismatch, "id", "They must have typed it wrong at the consulate."),
            (DayGenerator.Flaw.ForgedSeal, "issued", "The consulate gave me that. Who else would have?"),
            (DayGenerator.Flaw.ForgedSeal, "issued", "I paid good money for that stamp. I mean, the fee."),
            (DayGenerator.Flaw.Contraband, "cargo", "It's only for personal use. All of it."),
            (DayGenerator.Flaw.Contraband, "cargo", "Yesterday that was perfectly legal, officer."),
            (DayGenerator.Flaw.BannedOrigin, "origin", "I only lived there. I've nothing to do with what's going on."),
            (DayGenerator.Flaw.BannedOrigin, "origin", "That's where I'm from. I can't change it."),
            (DayGenerator.Flaw.Quarantine, "route", "We didn't even leave the ship there. I feel fine."),
            (DayGenerator.Flaw.Quarantine, "route", "A short stop for fuel. Nobody was sick. Nobody I saw."),
            (DayGenerator.Flaw.Wanted, "name", "There must be plenty of people with that name."),
            (DayGenerator.Flaw.Wanted, "name", "You've got the wrong one. I get that a lot."),
        };

        // Written by Lena like the story letters (StoryDayContent): "Dad," ... "— Lena", Mom and Pip by name. A pool letter only
        // shows while all three live (a death brings the loss letters instead, P69), so none of them may talk about health.
        private static readonly (string title, string body)[] Letters =
        {
            ("A letter from home", "Dad,\nThe heater rattles again, but it still works if you hit it twice. Mom hits it a lot.\nWe count the days. Don't work too hard.\n— Lena"),
            ("A letter from home", "Dad,\nPip drew a picture of your station. It has too many windows and a cat on the roof.\nMom put it on the wall.\n— Lena"),
            ("A letter from home", "Dad,\nPrices went up again at the depot. Everyone says it will get better after the winter.\nNobody says which winter.\n— Lena"),
            ("A letter from home", "Dad,\nMom asks if you eat properly. I told her you do. Do you?\nWrite back when you can.\n— Lena"),
            ("A letter from home", "Dad,\nThe neighbors left for the inner colonies last week. Their windows are dark now.\nPip waves at them anyway. We miss you.\n— Lena"),
            ("A letter from home", "Dad,\nThere was a power cut all night. We sat under the blankets and Mom told stories about you.\nThey were mostly true.\n— Lena"),
            ("A letter from home", "Dad,\nSchool wants new shoes for the winter term. Mom said we'll see.\nDon't worry about it. Just come home.\n— Lena"),
            ("A letter from home", "Dad,\nI found your old jacket in the cupboard. It still smells like you, a little. I wear it to bed.\nHow long now?\n— Lena"),
            ("A letter from home", "Dad,\nThe radio says the routes are getting worse. Mom says be careful who you let through.\nI say be careful who you don't.\n— Lena"),
            ("A letter from home", "Dad,\nThe depot has started rationing soap. We smell like everybody else now, so nobody minds.\nWrite when you can.\n— Lena"),
            ("A letter from home", "Dad,\nThe stairwell light has been out for a month. Pip and I count the steps now. Nineteen.\nCome home before we forget the number.\n— Lena"),
            ("A letter from home", "Dad,\nSomebody painted over the old mural by the depot. You can still see the sun if you know where to look.\nWe know where to look.\n— Lena"),
            ("A letter from home", "Dad,\nPip lost another tooth and wants to send it to you. Mom said the drone costs too much.\nIt is in a matchbox, waiting.\n— Lena"),
            ("A letter from home", "Dad,\nThey moved the clinic to the other side of the river. It's a long walk. Mom says that's what legs are for.\nDon't worry.\n— Lena"),
            ("A letter from home", "Dad,\nIt snowed indoors at school, right through the roof. We all thought it was wonderful.\nThe teachers did not.\n— Lena"),
            ("A letter from home", "Dad,\nMom saw your name on an old list at the depot and stood there for a long time.\nShe didn't say anything. I just wanted you to know.\n— Lena"),
            ("A letter from home", "Dad,\nThe neighbor's dog has decided it lives with us. We can't feed it. We feed it.\nPip named it Kestrel. How are the people up there?\n— Lena"),
            ("A letter from home", "Dad,\nWe put your chair back at the table. It was in the way, but it looked wrong by the wall.\nSave what you can.\n— Lena"),
            ("A letter from home", "Dad,\nThe radio played that song you hate. We all sang it, badly, on purpose.\nYou would have left the room.\n— Lena"),
            ("A letter from home", "Dad,\nWe had soup three days in a row. Good soup, though. Mom says it counts as cooking.\nSave what you can. We can wait a bit longer.\n— Lena"),
        };

        private static readonly (string ev, float weight)[] Events =
        {
            ("BorderClosed", 1f), ("ColdSnap", 1f), ("SmugglersBusted", 0.8f), ("SecurityLockdown", 1f), ("ClinicShortage", 1f),
            ("DockStrike", 1f), ("InspectionWeek", 0.6f), ("OddosCaravan", 0.3f),
        };

        private static readonly (string tag, int amount)[] Wishes =
        {
            ("FOOD", 2), ("LUXURY", 2), ("MEDICINE", 1), ("POWER", 2), ("DRUG", 1), ("VALUABLE", 1), ("ALIEN", 1),
        };

        [MenuItem("GameOff/Build Day Pool")]
        public static void BuildFromMenu() => Debug.Log("[DayPoolBuilder] " + Build());

        public static string Build()
        {
            var pool = AssetDatabase.LoadAssetAtPath<DayPool>(PoolPath);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<DayPool>();
                AssetDatabase.CreateAsset(pool, PoolPath);
            }

            pool.species = new List<DayPool.SpeciesEntry>();
            foreach (var (name, prefix, origins) in Species)
                pool.species.Add(new DayPool.SpeciesEntry
                {
                    name = name,
                    idPrefix = prefix,
                    model = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + $"PF_NPC_{name}.prefab"),
                    photo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PhotoFolder}T_Doc_Photo_{name}.png"),
                    origins = new List<string>(origins),
                });
            pool.givenNames = new List<string>(Given);
            pool.familyNames = new List<string>(Family);
            pool.stops = new List<string>(Stops);
            pool.purposes = new List<string>(Purposes);
            pool.ships = new List<string>(Ships);
            pool.greetings = new List<string>(Greetings);
            pool.crimes = new List<string>(Crimes);

            pool.cargo = new List<DayPool.CargoLine>();
            int missing = 0;
            foreach (var (item, line, forbiddable) in Cargo)
            {
                var data = AssetDatabase.LoadAssetAtPath<ItemData>($"{ItemFolder}Item_{item}.asset");
                if (data == null || string.IsNullOrEmpty(data.CargoWord) || !TextMatch.ContainsWords(line, data.CargoWord)) { missing++; continue; }
                pool.cargo.Add(new DayPool.CargoLine { item = data, line = line, canBeForbidden = forbiddable });
            }

            // not a story name: Kael Dross of day 3 is behind bars by now (P15)
            pool.standingWanted = new List<WantedEntry> { new WantedEntry("DOVAN PRYCE", "DP-7342-18", WantedOrder.Arrest, "Armed robbery, New Hadra") };

            pool.flawLines = new List<DayPool.FlawLine>();
            foreach (var (flaw, key, line) in FlawLines) pool.flawLines.Add(new DayPool.FlawLine { flaw = flaw, key = key, line = line });
            pool.letters = new List<DayPool.Letter>();
            foreach (var (title, body) in Letters) pool.letters.Add(new DayPool.Letter { title = title, body = body });

            pool.marketEvents = new List<DayPool.WeightedEvent>();
            foreach (var (ev, weight) in Events)
            {
                var e = AssetDatabase.LoadAssetAtPath<MarketEvent>($"{MarketFolder}Market_{ev}.asset");
                if (e != null) pool.marketEvents.Add(new DayPool.WeightedEvent { marketEvent = e, weight = weight });
            }

            pool.barWishes = new List<BarWish>();
            foreach (var (tag, amount) in Wishes)
            {
                var t = AssetDatabase.LoadAssetAtPath<ItemTag>($"{TagFolder}Tag_{tag}.asset");
                if (t != null) pool.barWishes.Add(new BarWish { tag = t, amount = amount });
            }
            EditorUtility.SetDirty(pool);

            var schedule = AssetDatabase.LoadAssetAtPath<DaySchedule>(SchedulePath);
            if (schedule != null)
            {
                var so = new SerializedObject(schedule);
                so.FindProperty("loop").objectReferenceValue = pool;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            AssetDatabase.SaveAssets();
            return $"day pool: {pool.species.Count} species, {pool.cargo.Count} cargo lines ({missing} missing), {pool.marketEvents.Count} events, " +
                   $"{pool.barWishes.Count} bar wishes, {pool.letters.Count} letters; loop of {(schedule != null ? "DaySchedule" : "no schedule!")}";
        }
    }
}
