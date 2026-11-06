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
            new VendorSpot("Dealer", "SM_NPC_Dealer", "Stock_BlackMarket_Test", new Vector3(13.6f, 0.45f, 6.4f), new Vector3(0.6f, 1.3f, 0.6f), 90f,
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
            data.Items["spice"] = Item("ContrabandSpice", "Contraband spice", "Confiscated at the counter. Worth more under the bar than at the kiosk.", 12, ref created, contraband);
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
            { "Item_DocumentScanner", 1 }, { "Item_RationPack", 3 }, { "Item_Painkillers", 2 }, { "Item_NovaCola", 5 }, { "Item_ContrabandSpice", 3 },
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

            var vendors = new GameObject("Vendors").transform;
            vendors.SetParent(root.transform, false);
            foreach (var spot in Vendors) BuildVendor(vendors, spot, data.Stocks[spot.Stock], trade);
            BuildDrone(root.transform, trade);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            log.Add("PF_Economy built");
            return prefab;
        }

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

            var (interactable, debugView) = Trigger(go.transform, "Trigger", spot.TriggerCenter, spot.TriggerSize, "Talk to " + spot.Name);
            var vendor = go.AddComponent<Vendor>();
            SetFields(vendor, ("vendorName", spot.Name));
            Set(vendor, "stock", stock);
            Set(vendor, "interactable", interactable);
            Set(vendor, "window", trade);
            Set(vendor, "debugView", debugView);
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
            return go.transform;
        }

        private static Transform WindowRoot(Transform canvas, string name, out GameObject root)
        {
            root = new GameObject(name, typeof(RectTransform), typeof(Image));
            root.transform.SetParent(canvas, false);
            Stretch(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.9f);
            return Box(root.transform, "Window", Vector2.zero, new Vector2(880f, 490f), Backdrop);
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
            Object.DestroyImmediate(row.GetComponentInChildren<TMP_Text>().gameObject);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 26f;
            var label = Text(row.transform, "Label", "Item", 15, TextLight, Vector2.zero, Vector2.zero, TextAlignmentOptions.MidlineLeft);
            Stretch(label.rectTransform);
            label.rectTransform.offsetMin = new Vector2(8f, 0f);
            label.rectTransform.offsetMax = new Vector2(-70f, 0f);
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
            Text(w, "Title", "INVENTORY", 26, Amber, new Vector2(-180f, 215f), new Vector2(480f, 36f), TextAlignmentOptions.MidlineLeft);
            var credits = Text(w, "Credits", "0 CR", 22, Amber, new Vector2(300f, 215f), new Vector2(240f, 36f), TextAlignmentOptions.MidlineRight);
            var items = ListBox(w, "ItemList", new Vector2(-217f, 75f), new Vector2(420f, 210f), "YOUR ITEMS", out var itemsHeader, out var itemsEmpty);
            itemsEmpty.text = "Nothing on you.";

            var detail = Box(w, "Details", new Vector2(217f, 75f), new Vector2(420f, 210f), Panel);
            var name = Text(detail, "Name", "", 18, Amber, new Vector2(0f, 80f), new Vector2(390f, 26f), TextAlignmentOptions.MidlineLeft);
            var tags = Text(detail, "Tags", "", 13, TextLight, new Vector2(0f, 55f), new Vector2(390f, 20f), TextAlignmentOptions.MidlineLeft);
            var desc = Text(detail, "Description", "", 14, TextDim, new Vector2(0f, -5f), new Vector2(390f, 90f), TextAlignmentOptions.TopLeft);
            var value = Text(detail, "Value", "", 14, TextLight, new Vector2(0f, -85f), new Vector2(390f, 22f), TextAlignmentOptions.MidlineLeft);

            var today = ListBox(w, "TodayList", new Vector2(0f, -122f), new Vector2(854f, 170f), "TODAY", out _, out var todayEmpty);
            todayEmpty.text = "No money moved today.";
            var close = MenuButton(w, "CloseButton", "CLOSE  [I]", new Vector2(327f, -226f), new Vector2(200f, 30f), 15);

            var window = holder.AddComponent<InventoryWindow>();
            Set(window, "root", root);
            Set(window, "credits", credits);
            Set(window, "itemsHeader", itemsHeader);
            Set(window, "itemList", items);
            Set(window, "itemsEmpty", itemsEmpty);
            Set(window, "detailName", name);
            Set(window, "detailTags", tags);
            Set(window, "detailDescription", desc);
            Set(window, "detailValue", value);
            Set(window, "todayList", today);
            Set(window, "todayEmpty", todayEmpty);
            Set(window, "closeButton", close);
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
                var label = b.GetComponentInChildren<TMP_Text>();
                label.alignment = TextAlignmentOptions.MidlineLeft;
                label.rectTransform.offsetMin = new Vector2(12f, 0f);
                buttons[i] = b;
                labels[i] = label;
            }

            var familyBox = Box(w, "Family", new Vector2(0f, -88f), new Vector2(854f, 150f), Panel);
            var family = Text(familyBox, "Lines", "", 15, TextLight, new Vector2(0f, 0f), new Vector2(830f, 136f), TextAlignmentOptions.TopLeft);
            var balance = Text(w, "Balance", "", 20, Amber, new Vector2(-150f, -205f), new Vector2(540f, 30f), TextAlignmentOptions.MidlineLeft);
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

            var economy = instance.GetComponent<PlayerEconomy>();
            Set(economy, "dayManager", day);
            Record(economy);
            foreach (var w in instance.GetComponentsInChildren<TradeWindow>(true)) { Set(w, "player", player); Record(w); }
            foreach (var w in instance.GetComponentsInChildren<InventoryWindow>(true)) { Set(w, "player", player); Record(w); }
            foreach (var v in instance.GetComponentsInChildren<Vendor>(true))
            {
                Set(v, "player", player);
                Set(v, "day", day);
                Set(v, "dialogs", dialogs);
                Record(v);
            }
            var shift = Object.FindFirstObjectByType<CounterShift>();
            foreach (var w in instance.GetComponentsInChildren<SettlementWindow>(true))
            {
                Set(w, "dayManager", day);
                Set(w, "shift", shift);
                Record(w);
            }
            if (shift == null) log.Add("WARNING: no CounterShift in the scene - no wage");
            var drone = instance.GetComponentInChildren<DroneTerminal>(true);
            Set(drone, "day", day);
            Record(drone);
            string fit = FitDroneTrigger(scene, instance, drone);

            EditorSceneManager.MarkSceneDirty(scene);
            log.Add($"placed in {scene.name} (player {Ok(player)}, day {Ok(day)}, dialogs {Ok(dialogs)}, {fit})");
        }

        private static string FitDroneTrigger(Scene scene, GameObject economy, DroneTerminal drone)
        {
            var model = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == DroneModelName && !t.IsChildOf(economy.transform));
            var renderers = model != null ? model.GetComponentsInChildren<Renderer>() : new Renderer[0];
            if (renderers.Length == 0) return "drone model not found, default trigger";
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            // Solid colliders around the drone (its own and the pad's: the pad's box is 1.4 m high and encloses the drone) would
            // catch the interaction ray first, so the trigger wraps all of them.
            var droneBounds = bounds;
            foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            {
                if (c.isTrigger || c.transform.IsChildOf(economy.transform)) continue;
                var cb = c.bounds;
                if (cb.size.x > 5f || cb.size.z > 5f || !cb.Intersects(droneBounds)) continue; // skip floors and walls
                bounds.Encapsulate(cb);
            }
            bounds.Expand(new Vector3(0.2f, 0f, 0.2f));
            var center = bounds.center + Vector3.up * 0.1f;
            var size = bounds.size + Vector3.up * 0.2f;
            var trigger = drone.transform.Find("Trigger");
            trigger.position = center;
            trigger.GetComponent<BoxCollider>().size = size;
            trigger.Find("DebugView").localScale = size;
            Record(trigger);
            Record(trigger.GetComponent<BoxCollider>());
            Record(trigger.Find("DebugView"));
            return $"drone trigger fitted ({size.x:0.0} × {size.y:0.0} × {size.z:0.0} m)";
        }

        private static void Record(Object o) => PrefabUtility.RecordPrefabInstancePropertyModifications(o);

        private static string Ok(Object o) => o != null ? "ok" : "MISSING";
    }
}
