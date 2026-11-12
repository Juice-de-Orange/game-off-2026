using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Day;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Shops;
using GameOff.Travelers;
using GameOff.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameOff.EditorTools.MenuSceneBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds the money/inventory backbone (P46), idempotent:
    /// - test data (only created if missing – it is content, tune it in the inspector): tags, items, vendor stocks, EconomyTuning
    /// - Prefabs/Economy/PF_Economy.prefab: PlayerEconomy, trade window (design A), inventory window (design 1, key I),
    ///   vendor placeholders Holly (kiosk), barkeeper and black market dealer (bar), cargo drone terminal (hangar)
    /// - places it in the open scene wired to the player, DayManager and the dialogs of PF_DayFlow.
    /// Vendor figures: the model at Art/Models/Characters/SM_NPC_*.fbx if delivered, otherwise a grey box. Rebuild after delivery.
    /// Menu: GameOff → Build Economy (open scene) / Build Economy into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class EconomyBuilder
    {
        private static readonly Color DevRed = new Color(0.89f, 0.27f, 0.25f); // STYLE.md alarm red, like Dev_end_Day
        private const string PrefabPath = "Assets/_Project/Prefabs/Economy/PF_Economy.prefab";
        private const string TagFolder = "Assets/_Project/Data/Items/Tags/";
        private const string ItemFolder = "Assets/_Project/Data/Items/";
        private const string EconomyFolder = "Assets/_Project/Data/Economy/";
        private const string ModelFolder = "Assets/_Project/Art/Models/Characters/";
        private const string PlaceholderMaterial = "Assets/_Project/Art/Materials/M_Placeholder.mat";
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string DroneModelName = "SM_Hangar_CargoDrone";

        /// <summary>Vendor placeholder: figure (box size) at a world position facing yaw; trigger box around it.</summary>
        private readonly struct VendorSpot
        {
            public readonly string Name, Model, Stock;
            public readonly Vector3 Figure, FigureSize, TriggerCenter, TriggerSize;
            public readonly float Yaw;

            public VendorSpot(string name, string model, string stock, Vector3 figure, Vector3 figureSize, float yaw,
                Vector3 triggerCenter, Vector3 triggerSize)
            {
                Name = name; Model = model; Stock = stock; Figure = figure; FigureSize = figureSize; Yaw = yaw;
                TriggerCenter = triggerCenter; TriggerSize = triggerSize;
            }
        }

        // Positions from docs/STATION.md and the delivered counters (measured 05.11.): kiosk counter x 8.2..9 z -5.4..-2.6 with the
        // hatch to the hall at x 8; bar counter x 14.15..14.85 z 1..7; bar stool 4 at x 13.6 z 6.4. Yaw -90 = facing the hall (-x).
        private static readonly VendorSpot[] Vendors =
        {
            new VendorSpot("Holly", "SM_NPC_Holly", "Stock_Kiosk_Test", new Vector3(9.6f, 0f, -4f), new Vector3(0.55f, 1.7f, 0.45f), -90f,
                new Vector3(9.0f, 1.2f, -4f), new Vector3(2.2f, 1.6f, 2.6f)),
            new VendorSpot("Barkeeper", "SM_NPC_Barkeeper", "Stock_Bar_Test", new Vector3(15.5f, 0f, 4f), new Vector3(0.8f, 1.9f, 0.6f), -90f,
                new Vector3(15.1f, 1.25f, 4f), new Vector3(2.2f, 1.5f, 2.0f)),
            new VendorSpot("Dealer", "SM_NPC_Dealer", "Stock_BlackMarket_Test", new Vector3(13.6f, 0.74f, 6.4f), new Vector3(0.6f, 1.3f, 0.6f), 90f,
                new Vector3(13.6f, 1.05f, 6.4f), new Vector3(1.0f, 1.7f, 1.0f)),
        };

        [MenuItem("GameOff/Build Economy")]
        public static void BuildFromMenu() => Debug.Log(Build(SceneManager.GetActiveScene()));

        [MenuItem("GameOff/Build Economy into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>Only the data (tags, items, stocks, trade rules) – touches no scene or prefab (P64).</summary>
        [MenuItem("GameOff/Update Economy Data")]
        public static void UpdateDataFromMenu()
        {
            var log = new List<string>();
            EnsureData(log);
            Debug.Log("[EconomyBuilder] data updated; " + string.Join("; ", log));
        }

        public static string Build(Scene scene)
        {
            var log = new List<string>();
            var data = EnsureData(log);
            var prefab = BuildPrefab(data, log);
            Place(scene, prefab, log);
            return "[EconomyBuilder] " + string.Join("; ", log);
        }

        // ---------- Test data ----------

        private class Data
        {
            public EconomyTuning Tuning;
            public ConfiscationTuning Confiscation;
            public HouseholdTuning Household;
            public readonly Dictionary<string, ItemData> Items = new Dictionary<string, ItemData>();
            public readonly Dictionary<string, VendorStock> Stocks = new Dictionary<string, VendorStock>();
        }

        private static Data EnsureData(List<string> log)
        {
            int created = 0;
            var data = new Data();
            data.Tuning = LoadOrCreate<EconomyTuning>(EconomyFolder + "EconomyTuning.asset", t => { t.startCredits = 40; t.currency = "CR"; }, ref created);

            var contraband = Tag("CONTRABAND", "7A1F2B", ref created);
            var medicine = Tag("MEDICINE", "2F5B66", ref created);
            var gadget = Tag("GADGET", "5E3A24", ref created);
            var food = Tag("FOOD", "3F7A3A", ref created);

            data.Items["scanner"] = Item("DocumentScanner", "Document scanner", "Marks forged fields on papers. Use it at the counter (P16).", 60, ref created, gadget);
            SetFields(data.Items["scanner"], ("unique", true)); // a gadget: owned once
            data.Items["ration"] = Item("RationPack", "Ration pack", "Grey, dense and filling. A day of food for one.", 8, ref created, food);
            data.Items["painkillers"] = Item("Painkillers", "Painkillers", "Strong enough for a bad cough. Not much more.", 15, ref created, medicine);
            data.Items["cola"] = Item("NovaCola", "Nova Cola", "Warm, flat and sweet. Keeps you awake at the counter.", 2, ref created, food);
            data.Items["spice"] = Item("Spice", "Spice", "Banned, sweet-smelling and much asked for. Worth more under the bar than at the kiosk.", 12, ref created, contraband);
            data.Items["watch"] = Item("PocketWatch", "Pocket watch", "Old, heavy, still ticking. Someone will want it.", 20, ref created);

            data.Stocks["Stock_Kiosk_Test"] = Stock("Stock_Kiosk_Test", "KIOSK · HOLLY", 1f, 0.5f, "Sold out.",
                new[] { data.Items["scanner"], data.Items["ration"], data.Items["painkillers"], data.Items["cola"] }, new[] { contraband }, ref created);
            data.Stocks["Stock_Bar_Test"] = Stock("Stock_Bar_Test", "THE LAST ORBIT · BAR", 1f, 0f,
                "Drinks and talk come later.", new ItemData[0], new ItemTag[0], ref created);
            data.Stocks["Stock_BlackMarket_Test"] = Stock("Stock_BlackMarket_Test", "UNDER THE COUNTER", 1.5f, 1.2f, "Nothing today.",
                new[] { data.Items["spice"] }, new ItemTag[0], ref created);

            // Shared label set (P64, agreed 06.11.): category labels are what rumours (P65) and wish lists refer to.
            var confiscated = Tag("CONFISCATED", "E2463F", ref created);
            var drug = Tag("DRUG", "B9824A", ref created);
            Tag("TECH", "5EC8FF", ref created);
            Tag("WEAPON", "444B55", ref created);
            var valuable = Tag("VALUABLE", "FFD36B", ref created);
            Tag("ALIEN", "C27BD6", ref created);
            Tag("PERISHABLE", "9BE05A", ref created);
            ApplyTradeRules(data, new[] { contraband, confiscated }, drug, valuable, log);
            foreach (var stock in data.Stocks.Values) MigrateToEntries(stock, log);

            // P10: wage, rent, needs and the family (placeholder names until P15).
            data.Household = LoadOrCreate<HouseholdTuning>(EconomyFolder + "Household.asset", h =>
            {
                // Same people as the placeholder letters: Lena writes to "Dad" about Mom and little Pip.
                h.members.Add(new FamilyMember { name = "Mom", role = "your wife" });
                h.members.Add(new FamilyMember { name = "Lena", role = "daughter" });
                h.members.Add(new FamilyMember { name = "Pip", role = "son, has a cough", feelsCold = true });
            }, ref created);

            // P13: heating needs power – the fusion cell (Martin, 06.11.); needs are covered by tag, money at the bed = value × 1.35.
            var power = Tag("POWER", "FF9A3C", ref created);
            data.Items["fusion"] = Item("FusionCell", "Fusion cell", "Keeps a small heater running for a night. Hard to come by back home.", 11, ref created, power);
            AddEntry(data.Stocks["Stock_Kiosk_Test"], data.Items["fusion"], 2, log);
            SetNeeds(data.Household, (food, data.Items["ration"]), (power, data.Items["fusion"]), (medicine, data.Items["painkillers"]), log);

            RenameStim(log);
            BuildPriceList(data, contraband, medicine, gadget, food, power, drug, valuable, log, ref created);
            BuildMarket(data, contraband, food, power, drug, log, ref created);
            // P30: the drinks machine in the hall sells Nova Cola, 6 a day.
            data.Stocks["Stock_Vending"] = Stock("Stock_Vending", "NOVA COLA MACHINE", 1f, 0f, "Sold out.", new[] { data.Items["cola"] }, new ItemTag[0], ref created);
            MigrateToEntries(data.Stocks["Stock_Vending"], log);
            SetDailyAmount(data.Stocks["Stock_Vending"], data.Items["cola"], 6);
            BuildGoodsList2(data, contraband, gadget, valuable, confiscated, log, ref created);
            // P132: five cassettes (CassetteBuilder; no goods: unique, no labels, nobody buys them back), two on the kiosk shelf, one a day.
            foreach (var (cassette, fromDay) in CassetteBuilder.EnsureItems(log, ref created))
                AddEntry(data.Stocks["Stock_Kiosk_Test"], cassette, 1, log, fromDay);
            // P08: numbers for confiscation and the shift-end answer; the goods list follows the cargo words (P82).
            data.Confiscation = LoadOrCreate<ConfiscationTuning>(EconomyFolder + "ConfiscationTuning.asset", _ => { }, ref created);
            data.Confiscation.goods = data.Items.Values.Where(i => i != null && !string.IsNullOrEmpty(i.CargoWord)).OrderBy(i => i.name).ToList();
            // P110: what drone customs looks for.
            if (data.Confiscation.contrabandTag == null) data.Confiscation.contrabandTag = contraband;
            if (data.Confiscation.confiscatedTag == null) data.Confiscation.confiscatedTag = confiscated;
            EditorUtility.SetDirty(data.Confiscation);

            AssetDatabase.SaveAssets();
            if (created > 0) log.Add($"{created} test assets created");
            return data;
        }

        /// <summary>
        /// P64: contraband and confiscated goods are bought only under the counter, and the black market buys nothing else.
        /// Runs on existing assets too (LoadOrCreate only creates missing ones); only adds, never removes what was tuned by hand.
        /// </summary>
        private static void ApplyTradeRules(Data data, ItemTag[] illegal, ItemTag drug, ItemTag valuable, List<string> log)
        {
            int changed = 0;
            changed += AddToList(data.Stocks["Stock_Kiosk_Test"], "refusedTags", illegal);
            changed += AddToList(data.Stocks["Stock_Bar_Test"], "refusedTags", illegal);
            changed += AddToList(data.Stocks["Stock_BlackMarket_Test"], "onlyTags", illegal);
            changed += AddToList(data.Items["spice"], "tags", new[] { drug });
            changed += AddToList(data.Items["watch"], "tags", new[] { valuable });
            if (changed > 0) log.Add($"trade rules: {changed} labels added");
        }

        // Daily amounts of the test stocks (P11); anything else gets 3.
        private static readonly Dictionary<string, int> DailyAmounts = new Dictionary<string, int>
        {
            { "Item_DocumentScanner", 1 }, { "Item_RationPack", 3 }, { "Item_Painkillers", 2 }, { "Item_NovaCola", 5 }, { "Item_Spice", 3 },
        };

        /// <summary>P11: moves the old item list (without amounts) into entries with a daily amount. Leaves tuned entries alone.</summary>
        private static void MigrateToEntries(VendorStock stock, List<string> log)
        {
            var so = new SerializedObject(stock);
            var items = so.FindProperty("items");
            var entries = so.FindProperty("entries");
            if (items.arraySize == 0) return;
            for (int i = 0; i < items.arraySize; i++)
            {
                var item = items.GetArrayElementAtIndex(i).objectReferenceValue;
                if (item == null) continue;
                entries.arraySize++;
                var e = entries.GetArrayElementAtIndex(entries.arraySize - 1);
                e.FindPropertyRelative("item").objectReferenceValue = item;
                e.FindPropertyRelative("dailyAmount").intValue = DailyAmounts.TryGetValue(item.name, out int n) ? n : 3;
            }
            items.arraySize = 0;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(stock);
            log.Add($"{stock.name}: {entries.arraySize} entries with daily amounts");
        }

        /// <summary>
        /// P70 price list (Martin, 06.11.): real goods with base market price, tags, daily amount and a fixed price change per day
        /// (day 1 / 2 / 3). Creates missing items, sets daily changes only where none are set, adds stock entries that are missing.
        /// Holly (kiosk) sells everyday goods, the dealer contraband; valuables have no vendor (confiscation/rewards later).
        /// </summary>
        private static void BuildPriceList(Data data, ItemTag contraband, ItemTag medicine, ItemTag gadget, ItemTag food, ItemTag power,
            ItemTag drug, ItemTag valuable, List<string> log, ref int created)
        {
            var perishable = Tag("PERISHABLE", "9BE05A", ref created);
            var tech = Tag("TECH", "5EC8FF", ref created);
            var weapon = Tag("WEAPON", "444B55", ref created);
            var alien = Tag("ALIEN", "C27BD6", ref created);
            var luxury = Tag("LUXURY", "B9824A", ref created);
            var kiosk = data.Stocks["Stock_Kiosk_Test"];
            var dealer = data.Stocks["Stock_BlackMarket_Test"];

            // Nova Cola is a small luxury, not family food: with FOOD a 2 CR can would cover the family's dinner.
            if (RemoveTag(data.Items["cola"], food)) log.Add("Nova Cola: FOOD removed");
            AddToList(data.Items["cola"], "tags", new Object[] { luxury });

            int made = 0;
            void Goods(string key, string file, string name, string text, int value, ItemTag[] tags, VendorStock stock, int amount, params float[] change)
            {
                if (!data.Items.TryGetValue(key, out var item)) data.Items[key] = item = Item(file, name, text, value, ref made, tags);
                SetDailyChange(item, change);
                if (stock != null) AddEntry(stock, item, amount, log);
            }

            Goods("protein", "ProteinBar", "Protein bar", "Tastes of nothing. Fills a stomach for a day.", 5, new[] { food }, kiosk, 4, 0f, 0f, 0.2f);
            Goods("ration", "RationPack", "", "", 8, null, kiosk, 3, 0f, 0.1f, 0.25f);
            Goods("stew", "CannedStew", "Canned stew", "Real meat, they say. A warm meal for the whole family.", 12, new[] { food }, kiosk, 2, 0f, 0f, 0.1f);
            Goods("fruit", "FreshFruit", "Fresh fruit", "Grown on the station's hydro deck. Won't last long.", 14, new[] { food, perishable }, kiosk, 1, 0f, -0.1f, 0.3f);
            Goods("battery", "BatteryPack", "Battery pack", "Enough charge for a heater and a lamp, one night.", 6, new[] { power }, kiosk, 3, 0f, 0.1f, 0f);
            Goods("fusion", "FusionCell", "", "", 11, null, kiosk, 2, 0f, 0.15f, 0.2f);
            Goods("syrup", "CoughSyrup", "Cough syrup", "Sweet, sticky and better than nothing.", 9, new[] { medicine }, kiosk, 2, 0f, 0f, 0.15f);
            Goods("painkillers", "Painkillers", "", "", 15, null, kiosk, 2, 0f, 0.1f, 0.3f);
            Goods("cola", "NovaCola", "", "", 2, null, kiosk, 5, 0f, 0f, 0f);
            // P16: on the shelf from day 6, once contradicting papers get common.
            Goods("scanner", "DocumentScanner", "", "", 60, null, null, 0, 0f, 0f, -0.1f);
            AddEntry(kiosk, data.Items["scanner"], 1, log, 6);
            SetFields(data.Items["scanner"], ("description", "Click it on your desk: fields that contradict each other across the papers light up."));
            Goods("spice", "Spice", "", "", 12, null, dealer, 3, 0f, 0.2f, -0.1f);
            Goods("stim", "StimulantVials", "Stimulant vials", "Keeps you standing. Not legal anywhere decent.", 18, new[] { contraband, drug, medicine }, dealer, 2, 0f, 0f, 0.2f);
            Goods("antibiotics", "Antibiotics", "Antibiotics", "Military stock. The clinic back home would kill for these.", 30, new[] { contraband, medicine }, dealer, 1, 0f, 0.1f, 0.4f);
            Goods("chip", "DataChip", "Data chip", "Someone's records. Someone else wants them.", 25, new[] { contraband, tech }, dealer, 2, 0f, -0.1f, 0f);
            Goods("pistol", "Pistol", "Pistol", "Small, old and loaded.", 60, new[] { contraband, weapon }, dealer, 1, 0f, 0f, 0.1f);
            Goods("watch", "PocketWatch", "", "", 20, null, null, 0, 0f, 0f, 0f);
            Goods("ring", "SilverRing", "Silver ring", "Engraved on the inside. The name is scratched out.", 35, new[] { valuable }, null, 0, 0f, 0.1f, 0f);
            Goods("trinket", "AlienTrinket", "Alien trinket", "Warm to the touch. Nobody knows what it does.", 30, new[] { alien, valuable }, null, 0, 0f, 0f, 0.2f);
            // P71: a cheap TECH item, so the barkeeper's TECH wish costs ~6 CR instead of a 44 CR data chip.
            Goods("fuse", "SpareFuse", "Spare fuse", "A fuse for a station console. Somebody always needs one.", 6, new[] { tech }, kiosk, 2, 0f, 0f, 0f);
            created += made;
        }

        /// <summary>P82: "Stim injector" is "Stimulant vials" now, like the cargo line and the STIMULANT rule (asset moved, GUID kept).</summary>
        private static void RenameStim(List<string> log)
        {
            const string oldPath = ItemFolder + "Item_StimInjector.asset", newPath = ItemFolder + "Item_StimulantVials.asset";
            if (AssetDatabase.LoadAssetAtPath<ItemData>(oldPath) == null || AssetDatabase.LoadAssetAtPath<ItemData>(newPath) != null) return;
            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error)) { log.Add("stim rename failed: " + error); return; }
            SetFields(AssetDatabase.LoadAssetAtPath<ItemData>(newPath), ("displayName", "Stimulant vials"));
            log.Add("Stim injector -> Stimulant vials");
        }

        private const string ConfiscatedFolder = ItemFolder + "Confiscated/";
        private const string IconFolder = "Assets/_Project/Art/UI/Icons/";

        // P82: the word a cargo manifest uses for each item (whole word, upper case). P15 writes cargo with these, P08 confiscates by them.
        private static readonly Dictionary<string, string> CargoWords = new Dictionary<string, string>
        {
            { "ration", "RATION" }, { "protein", "PROTEIN" }, { "stew", "STEW" }, { "fruit", "FRUIT" }, { "cola", "COLA" },
            { "battery", "BATTERY" }, { "fusion", "FUSION" }, { "fuse", "FUSE" }, { "clownshoes", "CLOWN" },
            { "syrup", "SYRUP" }, { "painkillers", "PAINKILLERS" }, { "antibiotics", "ANTIBIOTICS" }, { "stim", "STIMULANT" },
            { "spice", "SPICE" }, { "chip", "CHIP" }, { "pistol", "PISTOL" }, { "weaponparts", "WEAPON" },
            { "watch", "WATCH" }, { "ring", "RING" }, { "trinket", "TRINKET" }, { "cash", "CASH" }, { "liquor", "LIQUOR" }, { "specimen", "SPECIMEN" },
        };

        // P82: delivered models that belong to an item (Pill bottle = Painkillers, Martin 07.11.).
        private static readonly Dictionary<string, string> ItemModels = new Dictionary<string, string>
        {
            { "painkillers", "Assets/_Project/Art/Models/Items/SM_Item_PillBottle.fbx" },
            { "chip", "Assets/_Project/Art/Models/Items/SM_Item_DataChip.fbx" },
            { "scanner", "Assets/_Project/Art/Models/Items/SM_Item_DocScanner.fbx" },
            { "liquor", "Assets/_Project/Art/Models/Items/SM_Item_Liquor.fbx" },
            { "weaponparts", "Assets/_Project/Art/Models/Items/SM_Item_WeaponParts.fbx" },
            { "cash", "Assets/_Project/Art/Models/Items/SM_Item_CashBundle.fbx" },
            { "specimen", "Assets/_Project/Art/Models/Items/SM_Item_AlienSpecimen.fbx" },
            { "uvlamp", "Assets/_Project/Art/Models/Items/SM_Item_UVLamp.fbx" },
            { "clownshoes", "Assets/_Project/Art/Models/Items/SM_Item_ClownShoes.fbx" },
            { "cola", "Assets/_Project/Art/Models/Props/SM_Prop_SodaCan.fbx" },
        };

        /// <summary>
        /// P82 goods list 2 (call 07.11.): delivered models become goods, every item gets its icon, model and cargo word, and everything
        /// travelers can carry gets a CONFISCATED variant (own item: same labels + CONFISCATED, so confiscated medicine still helps the
        /// family, but only the black market buys it). Goods are no stronger than others - they are another way to get them (Martin).
        /// Hand-set icon/model/cargo word stay; variants are generated and synced on every run.
        /// </summary>
        private static void BuildGoodsList2(Data data, ItemTag contraband, ItemTag gadget, ItemTag valuable, ItemTag confiscated, List<string> log, ref int created)
        {
            var weapon = Tag("WEAPON", "444B55", ref created);
            var alien = Tag("ALIEN", "C27BD6", ref created);
            var luxury = Tag("LUXURY", "B9824A", ref created);
            int made = 0;
            void New(string key, string file, string name, string text, int value, params ItemTag[] tags)
            {
                if (!data.Items.ContainsKey(key)) data.Items[key] = Item(file, name, text, value, ref made, tags);
            }
            New("liquor", "Liquor", "Liquor", "Station-brewed, strong enough to strip paint. Someone always wants a bottle.", 14, luxury);
            New("weaponparts", "WeaponParts", "Weapon parts", "Springs, a barrel, a trigger group. Nobody needs them for anything good.", 45, contraband, weapon);
            New("cash", "CashBundle", "Cash bundle", "Old notes in a rubber band. Hard to explain at the counter.", 40, valuable);
            New("specimen", "AlienSpecimen", "Alien specimen", "Something in a jar. It moved once. Collectors pay well.", 50, alien, contraband);
            New("uvlamp", "UVLamp", "UV lamp", "Shows hidden seals on papers. Forged ones glow red.", 45, gadget);
            SetFields(data.Items["uvlamp"], ("unique", true), ("description", "Click it on your desk: every seal shows up, forged ones glow red."));
            AddEntry(data.Stocks["Stock_Kiosk_Test"], data.Items["uvlamp"], 1, log, 4); // P62: forged seals start on day 5
            // P63: comes off Pim Oddo's crates at the counter; only wearing them makes them worth anything to you.
            New("clownshoes", "ClownShoes", "Clown shoes", "Size 48. They squeak at every step, and nobody can tell you not to.", 8, luxury);
            SetFields(data.Items["clownshoes"], ("wearable", true));

            if (!AssetDatabase.IsValidFolder(ConfiscatedFolder.TrimEnd('/'))) AssetDatabase.CreateFolder(ItemFolder.TrimEnd('/'), "Confiscated");
            int icons = 0, models = 0, variants = 0;
            foreach (var pair in data.Items)
            {
                var item = pair.Value;
                if (item == null) continue;
                var so = new SerializedObject(item);
                var icon = so.FindProperty("icon");
                if (icon.objectReferenceValue == null)
                {
                    var sprite = AssetDatabase.LoadAllAssetsAtPath(IconFolder + "UI_Icon_" + item.name + ".png").OfType<Sprite>().FirstOrDefault();
                    if (sprite != null) { icon.objectReferenceValue = sprite; icons++; }
                }
                var model = so.FindProperty("model");
                if (model.objectReferenceValue == null && ItemModels.TryGetValue(pair.Key, out var modelPath))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                    if (go != null) { model.objectReferenceValue = go; models++; }
                }
                var word = so.FindProperty("cargoWord");
                if (string.IsNullOrEmpty(word.stringValue) && CargoWords.TryGetValue(pair.Key, out var w)) word.stringValue = w;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(item);

                if (item.Unique || string.IsNullOrEmpty(item.CargoWord)) continue;
                var variant = LoadOrCreate<ItemData>(ConfiscatedFolder + "Item_Confiscated_" + item.name.Substring("Item_".Length) + ".asset", _ => { }, ref made);
                SyncVariant(item, variant, confiscated);
                SetObject(item, "confiscatedVariant", variant);
                variants++;
            }
            created += made;
            log.Add($"goods list 2: {icons} icons, {models} models set, {variants} confiscated variants");
        }

        /// <summary>P82: the variant mirrors its original (name, text, price, daily change, labels + CONFISCATED, icon, model); no cargo word.</summary>
        private static void SyncVariant(ItemData item, ItemData variant, ItemTag confiscated)
        {
            string name = item.DisplayName;
            string lower = name.StartsWith("Nova") ? name : char.ToLowerInvariant(name[0]) + name.Substring(1);
            var src = new SerializedObject(item);
            var so = new SerializedObject(variant);
            so.FindProperty("displayName").stringValue = "Confiscated " + lower;
            so.FindProperty("description").stringValue = item.Description + " Taken at the counter - only the black market will touch it.";
            so.FindProperty("baseValue").intValue = item.BaseValue;
            so.CopyFromSerializedProperty(src.FindProperty("dailyChange"));
            so.FindProperty("icon").objectReferenceValue = item.Icon;
            so.FindProperty("model").objectReferenceValue = item.Model;
            so.FindProperty("cargoWord").stringValue = "";
            so.FindProperty("wearable").boolValue = item.Wearable;
            so.FindProperty("confiscatedVariant").objectReferenceValue = null;
            var tags = so.FindProperty("tags");
            var list = item.Tags.Where(t => t != confiscated).ToList();
            list.Add(confiscated);
            tags.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++) tags.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(variant);
        }

        private static void SetObject(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private const string MarketFolder = "Assets/_Project/Data/Market/";
        private const string SchedulePath = "Assets/_Project/Data/Days/DaySchedule.asset";

        /// <summary>
        /// P68 market (Martin, 07.11.): label base factors (only where still 1), the placeholder events and their days (only where the
        /// day has none), spoiling for PERISHABLE, and the one-time Spice correction (day 3 change 0 instead of −10 %).
        /// </summary>
        private static void BuildMarket(Data data, ItemTag contraband, ItemTag food, ItemTag power, ItemTag drug, List<string> log, ref int created)
        {
            var weapon = Tag("WEAPON", "444B55", ref created);
            var valuable = Tag("VALUABLE", "FFD36B", ref created);
            var luxury = Tag("LUXURY", "B9824A", ref created);
            var perishable = Tag("PERISHABLE", "9BE05A", ref created);
            foreach (var (tag, factor) in new[] { (contraband, 1.3f), (weapon, 1.2f), (valuable, 1.2f), (luxury, 1.2f), (perishable, 0.8f) })
            {
                var so = new SerializedObject(tag);
                var p = so.FindProperty("priceFactor");
                if (!Mathf.Approximately(p.floatValue, 1f)) continue; // tuned by hand: leave it
                p.floatValue = factor;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(tag);
            }
            if (data.Tuning.perishableTag == null) { data.Tuning.perishableTag = perishable; EditorUtility.SetDirty(data.Tuning); }

            // Spice: day 3 without the dip, so the barkeeper's rumour (P65) pays off.
            var spice = new SerializedObject(data.Items["spice"]);
            var change = spice.FindProperty("dailyChange");
            if (change.arraySize == 3 && Mathf.Approximately(change.GetArrayElementAtIndex(2).floatValue, -0.1f))
            {
                change.GetArrayElementAtIndex(2).floatValue = 0f;
                spice.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data.Items["spice"]);
                log.Add("spice day 3 change 0");
            }

            var kiosk = data.Stocks["Stock_Kiosk_Test"];
            var dealer = data.Stocks["Stock_BlackMarket_Test"];
            var border = LoadOrCreate<MarketEvent>(MarketFolder + "Market_BorderClosed.asset", e =>
            {
                e.bulletinLine = "Border closed on the home planet. Food deliveries are cut - food is scarce and expensive.";
                e.announcement = "Attention: due to the border closure, food supplies on the station are limited.";
                e.labels.Add(new LabelEffect { tag = food, price = 1.3f, amount = 0.5f });
                e.stock.Add(new StockChange { stock = kiosk, item = data.Items["fruit"], amount = 0 });
            }, ref created);
            var smugglers = LoadOrCreate<MarketEvent>(MarketFolder + "Market_SmugglersBusted.asset", e =>
            {
                e.bulletinLine = "Smuggler ring busted by station security. Drugs are hard to get; contraband sells high.";
                e.announcement = "Security notice: a smuggler ring has been arrested. Report any contraband at once.";
                e.labels.Add(new LabelEffect { tag = drug, price = 2.0f }); // 1.8 → 2.0 (P76 rebalancing: one raid on day 6 instead of every cycle)
                e.labels.Add(new LabelEffect { tag = contraband, buy = 1.3f });
                e.stock.Add(new StockChange { stock = dealer, item = data.Items["spice"], amount = 2 });
            }, ref created);
            var cold = LoadOrCreate<MarketEvent>(MarketFolder + "Market_ColdSnap.asset", e =>
            {
                e.bulletinLine = "Cold snap on the home planet. Power cells are in demand.";
                e.announcement = "Weather report from home: a cold snap. Power cells are in short supply.";
                e.labels.Add(new LabelEffect { tag = power, price = 1.25f });
            }, ref created);

            // P65: what the barkeeper whispers the evening before (only where still empty).
            void Rumour(MarketEvent e, string text)
            {
                if (!string.IsNullOrWhiteSpace(e.rumour)) return;
                e.rumour = text;
                EditorUtility.SetDirty(e);
            }
            Rumour(border, "Word is the border on the home planet closes tomorrow. Food will be scarce - stock up tonight.");
            Rumour(smugglers, "Security is about to raid a smuggler ring. By tomorrow drugs will be worth a fortune.");
            Rumour(cold, "A cold front is coming back home. Power cells will be dear tomorrow.");

            // P36: how hard each event shakes the station (only where still 0).
            void Crisis(MarketEvent e, float value)
            {
                if (e.crisis > 0f) return;
                e.crisis = value;
                EditorUtility.SetDirty(e);
            }
            Crisis(border, 0.4f);
            Crisis(smugglers, 0.3f);
            Crisis(cold, 0.3f);

            // P76 (docs/FIGUREN.md): events the recurring characters set off, plus the fixed inspection week (day 9).
            var medicine = Tag("MEDICINE", "2F5B66", ref created);
            var tech = Tag("TECH", "5EC8FF", ref created);
            var lockdown = LoadOrCreate<MarketEvent>(MarketFolder + "Market_SecurityLockdown.asset", e =>
            {
                e.bulletinLine = "Security lockdown after an incident at the counter. Weapons and tech are hard to get.";
                e.announcement = "Attention: station security has raised the alert level. Expect searches.";
                e.rumour = "Security is jumpy tomorrow. Somebody got through who shouldn't have.";
                e.labels.Add(new LabelEffect { tag = weapon, price = 1.4f, amount = 0.5f });
                e.labels.Add(new LabelEffect { tag = tech, price = 1.2f });
            }, ref created);
            var clinic = LoadOrCreate<MarketEvent>(MarketFolder + "Market_ClinicShortage.asset", e =>
            {
                e.bulletinLine = "Clinic shortage: the station clinic is out of antibiotics. Medicine is scarce and expensive.";
                e.announcement = "Notice from the station clinic: medical supplies are low. Please be patient.";
                e.rumour = "The clinic's shelves will be bare tomorrow. Medicine will cost you.";
                e.labels.Add(new LabelEffect { tag = medicine, price = 1.5f, amount = 0.5f });
            }, ref created);
            var strike = LoadOrCreate<MarketEvent>(MarketFolder + "Market_DockStrike.asset", e =>
            {
                e.bulletinLine = "Dock strike: the dock workers have laid down their tools. Fewer goods on every shelf.";
                e.announcement = "Attention: due to industrial action at the docks, deliveries are delayed.";
                e.rumour = "The dockers walk out tomorrow. Buy what you need tonight.";
                e.labels.Add(new LabelEffect { tag = food, amount = 0.6f });
                e.labels.Add(new LabelEffect { tag = power, amount = 0.6f });
                e.labels.Add(new LabelEffect { tag = medicine, amount = 0.6f });
            }, ref created);
            var inspection = LoadOrCreate<MarketEvent>(MarketFolder + "Market_InspectionWeek.asset", e =>
            {
                e.bulletinLine = "Inspection week: the Border Authority audits the station. Contraband is hard to move.";
                e.announcement = "The Border Authority is on board for its yearly inspection. Every decision is being reviewed.";
                e.rumour = "Inspectors on board tomorrow. Keep your hands clean.";
                e.labels.Add(new LabelEffect { tag = contraband, buy = 0.7f, amount = 0.5f });
            }, ref created);
            var caravan = LoadOrCreate<MarketEvent>(MarketFolder + "Market_OddosCaravan.asset", e =>
            {
                e.bulletinLine = "Oddo's caravan is in dock: luxuries at bargain prices, today only.";
                e.announcement = "Pim Oddo's travelling market has arrived at dock two. Everything must go!";
                e.rumour = "Old Oddo's caravan docks tomorrow. Luxuries for a song.";
                e.labels.Add(new LabelEffect { tag = luxury, price = 0.7f, amount = 1.5f });
            }, ref created);
            Crisis(lockdown, 0.4f);
            Crisis(clinic, 0.3f);
            Crisis(strike, 0.4f);
            Crisis(inspection, 0.3f);
            Crisis(caravan, 0.05f);

            var schedule = AssetDatabase.LoadAssetAtPath<GameOff.Day.DaySchedule>(SchedulePath);
            if (schedule == null) { log.Add("no DaySchedule - market events not placed"); return; }
            var sso = new SerializedObject(schedule);
            var days = sso.FindProperty("days");
            bool placed = false;

            // P65 bar wishes by label (Martin, 07.11.): day 1 two LUXURY (Nova Cola), day 2 one TECH (data chip); day 3 none.
            void Wish(int day, ItemTag tag, int amount)
            {
                if (days.arraySize < day) return;
                var wish = days.GetArrayElementAtIndex(day - 1).FindPropertyRelative("barWish");
                if (wish.FindPropertyRelative("amount").intValue > 0) return;
                wish.FindPropertyRelative("tag").objectReferenceValue = tag;
                wish.FindPropertyRelative("amount").intValue = amount;
                placed = true;
            }
            Wish(1, Tag("LUXURY", "B9824A", ref created), 2);
            Wish(2, Tag("TECH", "5EC8FF", ref created), 1);

            // P66 bribe (Martin, 07.11.): day 3, the smuggler Quill Barragh, 60 CR – only where the day has none. Not day 2: refusing
            // makes the barkeeper sulk, and day 2 is the evening of the one rumour that pays.
            var quill = AssetDatabase.LoadAssetAtPath<GameOff.Travelers.TravelerData>("Assets/_Project/Data/Travelers/Test/T_Test_04_Quill.asset");
            if (quill != null && days.arraySize >= 3)
            {
                var bribe = days.GetArrayElementAtIndex(2).FindPropertyRelative("bribe");
                if (bribe.FindPropertyRelative("traveler").objectReferenceValue == null)
                {
                    bribe.FindPropertyRelative("traveler").objectReferenceValue = quill;
                    bribe.FindPropertyRelative("amount").intValue = 60;
                    placed = true;
                }
            }
            void Place(int day, params MarketEvent[] events)
            {
                if (days.arraySize < day) return;
                var list = days.GetArrayElementAtIndex(day - 1).FindPropertyRelative("marketEvents");
                if (list.arraySize > 0) return; // the day has its own events already
                list.arraySize = events.Length;
                for (int i = 0; i < events.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = events[i];
                placed = true;
            }
            BalanceCorrections(data, border, smugglers, cold, contraband, drug, log);
            Place(2, border);
            Place(3, smugglers, cold);
            if (!placed) return;
            sso.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(schedule);
            log.Add("day schedule: market events and bar wishes placed where empty");
        }

        /// <summary>
        /// P71 balancing (Martin, 07.11.), one-time corrections of the P68 placeholders – each only if the value is still the old one:
        /// events take the cheapest goods away (border: protein bar; cold snap: battery pack), the rumour pays 5–8 CR per unit
        /// (DRUG ×1.5 instead of ×1.8, no extra contraband buy price, Spice without the day-2 rise). See docs/ECONOMY-REPORT.md.
        /// </summary>
        private static void BalanceCorrections(Data data, MarketEvent border, MarketEvent smugglers, MarketEvent cold, ItemTag contraband,
            ItemTag drug, List<string> log)
        {
            var kiosk = data.Stocks["Stock_Kiosk_Test"];
            void Missing(MarketEvent e, ItemData item)
            {
                if (e.stock.Exists(c => c.stock == kiosk && c.item == item)) return;
                e.stock.Add(new StockChange { stock = kiosk, item = item, amount = 0 });
                EditorUtility.SetDirty(e);
                log.Add($"{e.name}: {item.name} missing that day");
            }
            Missing(border, data.Items["protein"]);
            Missing(cold, data.Items["battery"]);

            var drugEffect = smugglers.labels.Find(l => l.tag == drug);
            if (drugEffect != null && Mathf.Approximately(drugEffect.price, 1.8f)) { drugEffect.price = 1.5f; EditorUtility.SetDirty(smugglers); }
            if (smugglers.labels.RemoveAll(l => l.tag == contraband && Mathf.Approximately(l.buy, 1.3f) && Mathf.Approximately(l.price, 1f)) > 0)
                EditorUtility.SetDirty(smugglers);

            var spice = new SerializedObject(data.Items["spice"]);
            var change = spice.FindProperty("dailyChange");
            if (change.arraySize == 3 && Mathf.Approximately(change.GetArrayElementAtIndex(1).floatValue, 0.2f))
            {
                change.GetArrayElementAtIndex(1).floatValue = 0f;
                spice.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(data.Items["spice"]);
            }
        }

        private static bool RemoveEntry(VendorStock stock, ItemData item)
        {
            var so = new SerializedObject(stock);
            var entries = so.FindProperty("entries");
            for (int i = 0; i < entries.arraySize; i++)
            {
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("item").objectReferenceValue != item) continue;
                entries.DeleteArrayElementAtIndex(i);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stock);
                return true;
            }
            return false;
        }

        private static void SetDailyChange(ItemData item, float[] change)
        {
            var so = new SerializedObject(item);
            var p = so.FindProperty("dailyChange");
            if (p.arraySize > 0 || change.Length == 0) return; // tuned by hand: leave it
            p.arraySize = change.Length;
            for (int i = 0; i < change.Length; i++) p.GetArrayElementAtIndex(i).floatValue = change[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        private static bool RemoveTag(ItemData item, ItemTag tag)
        {
            var so = new SerializedObject(item);
            var p = so.FindProperty("tags");
            for (int i = 0; i < p.arraySize; i++)
            {
                if (p.GetArrayElementAtIndex(i).objectReferenceValue != tag) continue;
                p.DeleteArrayElementAtIndex(i);
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(item);
                return true;
            }
            return false;
        }

        /// <summary>Adds an item to a stock if it isn't sold there yet (existing assets too).</summary>
        private static void AddEntry(VendorStock stock, ItemData item, int dailyAmount, List<string> log, int fromDay = 1)
        {
            if (stock.Sells(item)) return;
            var so = new SerializedObject(stock);
            var entries = so.FindProperty("entries");
            entries.arraySize++;
            var e = entries.GetArrayElementAtIndex(entries.arraySize - 1);
            e.FindPropertyRelative("item").objectReferenceValue = item;
            e.FindPropertyRelative("dailyAmount").intValue = dailyAmount;
            e.FindPropertyRelative("fromDay").intValue = fromDay;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(stock);
            log.Add($"{item.name} added to {stock.name} (from day {fromDay})");
        }

        /// <summary>P13: tag and price item per family need, only where still empty (tuned values stay).</summary>
        private static void SetNeeds(HouseholdTuning h, (ItemTag tag, ItemData price) food, (ItemTag tag, ItemData price) heat,
            (ItemTag tag, ItemData price) medicine, List<string> log)
        {
            bool changed = false;
            if (h.foodTag == null) { h.foodTag = food.tag; changed = true; }
            if (h.foodPriceItem == null) { h.foodPriceItem = food.price; changed = true; }
            if (h.heatTag == null) { h.heatTag = heat.tag; changed = true; }
            if (h.heatPriceItem == null) { h.heatPriceItem = heat.price; changed = true; }
            if (h.medicineTag == null) { h.medicineTag = medicine.tag; changed = true; }
            if (h.medicinePriceItem == null) { h.medicinePriceItem = medicine.price; changed = true; }
            if (!changed) return;
            EditorUtility.SetDirty(h);
            log.Add($"needs: food {h.MoneyCost(FamilyNeed.Food)}, heat {h.MoneyCost(FamilyNeed.Heat)}, medicine {h.MoneyCost(FamilyNeed.Medicine)} at the bed");
        }

        private static int AddToList(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            int added = 0;
            foreach (var v in values)
            {
                bool has = false;
                for (int i = 0; i < p.arraySize; i++) has |= p.GetArrayElementAtIndex(i).objectReferenceValue == v;
                if (has) continue;
                p.arraySize++;
                p.GetArrayElementAtIndex(p.arraySize - 1).objectReferenceValue = v;
                added++;
            }
            if (added > 0) { so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(target); }
            return added;
        }

        private static T LoadOrCreate<T>(string path, System.Action<T> init, ref int created) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
            created++;
            return asset;
        }

        private static ItemTag Tag(string name, string hex, ref int created) =>
            LoadOrCreate<ItemTag>(TagFolder + "Tag_" + name + ".asset", t => SetFields(t, ("displayName", name), ("color", HexColor(hex))), ref created);

        private static ItemData Item(string file, string name, string description, int value, ref int created, params ItemTag[] tags) =>
            LoadOrCreate<ItemData>(ItemFolder + "Item_" + file + ".asset", i =>
            {
                SetFields(i, ("displayName", name), ("description", description), ("baseValue", value));
                SetList(i, "tags", tags);
            }, ref created);

        private static VendorStock Stock(string file, string title, float sell, float buy, string empty, ItemData[] items, ItemTag[] refused, ref int created) =>
            LoadOrCreate<VendorStock>(EconomyFolder + file + ".asset", s =>
            {
                SetFields(s, ("title", title), ("sellFactor", sell), ("buyFactor", buy), ("emptyText", empty), ("openAfterShiftOnly", true));
                SetList(s, "items", items);
                SetList(s, "refusedTags", refused);
            }, ref created);

        private static void SetFields(Object target, params (string field, object value)[] values)
        {
            var so = new SerializedObject(target);
            foreach (var (field, value) in values)
            {
                var p = so.FindProperty(field);
                switch (value)
                {
                    case string s: p.stringValue = s; break;
                    case int i: p.intValue = i; break;
                    case float f: p.floatValue = f; break;
                    case bool b: p.boolValue = b; break;
                    case Color c: p.colorValue = c; break;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetList(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color HexColor(string hex) => ColorUtility.TryParseHtmlString("#" + hex, out var c) ? c : Color.magenta;

        // ---------- Prefab ----------

        private static GameObject BuildPrefab(Data data, List<string> log)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("PF_Economy");
            var economy = root.AddComponent<PlayerEconomy>();
            Set(economy, "tuning", data.Tuning);
            Set(economy, "household", data.Household);
            SetList(economy, "devStartItems", new Object[] { data.Items["spice"], data.Items["watch"] });

            var canvas = CreateCanvas("EconomyCanvas", 60); // above the day UI (50), below the pause menu (100)
            canvas.SetParent(root.transform, false);
            var trade = BuildTradeWindow(canvas);
            var inventory = BuildInventoryWindow(canvas);
            BuildSettlementWindow(canvas);
            var report = BuildShiftReportWindow(canvas);
            var confiscation = root.AddComponent<Confiscation>();
            Set(confiscation, "tuning", data.Confiscation);
            int moraleCreated = 0;
            Set(confiscation, "morale", LoadOrCreate<MoraleTuning>(EconomyFolder + "MoraleTuning.asset", _ => { }, ref moraleCreated));
            Set(confiscation, "window", report);

            var vendors = new GameObject("Vendors").transform;
            vendors.SetParent(root.transform, false);
            foreach (var spot in Vendors) BuildVendor(vendors, spot, data.Stocks[spot.Stock], trade);
            BuildDrone(root.transform, trade);
            BuildVendingMachine(root.transform, data.Stocks["Stock_Vending"]);
            // P66: the bribe note lies behind the stamps on the left of the counter desk, where nothing else sits (#32: it used to lie under them).
            var bribeGo = new GameObject("BribeNote");
            bribeGo.transform.SetParent(root.transform, false);
            var bribeSpot = new GameObject("NoteSpot").transform;
            bribeSpot.SetParent(bribeGo.transform, false);
            bribeSpot.SetPositionAndRotation(new Vector3(-0.84f, 0.868f, -8.42f), Quaternion.Euler(0f, 84f, 0f)); // above the envelope, which is 1.3 cm high
            var bribe = bribeGo.AddComponent<BribeNote>();
            Set(bribe, "spot", bribeSpot);
            // P80: the envelope the note came in lies under it, only while the note lies there.
            var envelopeModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/Props/SM_Prop_BribeEnvelope.fbx");
            if (envelopeModel != null)
            {
                var envelope = (GameObject)PrefabUtility.InstantiatePrefab(envelopeModel);
                envelope.name = "Envelope";
                envelope.transform.SetParent(bribeGo.transform, false);
                envelope.transform.SetPositionAndRotation(bribeSpot.position + new Vector3(0.05f, -0.014f, 0.12f), Quaternion.Euler(0f, 70f, 0f));
                envelope.SetActive(false);
                Set(bribe, "envelope", envelope);
            }

            BuildTelescope(root.transform, canvas);
            var hangarShips = BuildHangarShips(root.transform);
            var arrivals = new GameObject("ArrivingGroup").AddComponent<GameOff.Station.ArrivingGroup>();
            arrivals.transform.SetParent(root.transform, false);
            Set(arrivals, "ships", hangarShips);
            Set(arrivals, "looks", AssetDatabase.LoadAssetAtPath<TravelerLooks>("Assets/_Project/Data/Travelers/TravelerLooks.asset"));
            var lockerGo = new GameObject("EvidenceLocker");
            lockerGo.transform.SetParent(root.transform, false);
            var (lockerUse, lockerView) = Trigger(lockerGo.transform, "Trigger", new Vector3(-4f, 0.9f, -6.45f), new Vector3(1.2f, 1.9f, 0.7f), "Evidence locker");
            Object.DestroyImmediate(lockerView);
            var locker = lockerGo.AddComponent<EvidenceLocker>();
            Set(locker, "interactable", lockerUse);
            LockerBoardBuilder.BuildLockerDisplay(lockerGo.transform, locker, confiscation, LockerBoardBuilder.EnsureTuning(log)); // P32
            LockerBoardBuilder.BuildBoardCloseUp(root.transform, canvas); // P33

            BuildCargoCrate(root.transform, data);

            // P101: those let through today stand around after the shift.
            var crowdGo = new GameObject("EveningCrowd");
            crowdGo.transform.SetParent(root.transform, false);
            var crowd = crowdGo.AddComponent<EveningCrowd>();
            Set(crowd, "tuning", AssetDatabase.LoadAssetAtPath<GameOff.Counter.DialogueTuning>("Assets/_Project/Data/Counter/DialogueTuning.asset"));
            Set(crowd, "looks", AssetDatabase.LoadAssetAtPath<TravelerLooks>("Assets/_Project/Data/Travelers/TravelerLooks.asset"));
            var crowdSpots = new List<Object>();
            for (int i = 0; i < CrowdSpots.Length; i++)
            {
                var (position, yaw) = CrowdSpots[i];
                var spot = new GameObject("CrowdSpot_" + i).transform;
                spot.SetParent(crowdGo.transform, false);
                spot.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
                crowdSpots.Add(spot);
            }
            var crowdSo = new SerializedObject(crowd);
            var crowdList = crowdSo.FindProperty("spots");
            crowdList.arraySize = crowdSpots.Count;
            for (int i = 0; i < crowdSpots.Count; i++) crowdList.GetArrayElementAtIndex(i).objectReferenceValue = crowdSpots[i];
            crowdSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            log.Add("PF_Economy built");
            return prefab;
        }

        private static IdleBreathingTuning IdleTuning()
        {
            string path = EconomyFolder + "IdleBreathingTuning.asset";
            var tuning = AssetDatabase.LoadAssetAtPath<IdleBreathingTuning>(path);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<IdleBreathingTuning>();
            AssetDatabase.CreateAsset(tuning, path);
            return tuning;
        }

        /// <summary>P90: the kiosk's roller shutter fills the hatch while it is closed; hidden while open.</summary>
        private static void BuildKioskShutter(Transform parent, Vendor vendor)
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/Kiosk/SM_Kiosk_Shutter.fbx");
            if (model == null) return;
            var shutterT = new GameObject("KioskShutter").transform;
            shutterT.SetParent(parent, false);
            shutterT.position = new Vector3(8.28f, 0f, -4f); // just behind the sign strip on the hall side
            var slats = new GameObject("Slats").transform;
            slats.SetParent(shutterT, false);
            ((GameObject)PrefabUtility.InstantiatePrefab(model)).transform.SetParent(slats, false);
            var shutter = shutterT.gameObject.AddComponent<Counter.SecurityShutter>();
            Set(shutter, "slats", slats);
            SetFields(shutter, ("closedHeight", 2.34f), ("openHeight", 3.64f));
            Set(vendor, "shutter", shutter);
            Set(vendor, "shutterTuning", AssetDatabase.LoadAssetAtPath<Counter.DialogueTuning>("Assets/_Project/Data/Counter/DialogueTuning.asset")); // P109
            slats.gameObject.SetActive(false); // SecurityShutter shows it when it falls
        }

        /// <summary>P107: the ships that can stand in the hangar and their pads, away from the shuttle ramp and the walkways.</summary>
        /// <summary>P35: the flagged crate east of the queue lane, off unless today has a case.</summary>
        private static void BuildCargoCrate(Transform parent, Data data)
        {
            int made = 0;
            var tuning = LoadOrCreate<GameOff.Hangar.CargoCaseTuning>(EconomyFolder + "CargoCases.asset", _ => { }, ref made);
            if (tuning.cases.Count == 0)
            {
                foreach (var (day, traveler, item, count) in new[]
                {
                    (3, "TOMI LARK", "stim", 2), (5, "MAKO DRENN", "spice", 3), (8, "GROM ULLAR", "chip", 1), (10, "ISSK DORR", "trinket", 2),
                })
                    tuning.cases.Add(new GameOff.Hangar.CargoCase { day = day, traveler = traveler, item = data.Items[item], count = count });
                EditorUtility.SetDirty(tuning);
            }

            if (tuning.loopGoods.Count == 0) // P126
            {
                foreach (string item in new[] { "stim", "spice", "chip", "trinket" }) tuning.loopGoods.Add(data.Items[item]);
                EditorUtility.SetDirty(tuning);
            }

            var go = new GameObject("CargoCrate");
            go.transform.SetParent(parent, false);
            var visual = new GameObject("Visual");
            visual.transform.SetParent(go.transform, false);
            visual.transform.SetPositionAndRotation(new Vector3(5.6f, 0f, -15.5f), Quaternion.Euler(0f, 90f, 0f));
            var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/Hangar/SM_Hangar_Crate.fbx"));
            model.transform.SetParent(visual.transform, false);

            var text = new GameObject("Label", typeof(RectTransform), typeof(TMPro.TextMeshPro)).GetComponent<TMPro.TextMeshPro>();
            text.transform.SetParent(visual.transform, false);
            text.transform.localPosition = new Vector3(0f, 0.48f, -0.625f); // on the face toward the lane (local -Z, world -X at yaw 90)
            text.rectTransform.sizeDelta = new Vector2(1.1f, 0.5f);
            text.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/_Project/Fonts/ShareTechMono SDF.asset");
            text.fontSize = 1.1f;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = new Color(0.95f, 0.8f, 0.2f);

            var body = visual.AddComponent<BoxCollider>();
            body.center = new Vector3(0f, 0.5f, 0f);
            body.size = new Vector3(1.2f, 1f, 1.2f);
            var interactable = visual.AddComponent<Interactable>();
            interactable.SetPrompt("Search the crate");

            var crate = go.AddComponent<GameOff.Hangar.CargoCrate>();
            Set(crate, "tuning", tuning);
            Set(crate, "interactable", interactable);
            Set(crate, "visual", visual);
            Set(crate, "label", text);
            visual.SetActive(false);
        }

        private static GameOff.Station.HangarShips BuildHangarShips(Transform parent)
        {
            var go = new GameObject("HangarShips");
            go.transform.SetParent(parent, false);
            var models = new[] { "Courier", "Hauler", "Skiff", "Family" }
                .Select(n => AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Art/Models/Hangar/SM_Hangar_Ship_{n}.fbx")).Where(m => m != null).ToArray();
            var pads = new List<Object>();
            foreach (var (position, yaw) in new[] { (new Vector3(-10f, 0f, -23f), 90f), (new Vector3(10f, 0f, -23f), -90f), (new Vector3(-8f, 0f, -33f), 90f), (new Vector3(10f, 0f, -35f), -90f) })
            {
                var pad = new GameObject("Pad").transform;
                pad.SetParent(go.transform, false);
                pad.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
                pads.Add(pad);
            }
            var ships = go.AddComponent<GameOff.Station.HangarShips>();
            var so = new SerializedObject(ships);
            var shipList = so.FindProperty("ships");
            shipList.arraySize = models.Length;
            for (int i = 0; i < models.Length; i++) shipList.GetArrayElementAtIndex(i).objectReferenceValue = models[i];
            var padList = so.FindProperty("pads");
            padList.arraySize = pads.Count;
            for (int i = 0; i < pads.Count; i++) padList.GetArrayElementAtIndex(i).objectReferenceValue = pads[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return ships;
        }

        /// <summary>P29: trigger at the coin telescope on the deck, eye behind the eyepiece, overlay over the HUD.</summary>
        private static void BuildTelescope(Transform parent, Transform canvas)
        {
            var go = new GameObject("CoinTelescope");
            go.transform.SetParent(parent, false);
            var (interactable, debugView) = Trigger(go.transform, "Trigger", new Vector3(0f, 0.75f, 9.6f), new Vector3(0.9f, 1.8f, 0.9f), "Look through the telescope (1 CR)");
            Object.DestroyImmediate(debugView);
            var eye = new GameObject("Eye").transform;
            eye.SetParent(go.transform, false);
            eye.position = new Vector3(0f, 1.3f, 11.6f); // outside the glass, so no window frames in the view
            // P111: black under the eyepiece frame for the focus blink between planet and home street.
            var blink = new GameObject("TelescopeBlink", typeof(RectTransform), typeof(Image));
            blink.transform.SetParent(canvas, false);
            Stretch(blink.GetComponent<RectTransform>());
            var blinkImage = blink.GetComponent<Image>();
            blinkImage.color = new Color(0f, 0f, 0f, 0f);
            blinkImage.raycastTarget = false;
            blink.SetActive(false);
            var overlay = new GameObject("TelescopeOverlay", typeof(RectTransform), typeof(Image));
            overlay.transform.SetParent(canvas, false);
            Stretch(overlay.GetComponent<RectTransform>());
            var image = overlay.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/UI/UI_Telescope_Overlay.png").OfType<Sprite>().FirstOrDefault();
            image.color = Color.white;
            image.raycastTarget = false;
            // The overlay's time panel has fixed digits painted in: a black box hides them.
            var digits = Box(overlay.transform, "Digits", new Vector2(56f, -229f), new Vector2(104f, 42f), Color.black);
            var count = Text(digits, "Countdown", "30", 30, Amber, Vector2.zero, new Vector2(100f, 40f), TextAlignmentOptions.Center);
            // P140: no key line under the eyepiece any more; the keys are behind [H] (HelpOverlay).
            var telescope = go.AddComponent<CoinTelescope>();
            Set(telescope, "interactable", interactable);
            Set(telescope, "eye", eye);
            Set(telescope, "overlay", overlay);
            Set(telescope, "countdown", count);
            var homeTuning = HomeDioramaBuilder.EnsureTuning(); // P111: the home street at full zoom
            var home = HomeDioramaBuilder.Build(go.transform, homeTuning);
            Debug.Log("[EconomyBuilder] " + home.Log);
            Set(telescope, "homeTuning", homeTuning);
            Set(telescope, "homeDiorama", home.Root);
            Set(telescope, "homeEye", home.Eye);
            Set(telescope, "blink", blinkImage);
            go.AddComponent<HomeLights>();
        }

        /// <summary>P101: where people stand in the evening: one at the bar, the rest in the hall, checked free of furniture.</summary>
        private static readonly (Vector3 position, float yaw)[] CrowdSpots =
        {
            (new Vector3(12f, 0f, 2.2f), 200f),
            (new Vector3(5.6f, 0f, 2.4f), 120f),
            (new Vector3(-2.2f, 0f, -4.4f), 20f),
            (new Vector3(4.8f, 0f, -2.2f), 300f),
            (new Vector3(-5.2f, 0f, 3.6f), 140f),
            (new Vector3(1.4f, 0f, 2f), 210f),
        };

        private static void BuildVendor(Transform parent, VendorSpot spot, VendorStock stock, TradeWindow trade)
        {
            var go = new GameObject("Vendor_" + spot.Name);
            go.transform.SetParent(parent, false);

            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + spot.Model + ".fbx");
            GameObject figure;
            if (modelAsset != null) figure = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            else
            {
                figure = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(figure.GetComponent<Collider>());
                figure.transform.localScale = spot.FigureSize;
                figure.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(PlaceholderMaterial);
            }
            figure.name = modelAsset != null ? spot.Model : spot.Model + "_Placeholder";
            figure.transform.SetParent(go.transform, false);
            figure.transform.position = modelAsset != null ? spot.Figure : spot.Figure + Vector3.up * spot.FigureSize.y / 2f;
            figure.transform.rotation = Quaternion.Euler(0f, spot.Yaw, 0f);
            if (modelAsset != null) Set(figure.AddComponent<IdleBreathing>(), "tuning", IdleTuning()); // P96: the delivered figures have no skeleton

            var (interactable, debugView) = Trigger(go.transform, "Trigger", spot.TriggerCenter, spot.TriggerSize, "Talk to " + spot.Name);
            if (spot.Name == "Barkeeper")
            {
                // P65: the barkeeper trades rumours for wished items, not goods for money.
                var barkeeper = go.AddComponent<Barkeeper>();
                Set(barkeeper, "interactable", interactable);
                Set(barkeeper, "debugView", debugView);
                return;
            }
            var vendor = go.AddComponent<Vendor>();
            SetFields(vendor, ("vendorName", spot.Name));
            Set(vendor, "stock", stock);
            Set(vendor, "interactable", interactable);
            Set(vendor, "window", trade);
            Set(vendor, "debugView", debugView);
            if (spot.Name == "Holly") BuildKioskShutter(go.transform, vendor);
        }

        private const string VendingModelName = "SM_Hall_VendingMachine";

        /// <summary>P30: [E] at the hall's drinks machine (x −7.4, z 4.5); Place() fits the trigger to the delivered model.</summary>
        private static void BuildVendingMachine(Transform parent, VendorStock stock)
        {
            var go = new GameObject("VendingMachine");
            go.transform.SetParent(parent, false);
            var (interactable, debugView) = Trigger(go.transform, "Trigger", new Vector3(-7.4f, 1f, 4.5f), new Vector3(1.0f, 2.0f, 1.2f), "Buy Nova Cola");
            var machine = go.AddComponent<VendingMachine>();
            Set(machine, "stock", stock);
            Set(machine, "interactable", interactable);
            Set(machine, "debugView", debugView);
        }

        private static void SetDailyAmount(VendorStock stock, ItemData item, int amount)
        {
            var so = new SerializedObject(stock);
            var entries = so.FindProperty("entries");
            for (int i = 0; i < entries.arraySize; i++)
            {
                var e = entries.GetArrayElementAtIndex(i);
                if (e.FindPropertyRelative("item").objectReferenceValue != item || e.FindPropertyRelative("dailyAmount").intValue == amount) continue;
                int now = e.FindPropertyRelative("dailyAmount").intValue;
                if (now != 3 && now != 5) return; // tuned by hand (3 and 5 come from the migration): leave it
                e.FindPropertyRelative("dailyAmount").intValue = amount;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(stock);
            }
        }

        private static void BuildDrone(Transform parent, TradeWindow trade)
        {
            var go = new GameObject("DroneTerminal");
            go.transform.SetParent(parent, false);
            // Default box; Place() fits it to the delivered drone model in the scene.
            var (interactable, debugView) = Trigger(go.transform, "Trigger", new Vector3(-13f, 0.75f, -16.5f), new Vector3(1.9f, 1.1f, 1.9f), "Send something home");
            var drone = go.AddComponent<DroneTerminal>();
            Set(drone, "interactable", interactable);
            Set(drone, "window", trade);
            Set(drone, "debugView", debugView);
        }

        private static (Interactable, GameObject) Trigger(Transform parent, string name, Vector3 center, Vector3 size, string prompt)
        {
            var trigger = new GameObject(name, typeof(BoxCollider));
            trigger.transform.SetParent(parent, false);
            trigger.transform.position = center;
            var box = trigger.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var interactable = trigger.AddComponent<Interactable>();
            SetFields(interactable, ("prompt", prompt));

            var view = GameObject.CreatePrimitive(PrimitiveType.Cube);
            view.name = "DebugView";
            Object.DestroyImmediate(view.GetComponent<Collider>());
            view.transform.SetParent(trigger.transform, false);
            view.transform.localScale = size;
            var r = view.GetComponent<MeshRenderer>();
            r.sharedMaterial = DayFlowBuilder.DebugMaterial();
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return (interactable, view);
        }

        // ---------- Windows ----------

        private static Transform Box(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = color;
            if (color == Panel) TerminalStyle.Inset(go.GetComponent<Image>()); // P95
            return go.transform;
        }

        private static Transform WindowRoot(Transform canvas, string name, out GameObject root)
        {
            root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas, false);
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.9f);
            var window = Box(root.transform, "Window", Vector2.zero, new Vector2(880f, 490f), Backdrop);
            TerminalStyle.Frame(window.GetComponent<Image>()); // P95
            TerminalStyle.AddDivider(window, new Vector2(0f, 192f), 820f);
            return window;
        }

        private static TMP_Text Text(Transform parent, string name, string text, float size, Color color, Vector2 pos, Vector2 box, TextAlignmentOptions align)
        {
            var t = Label(parent, name, text, size, color, pos, box);
            t.alignment = align;
            return t;
        }

        /// <summary>List box: header at the top, rows below (clipped), optional empty text.</summary>
        private static ItemListView ListBox(Transform parent, string name, Vector2 pos, Vector2 size, string header,
            out TMP_Text headerText, out TMP_Text emptyText)
        {
            var box = Box(parent, name, pos, size, Panel);
            box.gameObject.AddComponent<RectMask2D>();
            headerText = Text(box, "Header", header, 13, TextDim, new Vector2(0f, size.y / 2f - 14f), new Vector2(size.x - 20f, 20f), TextAlignmentOptions.MidlineLeft);
            emptyText = Text(box, "Empty", "", 14, TextDim, new Vector2(0f, -10f), new Vector2(size.x - 30f, size.y - 50f), TextAlignmentOptions.TopLeft);

            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(box, false);
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.anchoredPosition = new Vector2(0f, -28f);
            crt.sizeDelta = new Vector2(-16f, size.y - 34f);
            var layout = content.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 2f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var row = MenuButton(content.transform, "RowTemplate", "", Vector2.zero, new Vector2(size.x - 16f, 26f), 15);
            TerminalStyle.Flat(row); // rows show selection by colour
            Object.DestroyImmediate(row.GetComponentInChildren<TMP_Text>().gameObject);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;
            var label = Text(row.transform, "Label", "Item", 15, TextLight, Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(34f, 0f);
            label.rectTransform.offsetMax = new Vector2(-70f, 0f);
            var icon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icon.transform.SetParent(row.transform, false);
            var iconRect = (RectTransform)icon.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(6f, 0f);
            iconRect.sizeDelta = new Vector2(24f, 24f);
            var iconImage = icon.GetComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.enabled = false;
            var value = Text(row.transform, "Value", "0", 15, TextLight, Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineRight);
            Stretch(value.rectTransform);
            value.rectTransform.offsetMax = new Vector2(-8f, 0f);

            var view = box.gameObject.AddComponent<ItemListView>();
            Set(view, "content", content.transform);
            Set(view, "rowTemplate", row);
            return view;
        }

        private static TradeWindow BuildTradeWindow(Transform canvas)
        {
            var w = WindowRoot(canvas, "TradeWindow", out var root);
            var title = Text(w, "Title", "VENDOR", 26, Amber, new Vector2(-180f, 215f), new Vector2(480f, 36f), TextAlignmentOptions.MidlineLeft);
            var credits = Text(w, "Credits", "0 CR", 22, Amber, new Vector2(300f, 215f), new Vector2(240f, 36f), TextAlignmentOptions.MidlineRight);
            var left = ListBox(w, "VendorList", new Vector2(-217f, 55f), new Vector2(420f, 250f), "FOR SALE", out var leftHeader, out var leftEmpty);
            var right = ListBox(w, "PlayerList", new Vector2(217f, 55f), new Vector2(420f, 250f), "YOUR ITEMS", out var rightHeader, out _);

            var detail = Box(w, "Details", new Vector2(0f, -145f), new Vector2(854f, 130f), Panel);
            var name = Text(detail, "Name", "", 18, Amber, new Vector2(-170f, 45f), new Vector2(490f, 26f), TextAlignmentOptions.MidlineLeft);
            var tags = Text(detail, "Tags", "", 13, TextLight, new Vector2(-170f, 20f), new Vector2(490f, 20f), TextAlignmentOptions.MidlineLeft);
            var desc = Text(detail, "Description", "", 14, TextDim, new Vector2(-170f, -25f), new Vector2(490f, 64f), TextAlignmentOptions.TopLeft);
            var action = MenuButton(detail, "ActionButton", "BUY", new Vector2(305f, 25f), new Vector2(220f, 40f), 17);
            var back = MenuButton(detail, "BackButton", "BACK", new Vector2(305f, -30f), new Vector2(220f, 36f), 16);
            var message = Text(w, "Message", "", 15, Amber, new Vector2(0f, -228f), new Vector2(854f, 22f), TextAlignmentOptions.Center);

            var window = canvas.gameObject.AddComponent<TradeWindow>();
            Set(window, "root", root);
            Set(window, "title", title);
            Set(window, "credits", credits);
            Set(window, "leftHeader", leftHeader);
            Set(window, "leftList", left);
            Set(window, "leftEmpty", leftEmpty);
            Set(window, "rightHeader", rightHeader);
            Set(window, "rightList", right);
            Set(window, "detailName", name);
            Set(window, "detailTags", tags);
            Set(window, "detailDescription", desc);
            Set(window, "actionButton", action);
            Set(window, "actionLabel", action.GetComponentInChildren<TMP_Text>());
            Set(window, "backButton", back);
            Set(window, "message", message);
            return window;
        }

        private static InventoryWindow BuildInventoryWindow(Transform canvas)
        {
            var holder = new GameObject("InventoryWindowLogic", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            var w = WindowRoot(canvas, "InventoryWindow", out var root);
            var title = Text(w, "Title", "INVENTORY", 26, Amber, new Vector2(-180f, 215f), new Vector2(480f, 36f), TextAlignmentOptions.MidlineLeft);
            var credits = Text(w, "Credits", "0 CR", 22, Amber, new Vector2(300f, 215f), new Vector2(240f, 36f), TextAlignmentOptions.MidlineRight);
            var items = ListBox(w, "ItemList", new Vector2(-217f, 75f), new Vector2(420f, 210f), "YOUR ITEMS", out var itemsHeader, out var itemsEmpty);
            itemsEmpty.text = "Nothing on you.";

            var detail = Box(w, "Details", new Vector2(217f, 75f), new Vector2(420f, 210f), Panel);
            var name = Text(detail, "Name", "", 18, Amber, new Vector2(0f, 80f), new Vector2(390f, 26f), TextAlignmentOptions.MidlineLeft);
            var tags = Text(detail, "Tags", "", 13, TextLight, new Vector2(0f, 55f), new Vector2(390f, 20f), TextAlignmentOptions.MidlineLeft);
            var desc = Text(detail, "Description", "", 14, TextDim, new Vector2(0f, -5f), new Vector2(390f, 90f), TextAlignmentOptions.TopLeft);
            var value = Text(detail, "Value", "", 14, TextLight, new Vector2(0f, -85f), new Vector2(390f, 22f), TextAlignmentOptions.MidlineLeft);
            var wear = MenuButton(detail, "WearButton", "PUT ON", new Vector2(115f, -80f), new Vector2(160f, 30f), 15); // P63
            wear.gameObject.SetActive(false);

            var today = ListBox(w, "TodayList", new Vector2(0f, -122f), new Vector2(854f, 170f), "TODAY", out _, out var todayEmpty);
            todayEmpty.text = "No money moved today.";
            var close = MenuButton(w, "CloseButton", "CLOSE  [I]", new Vector2(327f, -226f), new Vector2(200f, 30f), 15);
            // P81: only in the shift view with the dev tools on
            var devFill = MenuButton(w, "DevFillShiftButton", "Dev_fill_Shift", new Vector2(-327f, -226f), new Vector2(200f, 30f), 15);
            var devColors = devFill.colors;
            devColors.normalColor = DevRed;
            devColors.highlightedColor = Color.Lerp(DevRed, Color.white, 0.25f);
            devColors.selectedColor = DevRed;
            devColors.pressedColor = Color.Lerp(DevRed, Color.black, 0.3f);
            devFill.colors = devColors;
            TerminalStyle.Flat(devFill);
            devFill.gameObject.SetActive(false);

            var window = holder.AddComponent<InventoryWindow>();
            Set(window, "root", root);
            Set(window, "title", title);
            Set(window, "devFillButton", devFill);
            Set(window, "credits", credits);
            Set(window, "itemsHeader", itemsHeader);
            Set(window, "itemList", items);
            Set(window, "itemsEmpty", itemsEmpty);
            Set(window, "detailName", name);
            Set(window, "detailTags", tags);
            Set(window, "detailDescription", desc);
            Set(window, "detailValue", value);
            Set(window, "wearButton", wear);
            Set(window, "todayList", today);
            Set(window, "todayEmpty", todayEmpty);
            Set(window, "closeButton", close);
            return window;
        }

        /// <summary>P08: the supervisor's question at shift end – confiscated goods left, the three answers right, outcome and CONTINUE.</summary>
        private static ShiftReportWindow BuildShiftReportWindow(Transform canvas)
        {
            var holder = new GameObject("ShiftReportWindowLogic", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            var w = WindowRoot(canvas, "ShiftReportWindow", out var root);
            Text(w, "Title", "END OF SHIFT  ·  CONFISCATIONS", 26, Amber, new Vector2(-120f, 215f), new Vector2(600f, 36f), TextAlignmentOptions.MidlineLeft);
            var items = ListBox(w, "ItemList", new Vector2(-217f, 30f), new Vector2(420f, 300f), "CONFISCATED TODAY", out _, out var empty);
            empty.text = "";
            var side = Box(w, "Answers", new Vector2(217f, 30f), new Vector2(420f, 300f), Panel);
            var question = Text(side, "Question", "", 15, TextLight, new Vector2(0f, 110f), new Vector2(390f, 60f), TextAlignmentOptions.TopLeft);
            var nothing = MenuButton(side, "NothingButton", "\"NOTHING TO REPORT\"", new Vector2(0f, 30f), new Vector2(390f, 40f), 15);
            var missed = MenuButton(side, "MissedButton", "\"OH, SEEMS I MISSED THAT\"", new Vector2(0f, -20f), new Vector2(390f, 40f), 13);
            var ordered = MenuButton(side, "OrderedButton", "\"AS ORDERED\"", new Vector2(0f, -70f), new Vector2(390f, 40f), 15);
            var outcome = Text(w, "Outcome", "", 16, TextLight, new Vector2(0f, -170f), new Vector2(854f, 70f), TextAlignmentOptions.TopLeft);
            var cont = MenuButton(w, "ContinueButton", "CONTINUE", new Vector2(327f, -226f), new Vector2(200f, 30f), 15);

            var window = holder.AddComponent<ShiftReportWindow>();
            Set(window, "root", root);
            Set(window, "itemList", items);
            Set(window, "question", question);
            Set(window, "nothingButton", nothing);
            Set(window, "missedButton", missed);
            Set(window, "missedLabel", missed.GetComponentInChildren<TMP_Text>());
            Set(window, "orderedButton", ordered);
            Set(window, "outcome", outcome);
            Set(window, "continueButton", cont);
            return window;
        }

        /// <summary>P10: nightly settlement – income and rent, three needs to tick off, family before → after, balance, CONFIRM.</summary>
        private static SettlementWindow BuildSettlementWindow(Transform canvas)
        {
            var holder = new GameObject("SettlementWindowLogic", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            var w = WindowRoot(canvas, "SettlementWindow", out var root);
            var title = Text(w, "Title", "DAY 1  ·  SETTLEMENT", 26, Amber, new Vector2(-120f, 215f), new Vector2(600f, 36f), TextAlignmentOptions.MidlineLeft);

            var incomeBox = Box(w, "Income", new Vector2(-217f, 95f), new Vector2(420f, 170f), Panel);
            Text(incomeBox, "Header", "TODAY", 13, TextDim, new Vector2(0f, 71f), new Vector2(400f, 20f), TextAlignmentOptions.MidlineLeft);
            var income = Text(incomeBox, "Lines", "", 15, TextLight, new Vector2(0f, -10f), new Vector2(400f, 130f), TextAlignmentOptions.TopLeft);

            var needsBox = Box(w, "Needs", new Vector2(217f, 95f), new Vector2(420f, 170f), Panel);
            Text(needsBox, "Header", "FOR THE FAMILY (click to change)", 13, TextDim, new Vector2(0f, 71f), new Vector2(400f, 20f), TextAlignmentOptions.MidlineLeft);
            var buttons = new Object[3];
            var labels = new Object[3];
            for (int i = 0; i < 3; i++)
            {
                var b = MenuButton(needsBox, "Need" + i, "", new Vector2(0f, 30f - i * 44f), new Vector2(396f, 38f), 16);
                var colors = b.colors;
                colors.disabledColor = Backdrop; // locked rows (by drone, nobody sick, not enough) sit darker than clickable ones
                b.colors = colors;
                var label = b.GetComponentInChildren<TMP_Text>();
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.rectTransform.offsetMin = new Vector2(12f, 0f);
                buttons[i] = b;
                labels[i] = label;
            }

            var familyBox = Box(w, "Family", new Vector2(0f, -88f), new Vector2(854f, 150f), Panel);
            var family = Text(familyBox, "Lines", "", 15, TextLight, new Vector2(0f, 0f), new Vector2(830f, 136f), TextAlignmentOptions.TopLeft);
            var balance = Text(w, "Balance", "", 20, Amber, new Vector2(-222f, -205f), new Vector2(400f, 30f), TextAlignmentOptions.MidlineLeft);
            var back = MenuButton(w, "ReturnButton", "RETURN", new Vector2(87f, -205f), new Vector2(220f, 40f), 17);
            var confirm = MenuButton(w, "ConfirmButton", "CONFIRM", new Vector2(317f, -205f), new Vector2(220f, 40f), 17);

            var window = holder.AddComponent<SettlementWindow>();
            Set(window, "root", root);
            Set(window, "title", title);
            Set(window, "income", income);
            SetList(window, "needButtons", buttons);
            SetList(window, "needLabels", labels);
            Set(window, "family", family);
            Set(window, "balance", balance);
            Set(window, "confirmButton", confirm);
            Set(window, "returnButton", back);
            return window;
        }

        // ---------- Placement ----------

        private static void Place(Scene scene, GameObject prefab, List<string> log)
        {
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var day = Object.FindFirstObjectByType<DayManager>();
            var dialogs = Object.FindFirstObjectByType<DayFlowUI>();

            var counter = Object.FindFirstObjectByType<GameOff.Counter.CounterStation>();
            var economy = instance.GetComponent<PlayerEconomy>();
            Set(economy, "dayManager", day);
            Set(economy, "shift", Object.FindFirstObjectByType<CounterShift>());
            Record(economy);
            foreach (var w in instance.GetComponentsInChildren<TradeWindow>(true)) { Set(w, "player", player); Record(w); }
            foreach (var w in instance.GetComponentsInChildren<InventoryWindow>(true)) { Set(w, "player", player); Set(w, "counter", counter); Record(w); }
            foreach (var v in instance.GetComponentsInChildren<Vendor>(true))
            {
                Set(v, "player", player);
                Set(v, "day", day);
                Set(v, "dialogs", dialogs);
                Record(v);
            }
            var planetT = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "HomePlanet");
            foreach (var t in instance.GetComponentsInChildren<CoinTelescope>(true))
            {
                Set(t, "player", player);
                Set(t, "day", day);
                Set(t, "playerCamera", player != null ? player.GetComponentInChildren<Camera>(true) : null);
                Set(t, "homePlanet", planetT);
                Record(t);
                foreach (var w in t.GetComponentsInChildren<HomeWindows>(true)) { Set(w, "day", day); Record(w); } // P111
            }
            foreach (var h in instance.GetComponentsInChildren<HomeLights>(true))
            {
                Set(h, "day", day);
                Set(h, "planet", planetT != null ? planetT.GetComponentInChildren<Renderer>(true) : null);
                Record(h);
            }
            foreach (var l in instance.GetComponentsInChildren<EvidenceLocker>(true))
            {
                Set(l, "day", day);
                Set(l, "dialogs", dialogs);
                Set(l, "player", player);
                Record(l);
            }
            string lockerBoard = LockerBoardBuilder.WireInScene(instance, scene, player); // P32, P33
            if (lockerBoard.Length > 0) log.Add("locker/pinboard " + lockerBoard);
            foreach (var h in instance.GetComponentsInChildren<GameOff.Station.HangarShips>(true))
            {
                Set(h, "day", day);
                Record(h);
            }
            foreach (var a in instance.GetComponentsInChildren<GameOff.Station.ArrivingGroup>(true))
            {
                Set(a, "shift", Object.FindFirstObjectByType<CounterShift>());
                Record(a);
            }
            foreach (var c in instance.GetComponentsInChildren<GameOff.Hangar.CargoCrate>(true))
            {
                Set(c, "day", day);
                Set(c, "shift", Object.FindFirstObjectByType<CounterShift>());
                Set(c, "player", Object.FindFirstObjectByType<GameOff.Player.FirstPersonController>());
                Record(c);
            }
            foreach (var c in instance.GetComponentsInChildren<EveningCrowd>(true))
            {
                Set(c, "day", day);
                Set(c, "shift", Object.FindFirstObjectByType<CounterShift>());
                Record(c);
            }
            foreach (var b in instance.GetComponentsInChildren<Barkeeper>(true))
            {
                Set(b, "player", player);
                Set(b, "day", day);
                Set(b, "dialogs", dialogs);
                Record(b);
            }
            var desk = Object.FindFirstObjectByType<GameOff.Documents.DocumentDesk>();
            var counterShift = Object.FindFirstObjectByType<CounterShift>();
            foreach (var b in instance.GetComponentsInChildren<BribeNote>(true))
            {
                Set(b, "day", day);
                Set(b, "shift", counterShift);
                Set(b, "desk", desk);
                Record(b);
            }
            var shift = Object.FindFirstObjectByType<CounterShift>();
            foreach (var w in instance.GetComponentsInChildren<SettlementWindow>(true))
            {
                Set(w, "dayManager", day);
                Set(w, "shift", shift);
                Record(w);
            }
            if (shift == null) log.Add("WARNING: no CounterShift in the scene - no wage");
            // P08: confiscation listens to the decisions; the report window blocks the desk while it is open.
            foreach (var c in instance.GetComponentsInChildren<Confiscation>(true))
            {
                Set(c, "shift", shift);
                Set(c, "dayManager", day);
                Record(c);
            }
            foreach (var w in instance.GetComponentsInChildren<ShiftReportWindow>(true)) { Set(w, "counter", counter); Record(w); }
            foreach (var m in instance.GetComponentsInChildren<VendingMachine>(true))
            {
                Set(m, "player", player);
                Set(m, "day", day);
                Set(m, "dialogs", dialogs);
                Record(m);
                log.Add(FitTrigger(scene, instance, m.transform, VendingModelName, "vending"));
            }
            var drone = instance.GetComponentInChildren<DroneTerminal>(true);
            Set(drone, "day", day);
            Set(drone, "player", player);
            Set(drone, "dialogs", dialogs);
            Record(drone);
            string fit = FitDroneTrigger(scene, instance, drone);

            EditorSceneManager.MarkSceneDirty(scene);
            log.Add($"placed in {scene.name} (player {Ok(player)}, day {Ok(day)}, dialogs {Ok(dialogs)}, {fit})");
        }

        private static string FitDroneTrigger(Scene scene, GameObject economy, DroneTerminal drone) =>
            FitTrigger(scene, economy, drone.transform, DroneModelName, "drone");

        /// <summary>Fits a trigger box ("Trigger" child of owner) around a model in the scene and the small colliders touching it.</summary>
        private static string FitTrigger(Scene scene, GameObject economy, Transform owner, string modelName, string what)
        {
            var model = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == modelName && !t.IsChildOf(economy.transform));
            var renderers = model != null ? model.GetComponentsInChildren<Renderer>() : new Renderer[0];
            if (renderers.Length == 0) return what + " model not found, default trigger";
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            // Solid colliders around the drone (its own and the pad's: the pad's box is 1.4 m high and encloses the drone) would
            // catch the interaction ray first, so the trigger wraps all of them.
            var droneBounds = bounds;
            foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (c.isTrigger || c.transform.IsChildOf(economy.transform)) continue;
                var cb = c.bounds;
                if (cb.size.x > 5f || cb.size.z > 5f || cb.size.y > 3f || !cb.Intersects(droneBounds)) continue; // skip floors and walls
                bounds.Encapsulate(cb);
            }
            bounds.Expand(new Vector3(0.2f, 0f, 0.2f));
            var center = bounds.center + Vector3.up * 0.1f;
            var size = bounds.size + Vector3.up * 0.2f;
            var trigger = owner.Find("Trigger");
            trigger.position = center;
            trigger.GetComponent<BoxCollider>().size = size;
            trigger.Find("DebugView").localScale = size;
            Record(trigger);
            Record(trigger.GetComponent<BoxCollider>());
            Record(trigger.Find("DebugView"));
            return $"{what} trigger fitted ({size.x:0.0} × {size.y:0.0} × {size.z:0.0} m)";
        }

        private static void Record(Object o) => PrefabUtility.RecordPrefabInstancePropertyModifications(o);

        private static string Ok(Object o) => o != null ? "ok" : "MISSING";
    }
}
