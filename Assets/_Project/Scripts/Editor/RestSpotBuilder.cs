using System.IO;
using System.Linq;
using GameOff.Day;
using GameOff.Player;
using GameOff.Rest;
using GameOff.Shops;
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
    /// P28/P29, idempotent: Data/Rest/RestSpotTuning.asset (only created if missing – it is content) and
    /// Prefabs/Rest/PF_RestSpots.prefab = the places to sit down or look from: the three hall benches, bar stools 1–3 (stool 4 is
    /// the dealer's), the porthole in room 103, the family photo on the locker in 103 and the morning letter on its table; plus
    /// the small HUD they share. Positions come from the station's furniture in the open scene (SM_Hall_Bench_*, SM_Bar_Stool_*,
    /// SM_Motel_Locker, SM_Motel_Table), falling back to the StationGrayboxBuilder numbers.
    /// Placed in Main and wired to the player, its camera, the DayManager, the dialogs, the barkeeper and the home planet:
    /// run after Build Station, Build Sky &amp; Exterior, Build Day Flow and Build Economy. Part of Rebuild All.
    /// Menu: GameOff → Build Rest Spots into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class RestSpotBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Rest/PF_RestSpots.prefab";
        private const string TuningPath = "Assets/_Project/Data/Rest/RestSpotTuning.asset";
        private const string PhotoPath = "Assets/_Project/Art/UI/UI_Family_Photo.png";
        private const string HandFontPath = "Assets/_Project/Fonts/ShareTechMono SDF.asset";

        private static readonly Color Paper = new Color(0.72f, 0.7f, 0.65f);   // unlit UI in a dim room: below white
        private static readonly Color PhotoPaper = new Color(0.75f, 0.74f, 0.7f);
        private static readonly Color Ink = new Color(0.1f, 0.11f, 0.2f);

        // Room 103's porthole (StationGrayboxBuilder opening on the x = -20 wall, centre z 5, sill 1.2, head 1.8). The eye goes out
        // past the hull (like the coin telescope's) so neither the frame nor the outer wall is in the view of the home planet.
        private static readonly Vector3 PortholeCentre = new Vector3(-20f, 1.5f, 5f);
        private static readonly Vector3 PortholeApproach = new Vector3(-19.4f, 1.5f, 5f);
        private static readonly Vector3 PortholeEye = new Vector3(-21.5f, 1.5f, 5f);

        [MenuItem("GameOff/Build Rest Spots into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        public static string Build(Scene scene)
        {
            var tuning = EnsureTuning(out bool created);
            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == "PF_RestSpots")) Object.DestroyImmediate(old);

            int found = 0;
            var prefab = BuildPrefab(scene, tuning, ref found);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);

            var all = scene.GetRootGameObjects();
            var player = all.SelectMany(g => g.GetComponentsInChildren<FirstPersonController>(true)).FirstOrDefault();
            var day = all.SelectMany(g => g.GetComponentsInChildren<DayManager>(true)).FirstOrDefault();
            var dialogs = all.SelectMany(g => g.GetComponentsInChildren<DayFlowUI>(true)).FirstOrDefault();
            var camera = player != null ? player.GetComponentInChildren<Camera>(true) : null;
            var barkeeper = all.SelectMany(g => g.GetComponentsInChildren<Barkeeper>(true)).FirstOrDefault();
            var barkeeperTrigger = barkeeper != null
                ? new SerializedObject(barkeeper).FindProperty("interactable").objectReferenceValue as Interactable : null;
            var planet = all.SelectMany(g => g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t => t.name == "HomePlanet");

            foreach (var c in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var so = new SerializedObject(c);
                Link(so, "day", day);
                Link(so, "player", player);
                Link(so, "dialogs", dialogs);
                Link(so, "playerCamera", camera);
                if (c is BarStool) Link(so, "barkeeper", barkeeperTrigger);
                if (c is SeatView && c.name == "Porthole") Link(so, "lookAt", planet);
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return $"[RestSpotBuilder] PF_RestSpots in {scene.name}: tuning {(created ? "created" : "kept")}, {found}/8 furniture pieces found "
                + $"(rest from the builder numbers), player {Ok(player)}, camera {Ok(camera)}, day {Ok(day)}, dialogs {Ok(dialogs)}, "
                + $"barkeeper {Ok(barkeeperTrigger)}, home planet {Ok(planet)}";
        }

        private static GameObject BuildPrefab(Scene scene, RestSpotTuning tuning, ref int found)
        {
            var root = new GameObject("PF_RestSpots");
            var hud = BuildHud(root.transform);

            // Hall benches face north, to the observation deck (StationGrayboxBuilder: yaw 180, front = +z).
            foreach (var (name, x, z) in new[] { ("A", -2.5f, 3.2f), ("B", 2.5f, 3.2f), ("C", -2.5f, -3.5f) })
            {
                var b = Furniture(scene, "SM_Hall_Bench_" + name, new Vector3(x, 0f, z), new Vector3(2.8f, 0.9f, 0.6f), ref found);
                var go = Spot(root.transform, "Bench_" + name, b.center, b.size + Vector3.one * 0.2f, "Sit down", out var trigger);
                var eye = Point(go.transform, "Eye", new Vector3(b.center.x, 1.15f, b.center.z), Quaternion.Euler(6f, 0f, 0f));
                var seat = Seat(go, trigger, tuning, hud, eye, null, 0f, true, false, false, "[Q] Stand up");
                Set(go.AddComponent<HallBench>(), "seat", seat);
            }

            // Bar stools 1–3 face the bar counter (east). Stool 4 is where the dealer stands.
            for (int i = 1; i <= 3; i++)
            {
                var b = Furniture(scene, $"SM_Bar_Stool_{i}", new Vector3(13.6f, 0f, 1.6f + (i - 1) * 1.6f), new Vector3(0.4f, 0.75f, 0.4f), ref found);
                var go = Spot(root.transform, $"BarStool_{i}", b.center, b.size + Vector3.one * 0.2f, "Sit at the bar", out var trigger);
                var eye = Point(go.transform, "Eye", new Vector3(b.center.x, b.max.y + 0.72f, b.center.z), Quaternion.Euler(8f, 90f, 0f));
                var seat = Seat(go, trigger, tuning, hud, eye, null, 0f, false, true, false, "");
                Set(go.AddComponent<BarStool>(), "seat", seat);
            }

            // Porthole in room 103: first in front of the glass, then out to the home planet (lookAt is wired in the scene).
            {
                var go = Spot(root.transform, "Porthole", PortholeCentre + new Vector3(0.12f, 0f, 0f), new Vector3(0.3f, 0.75f, 0.75f),
                    "Look out of the porthole", out var trigger);
                var approach = Point(go.transform, "Approach", PortholeApproach, Quaternion.Euler(0f, -90f, 0f));
                var eye = Point(go.transform, "Eye", PortholeEye, Quaternion.Euler(0f, -90f, 0f));
                var seat = Seat(go, trigger, tuning, hud, eye, approach, tuning.portholeFov, true, false, true, "[Q] Step back");
                Set(go.AddComponent<PortholeView>(), "seat", seat);
            }

            // Family photo taped to the front of the locker in 103 (yaw 180: the doors face +z, into the room).
            {
                var b = Furniture(scene, "SM_Motel_Locker", new Vector3(-16.5f, 0f, 2.45f), new Vector3(0.6f, 1.9f, 0.5f), ref found);
                var at = new Vector3(b.center.x + 0.08f, 1.48f, b.max.z + 0.004f);
                var go = Spot(root.transform, "FamilyPhoto", at + new Vector3(0f, 0f, 0.06f), new Vector3(0.3f, 0.35f, 0.12f), "Look at the photo", out var trigger);
                var canvas = WorldCanvas(go.transform, "Photo", at, Quaternion.Euler(0f, 180f, -3f), new Vector2(0.135f, 0.195f));
                var border = Graphic<Image>(canvas, "PhotoPaper", PhotoPaper);
                Stretch(border.rectTransform);
                var photo = Graphic<RawImage>(canvas, "Picture", Color.white);
                Stretch(photo.rectTransform);
                photo.rectTransform.offsetMin = new Vector2(8f, 22f); // wider white edge at the bottom, like an instant photo
                photo.rectTransform.offsetMax = new Vector2(-8f, -8f);
                photo.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PhotoPath);
                photo.uvRect = new Rect(0f, 0f, 1f / 3f, 1f);
                var family = go.AddComponent<FamilyPhoto>();
                Set(family, "photo", photo);
                var eye = Point(go.transform, "Eye", new Vector3(at.x, 1.52f, b.max.z + 0.42f), Quaternion.Euler(0f, 180f, 0f));
                Seat(go, trigger, tuning, hud, eye, null, 45f, false, false, false, "[Q] Step back", canvas);
            }

            // The morning letter on the table in 103, near the front right corner (the TV stands in the middle).
            {
                var b = Furniture(scene, "SM_Motel_Table", new Vector3(-17f, 0f, 5.5f), new Vector3(0.8f, 0.75f, 0.6f), ref found);
                var at = new Vector3(b.max.x - 0.14f, b.max.y + 0.003f, b.min.z + 0.15f);
                var go = Spot(root.transform, "TableLetter", at + Vector3.up * 0.05f, new Vector3(0.26f, 0.1f, 0.3f), "Read the letter", out var trigger);
                var canvas = WorldCanvas(go.transform, "Letter", at, Quaternion.Euler(90f, -10f, 0f), new Vector2(0.15f, 0.21f));
                var paper = Graphic<Image>(canvas, "Paper", Paper);
                Stretch(paper.rectTransform);
                var text = Label(canvas, "Text", "", 15f, Ink, Vector2.zero, Vector2.zero);
                Stretch(text.rectTransform);
                text.rectTransform.offsetMin = new Vector2(18f, 18f);
                text.rectTransform.offsetMax = new Vector2(-18f, -18f);
                var hand = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HandFontPath);
                if (hand != null) text.font = hand;
                text.alignment = TextAlignmentOptions.TopLeft;
                text.fontSizeMin = 6f;
                text.fontSizeMax = 15f;
                text.textWrappingMode = TextWrappingModes.Normal;
                var letter = go.AddComponent<TableLetter>();
                Set(letter, "text", text);
                Set(letter, "tuning", tuning);
                var eye = Point(go.transform, "Eye", at + new Vector3(0f, 0.42f, -0.16f), Quaternion.Euler(70f, 0f, 0f));
                Seat(go, trigger, tuning, hud, eye, null, 45f, false, false, false, "[Q] Put it down", canvas);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static RestHud BuildHud(Transform parent)
        {
            var canvas = CreateCanvas("RestHudCanvas", 45); // below the day UI (50): its hints and dialogs stay on top
            canvas.SetParent(parent, false);
            var root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvas, false);
            Stretch(root.GetComponent<RectTransform>());
            var keys = Label(root.transform, "Keys", "[Q] Stand up", 14f, TextDim, new Vector2(0f, 18f), new Vector2(700f, 24f));
            BottomCentre(keys.rectTransform);
            var line = Label(root.transform, "Line", "", 18f, TextLight, new Vector2(0f, 50f), new Vector2(820f, 70f));
            BottomCentre(line.rectTransform);
            line.alignment = TextAlignmentOptions.Bottom;
            var hud = canvas.gameObject.AddComponent<RestHud>();
            Set(hud, "root", root);
            Set(hud, "keys", keys);
            Set(hud, "line", line);
            return hud;
        }

        private static void BottomCentre(RectTransform rt)
        {
            var pos = rt.anchoredPosition;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = pos;
        }

        /// <summary>World bounds of a station furniture piece (its fitted BoxCollider), or the builder's numbers if it is missing.</summary>
        private static Bounds Furniture(Scene scene, string name, Vector3 floorCentre, Vector3 size, ref int found)
        {
            var box = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true))
                .FirstOrDefault(c => c.name == name && !c.isTrigger);
            if (box != null && box.gameObject.activeInHierarchy && box.bounds.size.sqrMagnitude > 0.01f)
            {
                found++;
                return box.bounds;
            }
            Debug.LogWarning($"[RestSpotBuilder] {name} not found in the scene, using the builder position");
            return new Bounds(floorCentre + Vector3.up * (size.y * 0.5f), size);
        }

        /// <summary>A spot: its object and the [E] trigger box (world centre and size).</summary>
        private static GameObject Spot(Transform parent, string name, Vector3 centre, Vector3 size, string prompt, out Interactable trigger)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = new GameObject("Trigger", typeof(BoxCollider));
            t.transform.SetParent(go.transform, false);
            t.transform.position = centre;
            var box = t.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            trigger = t.AddComponent<Interactable>();
            SetValue(trigger, "prompt", prompt);
            return go;
        }

        private static Transform Point(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.SetPositionAndRotation(position, rotation);
            return t;
        }

        private static SeatView Seat(GameObject go, Interactable trigger, RestSpotTuning tuning, RestHud hud, Transform eye, Transform approach,
            float fov, bool mouseLook, bool standWhenControlReturns, bool looksOutside, string keys, Transform lookAt = null)
        {
            var seat = go.AddComponent<SeatView>();
            Set(seat, "interactable", trigger);
            Set(seat, "tuning", tuning);
            Set(seat, "hud", hud);
            Set(seat, "eye", eye);
            if (approach != null) Set(seat, "approach", approach);
            if (lookAt != null) Set(seat, "lookAt", lookAt);
            SetValue(seat, "fieldOfView", fov);
            SetValue(seat, "mouseLook", mouseLook);
            SetValue(seat, "standWhenControlReturns", standWhenControlReturns);
            SetValue(seat, "looksOutside", looksOutside);
            SetValue(seat, "keys", keys);
            return seat;
        }

        /// <summary>World-space canvas of the given size in metres (1 mm = 2 canvas units).</summary>
        private static Transform WorldCanvas(Transform parent, string name, Vector3 position, Quaternion rotation, Vector2 metres)
        {
            const float unitsPerMetre = 2000f;
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, false);
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = metres * unitsPerMetre;
            rt.localScale = Vector3.one / unitsPerMetre;
            rt.SetPositionAndRotation(position, rotation);
            return go.transform;
        }

        private static T Graphic<T>(Transform parent, string name, Color color) where T : MaskableGraphic
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(T));
            go.transform.SetParent(parent, false);
            var g = go.GetComponent<T>();
            g.color = color;
            g.raycastTarget = false;
            return g;
        }

        private static void SetValue(Object target, string field, object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError($"[RestSpotBuilder] {target.GetType().Name}.{field} not found"); return; }
            switch (value)
            {
                case float f: p.floatValue = f; break;
                case bool b: p.boolValue = b; break;
                case string s: p.stringValue = s; break;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Link(SerializedObject so, string field, Object value)
        {
            var p = so.FindProperty(field);
            if (p != null && p.propertyType == SerializedPropertyType.ObjectReference) p.objectReferenceValue = value;
        }

        private static string Ok(Object o) => o != null ? "ok" : "MISSING";

        private static RestSpotTuning EnsureTuning(out bool created)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<RestSpotTuning>(TuningPath);
            created = tuning == null;
            if (!created) return tuning;
            Directory.CreateDirectory(Path.GetDirectoryName(TuningPath));
            tuning = ScriptableObject.CreateInstance<RestSpotTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            AssetDatabase.SaveAssets();
            return tuning;
        }
    }
}
