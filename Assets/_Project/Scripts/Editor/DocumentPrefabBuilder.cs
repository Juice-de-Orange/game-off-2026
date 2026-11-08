using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Documents;
using GameOff.Economy;
using GameOff.Travelers;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds everything for the documents on the counter (P06), idempotent: Orbitron TMP font asset (P54), document
    /// materials, the three document prefabs (layout below), test documents, tuning, and wires the DocumentDesk with the
    /// drawer into PF_CounterStation. Document background art is replaced by the pipeline at the same paths.
    /// </summary>
    public static class DocumentPrefabBuilder
    {
        private const string FontPath = "Assets/_Project/Fonts/Orbitron-Regular.ttf";
        private const string FontAssetPath = "Assets/_Project/Fonts/Orbitron SDF.asset";
        private const string TexFolder = "Assets/_Project/Art/Textures/Documents/";
        private const string MatFolder = "Assets/_Project/Art/Materials/Documents/";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Documents/";
        private const string DataFolder = "Assets/_Project/Data/Documents/";
        private const string CounterPrefab = "Assets/_Project/Prefabs/Counter/PF_CounterStation.prefab";
        private const string DocSlotModel = "Assets/_Project/Art/Models/Counter/SM_Counter_DocSlot.fbx";
        private const string PrinterModel = "Assets/_Project/Art/Models/Counter/SM_Counter_Printer.fbx";
        private const string TestDayPath = "Assets/_Project/Data/Days/Day1_Test.asset";
        private const float LabelSize = 0.0035f, ValueSize = 0.0055f, TitleSize = 0.006f; // cap heights in meters
        private const string InputActionsPath = "Assets/_Project/Data/Input/PlayerControls.inputactions";
        private const string CounterDataFolder = "Assets/_Project/Data/Counter/";
        private static readonly Color Ink = new Color(0.12f, 0.12f, 0.14f), LabelInk = new Color(0.35f, 0.35f, 0.38f);
        private static readonly Color NoticePaper = new Color(0.86f, 0.84f, 0.76f);

        /// <summary>Text on a page: x/z = top-left corner in page coordinates (meters, page center = 0), w = box width.</summary>
        private readonly struct Text
        {
            public readonly string Name, Content; public readonly float X, Z, W, Size; public readonly Color Color;
            public Text(string name, string content, float x, float z, float w, float size, Color color)
            {
                Name = name; Content = content; X = x; Z = z; W = w; Size = size; Color = color;
            }
        }

        private const string KorrTutorialTraveler = "Assets/_Project/Data/Travelers/Story/T_Story_Korr_1.asset";
        private static float _unitsPerFontSize; // cap height of the font at fontSize 1

        [MenuItem("GameOff/Build Documents")]
        public static void BuildFromMenu() => Debug.Log(Build());

        public static string Build()
        {
            var log = new List<string>();
            var font = FontAsset(log);
            var photoTex = Texture("T_Doc_Photo_Placeholder.png");
            var photoMat = UnlitMaterial("M_Doc_Photo", photoTex);

            var id = BuildPrefab(DocumentType.IdCard, new Vector2(0.12f, 0.08f), 1f, font, photoMat, new Rect(-0.056f, 0.022f, 0.032f, 0.04f),
                new List<Text>
                {
                    new Text("Title", "KESTREL-9  ·  IDENTITY CARD", -0.056f, 0.037f, 0.11f, TitleSize, Color.white),
                    Label("NAME", -0.018f, 0.02f), Value("name", -0.018f, 0.016f, 0.07f),
                    Label("SPECIES", -0.018f, 0.006f), Value("species", -0.018f, 0.002f, 0.07f),
                    Label("ORIGIN", -0.018f, -0.008f), Value("origin", -0.018f, -0.012f, 0.07f),
                    Label("ID NO.", -0.056f, -0.022f), Value("id", -0.056f, -0.026f, 0.05f),
                    Label("EXPIRES", 0.005f, -0.022f), Value("expires", 0.005f, -0.026f, 0.05f),
                    UvMark(-0.056f, 0.004f, 0.11f),
                }, log);
            var permit = BuildPrefab(DocumentType.EntryPermit, new Vector2(0.15f, 0.21f), 1.35f, font, photoMat, new Rect(0.028f, 0.072f, 0.038f, 0.048f),
                new List<Text>
                {
                    new Text("Title", "ENTRY PERMIT", -0.066f, 0.1f, 0.13f, TitleSize * 1.4f, Color.white),
                    new Text("Subtitle", "KESTREL-9 TRANSIT AUTHORITY", -0.066f, 0.082f, 0.09f, LabelSize, LabelInk),
                    Label("NAME", -0.066f, 0.06f), Value("name", -0.066f, 0.055f, 0.09f),
                    Label("ID NO.", -0.066f, 0.04f), Value("id", -0.066f, 0.035f, 0.09f),
                    Label("PURPOSE OF VISIT", -0.066f, 0.015f), Value("purpose", -0.066f, 0.01f, 0.13f),
                    Label("DURATION", -0.066f, -0.01f), Value("duration", -0.066f, -0.015f, 0.06f),
                    Label("VALID UNTIL", 0.0f, -0.01f), Value("expires", 0.0f, -0.015f, 0.066f),
                    Label("ISSUED AT", -0.066f, -0.035f), Value("issued", -0.066f, -0.04f, 0.13f),
                    Label("ROUTE", -0.066f, -0.06f), Value("route", -0.066f, -0.065f, 0.105f),
                    UvMark(-0.066f, 0.0f, 0.13f),
                }, log);
            var manifest = BuildPrefab(DocumentType.CargoManifest, new Vector2(0.21f, 0.30f), 1.6f, font, null, default,
                new List<Text>
                {
                    new Text("Title", "CARGO MANIFEST", -0.096f, 0.145f, 0.19f, TitleSize * 1.4f, Color.white),
                    Label("CARRIER", -0.096f, 0.11f), Value("name", -0.096f, 0.104f, 0.1f),
                    Label("SHIP", 0.01f, 0.11f), Value("ship", 0.01f, 0.104f, 0.09f),
                    Label("FROM", -0.096f, 0.085f), Value("origin", -0.096f, 0.079f, 0.1f),
                    Label("TO", 0.01f, 0.085f), Value("destination", 0.01f, 0.079f, 0.09f),
                    Label("DECLARED GOODS", -0.096f, 0.058f), Value("cargo", -0.096f, 0.05f, 0.19f),
                    UvMark(-0.096f, -0.03f, 0.19f),
                }, log);

            var citation = BuildPrefab(DocumentType.Citation, new Vector2(0.08f, 0.14f), 1f, font, null, default,
                new List<Text>
                {
                    new Text("Title", "CITATION", -0.034f, 0.066f, 0.07f, TitleSize * 1.2f, Color.white),
                    new Text("Subtitle", "KESTREL-9 TRANSIT AUTHORITY", -0.034f, 0.046f, 0.07f, LabelSize * 0.9f, LabelInk),
                    Label("NO.", -0.034f, 0.034f), Value("id", -0.034f, 0.03f, 0.03f),
                    Label("DATE", 0.002f, 0.034f), Value("date", 0.002f, 0.03f, 0.034f),
                    Label("VIOLATION", -0.034f, 0.014f), Value("reason", -0.034f, 0.01f, 0.068f),
                    Label("PENALTY", -0.034f, -0.032f),
                    new Text("Field_penalty", "", -0.034f, -0.036f, 0.068f, ValueSize, new Color(0.66f, 0.12f, 0.1f)),
                }, log);

            // Pinboard note (P41): big page, so the rules fit; can't be stamped
            var notice = BuildPrefab(DocumentType.Notice, new Vector2(0.30f, 0.42f), 2.2f, font, null, default,
                new List<Text>
                {
                    new Text("Subtitle", "KESTREL-9 TRANSIT AUTHORITY", -0.135f, 0.195f, 0.2f, LabelSize, LabelInk),
                    new Text("Field_title", "", -0.135f, 0.175f, 0.2f, TitleSize, Ink),
                    new Text("Field_date", "", 0.05f, 0.195f, 0.085f, LabelSize * 0.7f, LabelInk),
                    new Text("Field_body", "", -0.135f, 0.14f, 0.27f, ValueSize, Ink),
                }, log, stampable: false);

            var tuning = Asset<DocumentTuning>(DataFolder + "DocumentTuning.asset");
            WireCounter(id, permit, manifest, citation, notice, font, tuning, log);
            AssetDatabase.SaveAssets();
            return "DocumentPrefabBuilder:\n" + string.Join("\n", log);
        }

        private static Text UvMark(float x, float z, float width) => new Text("UvMark", "", x, z, width, TitleSize * 1.5f, Color.white); // P62: only under the lamp

        private static Text Label(string text, float x, float z) => new Text("Label_" + text.Replace(" ", "").Replace(".", ""), text, x, z, 0.06f, LabelSize, LabelInk);
        private static Text Value(string key, float x, float z, float w) => new Text("Field_" + key, "", x, z, w, ValueSize, Ink);

        private static TMP_FontAsset FontAsset(List<string> log)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (asset == null)
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
                asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024,
                    AtlasPopulationMode.Dynamic);
                asset.name = "Orbitron SDF";
                AssetDatabase.CreateAsset(asset, FontAssetPath);
                asset.atlasTextures[0].name = "Orbitron SDF Atlas";
                AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
                asset.material.name = "Orbitron SDF Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
                log.Add("font " + FontAssetPath);
            }
            // Measure the cap height at fontSize 1 once, so text sizes can be given in meters.
            var probe = new GameObject("probe").AddComponent<TextMeshPro>();
            probe.font = asset;
            probe.fontSize = 1f;
            probe.text = "H";
            probe.ForceMeshUpdate();
            _unitsPerFontSize = Mathf.Max(0.0001f, probe.textBounds.size.y);
            Object.DestroyImmediate(probe.gameObject);
            return asset;
        }

        /// <param name="textScale">Bigger pages get bigger type, so every page reads well when held up (it fills the same screen height).</param>
        private static DocumentView BuildPrefab(DocumentType type, Vector2 size, float textScale, TMP_FontAsset font, Material photoMat, Rect photoRect,
            List<Text> texts, List<string> log, bool stampable = true)
        {
            int layer = LayerMask.NameToLayer(StampTool.SharpLayer);
            var root = new GameObject("PF_Doc_" + type);
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.0006f, 0f);
            box.size = new Vector3(size.x, 0.0012f, size.y);
            if (stampable) root.AddComponent<StampTarget>();
            var view = root.AddComponent<DocumentView>();

            // Page: a quad lying flat (face up, texture top = local +Z)
            var sheet = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sheet.name = "Sheet";
            Object.DestroyImmediate(sheet.GetComponent<Collider>());
            sheet.transform.SetParent(root.transform, false);
            sheet.transform.localPosition = new Vector3(0f, 0.0005f, 0f);
            sheet.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            sheet.transform.localScale = new Vector3(size.x, size.y, 1f);
            sheet.GetComponent<MeshRenderer>().sharedMaterial = UnlitMaterial("M_Doc_" + type, Texture($"T_Doc_{type}.png"),
                type == DocumentType.Notice ? NoticePaper : (Color?)null);
            // The underside, so a paper held up never shows through from behind
            var back = Object.Instantiate(sheet, root.transform);
            back.name = "Back";
            back.transform.localPosition = new Vector3(0f, 0.0004f, 0f);
            back.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            back.GetComponent<MeshRenderer>().sharedMaterial = UnlitMaterial("M_Doc_Back", null, new Color(0.8f, 0.78f, 0.72f));

            MeshRenderer photo = null;
            if (photoMat != null)
            {
                var p = GameObject.CreatePrimitive(PrimitiveType.Quad);
                p.name = "Photo";
                Object.DestroyImmediate(p.GetComponent<Collider>());
                p.transform.SetParent(root.transform, false);
                p.transform.localPosition = new Vector3(photoRect.x + photoRect.width * 0.5f, 0.0007f, photoRect.y - photoRect.height * 0.5f);
                p.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                p.transform.localScale = new Vector3(photoRect.width, photoRect.height, 1f);
                photo = p.GetComponent<MeshRenderer>();
                photo.sharedMaterial = photoMat;
            }

            foreach (var t in texts)
            {
                var go = new GameObject(t.Name);
                go.transform.SetParent(root.transform, false);
                var tmp = go.AddComponent<TextMeshPro>();
                tmp.font = font;
                bool isTitle = t.Name == "Title";
                tmp.fontSize = t.Size * (isTitle ? 1f : textScale) / _unitsPerFontSize;
                tmp.color = t.Color;
                tmp.text = t.Content;
                tmp.alignment = TextAlignmentOptions.TopLeft;
                bool multiline = t.Name == "Field_body" || t.Name == "Field_cargo" || t.Name == "Field_reason";
                tmp.textWrappingMode = multiline ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
                tmp.overflowMode = TextOverflowModes.Overflow;
                if (!multiline)
                {
                    // Orbitron is wide: a long name or date shrinks to fit its field instead of running into the next one
                    tmp.fontSizeMax = tmp.fontSize;
                    tmp.fontSizeMin = tmp.fontSize * 0.6f;
                    tmp.enableAutoSizing = true;
                }
                var rect = tmp.rectTransform;
                rect.pivot = new Vector2(0f, 1f);
                rect.sizeDelta = new Vector2(t.W, 0.08f);
                // Lying on the page, reading toward local +X, top toward local +Z
                go.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                // Values sit under their label: move them down by what the label grew
                float zShift = t.Name.StartsWith("Field_") ? -(textScale - 1f) * LabelSize : 0f;
                go.transform.localPosition = new Vector3(t.X, 0.0008f, t.Z + zShift);
            }

            foreach (var tr in root.GetComponentsInChildren<Transform>(true)) tr.gameObject.layer = layer;
            var so = new SerializedObject(view);
            so.FindProperty("type").enumValueIndex = (int)type;
            so.FindProperty("photo").objectReferenceValue = photo;
            so.FindProperty("pageSize").vector2Value = size;
            so.ApplyModifiedPropertiesWithoutUndo();

            AssetFolders.EnsureForAsset(PrefabFolder + "x");
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabFolder + $"PF_Doc_{type}.prefab");
            Object.DestroyImmediate(root);
            log.Add("prefab PF_Doc_" + type);
            return prefab.GetComponent<DocumentView>();
        }

        private static Texture2D Texture(string file)
        {
            string path = TexFolder + file;
            if (AssetImporter.GetAtPath(path) is TextureImporter ti && (ti.filterMode != FilterMode.Bilinear || !ti.mipmapEnabled))
            {
                // Documents are read up close at full resolution: smooth filtering (unlike the point-filtered world)
                ti.filterMode = FilterMode.Bilinear;
                ti.mipmapEnabled = true;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        /// <summary>Unlit, so papers stay readable in the dim counter light.</summary>
        private static Material UnlitMaterial(string name, Texture2D tex, Color? color = null)
        {
            string path = MatFolder + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                AssetFolders.EnsureForAsset(path);
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetTexture("_BaseMap", tex);
            mat.SetColor("_BaseColor", color ?? Color.white);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static T Asset<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            AssetFolders.EnsureForAsset(path);
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>
        /// P89: both buttons are turned round (the model's front with its tab faces the officer, the cover's hinge the window),
        /// the airlock cover opens upward over that hinge, and both buttons get a label on the desk in front of them.
        /// </summary>
        private static void WireButtons(GameObject root, Transform controls, TMP_FontAsset font)
        {
            foreach (var clickable in root.GetComponentsInChildren<CounterClickable>(true))
            {
                if (clickable.Action != CounterAction.Airlock && clickable.Action != CounterAction.Arrest) continue;
                clickable.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                if (clickable.Action != CounterAction.Airlock) continue;
                var so = new SerializedObject(clickable);
                so.FindProperty("coverOpenAngle").floatValue = 110f; // measured: stays above the desk (-110 sinks 9 cm into it)
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            DeskLabel(controls, "Label_Arrest", "ARREST", new Vector3(0.8f, 0.002f, -0.55f), new Color(0.05f, 0.05f, 0.07f), font);
            DeskLabel(controls, "Label_Airlock", "AIRLOCK", new Vector3(0.8f, 0.002f, -0.83f), new Color(0.72f, 0.04f, 0.03f), font);
        }

        /// <summary>P16/P62: scanner and UV lamp lie at the back left of the desk, away from where papers land; hidden until bought (PaperGadgets).</summary>
        private static void WireGadgets(GameObject root, Transform controls, TMP_FontAsset font, List<string> log)
        {
            var layer = controls.Find("Stamp_Approve (SM_Counter_Stamp)");
            var uv = Gadget(controls, "Gadget_UvLamp", "Assets/_Project/Art/Models/Items/SM_Item_UVLamp.fbx", new Vector3(1.62f, 0f, 1.2f), CounterAction.UvLamp, "UV", font, layer);
            var scan = Gadget(controls, "Gadget_Scanner", "Assets/_Project/Art/Models/Items/SM_Item_DocScanner.fbx", new Vector3(1.62f, 0f, 1.5f), CounterAction.Scanner, "SCAN", font, layer);
            var gadgets = root.GetComponent<PaperGadgets>();
            if (gadgets == null) gadgets = root.AddComponent<PaperGadgets>();
            var so = new SerializedObject(gadgets);
            so.FindProperty("station").objectReferenceValue = root.GetComponent<CounterStation>();
            so.FindProperty("desk").objectReferenceValue = root.GetComponent<DocumentDesk>();
            so.FindProperty("shift").objectReferenceValue = root.GetComponent<CounterShift>();
            so.FindProperty("uvLampItem").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/Data/Items/Item_UVLamp.asset");
            so.FindProperty("scannerItem").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ItemData>("Assets/_Project/Data/Items/Item_DocumentScanner.asset");
            so.FindProperty("uvLamp").objectReferenceValue = uv;
            so.FindProperty("scanner").objectReferenceValue = scan;
            so.ApplyModifiedPropertiesWithoutUndo();
            var dso = new SerializedObject(root.GetComponent<DocumentDesk>()); // a paper dropped under a gadget could not be picked up again
            var clear = dso.FindProperty("keepClear");
            clear.arraySize = 2;
            clear.GetArrayElementAtIndex(0).objectReferenceValue = uv.GetComponent<BoxCollider>();
            clear.GetArrayElementAtIndex(1).objectReferenceValue = scan.GetComponent<BoxCollider>();
            dso.ApplyModifiedPropertiesWithoutUndo();
            log.Add("PaperGadgets wired");
        }

        private static GameObject Gadget(Transform controls, string name, string modelPath, Vector3 position, CounterAction action, string label, TMP_FontAsset font, Transform layerFrom)
        {
            var old = controls.Find(name);
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var go = new GameObject(name);
            go.transform.SetParent(controls, false);
            go.transform.localPosition = position;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model != null) ((GameObject)PrefabUtility.InstantiatePrefab(model, go.transform)).transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            var box = go.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.04f, 0f);
            box.size = new Vector3(0.2f, 0.08f, 0.12f);
            var clickable = go.AddComponent<CounterClickable>();
            var so = new SerializedObject(clickable);
            so.FindProperty("action").enumValueIndex = (int)action;
            so.ApplyModifiedPropertiesWithoutUndo();
            DeskLabel(go.transform, "Label", label, new Vector3(-0.14f, 0.002f, 0f), new Color(0.05f, 0.05f, 0.07f), font);
            if (layerFrom != null)
                foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = layerFrom.gameObject.layer;
            go.SetActive(false);
            return go;
        }

        private static void DeskLabel(Transform controls, string name, string text, Vector3 position, Color color, TMP_FontAsset font)
        {
            var label = controls.Find(name);
            if (label == null)
            {
                label = new GameObject(name).AddComponent<TextMeshPro>().transform;
                label.SetParent(controls, false);
            }
            label.localPosition = position;
            label.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.right); // flat on the desk, its top away from the officer
            var tmp = label.GetComponent<TextMeshPro>();
            tmp.rectTransform.sizeDelta = new Vector2(0.3f, 0.06f);
            tmp.font = font;
            tmp.text = text;
            tmp.color = color;
            tmp.fontStyle = FontStyles.Bold;
            tmp.fontSize = 0.042f / _unitsPerFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        /// <summary>
        /// PF_CounterStation: the drawer (moved here from the station builder, same spot), the paper spots on the desk and the
        /// DocumentDesk with its references. Removes the P38 placeholder sheet.
        /// </summary>
        private static void WireCounter(DocumentView id, DocumentView permit, DocumentView manifest, DocumentView citation, DocumentView noticeDoc,
            TMP_FontAsset font, DocumentTuning tuning, List<string> log)
        {
            var root = PrefabUtility.LoadPrefabContents(CounterPrefab);
            try
            {
                var controls = root.transform.Find("Controls"); // at desk height (0.85), origin = chair
                var old = controls.Find("Doc_Placeholder");
                if (old != null) Object.DestroyImmediate(old.gameObject);

                var drawer = controls.Find("Drawer");
                if (drawer == null)
                {
                    var model = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(DocSlotModel));
                    drawer = model.transform;
                    drawer.name = "Drawer";
                    drawer.SetParent(controls, false);
                    model.AddComponent<BoxCollider>();
                }
                // Same place as the old builder prop: world (-0.3, 0.85, -9). No yaw: the front with "DOCUMENTS" (model -X) faces the officer,
                // the drawer slides along its local +X toward the traveler (DocumentDesk.MoveDrawer).
                drawer.localPosition = new Vector3(1.7f, 0f, 0f);
                drawer.localRotation = Quaternion.identity;
                var tray = drawer.Find("Tray");
                if (tray == null) tray = new GameObject("Tray").transform;
                tray.SetParent(drawer, false);
                tray.localPosition = new Vector3(0f, 0.03f, 0f);
                tray.localRotation = Quaternion.Euler(0f, 90f, 0f); // papers lie with their top toward the traveler

                var spotsRoot = controls.Find("PaperSpots");
                if (spotsRoot == null) spotsRoot = new GameObject("PaperSpots").transform;
                spotsRoot.SetParent(controls, false);
                spotsRoot.localPosition = Vector3.zero;
                // Between the stamps (left) and the buttons (right); page top away from the officer (yaw 90)
                var spotPositions = new[] { new Vector3(1.0f, 0.001f, 0.05f), new Vector3(1.12f, 0.001f, -0.18f), new Vector3(0.9f, 0.001f, 0.32f) };
                var spots = new Transform[spotPositions.Length];
                for (int i = 0; i < spots.Length; i++)
                {
                    var s = spotsRoot.Find("Spot_" + i);
                    if (s == null) s = new GameObject("Spot_" + i).transform;
                    s.SetParent(spotsRoot, false);
                    s.localPosition = spotPositions[i];
                    s.localRotation = Quaternion.Euler(0f, 90f + (i - 1) * 6f, 0f);
                    spots[i] = s;
                }

                // Desk top: world x -1.3..-0.1, z -11..-7 (SM_Counter_Desk); papers stay 5 cm inside
                var area = controls.Find("PaperArea");
                if (area == null) area = new GameObject("PaperArea").transform;
                area.SetParent(controls, false);
                area.localPosition = new Vector3(1.3f, 0.001f, 0f);
                area.localRotation = Quaternion.identity;

                WireButtons(root, controls, font);
                WireGadgets(root, controls, font, log);

                // Citation printer on the desk, slot toward the officer (yaw 90: model front -Z → world -X)
                var printer = controls.Find("Printer");
                var printerAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrinterModel);
                if (printer != null && printerAsset != null && printer.Find("Printer_Placeholder") != null)
                {
                    Object.DestroyImmediate(printer.gameObject); // the delivered model replaces the grey box
                    printer = null;
                }
                if (printer == null)
                {
                    if (printerAsset != null) printer = ((GameObject)PrefabUtility.InstantiatePrefab(printerAsset)).transform;
                    else
                    {
                        printer = new GameObject("Printer").transform;
                        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        box.name = "Printer_Placeholder";
                        box.transform.SetParent(printer, false);
                        box.transform.localPosition = new Vector3(0f, 0.04f, 0f);
                        box.transform.localScale = new Vector3(0.22f, 0.08f, 0.18f);
                        box.GetComponent<MeshRenderer>().sharedMaterial = UnlitMaterial("M_Printer_Placeholder", null, new Color(0.32f, 0.32f, 0.3f));
                    }
                    printer.name = "Printer";
                    printer.SetParent(controls, false);
                }
                // In view: right of the drawer, above the buttons (world x -0.45, z -9.8)
                printer.localPosition = new Vector3(1.55f, 0f, -0.8f);
                printer.localRotation = Quaternion.Euler(0f, 90f, 0f);
                var printerOut = printer.Find("PaperOut");
                if (printerOut == null) printerOut = new GameObject("PaperOut").transform;
                printerOut.SetParent(printer, false);
                printerOut.localPosition = new Vector3(0f, 0.06f, -0.1f); // in front of the slot
                printerOut.localRotation = Quaternion.identity;
                var printerSpot = controls.Find("PrinterSpot");
                if (printerSpot == null) printerSpot = new GameObject("PrinterSpot").transform;
                printerSpot.SetParent(controls, false);
                printerSpot.localPosition = new Vector3(1.3f, 0.001f, -0.72f);
                printerSpot.localRotation = Quaternion.Euler(0f, 100f, 0f);

                var desk = root.GetComponent<DocumentDesk>();
                if (desk == null) desk = root.AddComponent<DocumentDesk>(); // no ?? on Unity objects (fake null)
                var so = new SerializedObject(desk);
                so.FindProperty("station").objectReferenceValue = root.GetComponent<CounterStation>();
                so.FindProperty("tuning").objectReferenceValue = tuning;
                so.FindProperty("drawer").objectReferenceValue = drawer;
                so.FindProperty("drawerTray").objectReferenceValue = tray;
                so.FindProperty("paperArea").objectReferenceValue = area;
                so.FindProperty("paperAreaSize").vector2Value = new Vector2(1.1f, 3.9f);
                var spotsProp = so.FindProperty("deskSpots");
                spotsProp.arraySize = spots.Length;
                for (int i = 0; i < spots.Length; i++) spotsProp.GetArrayElementAtIndex(i).objectReferenceValue = spots[i];
                so.FindProperty("idCardPrefab").objectReferenceValue = id;
                so.FindProperty("entryPermitPrefab").objectReferenceValue = permit;
                so.FindProperty("cargoManifestPrefab").objectReferenceValue = manifest;
                so.FindProperty("citationPrefab").objectReferenceValue = citation;
                so.FindProperty("noticePrefab").objectReferenceValue = noticeDoc;
                so.FindProperty("printerOut").objectReferenceValue = printerOut;
                so.FindProperty("printerSpot").objectReferenceValue = printerSpot;
                so.ApplyModifiedPropertiesWithoutUndo();

                // The shift: travelers of the test day until the day flow (P09) hands in real days
                var shift = root.GetComponent<CounterShift>();
                if (shift == null) shift = root.AddComponent<CounterShift>();
                var sso = new SerializedObject(shift);
                sso.FindProperty("nextTravelerDelay").floatValue = 0.5f; // P91: on top of the time the last traveler needs to leave
                sso.FindProperty("station").objectReferenceValue = root.GetComponent<CounterStation>();
                sso.FindProperty("desk").objectReferenceValue = desk;
                var testDay = AssetDatabase.LoadAssetAtPath<DayRules>(TestDayPath);
                if (testDay == null) log.Add("WARNING  no " + TestDayPath + " - run GameOff/Build Test Day, else no travelers come");
                sso.FindProperty("day").objectReferenceValue = testDay;
                sso.ApplyModifiedPropertiesWithoutUndo();

                var cso = new SerializedObject(root.GetComponent<CounterStation>());
                cso.FindProperty("documents").objectReferenceValue = desk;
                cso.ApplyModifiedPropertiesWithoutUndo();
                var hintT = root.transform.Find("SeatedHUD/Hint");
                if (hintT == null) throw new System.InvalidOperationException("PF_CounterStation has no SeatedHUD/Hint");
                var hint = hintT.GetComponent<TextMeshProUGUI>();
                hint.text = "Stamp, hand papers back through the drawer  ·  or ARREST / AIRLOCK  ·  click papers or notes to read side by side, [R] all, a field to ask  ·  [A]/[D] look  ·  [Q]/[Esc] back / leave";
                // Notice line above the hint (\"Stamp the papers first\" and the like), hidden until used
                var noticeT = root.transform.Find("SeatedHUD/Notice");
                if (noticeT == null)
                {
                    noticeT = Object.Instantiate(hint.gameObject, hint.transform.parent).transform;
                    noticeT.name = "Notice";
                }
                var notice = noticeT.GetComponent<TextMeshProUGUI>();
                var hintRect = hint.rectTransform;
                var noticeRect = notice.rectTransform;
                noticeRect.anchorMin = hintRect.anchorMin;
                noticeRect.anchorMax = hintRect.anchorMax;
                noticeRect.pivot = hintRect.pivot;
                noticeRect.sizeDelta = hintRect.sizeDelta;
                noticeRect.anchoredPosition = hintRect.anchoredPosition + new Vector2(0f, 60f);
                notice.fontSize = hint.fontSize * 1.6f;
                notice.color = new Color(1f, 0.72f, 0.3f);
                notice.text = "";
                noticeT.gameObject.SetActive(false);
                cso.FindProperty("notice").objectReferenceValue = notice;
                cso.ApplyModifiedPropertiesWithoutUndo();
                WireBoard(root, desk, shift, hint, log);
                WireTraveler(root, desk, shift, hint, log);
                WireTutorial(root, desk, shift, hint, log);
                WireShutter(root, log);
                var unusedOuter = root.transform.Find("Traveler_DoorOuter"); // P91: nobody walks to the outer hatch any more
                if (unusedOuter != null) Object.DestroyImmediate(unusedOuter.gameObject);
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true)) text.font = font; // hint, notice, subtitle, labels (P54)
                PrefabUtility.SaveAsPrefabAsset(root, CounterPrefab);
                log.Add("counter wired: drawer, 3 spots, DocumentDesk, pinboard, look-around, announcements");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        /// <summary>P41: the note spots on the pinboard, looking around while seated, loudspeaker lines with a subtitle bar.</summary>
        private static void WireBoard(GameObject root, DocumentDesk desk, CounterShift shift, TextMeshProUGUI hint, List<string> log)
        {
            var station = root.GetComponent<CounterStation>();
            var board = root.transform.Find("BoardSpots");
            if (board == null) board = new GameObject("BoardSpots").transform;
            board.SetParent(root.transform, false);
            // Mirrors SM_Counter_Board in StationGrayboxBuilder.BuildProps: 1 x 0.7 m, center at world (-1.5, 1.75), front at z -6.255;
            // the counter stands at (-2, 0, -9). Page top up, page face into the room (-Z).
            board.localPosition = new Vector3(0.5f, 1.75f, 2.74f);
            board.localRotation = Quaternion.LookRotation(Vector3.up, Vector3.back);
            // Three 0.30 m notes side by side on the 1 m board (P68 added MARKET, only hung on days with market events).
            var bulletinSpot = BoardSpot(board, "Bulletin", -0.33f);
            var wantedSpot = BoardSpot(board, "Wanted", 0f);
            var marketSpot = BoardSpot(board, "Market", 0.33f);
            var pinBoard = root.GetComponent<PinBoard>();
            if (pinBoard == null) pinBoard = root.AddComponent<PinBoard>();
            var pso = new SerializedObject(pinBoard);
            pso.FindProperty("shift").objectReferenceValue = shift;
            pso.FindProperty("desk").objectReferenceValue = desk;
            pso.FindProperty("bulletinSpot").objectReferenceValue = bulletinSpot;
            pso.FindProperty("wantedSpot").objectReferenceValue = wantedSpot;
            pso.FindProperty("marketSpot").objectReferenceValue = marketSpot;
            pso.ApplyModifiedPropertiesWithoutUndo();

            var deskCamera = (Component)new SerializedObject(station).FindProperty("deskCamera").objectReferenceValue;
            var look = root.GetComponent<SeatedLook>();
            if (look == null) look = root.AddComponent<SeatedLook>();
            var lso = new SerializedObject(look);
            lso.FindProperty("station").objectReferenceValue = station;
            lso.FindProperty("desk").objectReferenceValue = desk;
            lso.FindProperty("deskCamera").objectReferenceValue = deskCamera.transform;
            lso.FindProperty("moveAction").objectReferenceValue = PlayerAction("Move");
            lso.FindProperty("tuning").objectReferenceValue = Asset<SeatedLookTuning>(CounterDataFolder + "SeatedLookTuning.asset");
            lso.ApplyModifiedPropertiesWithoutUndo();

            // Subtitle bar low in the seated HUD, above the notice line: at the top it would cover the traveler's head (P40)
            var barT = hint.transform.parent.Find("SubtitleBar");
            if (barT == null)
            {
                var barGo = new GameObject("SubtitleBar", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                barT = barGo.transform;
                barT.SetParent(hint.transform.parent, false);
                var line = Object.Instantiate(hint.gameObject, barT);
                line.name = "Text";
            }
            var barRect = (RectTransform)barT;
            barRect.anchorMin = new Vector2(0.1f, 0f);
            barRect.anchorMax = new Vector2(0.9f, 0f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.sizeDelta = new Vector2(0f, 42f);
            barRect.anchoredPosition = new Vector2(0f, 58f); // below the stamps and buttons
            var barImage = barT.GetComponent<UnityEngine.UI.Image>();
            barImage.color = new Color(0f, 0f, 0f, 0.72f);
            barImage.raycastTarget = false;
            var subtitle = barT.Find("Text").GetComponent<TextMeshProUGUI>();
            var textRect = subtitle.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(16f, 4f);
            textRect.offsetMax = new Vector2(-16f, -4f);
            subtitle.fontSize = hint.fontSize * 1.1f;
            subtitle.fontSizeMax = subtitle.fontSize;
            subtitle.fontSizeMin = subtitle.fontSize * 0.6f;
            subtitle.enableAutoSizing = true; // a long line stays inside the bar
            subtitle.color = new Color(0.89f, 0.87f, 0.81f);
            subtitle.alignment = TextAlignmentOptions.Center;
            subtitle.raycastTarget = false;
            subtitle.text = "";
            barT.gameObject.SetActive(false);

            var announcer = root.GetComponent<Announcer>();
            if (announcer == null) announcer = root.AddComponent<Announcer>();
            var aso = new SerializedObject(announcer);
            aso.FindProperty("shift").objectReferenceValue = shift;
            aso.FindProperty("station").objectReferenceValue = station;
            aso.FindProperty("speakerLamp").objectReferenceValue = SpeakerLamp(root);
            aso.FindProperty("lines").objectReferenceValue = Asset<Announcements>(CounterDataFolder + "Announcements.asset");
            aso.FindProperty("subtitleRoot").objectReferenceValue = barT.gameObject;
            aso.FindProperty("subtitle").objectReferenceValue = subtitle;
            aso.ApplyModifiedPropertiesWithoutUndo();
            log.Add("pinboard spots, SeatedLook, Announcer");
        }

        /// <summary>P33: a small lamp under SM_Counter_Speaker (world -2.5, 2.3 on the north wall; chair = origin), off until an announcement.</summary>
        private static GameObject SpeakerLamp(GameObject root)
        {
            var old = root.transform.Find("SpeakerLamp");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lamp.name = "SpeakerLamp";
            Object.DestroyImmediate(lamp.GetComponent<Collider>());
            lamp.transform.SetParent(root.transform, false);
            lamp.transform.localPosition = new Vector3(-0.5f, 2.1f, 2.93f);
            lamp.transform.localScale = new Vector3(0.05f, 0.05f, 0.02f);
            var material = UnlitMaterial("M_Counter_SpeakerLamp", null, new Color(1f, 0.72f, 0.25f));
            lamp.GetComponent<MeshRenderer>().sharedMaterial = material;
            lamp.SetActive(false);
            return lamp;
        }

        private const string ShutterModel = "Assets/_Project/Art/Models/Counter/SM_Counter_SecurityShutter.fbx";

        /// <summary>
        /// P91: the shutter behind the counter window on the lock side (chair = origin, glass at x 2, window 4 m wide). The delivered model
        /// if it exists (pivot top_center), otherwise grey slats.
        /// </summary>
        private static void WireShutter(GameObject root, List<string> log)
        {
            var shutterT = root.transform.Find("SecurityShutter");
            if (shutterT != null) Object.DestroyImmediate(shutterT.gameObject);
            shutterT = new GameObject("SecurityShutter").transform;
            shutterT.SetParent(root.transform, false);
            shutterT.localPosition = new Vector3(2.12f, 0f, 0f);
            var slats = new GameObject("Slats").transform;
            slats.SetParent(shutterT, false);
            // Its height is set by SecurityShutter at runtime.
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ShutterModel);
            if (model != null) ((GameObject)PrefabUtility.InstantiatePrefab(model)).transform.SetParent(slats, false);
            else
            {
                var material = UnlitMaterial("M_Counter_Shutter_Placeholder", null, new Color(0.15f, 0.16f, 0.19f));
                for (int i = 0; i < 12; i++)
                {
                    var slat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    slat.name = "Slat_" + i;
                    Object.DestroyImmediate(slat.GetComponent<Collider>());
                    slat.transform.SetParent(slats, false);
                    slat.transform.localPosition = new Vector3(0f, -0.09f - i * 0.185f, 0f);
                    slat.transform.localScale = new Vector3(0.03f, 0.17f, 4.4f);
                    slat.GetComponent<MeshRenderer>().sharedMaterial = material;
                }
            }
            var shutter = shutterT.gameObject.AddComponent<SecurityShutter>();
            var so = new SerializedObject(shutter);
            so.FindProperty("slats").objectReferenceValue = slats;
            so.ApplyModifiedPropertiesWithoutUndo();
            slats.gameObject.SetActive(false); // shown when it falls; at height 0 it would hang below the floor
            var pso = new SerializedObject(root.GetComponent<TravelerPresenter>());
            pso.FindProperty("shutter").objectReferenceValue = shutter;
            pso.FindProperty("looks").objectReferenceValue = AssetDatabase.LoadAssetAtPath<TravelerLooks>("Assets/_Project/Data/Travelers/TravelerLooks.asset");
            // P104: the guard's own model when delivered, until then a human traveler
            var guard = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + "PF_NPC_Guard.prefab");
            if (guard == null) guard = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + "PF_NPC_Human.prefab");
            pso.FindProperty("guardModel").objectReferenceValue = guard;
            // P105: the lock's alarm, a red light above the marking (chair = origin, lock centre at x 4)
            var alarmT = root.transform.Find("AirlockAlarm");
            if (alarmT == null) alarmT = new GameObject("AirlockAlarm").transform;
            alarmT.SetParent(root.transform, false);
            alarmT.localPosition = new Vector3(4f, 2.6f, 0f);
            var alarm = alarmT.GetComponent<Light>();
            if (alarm == null) alarm = alarmT.gameObject.AddComponent<Light>();
            alarm.type = LightType.Point;
            alarm.color = new Color(1f, 0.12f, 0.08f);
            alarm.intensity = 8f;
            alarm.range = 7f;
            alarm.shadows = LightShadows.None;
            alarm.enabled = false;
            pso.FindProperty("alarmLight").objectReferenceValue = alarm;
            pso.ApplyModifiedPropertiesWithoutUndo();
            log.Add("security shutter");
        }

        /// <summary>P86: Inspector Korr's board, a strip along the top of the seated HUD, right of the day label and above the papers held up (they end at 90 %).</summary>
        private static void WireTutorial(GameObject root, DocumentDesk desk, CounterShift shift, TextMeshProUGUI hint, List<string> log)
        {
            var hud = hint.transform.parent;
            var panelT = hud.Find("TutorialPanel");
            if (panelT == null)
            {
                panelT = new GameObject("TutorialPanel", typeof(RectTransform), typeof(UnityEngine.UI.Image)).transform;
                panelT.SetParent(hud, false);
                Object.Instantiate(hint.gameObject, panelT).name = "Text";
            }
            var panelRect = (RectTransform)panelT;
            panelRect.anchorMin = new Vector2(0.25f, 0.905f);
            panelRect.anchorMax = new Vector2(0.98f, 0.995f);
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            var image = panelT.GetComponent<UnityEngine.UI.Image>();
            image.color = new Color(0f, 0f, 0f, 0.8f);
            image.raycastTarget = false;
            var text = panelT.Find("Text").GetComponent<TextMeshProUGUI>();
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(12f, 3f);
            textRect.offsetMax = new Vector2(-12f, -3f);
            text.fontSize = hint.fontSize * 1.05f;
            text.fontSizeMax = text.fontSize;
            text.fontSizeMin = text.fontSize * 0.6f;
            text.enableAutoSizing = true; // the longest step stays inside the board
            text.color = new Color(0.89f, 0.87f, 0.81f);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.raycastTarget = false;
            text.text = "";
            panelT.gameObject.SetActive(false);

            var korr = AssetDatabase.LoadAssetAtPath<TravelerData>(KorrTutorialTraveler);
            if (korr == null) log.Add("WARNING  no " + KorrTutorialTraveler + " - run GameOff/Build Story Characters, else no tutorial");
            var tutorial = root.GetComponent<CounterTutorial>();
            if (tutorial == null) tutorial = root.AddComponent<CounterTutorial>();
            var so = new SerializedObject(tutorial);
            so.FindProperty("shift").objectReferenceValue = shift;
            so.FindProperty("desk").objectReferenceValue = desk;
            so.FindProperty("station").objectReferenceValue = root.GetComponent<CounterStation>();
            so.FindProperty("tutorialTraveler").objectReferenceValue = korr;
            so.FindProperty("panel").objectReferenceValue = panelT.gameObject;
            so.FindProperty("text").objectReferenceValue = text;
            so.ApplyModifiedPropertiesWithoutUndo();
            log.Add("Korr's tutorial board");
        }

        /// <summary>P40: the traveler's marking and doors in the inspection lock, the speech bubble, a desk view that shows them.</summary>
        private static void WireTraveler(GameObject root, DocumentDesk desk, CounterShift shift, TextMeshProUGUI hint, List<string> log)
        {
            var station = root.GetComponent<CounterStation>();
            // World positions from STATION.md (lock x 0..4, z -12..-6; the counter stands at -2, 0, -9)
            var stand = LockPoint(root.transform, "Traveler_Stand", new Vector3(4f, 0f, 0f), Quaternion.Euler(0f, -90f, 0f));
            var hallDoor = LockPoint(root.transform, "Traveler_DoorHall", new Vector3(4f, 0f, 3f), Quaternion.identity);
            var hangarDoor = LockPoint(root.transform, "Traveler_DoorHangar", new Vector3(4f, 0f, -3f), Quaternion.identity);

            // The desk view must reach over the tallest traveler (Vellani, 2.2 m); the desk itself stays in view
            var deskCamera = (Component)new SerializedObject(station).FindProperty("deskCamera").objectReferenceValue;
            deskCamera.transform.localRotation = Quaternion.Euler(20f, 90f, 0f);

            var parent = hint.transform.parent;
            var panelT = parent.Find("SpeechBubble");
            if (panelT == null)
            {
                panelT = new GameObject("SpeechBubble", typeof(RectTransform), typeof(UnityEngine.UI.Image)).transform;
                panelT.SetParent(parent, false);
                var line = Object.Instantiate(hint.gameObject, panelT);
                line.name = "Text";
            }
            var panel = (RectTransform)panelT;
            panel.pivot = new Vector2(0f, 1f);
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f, 0.5f);
            var image = panelT.GetComponent<UnityEngine.UI.Image>();
            image.color = new Color(0.89f, 0.87f, 0.81f, 0.95f);
            image.raycastTarget = false;
            var text = panelT.Find("Text").GetComponent<TextMeshProUGUI>();
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(12f, 12f);
            textRect.offsetMax = new Vector2(-12f, -12f);
            text.fontSize = hint.fontSize * 1.1f;
            text.color = new Color(0.1f, 0.1f, 0.12f);
            text.alignment = TextAlignmentOptions.TopLeft;
            text.raycastTarget = false;
            text.text = "";
            panelT.gameObject.SetActive(false);

            var tuning = Asset<DialogueTuning>(CounterDataFolder + "DialogueTuning.asset");
            var bubble = root.GetComponent<SpeechBubble>();
            if (bubble == null) bubble = root.AddComponent<SpeechBubble>();
            var bso = new SerializedObject(bubble);
            bso.FindProperty("station").objectReferenceValue = station;
            bso.FindProperty("tuning").objectReferenceValue = tuning;
            bso.FindProperty("panel").objectReferenceValue = panel;
            bso.FindProperty("canvasArea").objectReferenceValue = parent;
            bso.FindProperty("text").objectReferenceValue = text;
            bso.ApplyModifiedPropertiesWithoutUndo();

            var presenter = root.GetComponent<TravelerPresenter>();
            if (presenter == null) presenter = root.AddComponent<TravelerPresenter>();
            var pso = new SerializedObject(presenter);
            pso.FindProperty("shift").objectReferenceValue = shift;
            pso.FindProperty("desk").objectReferenceValue = desk;
            pso.FindProperty("bubble").objectReferenceValue = bubble;
            pso.FindProperty("tuning").objectReferenceValue = tuning;
            pso.FindProperty("standSpot").objectReferenceValue = stand;
            pso.FindProperty("hallDoor").objectReferenceValue = hallDoor;
            pso.FindProperty("hangarDoor").objectReferenceValue = hangarDoor;
            pso.ApplyModifiedPropertiesWithoutUndo();
            log.Add("TravelerPresenter, SpeechBubble, desk view 20 degrees");
        }

        private static Transform LockPoint(Transform root, string name, Vector3 position, Quaternion rotation)
        {
            var point = root.Find(name);
            if (point == null) point = new GameObject(name).transform;
            point.SetParent(root, false);
            point.localPosition = position;
            point.localRotation = rotation;
            return point;
        }

        private static Transform BoardSpot(Transform board, string name, float x)
        {
            return LockPoint(board, name, new Vector3(x, 0f, 0f), Quaternion.identity);
        }

        private static InputActionReference PlayerAction(string action)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(InputActionsPath))
                if (asset is InputActionReference reference && reference.action != null && reference.action.name == action && reference.action.actionMap.name == "Player")
                    return reference;
            throw new System.InvalidOperationException($"{InputActionsPath} has no Player/{action} reference");
        }
    }
}
