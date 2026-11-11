using System.Collections.Generic;
using System.Linq;
using GameOff.Counter;
using GameOff.Day;
using GameOff.Departure;
using GameOff.Documents;
using GameOff.Player;
using GameOff.Rest;
using GameOff.Shops;
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
    /// P140: the [H] help (HelpOverlay) in Main, idempotent: its own canvas with the "[H] Help" line at the bottom edge and the
    /// key list in a terminal frame, wired to everything it looks at in the scene. It holds scene references to objects other
    /// builders make (counter, rest spots, telescope, pause menu, shuttle ...), so it runs last in Rebuild All; after running one
    /// of those builders on its own, run this one again. Menu: GameOff → Build Help Overlay into Main (claim Main first).
    /// </summary>
    public static class HelpOverlayBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string RootName = "HelpOverlay";
        private const string MonoFontPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";

        [MenuItem("GameOff/Build Help Overlay into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        public static string Build(Scene scene)
        {
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == RootName)) Object.DestroyImmediate(old);

            // Above the day UI (50), the economy windows (60) and the telescope; below the pause menu (100) and the main menu (200).
            var canvas = CreateCanvas(RootName, 90);
            SceneManager.MoveGameObjectToScene(canvas.gameObject, scene);

            var prompt = Label(canvas, "Prompt", HelpKeys.Prompt, 13f, new Color(TextLight.r, TextLight.g, TextLight.b, 0.7f),
                new Vector2(0f, 8f), new Vector2(300f, 22f));
            var promptRt = prompt.rectTransform;
            promptRt.anchorMin = promptRt.anchorMax = promptRt.pivot = new Vector2(0.5f, 0f);
            promptRt.anchoredPosition = new Vector2(0f, 8f);

            // The open help: the game dimmed a little behind a terminal window.
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvas, false);
            Stretch(panel.GetComponent<RectTransform>());
            var dim = panel.GetComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.35f);
            dim.raycastTarget = false;

            var window = new GameObject("Window", typeof(RectTransform), typeof(Image));
            window.transform.SetParent(panel.transform, false);
            window.GetComponent<RectTransform>().sizeDelta = new Vector2(680f, 330f);
            var frame = window.GetComponent<Image>();
            frame.color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.94f);
            frame.raycastTarget = false;
            TerminalStyle.Frame(frame);

            var heading = Label(window.transform, "Heading", "KEYS", 12f, TextDim, new Vector2(0f, 140f), new Vector2(620f, 18f));
            heading.characterSpacing = 4f;
            var title = Label(window.transform, "Title", "", 20f, Amber, new Vector2(0f, 116f), new Vector2(620f, 28f));
            TerminalStyle.AddDivider(window.transform, new Vector2(0f, 98f), 620f);

            var body = Label(window.transform, "Body", "", 14f, TextLight, new Vector2(0f, -12f), new Vector2(620f, 210f));
            var mono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(MonoFontPath);
            if (mono != null) body.font = mono;
            body.alignment = TextAlignmentOptions.TopLeft;
            body.richText = true;
            body.lineSpacing = 10f;
            body.textWrappingMode = TextWrappingModes.Normal;
            body.fontSizeMin = 10f;

            Label(window.transform, "Footer", HelpKeys.Footer, 12f, TextDim, new Vector2(0f, -144f), new Vector2(620f, 20f));

            var overlay = canvas.gameObject.AddComponent<HelpOverlay>();
            Set(overlay, "panel", panel);
            Set(overlay, "title", title);
            Set(overlay, "body", body);
            Set(overlay, "prompt", prompt.gameObject);
            var so = new SerializedObject(overlay);
            so.FindProperty("keyColor").colorValue = Amber;
            so.ApplyModifiedPropertiesWithoutUndo();

            var log = new List<string>();
            var all = scene.GetRootGameObjects();
            T One<T>(string field) where T : Component
            {
                var found = all.SelectMany(g => g.GetComponentsInChildren<T>(true)).FirstOrDefault();
                Set(overlay, field, found);
                if (found == null) log.Add($"no {typeof(T).Name}");
                return found;
            }
            T[] Many<T>(string field) where T : Component
            {
                var found = all.SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
                SetArray(overlay, field, found.Cast<Object>().ToArray());
                if (found.Length == 0) log.Add($"no {typeof(T).Name}");
                return found;
            }
            One<FirstPersonController>("player");
            One<DayManager>("day");
            One<DayFlowUI>("dayUi");
            One<PauseMenu>("pause");
            One<ShiftReportWindow>("shiftReport");
            One<CounterStation>("counter");
            One<DocumentDesk>("desk");
            One<CounterTerminal>("terminal");
            One<PinBoardCloseUp>("pinBoard");
            One<InventoryWindow>("inventory");
            var trades = Many<TradeWindow>("trades");
            One<CoinTelescope>("telescope");
            One<ShuttleFlight>("flight");
            var seats = Many<SeatView>("seats");
            var benches = Many<HallBench>("benches");

            panel.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            return $"[HelpOverlayBuilder] {RootName} in {scene.name}: {seats.Length} seats, {benches.Length} benches, {trades.Length} trade windows"
                + (log.Count > 0 ? "; MISSING (that help situation never shows): " + string.Join(", ", log) : "; everything wired");
        }
    }
}
