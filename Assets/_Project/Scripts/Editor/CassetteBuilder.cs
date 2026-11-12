using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Economy;
using GameOff.Player;
using GameOff.Rest;
using UnityEditor;
using UnityEngine;
using static GameOff.EditorTools.MenuSceneBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P132, idempotent. Data (called by EconomyBuilder, so "Update Economy Data" and "Build Economy" make them): five cassettes
    /// Data/Items/Item_Cassette_01..05 (CassetteData: unique, no labels, no cargo word), each with its song (Audio/Music/MUS_Cassette_0N,
    /// MUS_Floppy_01 until that is delivered: the next run picks the song up) and its label: the label tile is painted into a copy of the
    /// cassette albedo (Art/Textures/Items/Cassettes, generated) with its own material (Art/Materials/Items/M_Item_Cassette_0N).
    /// Scene pieces (called by RestSpotBuilder): the tape deck (SM_Counter_CassettePlayer with one [E] trigger per slot, a cassette model
    /// per slot, lamps, lever, 3D music source) and a cassette lying somewhere to pick up.
    /// Sources: kiosk 01 + 05, Tobin Rask's gift 02 (StoryCharacterBuilder), lying in the hangar 03 and at the bar 04.
    /// </summary>
    public static class CassetteBuilder
    {
        private const string ItemFolder = "Assets/_Project/Data/Items/";
        private const string IconPath = "Assets/_Project/Art/UI/Icons/UI_Icon_Item_Cassette.png";
        private const string CassetteModelPath = "Assets/_Project/Art/Models/Items/SM_Item_Cassette.fbx";
        private const string DeckModelPath = "Assets/_Project/Art/Models/Counter/SM_Counter_CassettePlayer.fbx";
        private const string AlbedoPath = "Assets/_Project/Art/Textures/Items/T_Item_Cassette_Albedo.png";
        private const string LabelFormat = "Assets/_Project/Art/Textures/Items/T_Item_Cassette_Label_{0:00}.png";
        private const string TapeTextureFormat = "Assets/_Project/Art/Textures/Items/Cassettes/T_Item_Cassette_Tape_{0:00}.png";
        private const string MaterialFormat = "Assets/_Project/Art/Materials/Items/M_Item_Cassette_{0:00}.mat";
        private const string BaseMaterialPath = "Assets/_Project/Art/Materials/Items/M_Item_Cassette.mat"; // DeliveryIntegrator makes it
        private const string TemplateMaterialPath = "Assets/_Project/Art/Materials/Diner/M_Diner_Counter.mat"; // as in DeliveryIntegrator
        private const string SongFormat = "Assets/_Project/Audio/Music/MUS_Cassette_{0:00}.ogg";
        private const string FallbackSongPath = "Assets/_Project/Audio/Music/MUS_Floppy_01.ogg";

        // The label tile (64 x 32) in the delivered 128 px albedo, counted from the top left; T_Item_Cassette_Label_01 is the one painted in.
        private const int LabelX = 6, LabelTop = 3;
        /// <summary>Kiosk price (base value, no labels, no daily change): modest. The Economy Report never buys cassettes.</summary>
        private const int Price = 6;

        /// <summary>Number (= song), title, label tile, text, on the kiosk shelf from this day (0 = not sold).</summary>
        private static readonly (int number, string title, int label, string text, int kioskFromDay)[] Tapes =
        {
            (1, "Night Shift", 1, "Slow, warm synths for the hours when nobody else is awake. The label is written by hand.", 1),
            (2, "Dock 7 Mix", 2, "Loose grooves taped off the canteen radio on dock seven. Somebody wrote TOBIN on the back.", 0),
            (3, "Long Haul", 3, "A few notes and long, slightly detuned pads. Music for missing home.", 0),
            (4, "Slow Dance", 5, "Lounge synths with brushed drums. It smells faintly of the bar.", 0),
            (5, "Quiet Hours", 4, "A steady, humming pulse, like the station when it sleeps.", 1),
        };

        // Deck model (pivot bottom centre, front = local -Z): a cassette stands upright in a slot, label to the front, its lower edge
        // on the slot's empty (Slot_1..3), so the upper part with the label shows. The model lies flat (label up, 12 mm thick).
        private static readonly Vector3 CassetteInSlot = new Vector3(0f, 0.032f, 0.006f);
        private static readonly Quaternion CassetteUpright = Quaternion.Euler(-90f, 0f, 0f);
        private static readonly Vector3 DeckSize = new Vector3(0.45f, 0.123f, 0.2f);

        public static string ItemPath(int number) => $"{ItemFolder}Item_Cassette_{number:00}.asset";

        public static CassetteData Cassette(int number) => AssetDatabase.LoadAssetAtPath<CassetteData>(ItemPath(number));

        // ---------- Data ----------

        /// <summary>Creates or updates the five cassettes; returns the ones the kiosk sells (with their first day).</summary>
        public static List<(CassetteData item, int fromDay)> EnsureItems(List<string> log, ref int created)
        {
            var kiosk = new List<(CassetteData, int)>();
            var icon = Icon(log);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(CassetteModelPath);
            var fallbackSong = AssetDatabase.LoadAssetAtPath<AudioClip>(FallbackSongPath);
            int songs = 0;
            foreach (var t in Tapes)
            {
                string path = ItemPath(t.number);
                var item = AssetDatabase.LoadAssetAtPath<CassetteData>(path);
                if (item == null)
                {
                    if (AssetDatabase.LoadAssetAtPath<Object>(path) != null)
                    {
                        log.Add($"ERROR {path} exists but is no CassetteData - delete it and run again");
                        continue;
                    }
                    item = ScriptableObject.CreateInstance<CassetteData>();
                    AssetFolders.EnsureForAsset(path);
                    AssetDatabase.CreateAsset(item, path);
                    // Content: set once, tune it in the inspector.
                    var init = new SerializedObject(item);
                    init.FindProperty("displayName").stringValue = "Cassette: " + t.title;
                    init.FindProperty("description").stringValue = t.text;
                    init.FindProperty("baseValue").intValue = Price;
                    init.ApplyModifiedPropertiesWithoutUndo();
                    created++;
                }
                var song = AssetDatabase.LoadAssetAtPath<AudioClip>(string.Format(SongFormat, t.number));
                if (song != null) songs++;
                var so = new SerializedObject(item);
                so.FindProperty("title").stringValue = t.title;
                so.FindProperty("unique").boolValue = true; // one of each
                so.FindProperty("tags").arraySize = 0; // no goods: no label may pull it into the market, wishes or needs
                so.FindProperty("cargoWord").stringValue = "";
                so.FindProperty("wearable").boolValue = false;
                if (icon != null) so.FindProperty("icon").objectReferenceValue = icon;
                if (model != null) so.FindProperty("model").objectReferenceValue = model;
                so.FindProperty("song").objectReferenceValue = song != null ? song : fallbackSong;
                var material = TapeMaterial(t.number, t.label, log);
                if (material != null) so.FindProperty("material").objectReferenceValue = material;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(item);
                if (t.kioskFromDay > 0) kiosk.Add((item, t.kioskFromDay));
            }
            log.Add($"cassettes: {Tapes.Length} ({songs} own songs, the rest play {Path.GetFileNameWithoutExtension(FallbackSongPath)})");
            return kiosk;
        }

        /// <summary>The shared inventory icon as a sprite (icons are set up by hand; this one is set up here).</summary>
        private static Sprite Icon(List<string> log)
        {
            if (!(AssetImporter.GetAtPath(IconPath) is TextureImporter importer))
            {
                log.Add("WARNING no " + IconPath);
                return null;
            }
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
                log.Add("cassette icon imported as a sprite");
            }
            return AssetDatabase.LoadAllAssetsAtPath(IconPath).OfType<Sprite>().FirstOrDefault();
        }

        /// <summary>M_Item_Cassette_0N: the cassette material with this label painted into its albedo.</summary>
        private static Material TapeMaterial(int number, int label, List<string> log)
        {
            var source = AssetDatabase.LoadAssetAtPath<Material>(BaseMaterialPath);
            if (source == null) source = AssetDatabase.LoadAssetAtPath<Material>(TemplateMaterialPath);
            if (source == null)
            {
                log.Add("WARNING no cassette material to copy - run GameOff/Integrate Asset Deliveries first");
                return null;
            }
            var albedo = LabelledAlbedo(number, label, log);
            if (albedo == null) return null;
            string path = string.Format(MaterialFormat, number);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(source) { name = Path.GetFileNameWithoutExtension(path) };
                AssetFolders.EnsureForAsset(path);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = source.shader;
                mat.CopyPropertiesFromMaterial(source);
            }
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BaseMap", albedo);
            mat.SetTexture("_MainTex", albedo);
            mat.SetTexture("_EmissionMap", null); // the cassette doesn't glow (the template material does)
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            EditorUtility.SetDirty(mat);
            return mat;
        }

        /// <summary>A copy of the cassette albedo with label tile n painted in (written only when it changed). Null if a file is missing.</summary>
        private static Texture2D LabelledAlbedo(int number, int label, List<string> log)
        {
            string labelPath = string.Format(LabelFormat, label);
            if (!File.Exists(AlbedoPath) || !File.Exists(labelPath))
            {
                log.Add($"WARNING cassette {number:00}: {AlbedoPath} or {labelPath} missing, no label");
                return null;
            }
            var albedo = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var tile = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            byte[] png;
            try
            {
                albedo.LoadImage(File.ReadAllBytes(AlbedoPath));
                tile.LoadImage(File.ReadAllBytes(labelPath));
                int y = albedo.height - LabelTop - tile.height;
                if (LabelX + tile.width > albedo.width || y < 0)
                {
                    log.Add($"WARNING cassette {number:00}: the label tile doesn't fit the albedo, no label");
                    return null;
                }
                albedo.SetPixels(LabelX, y, tile.width, tile.height, tile.GetPixels());
                albedo.Apply();
                png = albedo.EncodeToPNG();
            }
            finally
            {
                Object.DestroyImmediate(albedo);
                Object.DestroyImmediate(tile);
            }

            string path = string.Format(TapeTextureFormat, number);
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(png))
            {
                AssetFolders.EnsureForAsset(path);
                File.WriteAllBytes(path, png);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
            if (AssetImporter.GetAtPath(path) is TextureImporter importer
                && (importer.filterMode != FilterMode.Point || importer.mipmapEnabled)) // pixel art, like every world texture (DeliveryIntegrator)
            {
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        // ---------- Scene pieces ----------

        /// <summary>
        /// A tape deck standing at <paramref name="position"/> (the table top), its front facing local -Z turned by yaw. The DayManager
        /// reference is linked by the caller (RestSpotBuilder links "day" on everything it builds).
        /// </summary>
        public static CassetteDeck BuildDeck(Transform parent, string id, Vector3 position, float yaw)
        {
            var root = new GameObject("CassetteDeck_" + id);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));

            // Not static: the lever moves and the lamps change, light probes light it.
            Transform body;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(DeckModelPath);
            if (model != null)
            {
                body = ((GameObject)PrefabUtility.InstantiatePrefab(model)).transform;
                body.name = "Deck";
                body.SetParent(root.transform, false);
            }
            else
            {
                Debug.LogWarning($"[CassetteBuilder] {DeckModelPath} missing, grey box instead");
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(box.GetComponent<Collider>());
                box.name = "Deck";
                box.transform.SetParent(root.transform, false);
                box.transform.localPosition = new Vector3(0f, DeckSize.y * 0.5f, 0f);
                box.transform.localScale = DeckSize;
                body = box.transform;
            }

            var cassetteModel = AssetDatabase.LoadAssetAtPath<GameObject>(CassetteModelPath);
            var triggers = new Interactable[3];
            var cassettes = new GameObject[3];
            var lamps = new Renderer[3];
            for (int i = 0; i < 3; i++)
            {
                // Slot position in deck space (the empties of the model; even steps across the top if they are missing).
                var slotPoint = Find(body, $"Slot_{i + 1}");
                Vector3 slot = slotPoint != null ? root.transform.InverseTransformPoint(slotPoint.position) : new Vector3((i - 1) * 0.124f, 0.112f, 0.043f);

                var t = new GameObject($"Slot_{i + 1}_Trigger", typeof(BoxCollider));
                t.transform.SetParent(root.transform, false);
                t.transform.localPosition = new Vector3(slot.x, 0.115f, 0f); // the whole depth of the deck: lamp, number and well
                var collider = t.GetComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(0.118f, 0.13f, DeckSize.z);
                triggers[i] = t.AddComponent<Interactable>();
                SetPrompt(triggers[i], "Insert a cassette");

                if (cassetteModel != null) cassettes[i] = (GameObject)PrefabUtility.InstantiatePrefab(cassetteModel);
                else
                {
                    cassettes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Object.DestroyImmediate(cassettes[i].GetComponent<Collider>());
                    cassettes[i].transform.localScale = new Vector3(0.1f, 0.012f, 0.064f);
                }
                cassettes[i].name = $"Cassette_{i + 1}";
                cassettes[i].transform.SetParent(root.transform, false);
                cassettes[i].transform.localPosition = slot + CassetteInSlot;
                cassettes[i].transform.localRotation = CassetteUpright;
                cassettes[i].SetActive(false);

                var lamp = Find(body, $"Lamp_{i + 1}");
                lamps[i] = lamp != null ? lamp.GetComponent<Renderer>() : null;
            }

            var source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;
            source.minDistance = 1.2f;
            source.maxDistance = 9f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.dopplerLevel = 0f;

            var deck = root.AddComponent<CassetteDeck>();
            var so = new SerializedObject(deck);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            SetArray(deck, "slots", triggers.Cast<Object>().ToArray());
            SetArray(deck, "cassetteModels", cassettes.Cast<Object>().ToArray());
            SetArray(deck, "lamps", lamps.Cast<Object>().ToArray());
            var lever = Find(body, "Lever");
            if (lever != null) Set(deck, "lever", lever);
            Set(deck, "source", source);
            int lampCount = lamps.Count(l => l != null);
            if (model != null && (lampCount < 3 || lever == null))
                Debug.LogWarning($"[CassetteBuilder] deck {id}: {lampCount}/3 lamps, lever {(lever != null ? "ok" : "MISSING")} in the model");
            return deck;
        }

        /// <summary>A cassette lying flat at <paramref name="position"/> (a surface), turned by yaw, to pick up with [E].</summary>
        public static CassettePickup BuildPickup(Transform parent, string id, CassetteData cassette, Vector3 position, float yaw)
        {
            if (cassette == null)
            {
                Debug.LogWarning($"[CassetteBuilder] pickup {id}: cassette missing - run GameOff/Update Economy Data first");
                return null;
            }
            var root = new GameObject("CassettePickup_" + id);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            var cassetteModel = AssetDatabase.LoadAssetAtPath<GameObject>(CassetteModelPath);
            if (cassetteModel != null)
            {
                var model = (GameObject)PrefabUtility.InstantiatePrefab(cassetteModel);
                model.name = "Cassette";
                model.transform.SetParent(root.transform, false);
                if (cassette.Material != null)
                    foreach (var r in model.GetComponentsInChildren<Renderer>(true)) r.sharedMaterial = cassette.Material;
            }

            var t = new GameObject("Trigger", typeof(BoxCollider));
            t.transform.SetParent(root.transform, false);
            t.transform.localPosition = new Vector3(0f, 0.06f, 0f);
            var collider = t.GetComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(0.3f, 0.2f, 0.3f); // generous: the cassette itself is 10 x 6 cm
            var trigger = t.AddComponent<Interactable>();
            SetPrompt(trigger, "Take the cassette");

            var pickup = root.AddComponent<CassettePickup>();
            var so = new SerializedObject(pickup);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            Set(pickup, "cassette", cassette);
            Set(pickup, "interactable", trigger);
            return pickup;
        }

        private static Transform Find(Transform root, string name) =>
            root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);

        private static void SetPrompt(Interactable interactable, string prompt)
        {
            var so = new SerializedObject(interactable);
            so.FindProperty("prompt").stringValue = prompt;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
