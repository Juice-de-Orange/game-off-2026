using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Core;
using GameOff.Day;
using GameOff.Player;
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
    /// Builds the day flow (P09), idempotent: Data/Days/DaySchedule.asset (only created if missing – it is content, edit it
    /// in the inspector), Prefabs/Day/PF_DayFlow.prefab (DayManager, bed trigger in room 103, day UI) and places it in the
    /// open scene wired to CounterShift, SkyRig, the player and the PlayerSpawn marker.
    /// Menu: GameOff → Build Day Flow (in the open scene) / Build Day Flow into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class DayFlowBuilder
    {
        private const string SchedulePath = "Assets/_Project/Data/Days/DaySchedule.asset";
        private const string TestDayPath = "Assets/_Project/Data/Days/Day1_Test.asset";
        private const string PrefabPath = "Assets/_Project/Prefabs/Day/PF_DayFlow.prefab";
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string DebugMaterialPath = "Assets/_Project/Art/Materials/Debug/M_Debug_Trigger.mat";

        // Bed in room 103. The trigger is fitted to the real bed model in the scene (SM_Motel_Bed from the station builder):
        // its renderer bounds plus a margin, so the prompt appears exactly when looking at the bed and the ray hits the trigger
        // before the bed's own collider. Fallback if no bed is found: the position from StationGrayboxBuilder (x -19.29, z 4.8).
        private const string BedModelName = "SM_Motel_Bed";
        private const float BedMarginSide = 0.15f, BedMarginTop = 0.3f;
        private static readonly Vector3 BedCenter = new Vector3(-19.29f, 0.45f, 4.8f);
        private static readonly Vector3 BedSize = new Vector3(1.3f, 0.9f, 2.3f);
        private static readonly Color DevRed = new Color(0.89f, 0.27f, 0.25f); // STYLE.md alarm red

        private static readonly Color Paper = new Color(0.89f, 0.87f, 0.81f);
        private static readonly Color Ink = new Color(0.12f, 0.12f, 0.14f);
        private static readonly Color InkRed = new Color(0.48f, 0.12f, 0.17f);

        [MenuItem("GameOff/Build Day Flow")]
        public static void BuildFromMenu() => Debug.Log(Build(SceneManager.GetActiveScene()));

        [MenuItem("GameOff/Build Day Flow into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        public static string Build(Scene scene)
        {
            var log = new List<string>();
            var schedule = EnsureSchedule(log);
            var prefab = BuildPrefab(log);
            Place(scene, prefab, schedule, log);
            return "[DayFlowBuilder] " + string.Join("; ", log);
        }

        private static DaySchedule EnsureSchedule(List<string> log)
        {
            var schedule = AssetDatabase.LoadAssetAtPath<DaySchedule>(SchedulePath);
            if (schedule != null) return schedule;
            var testDay = AssetDatabase.LoadAssetAtPath<DayRules>(TestDayPath);
            schedule = ScriptableObject.CreateInstance<DaySchedule>();
            AssetDatabase.CreateAsset(schedule, SchedulePath);
            var so = new SerializedObject(schedule);
            var days = so.FindProperty("days");
            string[] letters =
            {
                "Dear Dad,\nMom says the heater makes the funny noise again. Pip drew you a picture of the station, it is in the next parcel.\nWe miss you. Work hard and come home soon.\n— Lena",
                "Dad,\nThe rent man came by. Mom gave him what we had. Pip has a cough but it is not bad.\nPlease send a little more this week if you can.\n— Lena",
                "Dad,\nPip is sick now, really sick. The clinic wants money first.\nMom does not want me to tell you. I am telling you anyway.\n— Lena",
            };
            string[] newToday = { "", "New today: check the cargo manifest against the ID.", "New today: security level raised." };
            for (int i = 0; i < letters.Length; i++)
            {
                days.InsertArrayElementAtIndex(i);
                var entry = days.GetArrayElementAtIndex(i);
                entry.FindPropertyRelative("rules").objectReferenceValue = testDay;
                entry.FindPropertyRelative("letterTitle").stringValue = "A letter from home";
                entry.FindPropertyRelative("letter").stringValue = letters[i] + "\n\n(placeholder text – P15)";
                entry.FindPropertyRelative("newToday").stringValue = newToday[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            log.Add("DaySchedule created (3 × Day1_Test, placeholder letters)");
            return schedule;
        }

        private static GameObject BuildPrefab(List<string> log)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("PF_DayFlow");
            var manager = root.AddComponent<DayManager>();

            var bed = new GameObject("BedTrigger", typeof(BoxCollider));
            bed.transform.SetParent(root.transform, false);
            bed.transform.position = BedCenter;
            var box = bed.GetComponent<BoxCollider>();
            box.size = BedSize;
            box.isTrigger = true;
            var interactable = bed.AddComponent<Interactable>();

            // Dev view: translucent red box exactly over the trigger (no collider, no shadows); DayManager shows it only
            // while showDevTools is on.
            var debugView = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugView.name = "DebugView";
            Object.DestroyImmediate(debugView.GetComponent<Collider>());
            debugView.transform.SetParent(bed.transform, false);
            debugView.transform.localScale = BedSize;
            var debugRenderer = debugView.GetComponent<MeshRenderer>();
            debugRenderer.sharedMaterial = DebugMaterial();
            debugRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            debugRenderer.receiveShadows = false;

            var ui = BuildUI(root.transform);

            Set(manager, "bed", interactable);
            Set(manager, "bedDebugView", debugView);
            Set(manager, "ui", ui);
            // P109/P133: the curfew and first-morning lines live with the counter's loudspeaker lines (built by Build Documents)
            var announcements = AssetDatabase.LoadAssetAtPath<GameOff.Counter.Announcements>("Assets/_Project/Data/Counter/Announcements.asset");
            if (announcements != null) Set(manager, "announcements", announcements);
            else log.Add("Announcements.asset missing: curfew uses the default texts (run GameOff/Build Documents)");
            BuildHallBoard(root.transform);
            var mood = root.AddComponent<StationMood>();
            Set(mood, "day", manager);
            Set(mood, "tuning", EnsureMoodTuning(log));
            // P75: recurring characters (story ones come with P76; the test character only with the dev tools on)
            var director = root.AddComponent<GameOff.Characters.CharacterDirector>();
            Set(director, "dayManager", manager);
            log.Add(CharacterBuilder.Build());
            var test = AssetDatabase.LoadAssetAtPath<GameOff.Characters.RecurringCharacter>(CharacterBuilder.CharacterPath);
            if (test != null) SetArray(director, "devCharacters", new Object[] { test });
            // P76: the story characters
            log.Add(StoryCharacterBuilder.Build());
            log.Add(DayPoolBuilder.Build()); // P73: endless days after the story
            EconomySimulator.ResetCharacters();
            var story = AssetDatabase.FindAssets("t:RecurringCharacter", new[] { "Assets/_Project/Data/Characters/Story" })
                .Select(g => (Object)AssetDatabase.LoadAssetAtPath<GameOff.Characters.RecurringCharacter>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
            SetArray(director, "characters", story);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            log.Add("PF_DayFlow built");
            return prefab;
        }

        /// <summary>
        /// P31: text on the small CRTs of SM_Hall_DepartureBoard (hall centre, x 0 z 0). Each of the four sides has two CRTs, 0.40 × 0.30 m,
        /// centred ±0.28 m beside the middle at 1.88 m height, 0.76 m from the centre (measured from the mesh on 07.11.); the header above
        /// stays as delivered. A dark backing hides the printed table; left CRT = status, right CRT = market notice (HallBoard).
        /// </summary>
        private static void BuildHallBoard(Transform parent)
        {
            var go = new GameObject("HallBoard");
            go.transform.SetParent(parent, false);
            var status = new List<Object>();
            var notice = new List<Object>();
            var backing = HallBoardBacking();
            foreach (float yaw in new[] { 0f, 180f, -90f, 90f })
            {
                var rotation = Quaternion.Euler(0f, yaw, 0f);
                foreach (int side in new[] { -1, 1 })
                {
                    // Local frame of the −Z side: the viewer stands at −Z looking +Z, so −X is on the left.
                    var center = rotation * new Vector3(side * 0.28f, 1.88f, -0.766f);
                    var back = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    back.name = "Backing";
                    Object.DestroyImmediate(back.GetComponent<Collider>());
                    back.transform.SetParent(go.transform, false);
                    back.transform.SetPositionAndRotation(center + rotation * new Vector3(0f, 0f, 0.002f), rotation);
                    back.transform.localScale = new Vector3(0.38f, 0.28f, 1f);
                    var r = back.GetComponent<MeshRenderer>();
                    r.sharedMaterial = backing;
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                    var t = new GameObject(side < 0 ? "Status" : "Notice", typeof(RectTransform), typeof(TextMeshPro));
                    t.transform.SetParent(go.transform, false);
                    t.transform.SetPositionAndRotation(center + rotation * new Vector3(0f, 0f, -0.002f), rotation);
                    t.GetComponent<RectTransform>().sizeDelta = new Vector2(0.35f, 0.26f);
                    var tmp = t.GetComponent<TextMeshPro>();
                    tmp.font = TitleFont();
                    tmp.enableAutoSizing = true;
                    tmp.fontSizeMin = 0.05f;
                    tmp.fontSizeMax = 0.35f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color(0.61f, 0.88f, 0.35f); // CRT green #9BE05A
                    tmp.text = side < 0 ? "KESTREL-9" : "NOTICE";
                    (side < 0 ? status : notice).Add(tmp);
                }
            }
            var board = go.AddComponent<HallBoard>();
            SetArray(board, "statusScreens", status.ToArray());
            SetArray(board, "noticeScreens", notice.ToArray());
        }

        private const string HallBoardBackingPath = "Assets/_Project/Art/Materials/Hall/M_Hall_DepartureBoard_Backing.mat";

        /// <summary>Near-black unlit screen backing (created once).</summary>
        private static Material HallBoardBacking()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(HallBoardBackingPath);
            if (mat != null) return mat;
            Directory.CreateDirectory(Path.GetDirectoryName(HallBoardBackingPath));
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = new Color(0.02f, 0.04f, 0.03f) };
            AssetDatabase.CreateAsset(mat, HallBoardBackingPath);
            return mat;
        }

        private static DayFlowUI BuildUI(Transform parent)
        {
            var canvas = CreateCanvas("DayCanvas", 50); // below the pause menu (100)
            canvas.SetParent(parent, false);

            var dayLabel = Label(canvas, "DayLabel", "DAY 1", 16, TextDim, Vector2.zero, new Vector2(360f, 24f));
            Anchor(dayLabel.rectTransform, new Vector2(0f, 1f), new Vector2(16f, -14f));
            dayLabel.alignment = TextAlignmentOptions.TopLeft;

            var hintBar = new GameObject("Hint", typeof(RectTransform), typeof(Image));
            hintBar.transform.SetParent(canvas, false);
            var hintRt = hintBar.GetComponent<RectTransform>();
            hintRt.anchorMin = hintRt.anchorMax = hintRt.pivot = new Vector2(0.5f, 0f);
            hintRt.anchoredPosition = new Vector2(0f, 40f);
            hintRt.sizeDelta = new Vector2(720f, 38f);
            hintBar.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.8f);
            hintBar.GetComponent<Image>().raycastTarget = false;
            var hintText = Label(hintBar.transform, "Text", "", 18, TextLight, Vector2.zero, Vector2.zero);
            Stretch(hintText.rectTransform);

            // Letter: a sheet of paper in the middle
            var letter = new GameObject("Letter", typeof(RectTransform), typeof(Image));
            letter.transform.SetParent(canvas, false);
            var letterRt = letter.GetComponent<RectTransform>();
            letterRt.sizeDelta = new Vector2(520f, 380f);
            letter.GetComponent<Image>().color = Paper;
            var title = Label(letter.transform, "Title", "A letter from home", 26, Ink, new Vector2(0f, 155f), new Vector2(460f, 36f));
            var body = Label(letter.transform, "Body", "", 18, Ink, new Vector2(0f, 20f), new Vector2(460f, 220f));
            body.alignment = TextAlignmentOptions.TopLeft;
            var newToday = Label(letter.transform, "NewToday", "", 17, InkRed, new Vector2(0f, -112f), new Vector2(460f, 26f));
            newToday.alignment = TextAlignmentOptions.Left;
            var cont = MenuButton(letter.transform, "ContinueButton", "[SPACE]  START THE DAY", new Vector2(0f, -158f), new Vector2(300f, 40f), 18);

            // Bed dialog (looks like the menu panels)
            var dialog = new GameObject("BedDialog", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(canvas, false);
            Stretch(dialog.GetComponent<RectTransform>());
            dialog.GetComponent<Image>().color = new Color(Backdrop.r, Backdrop.g, Backdrop.b, 0.8f);
            var box = new GameObject("Box", typeof(RectTransform), typeof(Image));
            box.transform.SetParent(dialog.transform, false);
            box.GetComponent<RectTransform>().sizeDelta = new Vector2(560f, 250f);
            box.GetComponent<Image>().color = Panel;
            TerminalStyle.Frame(box.GetComponent<Image>()); // P95
            var dTitle = Label(box.transform, "Title", "End day 1?", 30, Amber, new Vector2(0f, 80f), new Vector2(520f, 40f));
            var dBody = Label(box.transform, "Body", "", 18, TextLight, new Vector2(0f, 20f), new Vector2(500f, 60f));
            var dConfirm = MenuButton(box.transform, "ConfirmButton", "SLEEP", new Vector2(-140f, -70f), new Vector2(200f, 44f), 20);
            var dCancel = MenuButton(box.transform, "BackButton", "BACK", new Vector2(140f, -70f), new Vector2(200f, 44f), 20);
            var dDev = MenuButton(dialog.transform, "DevEndDayButton", "Dev_end_Day", new Vector2(0f, -170f), new Vector2(200f, 36f), 16);
            var devColors = dDev.colors;
            devColors.normalColor = DevRed;
            devColors.highlightedColor = Color.Lerp(DevRed, Color.white, 0.25f);
            devColors.selectedColor = DevRed;
            devColors.pressedColor = Color.Lerp(DevRed, Color.black, 0.3f);
            dDev.colors = devColors;
            TerminalStyle.Flat(dDev);

            // Fade over everything except the end screen
            var fadeGo = new GameObject("Fade", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            fadeGo.transform.SetParent(canvas, false);
            Stretch(fadeGo.GetComponent<RectTransform>());
            fadeGo.GetComponent<Image>().color = Color.black;

            var end = new GameObject("EndScreen", typeof(RectTransform));
            end.transform.SetParent(canvas, false);
            Stretch(end.GetComponent<RectTransform>());
            // Every ending sets its own title and text (DayManager.ShowRunEnd); these only stand until then.
            var endTitle = Label(end.transform, "Title", "THE END", 48, Amber, new Vector2(0f, 60f), new Vector2(800f, 60f));
            var endBody = Label(end.transform, "Body", "Thanks for playing.", 18, TextLight, new Vector2(0f, -10f), new Vector2(800f, 60f));
            var menu = MenuButton(end.transform, "MenuButton", "MAIN MENU", new Vector2(0f, -100f));

            var ui = canvas.gameObject.AddComponent<DayFlowUI>();
            Set(ui, "fade", fadeGo.GetComponent<CanvasGroup>());
            Set(ui, "letterPanel", letter);
            Set(ui, "letterTitle", title);
            Set(ui, "letterBody", body);
            Set(ui, "letterNewToday", newToday);
            Set(ui, "letterContinue", cont);
            Set(ui, "hintBar", hintBar);
            Set(ui, "hint", hintText);
            Set(ui, "dayLabel", dayLabel);
            Set(ui, "endPanel", end);
            Set(ui, "endMenuButton", menu);
            Set(ui, "endTitle", endTitle);
            Set(ui, "endBody", endBody);
            Set(ui, "endMenuLabel", menu.GetComponentInChildren<TMP_Text>());
            Set(ui, "dialogPanel", dialog);
            Set(ui, "dialogTitle", dTitle);
            Set(ui, "dialogBody", dBody);
            Set(ui, "dialogConfirm", dConfirm);
            Set(ui, "dialogCancel", dCancel);
            Set(ui, "dialogDev", dDev);
            return ui;
        }

        private static void Place(Scene scene, GameObject prefab, DaySchedule schedule, List<string> log)
        {
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var manager = instance.GetComponent<DayManager>();
            var shift = Object.FindFirstObjectByType<CounterShift>();
            var sky = Object.FindFirstObjectByType<SkyRig>();
            var player = Object.FindFirstObjectByType<FirstPersonController>();
            var spawn = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == "PlayerSpawn");
            Set(manager, "schedule", schedule);
            Set(manager, "shift", shift);
            Set(manager, "sky", sky);
            Set(manager, "player", player);
            Set(manager, "wakeUpPoint", spawn);
            PrefabUtility.RecordPrefabInstancePropertyModifications(manager);
            foreach (var board in instance.GetComponentsInChildren<HallBoard>(true))
            {
                Set(board, "day", manager);
                Set(board, "shift", shift);
                PrefabUtility.RecordPrefabInstancePropertyModifications(board);
            }
            foreach (var director in instance.GetComponentsInChildren<GameOff.Characters.CharacterDirector>(true))
            {
                Set(director, "shift", shift);
                Set(director, "presenter", Object.FindFirstObjectByType<TravelerPresenter>());
                Set(director, "announcer", Object.FindFirstObjectByType<GameOff.Counter.Announcer>());
                PrefabUtility.RecordPrefabInstancePropertyModifications(director);
            }
            string mood = WireStationMood(scene, instance);
            string bed = FitBedTrigger(scene, instance);
            EditorSceneManager.MarkSceneDirty(scene);
            log.Add($"placed in {scene.name} (shift {Ok(shift)}, sky {Ok(sky)}, player {Ok(player)}, spawn {Ok(spawn)}, {bed}, {mood})");
        }

        private const string MoodTuningPath = "Assets/_Project/Data/Days/StationMoodTuning.asset";

        // P36: glowing signs that drop out when the station is in trouble (from StationGrayboxBuilder, letters included).
        private static readonly HashSet<string> NeonNames = new HashSet<string> { "SM_Bar_NeonSign", "Neon_Bar", "Neon_Kiosk" };

        /// <summary>Tuning of the station's situation (created once; then it is content, edit it in the inspector).</summary>
        private static StationMoodTuning EnsureMoodTuning(List<string> log)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<StationMoodTuning>(MoodTuningPath);
            if (tuning != null) return tuning;
            tuning = ScriptableObject.CreateInstance<StationMoodTuning>();
            AssetDatabase.CreateAsset(tuning, MoodTuningPath);
            log.Add("StationMoodTuning created");
            return tuning;
        }

        /// <summary>Wires StationMood to every flicker lamp and neon sign of the station in the scene.</summary>
        private static string WireStationMood(Scene scene, GameObject dayFlow)
        {
            var mood = dayFlow.GetComponent<StationMood>();
            var all = scene.GetRootGameObjects().Where(g => g != dayFlow).SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToList();
            var lamps = all.Select(t => t.GetComponent<LightFlicker>()).Where(l => l != null).Cast<Object>().ToArray();
            var neons = all.Where(t => NeonNames.Contains(t.name))
                .SelectMany(t => t.GetComponentsInChildren<Renderer>(true)).Distinct().Cast<Object>().ToArray();
            SetArray(mood, "lamps", lamps);
            SetArray(mood, "neons", neons);
            PrefabUtility.RecordPrefabInstancePropertyModifications(mood);
            return $"station mood: {lamps.Length} lamps, {neons.Length} neon renderers";
        }

        private static string Ok(Object o) => o != null ? "ok" : "MISSING";

        /// <summary>URP Unlit, transparent red (created once).</summary>
        internal static Material DebugMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(DebugMaterialPath);
            if (mat != null) return mat;
            Directory.CreateDirectory(Path.GetDirectoryName(DebugMaterialPath));
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetFloat("_Surface", 1f); // transparent
            mat.SetFloat("_Blend", 0f);   // alpha
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0f);
            mat.SetFloat("_Cull", 0f);    // visible from inside too
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.SetColor("_BaseColor", new Color(1f, 0.15f, 0.1f, 0.35f));
            AssetDatabase.CreateAsset(mat, DebugMaterialPath);
            return mat;
        }

        /// <summary>Puts the bed trigger over the bed model's renderer bounds (+ margin). Returns a log text.</summary>
        private static string FitBedTrigger(Scene scene, GameObject dayFlow)
        {
            var model = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(t => t.name == BedModelName && t.gameObject != dayFlow);
            var renderers = model != null ? model.GetComponentsInChildren<Renderer>() : new Renderer[0];
            if (renderers.Length == 0) return "bed model not found, default trigger";
            var bounds = renderers[0].bounds;
            foreach (var r in renderers) bounds.Encapsulate(r.bounds);
            bounds.Expand(new Vector3(2f * BedMarginSide, 0f, 2f * BedMarginSide));
            var center = bounds.center + Vector3.up * (BedMarginTop / 2f);
            var size = bounds.size + Vector3.up * BedMarginTop;

            var trigger = dayFlow.transform.Find("BedTrigger");
            trigger.position = center;
            trigger.rotation = Quaternion.identity;
            var box = trigger.GetComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = size;
            var debugView = trigger.Find("DebugView");
            debugView.localPosition = Vector3.zero;
            debugView.localScale = size;
            PrefabUtility.RecordPrefabInstancePropertyModifications(trigger);
            PrefabUtility.RecordPrefabInstancePropertyModifications(box);
            PrefabUtility.RecordPrefabInstancePropertyModifications(debugView);
            return $"bed trigger fitted to {BedModelName} ({size.x:0.00} × {size.y:0.00} × {size.z:0.00} m)";
        }

        private static void Anchor(RectTransform rt, Vector2 corner, Vector2 offset)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = corner;
            rt.anchoredPosition = offset;
        }
    }
}
