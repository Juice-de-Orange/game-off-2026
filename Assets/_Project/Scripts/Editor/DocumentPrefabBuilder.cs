using System.Collections.Generic;
using GameOff.Counter;
using GameOff.Documents;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds everything for the documents on the counter (P06), idempotent: Share Tech Mono TMP font asset, document
    /// materials, the three document prefabs (layout below), test documents, tuning, and wires the DocumentDesk with the
    /// drawer into PF_CounterStation. Document background art is replaced by the pipeline at the same paths.
    /// </summary>
    public static class DocumentPrefabBuilder
    {
        private const string FontPath = "Assets/_Project/Fonts/ShareTechMono-Regular.ttf";
        private const string FontAssetPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";
        private const string TexFolder = "Assets/_Project/Art/Textures/Documents/";
        private const string MatFolder = "Assets/_Project/Art/Materials/Documents/";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Documents/";
        private const string DataFolder = "Assets/_Project/Data/Documents/";
        private const string CounterPrefab = "Assets/_Project/Prefabs/Counter/PF_CounterStation.prefab";
        private const string DocSlotModel = "Assets/_Project/Art/Models/Counter/SM_Counter_DocSlot.fbx";
        private const float LabelSize = 0.0035f, ValueSize = 0.0055f, TitleSize = 0.006f; // cap heights in meters
        private static readonly Color Ink = new Color(0.12f, 0.12f, 0.14f), LabelInk = new Color(0.35f, 0.35f, 0.38f);

        /// <summary>Text on a page: x/z = top-left corner in page coordinates (meters, page center = 0), w = box width.</summary>
        private readonly struct Text
        {
            public readonly string Name, Content; public readonly float X, Z, W, Size; public readonly Color Color;
            public Text(string name, string content, float x, float z, float w, float size, Color color)
            {
                Name = name; Content = content; X = x; Z = z; W = w; Size = size; Color = color;
            }
        }

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
                }, log);

            var tuning = Asset<DocumentTuning>(DataFolder + "DocumentTuning.asset");
            var tests = TestDocuments(photoTex);
            WireCounter(id, permit, manifest, tuning, tests, log);
            AssetDatabase.SaveAssets();
            return "DocumentPrefabBuilder:\n" + string.Join("\n", log);
        }

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
                asset.name = "ShareTechMono SDF";
                AssetDatabase.CreateAsset(asset, FontAssetPath);
                asset.atlasTextures[0].name = "ShareTechMono SDF Atlas";
                AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
                asset.material.name = "ShareTechMono SDF Material";
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
            List<Text> texts, List<string> log)
        {
            int layer = LayerMask.NameToLayer(StampTool.SharpLayer);
            var root = new GameObject("PF_Doc_" + type);
            var box = root.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 0.0006f, 0f);
            box.size = new Vector3(size.x, 0.0012f, size.y);
            root.AddComponent<StampTarget>();
            var view = root.AddComponent<DocumentView>();

            // Page: a quad lying flat (face up, texture top = local +Z)
            var sheet = GameObject.CreatePrimitive(PrimitiveType.Quad);
            sheet.name = "Sheet";
            Object.DestroyImmediate(sheet.GetComponent<Collider>());
            sheet.transform.SetParent(root.transform, false);
            sheet.transform.localPosition = new Vector3(0f, 0.0005f, 0f);
            sheet.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            sheet.transform.localScale = new Vector3(size.x, size.y, 1f);
            sheet.GetComponent<MeshRenderer>().sharedMaterial = UnlitMaterial("M_Doc_" + type, Texture($"T_Doc_{type}.png"));
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
                tmp.textWrappingMode = TextWrappingModes.Normal;
                tmp.overflowMode = TextOverflowModes.Overflow;
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

        /// <summary>One consistent test traveler with all three documents (until P07 brings real travelers).</summary>
        private static List<DocumentData> TestDocuments(Texture2D photo)
        {
            var id = Asset<DocumentData>(DataFolder + "Test/Doc_Test_IdCard.asset");
            id.EditorSet(DocumentType.IdCard, photo, new List<DocumentField>
            {
                new DocumentField("name", "VESNA OKARU"), new DocumentField("species", "HUMAN"), new DocumentField("origin", "TORVIK COLONY"),
                new DocumentField("id", "KX-4471-09"), new DocumentField("expires", "2291-03-14"),
            });
            var permit = Asset<DocumentData>(DataFolder + "Test/Doc_Test_EntryPermit.asset");
            permit.EditorSet(DocumentType.EntryPermit, photo, new List<DocumentField>
            {
                new DocumentField("name", "VESNA OKARU"), new DocumentField("id", "KX-4471-09"), new DocumentField("purpose", "CARGO DELIVERY"),
                new DocumentField("duration", "2 CYCLES"), new DocumentField("expires", "2290-11-30"), new DocumentField("issued", "ASKA-3 CONSULATE"),
            });
            var manifest = Asset<DocumentData>(DataFolder + "Test/Doc_Test_CargoManifest.asset");
            manifest.EditorSet(DocumentType.CargoManifest, null, new List<DocumentField>
            {
                new DocumentField("name", "VESNA OKARU"), new DocumentField("ship", "MV HALDE"), new DocumentField("origin", "TORVIK"),
                new DocumentField("destination", "KESTREL-9"),
                new DocumentField("cargo", "12x  HYDROPONIC NUTRIENT PACK\n 4x  WATER FILTER CARTRIDGE\n 1x  MEDICAL KIT (SEALED)\n 2x  SPARE PARTS CRATE"),
            });
            foreach (var d in new[] { id, permit, manifest }) EditorUtility.SetDirty(d);
            return new List<DocumentData> { id, permit, manifest };
        }

        /// <summary>
        /// PF_CounterStation: the drawer (moved here from the station builder, same spot), the paper spots on the desk and the
        /// DocumentDesk with its references. Removes the P38 placeholder sheet.
        /// </summary>
        private static void WireCounter(DocumentView id, DocumentView permit, DocumentView manifest, DocumentTuning tuning,
            List<DocumentData> tests, List<string> log)
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
                // Same place as the old builder prop: world (-0.3, 0.85, -9), yaw 90 → slides along +X toward the traveler
                drawer.localPosition = new Vector3(1.7f, 0f, 0f);
                drawer.localRotation = Quaternion.Euler(0f, 90f, 0f);
                var tray = drawer.Find("Tray");
                if (tray == null) tray = new GameObject("Tray").transform;
                tray.SetParent(drawer, false);
                tray.localPosition = new Vector3(0f, 0.03f, 0f);
                tray.localRotation = Quaternion.identity;

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
                var testsProp = so.FindProperty("testDocuments");
                testsProp.arraySize = tests.Count;
                for (int i = 0; i < tests.Count; i++) testsProp.GetArrayElementAtIndex(i).objectReferenceValue = tests[i];
                so.ApplyModifiedPropertiesWithoutUndo();

                var cso = new SerializedObject(root.GetComponent<CounterStation>());
                cso.FindProperty("documents").objectReferenceValue = desk;
                cso.ApplyModifiedPropertiesWithoutUndo();
                var hint = root.GetComponentInChildren<TextMeshProUGUI>(true);
                if (hint != null)
                    hint.text = "Drag papers  ·  click a paper to read it  ·  papers into the drawer, click it to hand back  ·  stamps: click, then click the paper  ·  [Q]/[Esc]/right click: back / leave";
                PrefabUtility.SaveAsPrefabAsset(root, CounterPrefab);
                log.Add("counter wired: drawer, 3 spots, DocumentDesk");
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
    }
}
