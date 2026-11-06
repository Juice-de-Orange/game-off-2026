using System.Collections.Generic;
using GameOff.Documents;
using GameOff.Travelers;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds the test day for P07 (Data/Days/Day1_Test): today's rules and six travelers, one per kind of case, each with the
    /// decision the author intends. Real content for three days comes with P15 (same format). Idempotent.
    /// </summary>
    public static class TravelerContentBuilder
    {
        private const string DayPath = "Assets/_Project/Data/Days/Day1_Test.asset";
        private const string TravelerFolder = "Assets/_Project/Data/Travelers/Test/";
        private const string DocFolder = "Assets/_Project/Data/Documents/Test/";
        private const string PhotoFolder = "Assets/_Project/Art/Textures/Documents/";

        [MenuItem("GameOff/Build Test Day")]
        public static void BuildFromMenu() => Debug.Log(Build());

        public static string Build()
        {
            var travelers = new List<TravelerData>
            {
                Traveler("01_Vesna", "VESNA OKARU", "Human", Verdict.Admit, "Everything in order",
                    "Morning, officer. Long queue today, huh?",
                    Id(Photo("Human"), "VESNA OKARU", "HUMAN", "TORVIK COLONY", "KX-4471-09", "2292-03-14"),
                    Permit(Photo("Human"), "VESNA OKARU", "KX-4471-09", "CARGO DELIVERY", "2 CYCLES", "2290-11-30", "ASKA-3 CONSULATE",
                        "TORVIK > ASKA-3 > KESTREL-9"),
                    Manifest("VESNA OKARU", "MV HALDE", "TORVIK", "12x HYDROPONIC NUTRIENT PACK\n 4x WATER FILTER CARTRIDGE")),
                Traveler("02_Darro", "DARRO VENN", "Vellani", Verdict.Deny, "Permit issued to DARIO VENN, ID says DARRO VENN",
                    "Greetings. My papers are, how you say, impeccable.",
                    Id(Photo("Vellani"), "DARRO VENN", "VELLANI", "VELL PRIME", "VL-2210-33", "2291-08-01"),
                    Permit(Photo("Vellani"), "DARIO VENN", "VL-2210-33", "TRADE", "5 CYCLES", "2290-12-24", "VELL PRIME CONSULATE",
                        "VELL PRIME > KESTREL-9")),
                Traveler("03_Mira", "MIRA TALSK", "Sorvak", Verdict.Deny, "Entry permit expired on 2290-10-28",
                    "Please hurry. I have been travelling for nine days.",
                    Id(Photo("Sorvak"), "MIRA TALSK", "SORVAK", "SORV DEEP", "SV-0091-12", "2293-01-20"),
                    Permit(Photo("Sorvak"), "MIRA TALSK", "SV-0091-12", "FAMILY VISIT", "10 CYCLES", "2290-10-28", "ASKA-3 CONSULATE",
                        "SORV DEEP > ASKA-3 > KESTREL-9")),
                Traveler("04_Quill", "QUILL BARRAGH", "Quobb", Verdict.Arrest, "Declares chem stimulants, forbidden today",
                    "Just a few personal supplies. Nothing to worry about.",
                    Id(Photo("Quobb"), "QUILL BARRAGH", "QUOBB", "QUOB ATOLL", "QB-7720-05", "2291-11-11"),
                    Permit(Photo("Quobb"), "QUILL BARRAGH", "QB-7720-05", "BUSINESS", "3 CYCLES", "2290-11-20", "ASKA-3 CONSULATE",
                        "QUOB ATOLL > KESTREL-9"),
                    Manifest("QUILL BARRAGH", "SKIFF PELL", "QUOB ATOLL", "2x SPARE PARTS CRATE\n 6x CHEM STIMULANT VIALS")),
                Traveler("05_Ixa", "IXA REN", "Xirrik", Verdict.Airlock, "Route passes Ketra Station (quarantine)",
                    "Kss. I am tired. Let me through.",
                    Id(Photo("Xirrik"), "IXA REN", "XIRRIK", "LUMEN REACH", "XR-3302-77", "2292-06-06"),
                    Permit(Photo("Xirrik"), "IXA REN", "XR-3302-77", "TRANSIT", "1 CYCLE", "2290-11-09", "LUMEN REACH CONSULATE",
                        "LUMEN REACH > KETRA STATION > KESTREL-9")),
                Traveler("06_Shifter", "LONN ABERNATHY", "Human", Verdict.Admit, "The Shifter, day 1: harmless, papers clean",
                    "Officer! What a fine day to cross a border, wouldn't you say?",
                    Id(Photo("Human"), "LONN ABERNATHY", "HUMAN", "ASKA-3", "AB-0777-77", "2291-05-05"),
                    Permit(Photo("Human"), "LONN ABERNATHY", "AB-0777-77", "TOURISM", "1 CYCLE", "2290-11-06", "ASKA-3 CONSULATE",
                        "ASKA-3 > KESTREL-9"),
                    tag: "shifter"),
            };

            var day = Asset<DayRules>(DayPath);
            day.EditorSet("2290-11-05",
                new List<DayRule>
                {
                    new BannedOriginRule("ORVAN"),
                    new ForbiddenGoodsRule("STIMULANT"),
                    new QuarantineRule("KETRA STATION"),
                },
                new List<WantedEntry> { new WantedEntry("KAEL DROSS", "DX-0013-44", WantedOrder.Arrest, "Armed robbery, Aska-3") },
                travelers);
            EditorUtility.SetDirty(day);
            AssetDatabase.SaveAssets();

            var problems = TravelerValidator.Validate(day);
            return $"Test day built: {travelers.Count} travelers. Validator: " + (problems.Count == 0 ? "no problems" : string.Join("\n", problems));
        }

        private static TravelerData Traveler(string file, string name, string species, Verdict verdict, string reason, string greeting,
            params DocumentData[] docs) => Traveler(file, name, species, verdict, reason, greeting, docs, "");

        private static TravelerData Traveler(string file, string name, string species, Verdict verdict, string reason, string greeting,
            DocumentData id, DocumentData permit, string tag) => Traveler(file, name, species, verdict, reason, greeting, new[] { id, permit }, tag);

        private static TravelerData Traveler(string file, string name, string species, Verdict verdict, string reason, string greeting,
            DocumentData[] docs, string tag)
        {
            var t = Asset<TravelerData>(TravelerFolder + $"T_Test_{file}.asset");
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + $"PF_NPC_{species}.prefab");
            if (model == null) Debug.LogError($"[TestDay] No PF_NPC_{species}: run GameOff/Build Travelers first, or {name} stays invisible.");
            int n = 0;
            foreach (var d in docs)
            {
                // Each document gets its own asset next to the traveler, named after both
                var path = DocFolder + $"Doc_{file}_{d.Type}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<DocumentData>(path);
                if (existing == null) { AssetDatabase.CreateAsset(d, path); existing = d; }
                else { existing.EditorSet(d.Type, d.Photo, new List<DocumentField>(d.Fields), d.ForgedSeal); Object.DestroyImmediate(d); }
                EditorUtility.SetDirty(existing);
                docs[n++] = existing;
            }
            t.EditorSet(name, species, new List<DocumentData>(docs), verdict, reason, tag, greeting, null, model);
            EditorUtility.SetDirty(t);
            return t;
        }

        private static DocumentData Doc(DocumentType type, Texture2D photo, params (string, string)[] fields)
        {
            var list = new List<DocumentField>();
            foreach (var (k, v) in fields) list.Add(new DocumentField(k, v));
            var d = ScriptableObject.CreateInstance<DocumentData>();
            d.EditorSet(type, photo, list);
            return d;
        }

        private static Texture2D Photo(string species)
        {
            var photo = AssetDatabase.LoadAssetAtPath<Texture2D>($"{PhotoFolder}T_Doc_Photo_{species}.png");
            if (photo == null) Debug.LogError($"[TestDay] No passport photo for {species}.");
            return photo;
        }

        private static DocumentData Id(Texture2D photo, string name, string species, string origin, string id, string expires) =>
            Doc(DocumentType.IdCard, photo, ("name", name), ("species", species), ("origin", origin), ("id", id), ("expires", expires));

        private static DocumentData Permit(Texture2D photo, string name, string id, string purpose, string duration, string expires,
            string issued, string route) =>
            Doc(DocumentType.EntryPermit, photo, ("name", name), ("id", id), ("purpose", purpose), ("duration", duration),
                ("expires", expires), ("issued", issued), ("route", route));

        private static DocumentData Manifest(string name, string ship, string origin, string cargo) =>
            Doc(DocumentType.CargoManifest, null, ("name", name), ("ship", ship), ("origin", origin), ("destination", "KESTREL-9"), ("cargo", cargo));

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            AssetFolders.EnsureForAsset(path);
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }
}
