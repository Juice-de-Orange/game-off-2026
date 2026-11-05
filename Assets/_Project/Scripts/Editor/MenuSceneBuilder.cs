using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Counter;
using GameOff.Player;
using GameOff.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds the front end (P02; options in tabs: P43; audio: P18), idempotent: Boot.unity (splash), Menu.unity (main menu, options, credits),
    /// PF_PauseMenu.prefab, and places the pause menu in Main.unity wired to the player and the counter.
    /// Credits text is taken from CREDITS.md at build time, so rerun after adding credits.
    /// Placeholder look (Share Tech Mono, palette from STYLE.md); final UI art replaces the sprites later.
    /// </summary>
    public static class MenuSceneBuilder
    {
        private const string BootScene = "Assets/_Project/Scenes/Boot.unity";
        private const string MenuScene = "Assets/_Project/Scenes/Menu.unity";
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PauseMenuPrefab = "Assets/_Project/Prefabs/UI/PF_PauseMenu.prefab";
        private const string AudioManagerPrefab = "Assets/_Project/Prefabs/Audio/PF_AudioManager.prefab";
        private const string MixerPath = "Assets/_Project/Audio/GameMixer.mixer"; // created once by hand/script, groups Master > Music, SFX, Voice
        private const string FontAssetPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";
        // All text uses Orbitron (martin 05.11., revises DECISIONS 31.10.); Share Tech Mono stays as fallback for missing glyphs.
        private const string TitleFontPath = "Assets/_Project/Fonts/Orbitron-Regular.ttf";
        private const string TitleFontAssetPath = "Assets/_Project/Fonts/Orbitron SDF.asset";
        private const string CreditsFile = "CREDITS.md";
        private const string SkyboxPath = "Assets/_Project/Art/Materials/Station/M_Sky_Space.mat";
        private const string TrafficTuningPath = "Assets/_Project/Data/Menu/ShipTraffic_Menu.asset"; // P47, created once, tune in the inspector
        private const string ShipFolder = "Assets/_Project/Art/Models/Ships/";
        private static readonly string[] BackdropPrefabs =
        {
            "Assets/_Project/Prefabs/Environment/PF_SkyRig.prefab",
            "Assets/_Project/Prefabs/Environment/PF_Exterior.prefab",
            "Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab",
        };
        // Station + exterior together span about x -45..48, y 0..26, z -50..76 (measured in Main).
        private static readonly Vector3 BackdropTarget = new Vector3(2f, 9f, 5f);
        private static readonly Vector3 BackdropLookOffset = new Vector3(-28f, 0f, 0f); // keeps the station right of the menu
        private const float MenuStripWidth = 400f;

        private const string GameTitle = "KESTREL-9";
        private const string GameSubtitle = "CUSTOMS & EXCISE  ·  NIGHT SHIFT";

        // STYLE.md palette
        internal static readonly Color Backdrop = Hex("0D0F14");
        internal static readonly Color Panel = Hex("1B1F27");
        internal static readonly Color ButtonIdle = Hex("2C323C");
        internal static readonly Color ButtonHover = Hex("444B55");
        internal static readonly Color Amber = Hex("FFD36B");
        internal static readonly Color TextLight = Hex("E2DDCF");
        internal static readonly Color TextDim = Hex("8B9096");

        private static TMP_FontAsset _font;
        private static TMP_FontAsset _titleFont;
        private static GameObject _audioPrefab;

        /// <summary>Only Menu.unity (e.g. after tuning the ship traffic or a station rebuild) – Boot and Main stay untouched.</summary>
        [MenuItem("GameOff/Build Menu Scene Only")]
        public static void BuildMenuOnlyFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log(BuildMenuOnly());
        }

        public static string BuildMenuOnly()
        {
            _font = BaseFont();
            _audioPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AudioManagerPrefab);
            var log = new List<string>();
            BuildMenu(log);
            return "[MenuSceneBuilder] " + string.Join("; ", log);
        }

        [MenuItem("GameOff/Build Menu Scenes")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string previous = SceneManager.GetActiveScene().path;
            Debug.Log(Build());
            if (!string.IsNullOrEmpty(previous) && File.Exists(previous)) EditorSceneManager.OpenScene(previous);
        }

        public static string Build()
        {
            _font = BaseFont();
            var log = new List<string>();
            _audioPrefab = BuildAudioManagerPrefab(log);
            BuildBoot(log);
            BuildMenu(log);
            var pausePrefab = BuildPausePrefab(log);
            PlaceInMain(pausePrefab, log);
            CheckBuildSettings(log);
            return "[MenuSceneBuilder] " + string.Join("; ", log);
        }

        // ---------- Boot ----------

        private static void BuildBoot(List<string> log)
        {
            var scene = OpenCleared(BootScene);
            PlaceAudioManager(scene);
            CreateCamera();
            var canvas = CreateCanvas("SplashCanvas", 0);
            var group = new GameObject("Splash", typeof(RectTransform), typeof(CanvasGroup));
            group.transform.SetParent(canvas, false);
            Stretch(group.GetComponent<RectTransform>());
            Label(group.transform, "Presents", "iMATERIX  &  JUICE-DE-ORANGE", 26, TextDim, new Vector2(0f, 40f), new Vector2(800f, 40f));
            Title(Label(group.transform, "Title", GameTitle, 64, Amber, new Vector2(0f, -20f), new Vector2(800f, 80f)));
            Label(group.transform, "Jam", "made for GitHub Game Off 2026", 18, TextDim, new Vector2(0f, -200f), new Vector2(800f, 30f));

            var splash = new GameObject("BootSplash").AddComponent<BootSplash>();
            Set(splash, "splash", group.GetComponent<CanvasGroup>());
            EditorSceneManager.SaveScene(scene);
            log.Add("Boot built");
        }

        // ---------- Menu ----------

        private static void BuildMenu(List<string> log)
        {
            var scene = OpenCleared(MenuScene);
            PlaceAudioManager(scene);
            BuildStationBackdrop(scene, log);
            CreateEventSystem(null);
            var canvas = CreateCanvas("MenuCanvas", 0);

            // Main panel on the left over a dark strip, so the station stays visible on the right.
            var mainPanel = PanelRoot(canvas, "MainPanel", false);
            var strip = new GameObject("LeftStrip", typeof(RectTransform), typeof(Image));
            strip.transform.SetParent(mainPanel, false);
            var stripRt = strip.GetComponent<RectTransform>();
            stripRt.anchorMin = Vector2.zero;
            stripRt.anchorMax = new Vector2(0f, 1f);
            stripRt.pivot = new Vector2(0f, 0.5f);
            stripRt.sizeDelta = new Vector2(MenuStripWidth, 0f);
            strip.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.72f);
            strip.GetComponent<Image>().raycastTarget = false;
            float x = -480f + MenuStripWidth / 2f; // reference width 960 → left edge at -480
            Title(Label(mainPanel, "Title", GameTitle, 64, Amber, new Vector2(x, 170f), new Vector2(MenuStripWidth - 30f, 80f)));
            Label(mainPanel, "Subtitle", GameSubtitle, 16, TextDim, new Vector2(x, 122f), new Vector2(MenuStripWidth, 26f));
            var start = MenuButton(mainPanel, "StartButton", "START SHIFT", new Vector2(x, 30f));
            var options = MenuButton(mainPanel, "OptionsButton", "OPTIONS", new Vector2(x, -35f));
            var credits = MenuButton(mainPanel, "CreditsButton", "CREDITS", new Vector2(x, -100f));
            var quit = MenuButton(mainPanel, "QuitButton", "QUIT", new Vector2(x, -165f));
            Label(mainPanel, "Version", "Game Off 2026 · jam build", 14, TextDim, new Vector2(x, -245f), new Vector2(MenuStripWidth, 24f));

            var optionsPanel = BuildOptionsPanel(canvas, out var optionsBack);
            var creditsPanel = BuildCreditsPanel(canvas, out var creditsBack);
            // Sub pages dim the station instead of hiding it.
            optionsPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, SubPageDim);
            creditsPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, SubPageDim);
            FullscreenCorner(canvas); // last child: on top of every menu page

            var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
            Set(menu, "mainPanel", mainPanel.gameObject);
            Set(menu, "optionsPanel", optionsPanel.gameObject);
            Set(menu, "creditsPanel", creditsPanel.gameObject);
            Set(menu, "startButton", start);
            Set(menu, "optionsButton", options);
            Set(menu, "creditsButton", credits);
            Set(menu, "quitButton", quit);
            SetArray(menu, "backButtons", new Object[] { optionsBack, creditsBack });
            optionsPanel.gameObject.SetActive(false);
            creditsPanel.gameObject.SetActive(false);

            EditorSceneManager.SaveScene(scene);
            log.Add("Menu built");
        }

        /// <summary>
        /// Options in tabs (P43, DEV from P49): AUDIO / CONTROLS / DEV, one page each, rows 50 px apart from RowTop down,
        /// "Reset to defaults" at the bottom of the AUDIO and CONTROLS pages. New settings = one more row on the matching page here.
        /// </summary>
        private static Transform BuildOptionsPanel(Transform parent, out Button back)
        {
            var panel = PanelRoot(parent, "OptionsPanel");
            Title(Label(panel, "Heading", "OPTIONS", 40, Amber, new Vector2(0f, 205f), new Vector2(600f, 50f)));

            string[] tabNames = { "AUDIO", "CONTROLS", "DEV" };
            var tabs = new Button[tabNames.Length];
            var pages = new GameObject[tabNames.Length];
            for (int i = 0; i < tabNames.Length; i++)
            {
                tabs[i] = MenuButton(panel, $"Tab_{tabNames[i]}", tabNames[i], new Vector2((i - (tabNames.Length - 1) / 2f) * 175f, 145f), new Vector2(165f, 40f), 20);
                var page = new GameObject($"Page_{tabNames[i]}", typeof(RectTransform));
                page.transform.SetParent(panel, false);
                Stretch(page.GetComponent<RectTransform>());
                pages[i] = page;
            }

            // AUDIO (P18): one row per AudioChannel, same order as the enum (Master, Music, Sfx, Voice)
            var audio = pages[0].transform;
            string[] channelNames = { "MASTER VOLUME", "MUSIC", "EFFECTS", "VOICE" };
            var volumeSliders = new Slider[channelNames.Length];
            var volumeValues = new TMP_Text[channelNames.Length];
            for (int i = 0; i < channelNames.Length; i++)
            {
                volumeSliders[i] = SliderRow(audio, ((AudioChannel)i).ToString(), channelNames[i], new Vector2(0f, RowTop - i * RowStep), out volumeValues[i]);
                volumeSliders[i].gameObject.AddComponent<SliderRelease>(); // preview tone on release
            }
            var resetAudio = MenuButton(audio, "ResetButton", "RESET TO DEFAULTS", new Vector2(0f, ResetY), new Vector2(280f, 40f), 18);

            // CONTROLS
            var controls = pages[1].transform;
            var sensitivity = SliderRow(controls, "Sensitivity", "MOUSE SENSITIVITY", new Vector2(0f, RowTop), out var sensitivityValue);
            var invertY = ToggleRow(controls, "InvertY", "INVERT Y-AXIS", new Vector2(0f, RowTop - RowStep), out var invertYValue);
            var keys = Label(controls, "Keys", KeyOverview, 15, TextDim, new Vector2(0f, RowTop - 2.75f * RowStep), new Vector2(KeyBoxWidth, 120f));
            keys.alignment = TextAlignmentOptions.TopLeft;
            var resetControls = MenuButton(controls, "ResetButton", "RESET TO DEFAULTS", new Vector2(0f, ResetY), new Vector2(280f, 40f), 18);

            // DEV (P49): remove this tab for the jam submission (BOARD backlog)
            var dev = pages[2].transform;
            var devTools = ToggleRow(dev, "DevTools", "DEV TOOLS", new Vector2(0f, RowTop), out var devToolsValue);
            var devHelp = Label(dev, "Help", DevToolsHelp, 15, TextDim, new Vector2(0f, RowTop - 1.75f * RowStep), new Vector2(KeyBoxWidth, 80f));
            devHelp.alignment = TextAlignmentOptions.TopLeft;

            back = MenuButton(panel, "BackButton", "BACK", new Vector2(0f, -215f));

            var options = panel.gameObject.AddComponent<OptionsPanel>();
            SetArray(options, "tabButtons", tabs);
            SetArray(options, "pages", pages);
            SetArray(options, "volumeSliders", volumeSliders);
            SetArray(options, "volumeValues", volumeValues);
            Set(options, "resetAudio", resetAudio);
            Set(options, "sensitivity", sensitivity);
            Set(options, "sensitivityValue", sensitivityValue);
            Set(options, "invertY", invertY);
            Set(options, "invertYValue", invertYValue);
            Set(options, "resetControls", resetControls);
            Set(options, "devTools", devTools);
            Set(options, "devToolsValue", devToolsValue);
            for (int i = 1; i < pages.Length; i++) pages[i].SetActive(false);
            return panel;
        }

        private const float RowTop = 80f, RowStep = 50f, ResetY = -150f;

        // Options/credits over the station: readable, station still faintly visible. The project blends UI in linear
        // color space, so the leftover 1.5 % of a bright scene still shows as roughly 20 % brightness on screen.
        private const float SubPageDim = 0.985f;
        private const float KeyBoxWidth = 520f;

        private const string DevToolsHelp =
            "Red test buttons (end the day, open closed shops) and visible hitboxes.\n" +
            "Test items are added the next time a new game starts.";

        // Read-only key list (rebinding is out of scope, DECISIONS.md 05.11.). Key column left, action from <pos>.
        private const string KeyOverview =
            "<color=#E2DDCF>W A S D</color><pos=210>walk\n" +
            "<color=#E2DDCF>MOUSE</color><pos=210>look around\n" +
            "<color=#E2DDCF>E</color><pos=210>use / sit down\n" +
            "<color=#E2DDCF>I</color><pos=210>inventory\n" +
            "<color=#E2DDCF>LEFT CLICK</color><pos=210>press, stamp, drag papers\n" +
            "<color=#E2DDCF>Q / ESC / RIGHT CLICK</color><pos=210>step back / stand up\n" +
            "<color=#E2DDCF>ESC / P</color><pos=210>pause";

        /// <summary>
        /// Fullscreen corner button (P43, variant A): 40×40 in the top-right corner, four corners drawn from thin UI rects
        /// (outward = enter, inward = leave), "FULLSCREEN" below on hover. No asset needed, stays sharp at any size.
        /// </summary>
        private static void FullscreenCorner(Transform parent)
        {
            var button = MenuButton(parent, "FullscreenButton", "", Vector2.zero, new Vector2(40f, 40f), 12);
            Object.DestroyImmediate(button.GetComponentInChildren<TMP_Text>().gameObject);
            var rt = button.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one;
            rt.anchoredPosition = new Vector2(-16f, -16f);

            var parts = new List<Graphic>();
            const float outer = 11f, inner = 4f, arm = 7f;
            var expand = IconGroup(button.transform, "ExpandIcon");
            var collapse = IconGroup(button.transform, "CollapseIcon");
            foreach (var corner in new[] { new Vector2(-1f, 1f), new Vector2(1f, 1f), new Vector2(-1f, -1f), new Vector2(1f, -1f) })
            {
                // Expand: vertex at the outer corner, arms run along the edges toward the middle.
                CornerMark(expand, corner * outer, -corner.x, -corner.y, arm, parts);
                // Collapse: vertex near the center, arms run outward.
                CornerMark(collapse, corner * inner, corner.x, corner.y, arm, parts);
            }
            collapse.gameObject.SetActive(false);

            var hover = Label(button.transform, "HoverLabel", "FULLSCREEN", 13, Amber, new Vector2(-60f, -36f), new Vector2(160f, 20f));
            hover.alignment = TextAlignmentOptions.MidlineRight;
            hover.gameObject.SetActive(false);

            var fullscreen = button.gameObject.AddComponent<FullscreenButton>();
            Set(fullscreen, "button", button);
            Set(fullscreen, "expandIcon", expand.gameObject);
            Set(fullscreen, "collapseIcon", collapse.gameObject);
            SetArray(fullscreen, "iconParts", parts.ToArray());
            Set(fullscreen, "hoverLabel", hover);
        }

        private static Transform IconGroup(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            return go.transform;
        }

        /// <summary>An L: from the vertex one arm along x (dirX), one along y (dirY).</summary>
        private static void CornerMark(Transform parent, Vector2 vertex, float dirX, float dirY, float arm, List<Graphic> parts)
        {
            const float thickness = 2f;
            parts.Add(Bar(parent, vertex + new Vector2(dirX * (arm - thickness) / 2f, 0f), new Vector2(arm, thickness)));
            parts.Add(Bar(parent, vertex + new Vector2(0f, dirY * (arm - thickness) / 2f), new Vector2(thickness, arm)));
        }

        private static Graphic Bar(Transform parent, Vector2 pos, Vector2 size)
        {
            var go = new GameObject("Bar", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = TextLight;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>Caption on the left, an ON/OFF button on the right (simpler and clearer than a checkbox at 960×540).</summary>
        private static Button ToggleRow(Transform parent, string name, string caption, Vector2 pos, out TMP_Text value)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.anchoredPosition = pos;
            rowRt.sizeDelta = new Vector2(640f, 50f);
            var label = Label(row.transform, "Caption", caption, 20, TextLight, new Vector2(-200f, 0f), new Vector2(240f, 40f));
            label.alignment = TextAlignmentOptions.MidlineLeft;
            var button = MenuButton(row.transform, "Toggle", "OFF", new Vector2(90f, 0f), new Vector2(140f, 36f), 20);
            value = button.GetComponentInChildren<TMP_Text>();
            value.color = Amber;
            return button;
        }

        private static Transform BuildCreditsPanel(Transform parent, out Button back)
        {
            var panel = PanelRoot(parent, "CreditsPanel");
            Title(Label(panel, "Heading", "CREDITS", 40, Amber, new Vector2(0f, 200f), new Vector2(700f, 50f)));
            var text = Label(panel, "Body", CreditsText(), 18, TextLight, new Vector2(0f, 10f), new Vector2(760f, 320f));
            text.alignment = TextAlignmentOptions.Top;
            back = MenuButton(panel, "BackButton", "BACK", new Vector2(0f, -210f));
            return panel;
        }

        /// <summary>Team + every asset row of the CREDITS.md table (asset, author, license), both alphabetical.</summary>
        private static string CreditsText()
        {
            var lines = new List<string>
            {
                "<color=#FFD36B>A game by</color>",
                "Martin (iMaterix)  ·  Max (Juice-de-Orange)",
                "",
                "<color=#FFD36B>Lead Playtester</color>",
                "Pep (SuperGoat)",
                "",
                "<color=#FFD36B>Third-party assets</color>",
            };
            if (File.Exists(CreditsFile))
            {
                var assets = new List<string>();
                foreach (string row in File.ReadAllLines(CreditsFile))
                {
                    if (!row.StartsWith("|")) continue;
                    var cells = row.Trim('|').Split('|').Select(c => c.Trim()).ToArray();
                    if (cells.Length < 5 || cells[0] == "Asset" || cells[0].StartsWith("-")) continue;
                    string license = cells[4].Split('(')[0].Trim();
                    assets.Add($"{cells[0]} — {cells[2]} — {license}");
                }
                lines.AddRange(assets.OrderBy(a => a, System.StringComparer.OrdinalIgnoreCase));
            }
            lines.Add("");
            lines.Add("<color=#8B9096>Made with Unity for GitHub Game Off 2026</color>");
            return string.Join("\n", lines);
        }

        /// <summary>
        /// The station seen from space (variant A): instances of Max's generated prefabs (sky, exterior, station incl. hull),
        /// unchanged, plus the space look of Main (skybox, ambient, thin fog) and a slowly drifting camera.
        /// Main's baked light stays in Main, so here only the realtime sun and a realtime copy of the planet fill light shine.
        /// </summary>
        private static void BuildStationBackdrop(Scene scene, List<string> log)
        {
            int placed = 0;
            foreach (string path in BackdropPrefabs)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) { log.Add($"backdrop prefab missing: {path}"); continue; }
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                // Inside the station dozens of realtime lamps would cast shadows (Main bakes them); from space nobody sees
                // that, and in WebGL it costs a lot. Instance override in Menu only, the prefab stays untouched.
                foreach (var lamp in instance.GetComponentsInChildren<Light>(true))
                {
                    if (lamp.type == LightType.Directional || lamp.shadows == LightShadows.None) continue;
                    lamp.shadows = LightShadows.None;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(lamp);
                }
                placed++;
            }

            var fill = new GameObject("PlanetFillLight").AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.55f, 0.68f, 0.95f);
            fill.intensity = 0.5f;
            fill.shadows = LightShadows.None;
            fill.lightmapBakeType = LightmapBakeType.Realtime;
            fill.transform.rotation = Quaternion.Euler(19f, 171.5f, 0f); // like Main's PlanetLight

            var skybox = AssetDatabase.LoadAssetAtPath<Material>(SkyboxPath);
            if (skybox != null) RenderSettings.skybox = skybox;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.002f; // Main uses 0.012 indoors; from ~120 m away that would swallow the station
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.09f);

            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(MenuCameraDrift)).GetComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.farClipPlane = 5000f;
            cam.fieldOfView = 50f;
            var drift = cam.GetComponent<MenuCameraDrift>();
            SetVector(drift, "target", BackdropTarget);
            SetVector(drift, "lookOffset", BackdropLookOffset);
            log.Add($"station backdrop ({placed}/{BackdropPrefabs.Length} prefabs)");

            var traffic = new GameObject("ShipTraffic").AddComponent<ShipTraffic>();
            Set(traffic, "tuning", TrafficTuning(log));
        }

        /// <summary>
        /// P47 lanes around the station (target ~(2, 9, 5); the menu camera swings around x -40, z -108 looking north-east).
        /// Far: freighters/tankers in front of the planet · middle: customs patrol · near: small commuters. Created only if missing.
        /// </summary>
        private static ShipTrafficTuning TrafficTuning(List<string> log)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<ShipTrafficTuning>(TrafficTuningPath);
            if (tuning != null) return tuning;
            GameObject Ship(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(ShipFolder + name + ".fbx");
            tuning = ScriptableObject.CreateInstance<ShipTrafficTuning>();
            tuning.maxActive = 4;
            tuning.modelYaw = 180f; // the ship FBX noses point to -Z
            tuning.lanes.Add(new TrafficLane
            {
                name = "Far · freighters", from = new Vector3(-450f, 60f, 260f), to = new Vector3(450f, 40f, 260f),
                randomOffset = new Vector3(0f, 25f, 60f), ships = { Ship("SM_Ship_Freighter"), Ship("SM_Ship_Tanker") },
                scale = 0.6f, speed = new Vector2(12f, 20f), interval = new Vector2(20f, 40f), firstDelay = new Vector2(0f, 3f),
            });
            tuning.lanes.Add(new TrafficLane
            {
                name = "Middle · patrol", from = new Vector3(-220f, 55f, 45f), to = new Vector3(260f, 45f, 45f),
                randomOffset = new Vector3(0f, 8f, 15f), ships = { Ship("SM_Ship_Patrol") },
                scale = 1f, speed = new Vector2(35f, 50f), interval = new Vector2(30f, 60f), firstDelay = new Vector2(8f, 15f),
            });
            tuning.lanes.Add(new TrafficLane
            {
                name = "Near · commuters", from = new Vector3(-110f, 30f, -82f), to = new Vector3(70f, 20f, -70f),
                randomOffset = new Vector3(0f, 5f, 8f), ships = { Ship("SM_Ship_TrafficSmall") },
                scale = 1.5f, speed = new Vector2(18f, 26f), interval = new Vector2(10f, 20f), firstDelay = new Vector2(3f, 8f),
            });
            Directory.CreateDirectory(Path.GetDirectoryName(TrafficTuningPath));
            AssetDatabase.CreateAsset(tuning, TrafficTuningPath);
            AssetDatabase.SaveAssets();
            log.Add("ShipTraffic_Menu created");
            return tuning;
        }

        // ---------- Pause menu ----------

        private static GameObject BuildPausePrefab(List<string> log)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PauseMenuPrefab));
            var rootGo = new GameObject("PF_PauseMenu");
            var canvasGo = CreateCanvas("PauseCanvas", 100).gameObject;
            canvasGo.transform.SetParent(rootGo.transform, false);
            var dim = new GameObject("Dim", typeof(RectTransform), typeof(Image));
            dim.transform.SetParent(canvasGo.transform, false);
            Stretch(dim.GetComponent<RectTransform>());
            dim.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.85f);

            var mainPanel = PanelRoot(dim.transform, "MainPanel", false);
            Title(Label(mainPanel, "Heading", "PAUSED", 48, Amber, new Vector2(0f, 130f), new Vector2(600f, 60f)));
            var resume = MenuButton(mainPanel, "ResumeButton", "RESUME", new Vector2(0f, 30f));
            var options = MenuButton(mainPanel, "OptionsButton", "OPTIONS", new Vector2(0f, -35f));
            var menu = MenuButton(mainPanel, "MenuButton", "MAIN MENU", new Vector2(0f, -100f));
            Label(mainPanel, "Hint", "[Esc] / [P] resume", 16, TextDim, new Vector2(0f, -170f), new Vector2(600f, 24f));

            var optionsPanel = BuildOptionsPanel(dim.transform, out var optionsBack);
            optionsPanel.GetComponent<Image>().color = Color.clear; // dim backdrop is enough here
            optionsPanel.gameObject.SetActive(false);
            FullscreenCorner(dim.transform); // only visible while paused (dim is the pause root)

            CreateEventSystem(rootGo.transform);
            var pause = rootGo.AddComponent<PauseMenu>();
            Set(pause, "root", dim);
            Set(pause, "mainPanel", mainPanel.gameObject);
            Set(pause, "optionsPanel", optionsPanel.gameObject);
            Set(pause, "resumeButton", resume);
            Set(pause, "optionsButton", options);
            Set(pause, "optionsBackButton", optionsBack);
            Set(pause, "menuButton", menu);

            var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PauseMenuPrefab);
            Object.DestroyImmediate(rootGo);
            log.Add("PF_PauseMenu built");
            return prefab;
        }

        // ---------- Audio (P18) ----------

        /// <summary>PF_AudioManager: AudioManager + two music sources (crossfade), a 2D SFX source and a voice source, wired to GameMixer.</summary>
        private static GameObject BuildAudioManagerPrefab(List<string> log)
        {
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (mixer == null) { log.Add($"MIXER MISSING: {MixerPath}"); return null; }
            AudioMixerGroup Group(string name) => mixer.FindMatchingGroups(name).FirstOrDefault(g => g.name == name);
            var music = Group("Music");
            var sfx = Group("SFX");
            var voice = Group("Voice");

            Directory.CreateDirectory(Path.GetDirectoryName(AudioManagerPrefab));
            var root = new GameObject("PF_AudioManager");
            AudioSource Source(string name, AudioMixerGroup group, bool loop, bool ignorePause)
            {
                var go = new GameObject(name, typeof(AudioSource));
                go.transform.SetParent(root.transform, false);
                var source = go.GetComponent<AudioSource>();
                source.outputAudioMixerGroup = group;
                source.playOnAwake = false;
                source.loop = loop;
                source.spatialBlend = 0f;
                source.ignoreListenerPause = ignorePause;
                return source;
            }
            var musicA = Source("MusicA", music, true, true);
            var musicB = Source("MusicB", music, true, true);
            var sfx2D = Source("Sfx2D", sfx, false, true); // UI clicks must work in the pause menu
            var voiceSource = Source("Voice", voice, false, false);

            var manager = root.AddComponent<AudioManager>();
            Set(manager, "mixer", mixer);
            Set(manager, "musicGroup", music);
            Set(manager, "sfxGroup", sfx);
            Set(manager, "voiceGroup", voice);
            Set(manager, "musicA", musicA);
            Set(manager, "musicB", musicB);
            Set(manager, "sfx2D", sfx2D);
            Set(manager, "voice", voiceSource);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, AudioManagerPrefab);
            Object.DestroyImmediate(root);
            log.Add("PF_AudioManager built");
            return prefab;
        }

        private static void PlaceAudioManager(Scene scene)
        {
            if (_audioPrefab != null) PrefabUtility.InstantiatePrefab(_audioPrefab, scene);
        }

        private static void PlaceInMain(GameObject prefab, List<string> log)
        {
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var pause = instance.GetComponent<PauseMenu>();
            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var counter = Object.FindFirstObjectByType<CounterStation>();
            Set(pause, "player", player);
            Set(pause, "counter", counter);
            PrefabUtility.RecordPrefabInstancePropertyModifications(pause);
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == _audioPrefab.name)) Object.DestroyImmediate(old);
            PlaceAudioManager(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Add($"pause menu + audio manager placed in Main (player {(player != null ? "ok" : "MISSING")}, counter {(counter != null ? "ok" : "missing")})");
        }

        private static void CheckBuildSettings(List<string> log)
        {
            var wanted = new[] { BootScene, MenuScene, MainScene };
            var current = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            if (current.Take(3).SequenceEqual(wanted)) return;
            var rest = EditorBuildSettings.scenes.Where(s => !wanted.Contains(s.path));
            EditorBuildSettings.scenes = wanted.Select(p => new EditorBuildSettingsScene(p, true)).Concat(rest).ToArray();
            log.Add("build settings reordered (Boot, Menu, Main)");
        }

        // ---------- UI helpers ----------

        private static Scene OpenCleared(string path)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            foreach (var go in scene.GetRootGameObjects()) Object.DestroyImmediate(go);
            return scene;
        }

        private static void CreateCamera()
        {
            var cam = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Backdrop;
        }

        private static void CreateEventSystem(Transform parent)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            if (parent != null) es.transform.SetParent(parent, false);
        }

        internal static Transform CreateCanvas(string name, int sortingOrder)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960f, 540f);
            scaler.matchWidthOrHeight = 0.5f;
            return go.transform;
        }

        private static Transform PanelRoot(Transform parent, string name, bool background = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Stretch(go.GetComponent<RectTransform>());
            var image = go.GetComponent<Image>();
            image.color = background ? Backdrop : Color.clear;
            image.raycastTarget = background;
            return go.transform;
        }

        /// <summary>Switches a label to the Orbitron title font; it may shrink to fit its box (Orbitron is wide).</summary>
        private static TMP_Text Title(TMP_Text label)
        {
            var font = TitleFont();
            if (font == null) return label;
            label.font = font;
            label.enableAutoSizing = true;
            label.fontSizeMax = label.fontSize;
            label.fontSizeMin = label.fontSize * 0.5f;
            return label;
        }

        /// <summary>
        /// Orbitron TMP font asset (dynamic, like Share Tech Mono), created once next to the TTF. Fallbacks for characters Orbitron
        /// lacks: Share Tech Mono, then Liberation Sans.
        /// </summary>
        internal static TMP_FontAsset TitleFont()
        {
            if (_titleFont != null) return EnsureFallbacks(_titleFont);
            _titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TitleFontAssetPath);
            if (_titleFont != null) return EnsureFallbacks(_titleFont);
            var font = AssetDatabase.LoadAssetAtPath<Font>(TitleFontPath);
            if (font == null) { Debug.LogWarning($"[MenuSceneBuilder] {TitleFontPath} missing, titles stay Share Tech Mono"); return null; }
            _titleFont = TMP_FontAsset.CreateFontAsset(font, 90, 9, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic);
            _titleFont.name = "Orbitron SDF";
            AssetDatabase.CreateAsset(_titleFont, TitleFontAssetPath);
            _titleFont.atlasTextures[0].name = "Orbitron SDF Atlas";
            AssetDatabase.AddObjectToAsset(_titleFont.atlasTextures[0], _titleFont);
            _titleFont.material.name = "Orbitron SDF Material";
            AssetDatabase.AddObjectToAsset(_titleFont.material, _titleFont);
            AssetDatabase.SaveAssets();
            return EnsureFallbacks(_titleFont);
        }

        private const string LiberationFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";

        private static TMP_FontAsset EnsureFallbacks(TMP_FontAsset font)
        {
            font.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
            bool changed = false;
            foreach (var path in new[] { FontAssetPath, LiberationFontAssetPath })
            {
                var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (fallback == null || font.fallbackFontAssetTable.Contains(fallback)) continue;
                font.fallbackFontAssetTable.Add(fallback);
                changed = true;
            }
            if (changed)
            {
                EditorUtility.SetDirty(font);
                AssetDatabase.SaveAssetIfDirty(font);
            }
            return font;
        }

        internal static TMP_Text Label(Transform parent, string name, string text, float size, Color color, Vector2 pos, Vector2 box)
        {
            if (_font == null) _font = BaseFont();
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = box;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (_font != null) tmp.font = _font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            // Orbitron is wide: text may shrink a bit to stay inside its box instead of running out of it.
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = size;
            tmp.fontSizeMin = Mathf.Max(8f, size * 0.6f);
            return tmp;
        }

        /// <summary>
        /// Font for every text the builders create: Orbitron (martin 05.11., all text, revises DECISIONS 31.10.); Share Tech Mono
        /// only if the Orbitron asset can't be made.
        /// </summary>
        private static TMP_FontAsset BaseFont() => TitleFont() ?? AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);

        internal static Button MenuButton(Transform parent, string name, string text, Vector2 pos) =>
            MenuButton(parent, name, text, pos, new Vector2(280f, 50f), 24);

        internal static Button MenuButton(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.color = Color.white;
            var button = go.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = ButtonIdle;
            colors.highlightedColor = ButtonHover;
            colors.selectedColor = ButtonIdle;
            colors.pressedColor = Panel;
            colors.fadeDuration = 0.05f;
            button.colors = colors;
            var label = Label(go.transform, "Label", text, fontSize, TextLight, Vector2.zero, Vector2.zero);
            Stretch(label.rectTransform);
            return button;
        }

        private static Slider SliderRow(Transform parent, string name, string caption, Vector2 pos, out TMP_Text value)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rowRt = row.GetComponent<RectTransform>();
            rowRt.anchoredPosition = pos;
            rowRt.sizeDelta = new Vector2(640f, 50f);
            var label = Label(row.transform, "Caption", caption, 20, TextLight, new Vector2(-200f, 0f), new Vector2(240f, 40f));
            label.alignment = TextAlignmentOptions.MidlineLeft;
            value = Label(row.transform, "Value", "", 20, Amber, new Vector2(295f, 0f), new Vector2(100f, 40f));
            value.alignment = TextAlignmentOptions.MidlineRight;

            var resources = new DefaultControls.Resources
            {
                standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
                background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
                knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            };
            var sliderGo = DefaultControls.CreateSlider(resources);
            sliderGo.name = "Slider";
            sliderGo.transform.SetParent(row.transform, false);
            var srt = sliderGo.GetComponent<RectTransform>();
            srt.anchoredPosition = new Vector2(90f, 0f);
            srt.sizeDelta = new Vector2(300f, 20f);
            foreach (var image in sliderGo.GetComponentsInChildren<Image>())
            {
                if (image.name == "Background") image.color = ButtonIdle;
                else if (image.name == "Fill") image.color = Amber;
                else if (image.name == "Handle") image.color = TextLight;
            }
            return sliderGo.GetComponent<Slider>();
        }

        internal static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        internal static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogError($"[MenuSceneBuilder] {target.GetType().Name}.{field} not found"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetVector(Object target, string field, Vector3 value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).vector3Value = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetArray(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Color Hex(string hex) => ColorUtility.TryParseHtmlString("#" + hex, out var c) ? c : Color.magenta;
    }
}
