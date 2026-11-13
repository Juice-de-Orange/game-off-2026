using System.IO;
using System.Linq;
using GameOff.Day;
using GameOff.Departure;
using GameOff.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace GameOff.EditorTools
{
    /// <summary>
    /// The ticket home (P72), idempotent: Data/Departure/DepartureTuning.asset (only created if missing – it is content) and
    /// Prefabs/Departure/PF_Departure.prefab = ticket terminal beside the shuttle hatch in the hangar's west wall (model
    /// SM_Hangar_TicketTerminal or a grey box until it is delivered), the hatch both ways, the boarding trigger at the seat in the cabin,
    /// ShuttleFlight. Placed in Main and wired to the player, the DayManager, the dialogs and the docked shuttle. Part of Rebuild All;
    /// bake the light afterwards.
    /// Menu: GameOff → Build Departure into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class DepartureBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Departure/PF_Departure.prefab";
        private const string TuningPath = "Assets/_Project/Data/Departure/DepartureTuning.asset";
        private const string TerminalModelPath = "Assets/_Project/Art/Models/Hangar/SM_Hangar_TicketTerminal.fbx";
        private const string ShuttleModelPath = "Assets/_Project/Art/Models/Exterior/SM_Ext_Shuttle_Docked.fbx";
        private const string PlaceholderMaterialPath = "Assets/_Project/Art/Materials/Station/M_Station_Door_Slide.mat";
        private const string EngineClipPath = "Assets/_Project/Audio/SFX/SFX_Shuttle_Engine_Loop.ogg";
        private const string MixerPath = "Assets/_Project/Audio/GameMixer.mixer";
        private const string DockedShuttleName = "Shuttle_Docked", CabinName = "SM_Shuttle_Cabin_Interior";

        // Against the west wall south of the hatch (z -26), front facing into the hangar.
        private static readonly Vector3 TerminalPosition = new Vector3(-17.4f, 0f, -28.7f);
        private const float TerminalYaw = 90f;
        private static readonly Vector3 TerminalSize = new Vector3(0.8f, 1.8f, 0.6f);
        // Thin plates against the wall: the interaction ray starts at the camera and doesn't report a collider the camera is inside of.
        private static readonly Vector3 HatchTriggerCenter = new Vector3(-17.75f, 1.2f, -26f);
        private static readonly Vector3 HatchTriggerSize = new Vector3(0.2f, 2.4f, 2f);
        private static readonly Vector3 HatchOutside = new Vector3(-16.6f, 0f, -26f);

        [MenuItem("GameOff/Build Departure into Main")]
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
            var prefab = BuildPrefab(tuning);

            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var all = scene.GetRootGameObjects();
            var player = all.SelectMany(g => g.GetComponentsInChildren<FirstPersonController>(true)).FirstOrDefault();
            var day = all.SelectMany(g => g.GetComponentsInChildren<DayManager>(true)).FirstOrDefault();
            var dialogs = all.SelectMany(g => g.GetComponentsInChildren<DayFlowUI>(true)).FirstOrDefault();
            var transforms = all.SelectMany(g => g.GetComponentsInChildren<Transform>(true)).ToList();
            var shuttle = transforms.FirstOrDefault(t => t.name == DockedShuttleName);
            var cabin = transforms.FirstOrDefault(t => t.name == CabinName);
            var barrier = all.SelectMany(g => g.GetComponentsInChildren<GameOff.Core.EnergyBarrier>(true)).FirstOrDefault(); // P155
            var camera = player != null ? player.GetComponentInChildren<Camera>(true) : null;

            foreach (var c in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var so = new SerializedObject(c);
                Link(so, "day", day);
                Link(so, "player", player);
                Link(so, "dialogs", dialogs);
                Link(so, "ui", dialogs);
                Link(so, "playerCamera", camera);
                Link(so, "dockedShuttle", shuttle);
                Link(so, "cabin", cabin);
                Link(so, "barrier", barrier);
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            }
            EditorSceneManager.MarkSceneDirty(scene);

            bool model = AssetDatabase.LoadAssetAtPath<GameObject>(TerminalModelPath) != null;
            return $"[DepartureBuilder] PF_Departure in {scene.name}: tuning {(created ? "created" : "kept")}, terminal "
                + (model ? "model" : "grey box (SM_Hangar_TicketTerminal not delivered yet)")
                + $", player {Ok(player)}, camera {Ok(camera)}, day {Ok(day)}, dialogs {Ok(dialogs)}, docked shuttle {Ok(shuttle)}, cabin {Ok(cabin)}, barrier {Ok(barrier)}"
                + (AssetDatabase.LoadAssetAtPath<AudioClip>(EngineClipPath) == null ? ", no engine sound yet" : ", engine sound");
        }

        private static string Ok(Object o) => o != null ? "ok" : "MISSING";

        private static void Link(SerializedObject so, string field, Object value)
        {
            var p = so.FindProperty(field);
            if (p != null && p.propertyType == SerializedPropertyType.ObjectReference) p.objectReferenceValue = value;
        }

        private static DepartureTuning EnsureTuning(out bool created)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<DepartureTuning>(TuningPath);
            created = tuning == null;
            if (!created) return tuning;
            Directory.CreateDirectory(Path.GetDirectoryName(TuningPath));
            tuning = ScriptableObject.CreateInstance<DepartureTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            AssetDatabase.SaveAssets();
            return tuning;
        }

        private static GameObject BuildPrefab(DepartureTuning tuning)
        {
            var root = new GameObject("PF_Departure");

            // Ticket terminal: delivered model or a grey box of the ordered size, pivot bottom center.
            var terminal = new GameObject("TicketTerminal");
            terminal.transform.SetParent(root.transform, false);
            terminal.transform.SetPositionAndRotation(TerminalPosition, Quaternion.Euler(0f, TerminalYaw, 0f));
            var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(TerminalModelPath);
            GameObject model;
            if (modelAsset != null) model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
            else
            {
                model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                model.name = "SM_Hangar_TicketTerminal_Placeholder";
                model.transform.localScale = TerminalSize;
                model.transform.localPosition = new Vector3(0f, TerminalSize.y * 0.5f, 0f);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(PlaceholderMaterialPath);
                if (mat != null) model.GetComponent<MeshRenderer>().sharedMaterial = mat;
            }
            model.transform.SetParent(terminal.transform, false);
            if (modelAsset != null && model.GetComponentInChildren<Collider>() == null)
            {
                var box = model.AddComponent<BoxCollider>(); // pivot bottom center
                box.size = TerminalSize;
                box.center = new Vector3(0f, TerminalSize.y * 0.5f, 0f);
            }
            foreach (var t in model.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            var terminalTrigger = Trigger(terminal.transform, "Trigger", TerminalPosition + new Vector3(0f, 1f, 0f), new Vector3(1.4f, 2f, 1.4f), "Ticket home");
            var ticket = terminal.AddComponent<TicketTerminal>();
            Set(ticket, "interactable", terminalTrigger);

            // Flight and boarding.
            var flightGo = new GameObject("ShuttleFlight");
            flightGo.transform.SetParent(root.transform, false);
            var flight = flightGo.AddComponent<ShuttleFlight>();
            Set(flight, "tuning", tuning);
            Set(flight, "shuttleModel", AssetDatabase.LoadAssetAtPath<GameObject>(ShuttleModelPath));
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(EngineClipPath);
            if (clip != null)
            {
                var source = flightGo.AddComponent<AudioSource>();
                source.clip = clip;
                source.loop = true;
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
                source.outputAudioMixerGroup = mixer != null ? mixer.FindMatchingGroups("SFX").FirstOrDefault() : null;
                Set(flight, "engine", source);
            }

            // Seat at the nose end of the cabin's aisle, hatch at the hangar wall and at the cabin's exit end.
            var cabinCentre = StationGrayboxBuilder.ShuttleCabinCentre;
            var seat = new GameObject("ShuttleBoarding");
            seat.transform.SetParent(root.transform, false);
            var seatTrigger = Trigger(seat.transform, "Trigger", cabinCentre + new Vector3(0f, 1.2f, 2.6f), new Vector3(1.4f, 2.4f, 0.2f), "Take the seat");
            var boarding = seat.AddComponent<ShuttleBoarding>();
            Set(boarding, "interactable", seatTrigger);
            Set(boarding, "flight", flight);

            var inside = Point(root.transform, "Hatch_Inside", cabinCentre + new Vector3(0f, 0f, -3.1f), Quaternion.identity);
            var outside = Point(root.transform, "Hatch_Outside", HatchOutside, Quaternion.Euler(0f, 90f, 0f));
            Hatch(root.transform, "Hatch_Enter", HatchTriggerCenter, HatchTriggerSize, "Enter the shuttle", inside, flight);
            Hatch(root.transform, "Hatch_Leave", cabinCentre + new Vector3(0f, 1.2f, -3.8f), new Vector3(1.4f, 2.4f, 0.2f), "Leave the shuttle", outside, flight);

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Transform Point(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            var point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.SetPositionAndRotation(position, rotation);
            return point;
        }

        private static void Hatch(Transform parent, string name, Vector3 center, Vector3 size, string prompt, Transform destination, ShuttleFlight flight)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var hatch = go.AddComponent<ShuttleHatch>();
            Set(hatch, "interactable", Trigger(go.transform, "Trigger", center, size, prompt));
            Set(hatch, "destination", destination);
            Set(hatch, "flight", flight);
        }

        private static Interactable Trigger(Transform parent, string name, Vector3 center, Vector3 size, string prompt)
        {
            var trigger = new GameObject(name, typeof(BoxCollider));
            trigger.transform.SetParent(parent, false);
            trigger.transform.SetPositionAndRotation(center, Quaternion.identity);
            var box = trigger.GetComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            var interactable = trigger.AddComponent<Interactable>();
            var so = new SerializedObject(interactable);
            so.FindProperty("prompt").stringValue = prompt;
            so.ApplyModifiedPropertiesWithoutUndo();
            return interactable;
        }

        private static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
