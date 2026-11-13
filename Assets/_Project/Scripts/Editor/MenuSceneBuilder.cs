using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Audio;
using GameOff.Core;
using GameOff.Counter;
using GameOff.Day;
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
    /// Builds the front end (P02; options in tabs: P43; audio: P18), idempotent: Boot.unity (splash), the main menu with options and
    /// credits in Main.unity (FrontEnd, P48: there is no Menu scene), PF_PauseMenu.prefab and PF_AudioManager.prefab, and places the
    /// pause menu (wired to the player, the counter and the day), the audio manager and the game cursor in Main.unity.
    /// Credits text is taken from CREDITS.md at build time, so rerun after adding credits.
    /// Placeholder look (Share Tech Mono, palette from STYLE.md); final UI art replaces the sprites later.
    /// </summary>
    public static class MenuSceneBuilder
    {
        private const string BootScene = "Assets/_Project/Scenes/Boot.unity";
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PauseMenuPrefab = "Assets/_Project/Prefabs/UI/PF_PauseMenu.prefab";
        private const string AudioManagerPrefab = "Assets/_Project/Prefabs/Audio/PF_AudioManager.prefab";
        private const string CursorDefaultPath = "Assets/_Project/Art/UI/UI_Cursor_Default.png"; // P138
        private const string CursorHoverPath = "Assets/_Project/Art/UI/UI_Cursor_Hover.png";
        private const string CursorObjectName = "GameCursor";
        private const string MixerPath = "Assets/_Project/Audio/GameMixer.mixer"; // created once by hand/script, groups Master > Music, SFX, Voice
        private const string FontAssetPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";
        // All text uses Orbitron (martin 05.11., revises DECISIONS 31.10.); Share Tech Mono stays as fallback for missing glyphs.
        private const string TitleFontPath = "Assets/_Project/Fonts/Orbitron-Regular.ttf";
        private const string TitleFontAssetPath = "Assets/_Project/Fonts/Orbitron SDF.asset";
        private const string CreditsFile = "CREDITS.md";
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

        /// <summary>
        /// Only the main menu in Main (the FrontEnd object: menu, options, credits), e.g. after changing its layout or the credits.
        /// Boot, the pause menu, the audio manager and the cursor stay untouched.
        /// </summary>
        [MenuItem("GameOff/Build Front End into Main")]
        public static void BuildFrontEndFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Debug.Log(BuildFrontEnd());
        }

        public static string BuildFrontEnd()
        {
            _font = BaseFont();
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
            PlaceCursor(scene, log);
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
            // P48: the menu lives in Main, over the real station, on its own canvas above everything else.
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == "FrontEnd")) Object.DestroyImmediate(old);
            if (Object.FindFirstObjectByType<EventSystem>() == null) CreateEventSystem(null);
            var root = new GameObject("FrontEnd");
            var canvas = CreateCanvas("MenuCanvas", 200);
            canvas.SetParent(root.transform, false);

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
            // P128: CONTINUE on top (MainMenu hides it without a saved run), 60 px apart.
            var cont = MenuButton(mainPanel, "ContinueButton", "CONTINUE", new Vector2(x, 45f));
            var start = MenuButton(mainPanel, "StartButton", "START SHIFT", new Vector2(x, -15f));
            var options = MenuButton(mainPanel, "OptionsButton", "OPTIONS", new Vector2(x, -75f));
            var credits = MenuButton(mainPanel, "CreditsButton", "CREDITS", new Vector2(x, -135f));
            var quit = MenuButton(mainPanel, "QuitButton", "QUIT", new Vector2(x, -195f));
            Label(mainPanel, "Version", "Game Off 2026 · jam build", 14, TextDim, new Vector2(x, -245f), new Vector2(MenuStripWidth, 24f));

            var optionsPanel = BuildOptionsPanel(canvas, out var optionsBack);
            var creditsPanel = BuildCreditsPanel(canvas, out var creditsBack);
            var confirmPanel = BuildConfirmPanel(canvas, out var confirmText, out var confirmYes, out var confirmBack);
            // Sub pages dim the station instead of hiding it.
            optionsPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, SubPageDim);
            creditsPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, SubPageDim);
            confirmPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, SubPageDim);
            FullscreenCorner(canvas); // last child: on top of every menu page

            var menu = root.AddComponent<MainMenu>();
            Set(menu, "mainPanel", mainPanel.gameObject);
            Set(menu, "optionsPanel", optionsPanel.gameObject);
            Set(menu, "creditsPanel", creditsPanel.gameObject);
            Set(menu, "startButton", start);
            Set(menu, "optionsButton", options);
            Set(menu, "creditsButton", credits);
            Set(menu, "quitButton", quit);
            SetArray(menu, "backButtons", new Object[] { optionsBack, creditsBack });
            Set(menu, "continueButton", cont);
            Set(menu, "confirmPanel", confirmPanel.gameObject);
            Set(menu, "confirmText", confirmText);
            Set(menu, "confirmButton", confirmYes);
            Set(menu, "confirmBackButton", confirmBack);
            optionsPanel.gameObject.SetActive(false);
            creditsPanel.gameObject.SetActive(false);
            confirmPanel.gameObject.SetActive(false);

            var frontEnd = root.AddComponent<FrontEnd>();
            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var dayUi = Object.FindFirstObjectByType<DayFlowUI>(FindObjectsInactive.Include);
            Set(frontEnd, "player", player);
            Set(frontEnd, "playerCamera", player != null ? player.GetComponentInChildren<Camera>(true) : null);
            Set(frontEnd, "dayUi", dayUi);
            Set(frontEnd, "menuCanvas", canvas.gameObject);
            var interiors = new SerializedObject(frontEnd);
            var rooms = StationGrayboxBuilder.InteriorBounds();
            var hidden = interiors.FindProperty("hiddenRooms");
            hidden.arraySize = rooms.Length;
            for (int i = 0; i < rooms.Length; i++) hidden.GetArrayElementAtIndex(i).boundsValue = rooms[i];
            interiors.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Add($"menu built in Main (player {(player != null ? "ok" : "MISSING")}, day UI {(dayUi != null ? "ok" : "MISSING")})");
        }

        /// <summary>
        /// Options in tabs (P43, DEV from P49): AUDIO / CONTROLS / DEV, one page each, rows 50 px apart from RowTop down,
        /// "Reset to defaults" at the bottom of the AUDIO and CONTROLS pages. New settings = one more row on the matching page here.
        /// </summary>
        private static Transform BuildOptionsPanel(Transform parent, out Button back)
        {
            var panel = PanelRoot(parent, "OptionsPanel");
            TerminalStyle.AddFrame(panel, new Vector2(900f, 640f));
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

            // DEV (P49): stays built; the jam build hides it at runtime (OptionsPanel.HideDevTab, P146)
            var dev = pages[2].transform;
            var devTools = ToggleRow(dev, "DevTools", "DEV TOOLS", new Vector2(0f, RowTop), out var devToolsValue);
            var trafficLines = ToggleRow(dev, "TrafficLineDev", "TRAFFIC_LINE_DEV", new Vector2(0f, RowTop - RowStep), out var trafficLinesValue);
            // Money for testing P10/P11: caption, −100, balance, +100 (only works in Main, where PlayerEconomy lives).
            var creditsRow = new GameObject("DevCredits", typeof(RectTransform));
            creditsRow.transform.SetParent(dev, false);
            creditsRow.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, RowTop - 2f * RowStep);
            creditsRow.GetComponent<RectTransform>().sizeDelta = new Vector2(640f, 50f);
            var creditsCaption = Label(creditsRow.transform, "Caption", "CREDITS", 20, TextLight, new Vector2(-200f, 0f), new Vector2(240f, 40f));
            creditsCaption.alignment = TextAlignmentOptions.MidlineLeft;
            var creditsMinus = MenuButton(creditsRow.transform, "Minus100", "-100", new Vector2(-10f, 0f), new Vector2(90f, 36f), 18);
            var creditsValue = Label(creditsRow.transform, "Balance", "-", 18, Amber, new Vector2(110f, 0f), new Vector2(150f, 36f));
            var creditsPlus = MenuButton(creditsRow.transform, "Plus100", "+100", new Vector2(230f, 0f), new Vector2(90f, 36f), 18);

            var devHelp = Label(dev, "Help", DevToolsHelp, 15, TextDim, new Vector2(0f, RowTop - 3.9f * RowStep), new Vector2(KeyBoxWidth, 100f));
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
            Set(options, "trafficLines", trafficLines);
            Set(options, "trafficLinesValue", trafficLinesValue);
            Set(options, "creditsMinus", creditsMinus);
            Set(options, "creditsPlus", creditsPlus);
            Set(options, "creditsValue", creditsValue);
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
            "Test items are added the next time a new game starts.\n" +
            "TRAFFIC_LINE_DEV (own switch): ship lanes + edit panel, mouse free (Tab: look around).\n" +
            "CREDITS: add or take 100 CR (in game only, booked as \"Dev\").";

        // Read-only key list (rebinding is out of scope, DECISIONS.md 05.11.). Key column left, action from <pos>.
        private const string KeyOverview =
            "<color=#E2DDCF>W A S D</color><pos=210>walk\n" +
            "<color=#E2DDCF>MOUSE</color><pos=210>look around\n" +
            "<color=#E2DDCF>E</color><pos=210>use / sit down\n" +
            "<color=#E2DDCF>I / H</color><pos=210>inventory / help\n" +
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

        /// <summary>P128: START over a saved run asks first (MainMenu fills in the day).</summary>
        private static Transform BuildConfirmPanel(Transform parent, out TMP_Text text, out Button yes, out Button back)
        {
            var panel = PanelRoot(parent, "ConfirmNewRunPanel");
            TerminalStyle.AddFrame(panel, new Vector2(640f, 320f));
            Title(Label(panel, "Heading", "NEW RUN?", 40, Amber, new Vector2(0f, 90f), new Vector2(560f, 50f)));
            text = Label(panel, "Body", "Your saved run will be lost.\nStart a new run from day 1?", 20, TextLight, new Vector2(0f, 15f), new Vector2(560f, 80f));
            yes = MenuButton(panel, "NewRunButton", "NEW RUN", new Vector2(-120f, -90f), new Vector2(220f, 50f), 24);
            back = MenuButton(panel, "BackButton", "BACK", new Vector2(120f, -90f), new Vector2(220f, 50f), 24);
            return panel;
        }

        private static Transform BuildCreditsPanel(Transform parent, out Button back)
        {
            var panel = PanelRoot(parent, "CreditsPanel");
            TerminalStyle.AddFrame(panel, new Vector2(900f, 640f));
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
            var resume = MenuButton(mainPanel, "ResumeButton", "RESUME", new Vector2(0f, 50f));
            var options = MenuButton(mainPanel, "OptionsButton", "OPTIONS", new Vector2(0f, -15f));
            var bugReport = MenuButton(mainPanel, "BugReportButton", "REPORT BUG", new Vector2(0f, -80f)); // P78
            var menu = MenuButton(mainPanel, "MenuButton", "MAIN MENU", new Vector2(0f, -145f));
            Label(mainPanel, "Hint", "[Esc] / [P] resume", 16, TextDim, new Vector2(0f, -210f), new Vector2(600f, 24f));

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
            Set(pause, "bugReportButton", bugReport);
            Set(pause, "bugReportForm", BuildBugReportForm(dim.transform));

            var prefab = PrefabUtility.SaveAsPrefabAsset(rootGo, PauseMenuPrefab);
            Object.DestroyImmediate(rootGo);
            log.Add("PF_PauseMenu built");
            return prefab;
        }

        // ---------- Bug report form (P84) ----------

        private const string BugReportSettingsPath = "Assets/_Project/Data/Menu/BugReportSettings.asset";

        private static BugReportForm BuildBugReportForm(Transform parent)
        {
            var settings = AssetDatabase.LoadAssetAtPath<BugReportSettings>(BugReportSettingsPath);
            if (settings == null) // content: only created if missing
            {
                settings = ScriptableObject.CreateInstance<BugReportSettings>();
                AssetDatabase.CreateAsset(settings, BugReportSettingsPath);
            }
            var panel = PanelRoot(parent, "BugReportPanel", false);
            Title(Label(panel, "Heading", "REPORT A BUG", 36, Amber, new Vector2(0f, 190f), new Vector2(600f, 48f)));
            var input = InputField(panel, new Vector2(0f, 60f), new Vector2(600f, 180f));
            var status = Label(panel, "Status", "", 15, TextDim, new Vector2(0f, -70f), new Vector2(600f, 48f));
            status.enableAutoSizing = false;
            var send = MenuButton(panel, "SendButton", "SEND", new Vector2(-100f, -140f), new Vector2(180f, 46f), 22);
            var back = MenuButton(panel, "BackButton", "BACK", new Vector2(100f, -140f), new Vector2(180f, 46f), 22);
            Label(panel, "Hint", "[Esc] back", 14, TextDim, new Vector2(0f, -190f), new Vector2(600f, 22f));

            var form = panel.gameObject.AddComponent<BugReportForm>();
            Set(form, "settings", settings);
            Set(form, "input", input);
            Set(form, "sendButton", send);
            Set(form, "backButton", back);
            Set(form, "status", status);
            panel.gameObject.SetActive(false);
            return form;
        }

        /// <summary>Multi-line TMP input field in the station look (ShareTechMono, readable while typing).</summary>
        private static TMP_InputField InputField(Transform parent, Vector2 pos, Vector2 size)
        {
            var mono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            var go = new GameObject("Input", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            go.GetComponent<Image>().color = Panel;

            var area = new GameObject("TextArea", typeof(RectTransform), typeof(RectMask2D));
            area.transform.SetParent(go.transform, false);
            var areaRt = area.GetComponent<RectTransform>();
            Stretch(areaRt);
            areaRt.offsetMin = new Vector2(12f, 10f);
            areaRt.offsetMax = new Vector2(-12f, -10f);

            TMP_Text Text(string name, string value, Color color)
            {
                var t = Label(area.transform, name, value, 18, color, Vector2.zero, Vector2.zero);
                Stretch(t.rectTransform);
                if (mono != null) t.font = mono;
                t.enableAutoSizing = false;
                t.alignment = TextAlignmentOptions.TopLeft;
                t.textWrappingMode = TextWrappingModes.Normal;
                return t;
            }
            var text = Text("Text", "", TextLight);
            var placeholder = Text("Placeholder", "What happened? What did you expect?", TextDim);
            placeholder.fontStyle = FontStyles.Italic;

            var field = go.GetComponent<TMP_InputField>();
            field.textViewport = areaRt;
            field.textComponent = text;
            field.placeholder = placeholder;
            if (mono != null) field.fontAsset = mono;
            field.pointSize = 18;
            field.lineType = TMP_InputField.LineType.MultiLineNewline;
            field.characterLimit = BugReport.MaxLength;
            field.caretColor = Amber;
            field.customCaretColor = true;
            field.selectionColor = new Color(Amber.r, Amber.g, Amber.b, 0.35f);
            return field;
        }

        // ---------- Audio (P18) ----------

        /// <summary>PF_AudioManager: AudioManager + two 2D SFX sources (UI, game) and a voice source, wired to GameMixer.</summary>
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
            var sfx2D = Source("Sfx2D", sfx, false, true); // UI clicks must work in the pause menu
            var sfxGame = Source("SfxGame", sfx, false, false); // stops with the pause menu
            var voiceSource = Source("Voice", voice, false, false);

            var manager = root.AddComponent<AudioManager>();
            Set(manager, "mixer", mixer);
            Set(manager, "musicGroup", music);
            Set(manager, "sfxGroup", sfx);
            Set(manager, "voiceGroup", voice);
            Set(manager, "sfx2D", sfx2D);
            Set(manager, "sfxGame", sfxGame);
            Set(manager, "voice", voiceSource);
            Set(manager, "sounds", SoundSetBuilder.Build());

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, AudioManagerPrefab);
            Object.DestroyImmediate(root);
            log.Add("PF_AudioManager built");
            return prefab;
        }

        private static void PlaceAudioManager(Scene scene)
        {
            if (_audioPrefab != null) PrefabUtility.InstantiatePrefab(_audioPrefab, scene);
        }

        /// <summary>
        /// P138: the game's mouse pointer (GameCursor), like the audio manager in Boot and in Main (so Main can be played directly);
        /// the copy that comes second removes itself. Imports the pointer textures as cursors first (DeliveryIntegrator).
        /// </summary>
        private static void PlaceCursor(Scene scene, List<string> log)
        {
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == CursorObjectName)) Object.DestroyImmediate(old);
            DeliveryIntegrator.ConfigureCursorTextures(log);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(CursorDefaultPath);
            var hover = AssetDatabase.LoadAssetAtPath<Texture2D>(CursorHoverPath);
            var go = new GameObject(CursorObjectName);
            SceneManager.MoveGameObjectToScene(go, scene);
            var cursor = go.AddComponent<GameCursor>();
            Set(cursor, "normalTexture", normal);
            Set(cursor, "hoverTexture", hover);
            log.Add($"GameCursor in {scene.name} (pointer {(normal != null ? "ok" : "MISSING")}, hover {(hover != null ? "ok" : "MISSING")})");
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
            var dayManager = Object.FindFirstObjectByType<DayManager>();
            Set(pause, "dayManager", dayManager);
            PrefabUtility.RecordPrefabInstancePropertyModifications(pause);
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == _audioPrefab.name)) Object.DestroyImmediate(old);
            PlaceAudioManager(scene);
            PlaceCursor(scene, log);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Add($"pause menu + audio manager placed in Main (player {(player != null ? "ok" : "MISSING")}, counter {(counter != null ? "ok" : "missing")}, day manager {(dayManager != null ? "ok" : "missing")})");
        }

        private static void CheckBuildSettings(List<string> log)
        {
            var wanted = new[] { BootScene, MainScene };
            var current = EditorBuildSettings.scenes.Select(s => s.path).ToArray();
            if (current.SequenceEqual(wanted)) return;
            var rest = EditorBuildSettings.scenes.Where(s => !wanted.Contains(s.path));
            EditorBuildSettings.scenes = wanted.Select(p => new EditorBuildSettingsScene(p, true)).Concat(rest).ToArray();
            log.Add("build settings reordered (Boot, Main)");
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
            TerminalStyle.Button(button);
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
