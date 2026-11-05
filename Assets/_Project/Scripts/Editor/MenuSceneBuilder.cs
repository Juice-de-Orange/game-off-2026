using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Counter;
using GameOff.Player;
using GameOff.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds the front end (P02), idempotent: Boot.unity (splash), Menu.unity (main menu, options, credits),
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
        private const string FontAssetPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";
        private const string CreditsFile = "CREDITS.md";
        private const string SkyboxPath = "Assets/_Project/Art/Materials/Station/M_Sky_Space.mat";
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
        private static readonly Color Backdrop = Hex("0D0F14");
        private static readonly Color Panel = Hex("1B1F27");
        private static readonly Color ButtonIdle = Hex("2C323C");
        private static readonly Color ButtonHover = Hex("444B55");
        private static readonly Color Amber = Hex("FFD36B");
        private static readonly Color TextLight = Hex("E2DDCF");
        private static readonly Color TextDim = Hex("8B9096");

        private static TMP_FontAsset _font;

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
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            var log = new List<string>();
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
            CreateCamera();
            var canvas = CreateCanvas("SplashCanvas", 0);
            var group = new GameObject("Splash", typeof(RectTransform), typeof(CanvasGroup));
            group.transform.SetParent(canvas, false);
            Stretch(group.GetComponent<RectTransform>());
            Label(group.transform, "Presents", "iMATERIX  &  JUICE-DE-ORANGE", 26, TextDim, new Vector2(0f, 40f), new Vector2(800f, 40f));
            Label(group.transform, "Title", GameTitle, 64, Amber, new Vector2(0f, -20f), new Vector2(800f, 80f));
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
            Label(mainPanel, "Title", GameTitle, 64, Amber, new Vector2(x, 170f), new Vector2(MenuStripWidth, 80f));
            Label(mainPanel, "Subtitle", GameSubtitle, 16, TextDim, new Vector2(x, 122f), new Vector2(MenuStripWidth, 26f));
            var start = MenuButton(mainPanel, "StartButton", "START SHIFT", new Vector2(x, 30f));
            var options = MenuButton(mainPanel, "OptionsButton", "OPTIONS", new Vector2(x, -35f));
            var credits = MenuButton(mainPanel, "CreditsButton", "CREDITS", new Vector2(x, -100f));
            var quit = MenuButton(mainPanel, "QuitButton", "QUIT", new Vector2(x, -165f));
            Label(mainPanel, "Version", "Game Off 2026 · jam build", 14, TextDim, new Vector2(x, -245f), new Vector2(MenuStripWidth, 24f));

            var optionsPanel = BuildOptionsPanel(canvas, out var optionsBack);
            var creditsPanel = BuildCreditsPanel(canvas, out var creditsBack);
            // Sub pages dim the station instead of hiding it.
            optionsPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.85f);
            creditsPanel.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.85f);

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

        private static Transform BuildOptionsPanel(Transform parent, out Button back)
        {
            var panel = PanelRoot(parent, "OptionsPanel");
            Label(panel, "Heading", "OPTIONS", 40, Amber, new Vector2(0f, 150f), new Vector2(600f, 50f));
            var volume = SliderRow(panel, "Volume", "VOLUME", new Vector2(0f, 60f), out var volumeValue);
            var sensitivity = SliderRow(panel, "Sensitivity", "MOUSE SENSITIVITY", new Vector2(0f, -10f), out var sensitivityValue);
            back = MenuButton(panel, "BackButton", "BACK", new Vector2(0f, -130f));

            var options = panel.gameObject.AddComponent<OptionsPanel>();
            Set(options, "volume", volume);
            Set(options, "volumeValue", volumeValue);
            Set(options, "sensitivity", sensitivity);
            Set(options, "sensitivityValue", sensitivityValue);
            return panel;
        }

        private static Transform BuildCreditsPanel(Transform parent, out Button back)
        {
            var panel = PanelRoot(parent, "CreditsPanel");
            Label(panel, "Heading", "CREDITS", 40, Amber, new Vector2(0f, 200f), new Vector2(700f, 50f));
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
            Label(mainPanel, "Heading", "PAUSED", 48, Amber, new Vector2(0f, 130f), new Vector2(600f, 60f));
            var resume = MenuButton(mainPanel, "ResumeButton", "RESUME", new Vector2(0f, 30f));
            var options = MenuButton(mainPanel, "OptionsButton", "OPTIONS", new Vector2(0f, -35f));
            var menu = MenuButton(mainPanel, "MenuButton", "MAIN MENU", new Vector2(0f, -100f));
            Label(mainPanel, "Hint", "[Esc] / [P] resume", 16, TextDim, new Vector2(0f, -170f), new Vector2(600f, 24f));

            var optionsPanel = BuildOptionsPanel(dim.transform, out var optionsBack);
            optionsPanel.GetComponent<Image>().color = Color.clear; // dim backdrop is enough here
            optionsPanel.gameObject.SetActive(false);

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
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            log.Add($"pause menu placed in Main (player {(player != null ? "ok" : "MISSING")}, counter {(counter != null ? "ok" : "missing")})");
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

        private static Transform CreateCanvas(string name, int sortingOrder)
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

        private static TMP_Text Label(Transform parent, string name, string text, float size, Color color, Vector2 pos, Vector2 box)
        {
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
            return tmp;
        }

        private static Button MenuButton(Transform parent, string name, string text, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280f, 50f);
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
            var label = Label(go.transform, "Label", text, 24, TextLight, Vector2.zero, Vector2.zero);
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

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Set(Object target, string field, Object value)
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

        private static void SetArray(Object target, string field, Object[] values)
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
