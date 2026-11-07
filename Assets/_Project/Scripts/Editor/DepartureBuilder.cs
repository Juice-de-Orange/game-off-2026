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
    /// Prefabs/Departure/PF_Departure.prefab = ticket terminal west of the shuttle ramp (model SM_Hangar_TicketTerminal or a grey
    /// box until it is delivered), boarding trigger at the ramp, ShuttleFlight. Placed in Main and wired to the player, the
    /// DayManager, the dialogs and the static hangar shuttle of the station. Part of Rebuild All; bake the light afterwards.
    /// Menu: GameOff → Build Departure into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class DepartureBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Departure/PF_Departure.prefab";
        private const string TuningPath = "Assets/_Project/Data/Departure/DepartureTuning.asset";
        private const string TerminalModelPath = "Assets/_Project/Art/Models/Hangar/SM_Hangar_TicketTerminal.fbx";
        private const string ShuttleModelPath = "Assets/_Project/Art/Models/Hangar/SM_Hangar_Shuttle.fbx";
        private const string PlaceholderMaterialPath = "Assets/_Project/Art/Materials/Station/M_Station_Door_Slide.mat";
        private const string EngineClipPath = "Assets/_Project/Audio/SFX/SFX_Shuttle_Engine_Loop.ogg";
        private const string MixerPath = "Assets/_Project/Audio/GameMixer.mixer";
        private const string HangarShuttleName = "SM_Hangar_Shuttle";

        // West of the ramp (ramp end z -23, queue lanes east at x 1..3), front facing east toward the ramp.
        private static readonly Vector3 TerminalPosition = new Vector3(-4.5f, 0f, -21f);
        private const float TerminalYaw = 90f;
        private static readonly Vector3 TerminalSize = new Vector3(0.8f, 1.8f, 0.6f);
        private static readonly Vector3 RampTriggerCenter = new Vector3(0f, 1.2f, -22.6f);
        private static readonly Vector3 RampTriggerSize = new Vector3(3f, 2.4f, 1.6f);

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
            // The station builder's KitProp root: named after the model and carrying the fitted BoxCollider (child meshes don't).
            var shuttle = all.SelectMany(g => g.GetComponentsInChildren<BoxCollider>(true))
                .Select(b => b.transform).FirstOrDefault(t => t.name == HangarShuttleName);
            var camera = player != null ? player.GetComponentInChildren<Camera>(true) : null;

            foreach (var c in instance.GetComponentsInChildren<MonoBehaviour>(true))
            {
                var so = new SerializedObject(c);
                Link(so, "day", day);
                Link(so, "player", player);
                Link(so, "dialogs", dialogs);
                Link(so, "ui", dialogs);
                Link(so, "playerCamera", camera);
                Link(so, "hangarShuttle", shuttle);
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            }
            EditorSceneManager.MarkSceneDirty(scene);

            bool model = AssetDatabase.LoadAssetAtPath<GameObject>(TerminalModelPath) != null;
            return $"[DepartureBuilder] PF_Departure in {scene.name}: tuning {(created ? "created" : "kept")}, terminal "
                + (model ? "model" : "grey box (SM_Hangar_TicketTerminal not delivered yet)")
                + $", player {Ok(player)}, camera {Ok(camera)}, day {Ok(day)}, dialogs {Ok(dialogs)}, hangar shuttle {Ok(shuttle)}"
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

            var ramp = new GameObject("ShuttleBoarding");
            ramp.transform.SetParent(root.transform, false);
            var rampTrigger = Trigger(ramp.transform, "Trigger", RampTriggerCenter, RampTriggerSize, "Board the shuttle");
            var boarding = ramp.AddComponent<ShuttleBoarding>();
            Set(boarding, "interactable", rampTrigger);
            Set(boarding, "flight", flight);

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
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
