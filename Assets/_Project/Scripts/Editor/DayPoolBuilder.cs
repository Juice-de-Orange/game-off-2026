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

        // Origins and stops must not contain each other (the validator reports near misses).
        private static readonly (string species, string prefix, string[] origins)[] Species =
        {
            ("Human", "HM", new[] { "TORVIK COLONY", "ASKA-3", "NEW HADRA" }),
            ("Vellani", "VL", new[] { "VELL PRIME", "SELUNE" }),
            ("Quobb", "QB", new[] { "QUOBB MARSH", "ORVAN" }),
            ("Xirrik", "XR", new[] { "LUMEN REACH", "XIR HOLLOW" }),
            ("Sorvak", "SV", new[] { "SORVAK DEEP", "KHARN" }),
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
            "Armed robbery.", "Smuggling, three counts.", "Escaped from a labour camp.", "Desertion.", "Forgery of transit papers.",
            "Sabotage at a fuel depot.", "Fraud, Meridian Hub.", "Assault on an officer.",
        };

        private static readonly (string title, string body)[] Letters =
        {
            ("A letter from home", "The heater rattles again, but it still works if you hit it twice. We hit it a lot.\n\nWe count the days. Don't work too hard."),
            ("A letter from home", "The little one drew a picture of your station. It has too many windows and a cat on the roof.\n\nWe keep it on the wall."),
            ("A letter from home", "Prices went up again at the depot. Everyone says it will get better after the winter.\n\nNobody says which winter."),
            ("A letter from home", "Your mother asks if you eat properly. I told her you do. Do you?\n\nWrite back when you can."),
            ("A letter from home", "The neighbours left for the inner colonies last week. Their flat is dark now.\n\nWe are fine. We just miss you."),
            ("A letter from home", "There was a power cut all night. We sat together under the blankets and told stories about you.\n\nThey were mostly true."),
            ("A letter from home", "The school wants new shoes for the winter term. I said we'll see.\n\nDon't worry about it. Just come home."),
            ("A letter from home", "I found your old jacket in the cupboard. It still smells like you, a little.\n\nHow long now?"),
            ("A letter from home", "The radio says the routes are getting worse. Be careful who you let through.\n\nAnd be careful who you don't."),
            ("A letter from home", "We had soup three days in a row. Good soup, though.\n\nSave what you can. We can wait a bit longer."),
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

            pool.standingWanted = new List<WantedEntry> { new WantedEntry("KAEL DROSS", "DX-0013-44", WantedOrder.Arrest, "Armed robbery, Aska-3") };

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
