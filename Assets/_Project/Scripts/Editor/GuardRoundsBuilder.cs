using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.Player;
using GameOff.Station;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P148, idempotent: Data/Station/GuardTuning.asset (created if missing, an empty line pool is filled, otherwise kept – it is
    /// content) and Prefabs/Station/PF_GuardRounds.prefab = GuardRounds with the rounds below and the arrest guard's figure
    /// (PF_NPC_Guard from Build Travelers, P104; a human stands in until it exists). Placed in Main and wired to the DayManager and the
    /// player: run after Build Day Flow. Logs whether every waypoint is on the baked NavMesh and every leg of a round can be walked
    /// (right after Build Station there is no NavMesh until the bake). Part of Rebuild All.
    /// Menu: GameOff → Build Guard Rounds into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class GuardRoundsBuilder
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const string PrefabPath = "Assets/_Project/Prefabs/Station/PF_GuardRounds.prefab";
        internal const string TuningPath = "Assets/_Project/Data/Station/GuardTuning.asset";

        /// <summary>Rooms (StationGrayboxBuilder) no round may touch: the player's room 103, the locked rooms, behind the counter, the lock.</summary>
        internal static readonly string[] ForbiddenRooms =
            { "MotelRoom101", "MotelRoom102", "MotelRoom103", "Counter", "ServiceCorridor", "InspectionLock" };

        /// <summary>
        /// The rounds, one guard each (at most two, GuardRounds.MaxGuards). rooms = the StationGrayboxBuilder rooms the round stays in;
        /// yaw = facing while he stands there (0 = north, 90 = east). Checked by GuardRoundsTests against the station's colliders, the
        /// ship pads and the baked NavMesh.
        /// Hall: the motel corridor between 102 and 103 (clear of the doors' triggers and of the hab tube door), the west side of the
        /// hall, in front of the arrivals door (but 3.5 m from where the arrest guard comes in, P104), and between benches A and B
        /// looking out to the deck. All inside the cleaning bot's loop and clear of the evening guests' spots; no door on the way.
        /// Hangar (there is no gallery, so the hangar floor): by the counter window watching the queue lane, the middle of the floor,
        /// at the energy barrier, and between the east pads; at least 2.5 m from every ship pad's ship. The hangar is reached only
        /// through staff and lock doors, so this guard never leaves it.
        /// </summary>
        internal static readonly (string name, string[] rooms, (Vector3 position, float yaw)[] points)[] Routes =
        {
            ("Hall", new[] { "Hall", "MotelCorridor" }, new[]
            {
                (new Vector3(-15f, 0f, 1f), 270f),     // motel corridor, between 102 and 103: down the corridor
                (new Vector3(-4.8f, 0f, -0.6f), 220f), // hall west: the staff door and the diner
                (new Vector3(4.4f, 0f, -2.6f), 165f),  // hall south-east: the kiosk and the arrivals door
                (new Vector3(0f, 0f, 3.2f), 0f),       // between benches A and B: out to the deck and the planet
            }),
            ("Hangar", new[] { "Hangar" }, new[]
            {
                (new Vector3(-3f, 0f, -15f), 90f),     // under the counter window: the queue lane
                (new Vector3(-2.5f, 0f, -28f), 270f),  // middle of the floor: the west pads and the docked shuttle
                (new Vector3(2f, 0f, -36f), 180f),     // at the energy barrier: space
                (new Vector3(3.5f, 0f, -29f), 90f),    // between the east pads
            }),
        };

        [MenuItem("GameOff/Build Guard Rounds into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        public static string Build(Scene scene)
        {
            var tuning = EnsureTuning(out string tuningNote);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + "PF_NPC_Guard.prefab");
            bool standIn = model == null;
            if (standIn) model = AssetDatabase.LoadAssetAtPath<GameObject>(TravelerPrefabBuilder.PrefabFolder + "PF_NPC_Human.prefab");
            var prefab = BuildPrefab(tuning, model);

            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var all = scene.GetRootGameObjects();
            var player = all.SelectMany(g => g.GetComponentsInChildren<FirstPersonController>(true)).FirstOrDefault();
            var day = all.SelectMany(g => g.GetComponentsInChildren<DayManager>(true)).FirstOrDefault();
            var rounds = instance.GetComponent<GuardRounds>();
            var link = new SerializedObject(rounds);
            link.FindProperty("day").objectReferenceValue = day;
            link.FindProperty("player").objectReferenceValue = player;
            link.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(rounds);
            EditorSceneManager.MarkSceneDirty(scene);

            return $"[GuardRoundsBuilder] PF_GuardRounds in {scene.name}: {Routes.Length} rounds, tuning {tuningNote}, figure "
                + (model == null ? "MISSING (run GameOff/Build Travelers)" : standIn ? "PF_NPC_Human (PF_NPC_Guard not built yet)" : model.name)
                + $", day {Ok(day)}, player {Ok(player)}; {CheckNavMesh()}";
        }

        private static string Ok(Object o) => o != null ? "ok" : "MISSING";

        private static GameObject BuildPrefab(GuardTuning tuning, GameObject model)
        {
            var root = new GameObject("PF_GuardRounds");
            var rounds = root.AddComponent<GuardRounds>();
            var so = new SerializedObject(rounds);
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.FindProperty("guardModel").objectReferenceValue = model;
            var list = so.FindProperty("routes");
            list.arraySize = Routes.Length;
            for (int i = 0; i < Routes.Length; i++)
            {
                var (name, _, points) = Routes[i];
                var route = list.GetArrayElementAtIndex(i);
                route.FindPropertyRelative("name").stringValue = name;
                var positions = route.FindPropertyRelative("points");
                var yaws = route.FindPropertyRelative("yaws");
                positions.arraySize = yaws.arraySize = points.Length;
                for (int p = 0; p < points.Length; p++)
                {
                    positions.GetArrayElementAtIndex(p).vector3Value = points[p].position;
                    yaws.GetArrayElementAtIndex(p).floatValue = points[p].yaw;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetFolders.EnsureForAsset(PrefabPath);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>Created with the default lines if missing; an empty line pool is filled again; everything else is kept.</summary>
        internal static GuardTuning EnsureTuning(out string note)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<GuardTuning>(TuningPath);
            if (tuning == null)
            {
                AssetFolders.EnsureForAsset(TuningPath);
                tuning = ScriptableObject.CreateInstance<GuardTuning>();
                AssetDatabase.CreateAsset(tuning, TuningPath);
                AssetDatabase.SaveAssets();
                note = "created";
                return tuning;
            }
            note = "kept";
            if (tuning.lines == null || tuning.lines.Length == 0)
            {
                var defaults = ScriptableObject.CreateInstance<GuardTuning>();
                tuning.lines = defaults.lines;
                Object.DestroyImmediate(defaults);
                EditorUtility.SetDirty(tuning);
                AssetDatabase.SaveAssets();
                note = "kept, empty line pool filled";
            }
            return tuning;
        }

        /// <summary>Every waypoint on the NavMesh of the open scene, every leg of a round walkable (doors count as open here).</summary>
        private static string CheckNavMesh()
        {
            var path = new NavMeshPath();
            int on = 0, off = 0, broken = 0;
            var lengths = new List<string>();
            foreach (var (name, _, points) in Routes)
            {
                float length = 0f;
                for (int i = 0; i < points.Length; i++)
                {
                    var from = points[i].position;
                    var to = points[(i + 1) % points.Length].position;
                    if (NavMesh.SamplePosition(from, out _, 0.3f, NavMesh.AllAreas)) on++;
                    else
                    {
                        off++;
                        Debug.LogWarning($"[GuardRoundsBuilder] {name} waypoint {i} {from} is not on the NavMesh");
                    }
                    if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                        for (int c = 1; c < path.corners.Length; c++) length += Vector3.Distance(path.corners[c - 1], path.corners[c]);
                    else broken++;
                }
                lengths.Add($"{name} {length:0} m");
            }
            if (on == 0) return "NavMesh not baked yet: bake (GameOff/Setup Main Lighting + Bake), then run the EditMode tests (GuardRoundsTests)";
            if (off > 0 || broken > 0) Debug.LogWarning($"[GuardRoundsBuilder] {off} waypoint(s) off the NavMesh, {broken} leg(s) not walkable");
            return $"NavMesh: {on} waypoints on it, {off} off, {broken} legs not walkable; rounds {string.Join(", ", lengths)}";
        }
    }
}
