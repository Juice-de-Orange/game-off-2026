using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameOff.Core;
using GameOff.Player;
using GameOff.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Ship traffic in the game (P55): ShipTraffic (P47) with fog-free ship copies (shader GameOff/ShipNoFog) in front of the two big north windows – the glass front of the
    /// observation platform (centre x 0, z 6) and the bar window (z 8, x 10..18). Lanes run east-west north of the station,
    /// in front of the home planet (≈ z 550, r ≈ 200), high enough (~32° up) to pass above the exterior trusses (up to y 26 at z ≤ 76) that
    /// fill the lower view from the platform – ships cross the upper half of the glass front. Tuning Data/Traffic/ShipTraffic_Main.asset is created only if missing –
    /// change it in the inspector. Logs the clearance of every lane to station + exterior.
    /// Menu: GameOff → Build Traffic into Main (opens and saves Main – claim it first).
    /// </summary>
    public static class TrafficBuilder
    {
        private const string TuningPath = "Assets/_Project/Data/Traffic/ShipTraffic_Main.asset";
        private const string PrefabPath = "Assets/_Project/Prefabs/Environment/PF_Traffic.prefab";
        private const string ShipFolder = "Assets/_Project/Art/Models/Ships/";
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const float MinClearance = 15f;
        private const string ShipMaterialFolder = "Assets/_Project/Art/Materials/Ships/";
        private const string NoFogMaterialFolder = "Assets/_Project/Art/Materials/Ships/NoFog/";
        private const string NoFogShipFolder = "Assets/_Project/Prefabs/Environment/Ships/";
        private const string NoFogShader = "Assets/_Project/Art/Shaders/ShipNoFog.shader";
        private const string DevLineShader = "Assets/_Project/Art/Shaders/DevLine.shader";
        private const string DevLineMaterialPath = "Assets/_Project/Art/Materials/Debug/M_DevLine.mat";
        private static readonly string[] ShipNames = { "SM_Ship_Freighter", "SM_Ship_Tanker", "SM_Ship_Patrol", "SM_Ship_TrafficSmall" };

        [MenuItem("GameOff/Build Traffic into Main")]
        public static void BuildIntoMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            Debug.Log(Build(scene));
            EditorSceneManager.SaveScene(scene);
        }

        /// <summary>Applies lane values copied by the dev tool in a build (SAVE LANE / COPY ALL, P56) to the tuning asset.</summary>
        [MenuItem("GameOff/Paste Traffic Lanes")]
        public static void PasteLanesFromMenu()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<ShipTrafficTuning>(TuningPath);
            if (tuning == null) { Debug.LogWarning("[TrafficBuilder] " + TuningPath + " missing - run Build Traffic into Main"); return; }
            Undo.RecordObject(tuning, "Paste Traffic Lanes");
            int changed = TrafficLaneValues.Apply(GUIUtility.systemCopyBuffer, tuning.lanes);
            if (changed == 0) { Debug.LogWarning("[TrafficBuilder] clipboard holds no lane values (SAVE LANE or COPY ALL in the dev tool)"); return; }
            EditorUtility.SetDirty(tuning);
            AssetDatabase.SaveAssets();
            Debug.Log($"[TrafficBuilder] {changed} lane(s) pasted into {tuning.name}");
        }

        public static string Build(Scene scene)
        {
            var log = new List<string>();
            var ships = BuildNoFogShips(log);
            var tuning = Tuning(ships, log);

            Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
            var root = new GameObject("PF_Traffic");
            var traffic = root.AddComponent<ShipTraffic>();
            var so = new SerializedObject(traffic);
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.ApplyModifiedPropertiesWithoutUndo();
            var dev = new SerializedObject(root.AddComponent<TrafficLaneDevTool>());
            dev.FindProperty("traffic").objectReferenceValue = traffic;
            dev.FindProperty("lineMaterial").objectReferenceValue = DevLineMaterial();
            dev.FindProperty("minClearance").floatValue = MinClearance;
            if (StationBounds(scene, out var bounds)) dev.FindProperty("stationBounds").boundsValue = bounds;
            dev.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            foreach (var old in scene.GetRootGameObjects().Where(g => g.name == prefab.name)) Object.DestroyImmediate(old);
            var placed = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var player = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<FirstPersonController>(true)).FirstOrDefault();
            var devTool = new SerializedObject(placed.GetComponent<TrafficLaneDevTool>());
            devTool.FindProperty("player").objectReferenceValue = player;
            devTool.ApplyModifiedPropertiesWithoutUndo();
            if (player == null) log.Add("no player in the scene - dev tool cannot free the mouse");
            EditorSceneManager.MarkSceneDirty(scene);
            log.Add("PF_Traffic placed in " + scene.name);
            log.Add(Clearance(scene, tuning));
            return "[TrafficBuilder] " + string.Join("; ", log);
        }

        /// <summary>
        /// Fog-free copies of the ships (Main's indoor fog hides them otherwise): M_Ship_*_NoFog materials with shader
        /// GameOff/ShipNoFog, copying textures and colours from Max' M_Ship_* every build, and ship prefabs using them.
        /// </summary>
        private static Dictionary<string, GameObject> BuildNoFogShips(List<string> log)
        {
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(NoFogShader);
            Directory.CreateDirectory(NoFogMaterialFolder);
            Directory.CreateDirectory(NoFogShipFolder);
            var result = new Dictionary<string, GameObject>();
            foreach (var name in ShipNames)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(ShipFolder + name + ".fbx");
                if (model == null) continue;
                var source = AssetDatabase.LoadAssetAtPath<Material>(ShipMaterialFolder + name.Replace("SM_", "M_") + ".mat");
                string matPath = NoFogMaterialFolder + name.Replace("SM_", "M_") + "_NoFog.mat";
                var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat == null)
                {
                    mat = new Material(shader);
                    AssetDatabase.CreateAsset(mat, matPath);
                }
                mat.shader = shader;
                if (source != null)
                {
                    foreach (var prop in new[] { "_BaseMap", "_EmissionMap" })
                        if (source.HasProperty(prop)) mat.SetTexture(prop, source.GetTexture(prop));
                    foreach (var prop in new[] { "_BaseColor", "_EmissionColor" })
                        if (source.HasProperty(prop)) mat.SetColor(prop, source.GetColor(prop));
                }
                EditorUtility.SetDirty(mat);

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
                {
                    var mats = r.sharedMaterials;
                    for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                    r.sharedMaterials = mats;
                }
                result[name] = PrefabUtility.SaveAsPrefabAsset(instance, NoFogShipFolder + "PF_" + name.Replace("SM_", "") + "_NoFog.prefab");
                Object.DestroyImmediate(instance);
            }
            AssetDatabase.SaveAssets();
            log.Add($"{result.Count} fog-free ships");
            return result;
        }

        private static ShipTrafficTuning Tuning(Dictionary<string, GameObject> ships, List<string> log)
        {
            var tuning = AssetDatabase.LoadAssetAtPath<ShipTrafficTuning>(TuningPath);
            if (tuning != null)
            {
                // Keep the tuned lanes, only point raw FBX ships to their fog-free copies.
                int swapped = 0;
                foreach (var lane in tuning.lanes)
                    for (int i = 0; i < lane.ships.Count; i++)
                        if (lane.ships[i] != null && ships.TryGetValue(lane.ships[i].name, out var noFog)) { lane.ships[i] = noFog; swapped++; }
                if (swapped > 0) log.Add($"{swapped} ships switched to fog-free");
                NameLanes(tuning);
                EditorUtility.SetDirty(tuning);
                AssetDatabase.SaveAssets();
                return tuning;
            }
            GameObject Ship(string name) => ships.TryGetValue(name, out var s) ? s : AssetDatabase.LoadAssetAtPath<GameObject>(ShipFolder + name + ".fbx");
            tuning = ScriptableObject.CreateInstance<ShipTrafficTuning>();
            tuning.maxActive = 4;
            tuning.modelYaw = 180f; // the ship FBX noses point to -Z
            // Same rhythm as the menu (P47), lanes across the view north of both windows.
            tuning.lanes.Add(new TrafficLane
            {
                name = "Cargo", from = new Vector3(-450f, 190f, 300f), to = new Vector3(450f, 170f, 300f),
                randomOffset = new Vector3(0f, 15f, 20f), ships = { Ship("SM_Ship_Freighter"), Ship("SM_Ship_Tanker") },
                scale = 1f, speed = new Vector2(12f, 20f), interval = new Vector2(20f, 40f), firstDelay = new Vector2(0f, 3f),
            });
            tuning.lanes.Add(new TrafficLane
            {
                name = "Patrol", from = new Vector3(-320f, 105f, 160f), to = new Vector3(320f, 95f, 160f),
                randomOffset = new Vector3(0f, 8f, 15f), ships = { Ship("SM_Ship_Patrol") },
                scale = 1.6f, speed = new Vector2(35f, 50f), interval = new Vector2(30f, 60f), firstDelay = new Vector2(8f, 15f),
            });
            tuning.lanes.Add(new TrafficLane
            {
                name = "Civilian", from = new Vector3(-220f, 68f, 108f), to = new Vector3(220f, 60f, 100f),
                randomOffset = new Vector3(0f, 5f, 6f), ships = { Ship("SM_Ship_TrafficSmall") },
                scale = 2.5f, speed = new Vector2(18f, 26f), interval = new Vector2(10f, 20f), firstDelay = new Vector2(3f, 8f),
            });
            NameLanes(tuning);
            Directory.CreateDirectory(Path.GetDirectoryName(TuningPath));
            AssetDatabase.CreateAsset(tuning, TuningPath);
            AssetDatabase.SaveAssets();
            log.Add("ShipTraffic_Main created");
            return tuning;
        }

        /// <summary>Lane names and dev-tool colours (P56) by the ships they carry: Cargo orange, Civilian cyan, Patrol blue.</summary>
        private static void NameLanes(ShipTrafficTuning tuning)
        {
            foreach (var lane in tuning.lanes)
            {
                bool Has(string ship) => lane.ships.Any(s => s != null && s.name.Contains(ship));
                if (Has("Freighter") || Has("Tanker")) { lane.name = "Cargo"; lane.devColor = new Color(1f, 0.6f, 0.15f); }
                else if (Has("Patrol")) { lane.name = "Patrol"; lane.devColor = new Color(0.3f, 0.55f, 1f); }
                else if (Has("TrafficSmall")) { lane.name = "Civilian"; lane.devColor = new Color(0.2f, 0.9f, 0.95f); }
            }
        }

        /// <summary>Material for the dev tool's lines (shader GameOff/DevLine, created once).</summary>
        private static Material DevLineMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(DevLineMaterialPath);
            if (mat != null) return mat;
            Directory.CreateDirectory(Path.GetDirectoryName(DevLineMaterialPath));
            mat = new Material(AssetDatabase.LoadAssetAtPath<Shader>(DevLineShader));
            AssetDatabase.CreateAsset(mat, DevLineMaterialPath);
            return mat;
        }

        private static bool StationBounds(Scene scene, out Bounds bounds)
        {
            bounds = new Bounds();
            bool any = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                if (!(root.name.Contains("Station") || root.name.Contains("Exterior"))) continue;
                foreach (var r in root.GetComponentsInChildren<Renderer>())
                {
                    if (!any) { bounds = r.bounds; any = true; }
                    else bounds.Encapsulate(r.bounds);
                }
            }
            return any;
        }

        /// <summary>Smallest distance of every lane (incl. its random spread) to the station and the exterior.</summary>
        private static string Clearance(Scene scene, ShipTrafficTuning tuning)
        {
            if (!StationBounds(scene, out var bounds)) return "clearance: station/exterior not found";
            var parts = new List<string>();
            foreach (var lane in tuning.lanes)
            {
                float min = float.MaxValue;
                for (int i = 0; i <= 100; i++) min = Mathf.Min(min, Mathf.Sqrt(bounds.SqrDistance(Vector3.Lerp(lane.from, lane.to, i / 100f))));
                float spread = Mathf.Max(lane.randomOffset.y, lane.randomOffset.z);
                string flag = min - spread < MinClearance ? " TOO CLOSE" : "";
                parts.Add($"{lane.name} {min:0} m (spread {spread:0}){flag}");
                if (flag.Length > 0) Debug.LogWarning($"[TrafficBuilder] {lane.name} only {min - spread:0} m from the station");
            }
            return "clearance: " + string.Join(", ", parts);
        }
    }
}
