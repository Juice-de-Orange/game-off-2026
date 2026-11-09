using System.Collections.Generic;
using System.Linq;
using GameOff.Shops;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static GameOff.EditorTools.MenuSceneBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// P111: the home street diorama the coin telescope jumps to at full zoom. Built by EconomyBuilder.BuildTelescope into
    /// PF_Economy (child of CoinTelescope), placed far below the station so nothing else ever sees it, switched off except while the
    /// telescope shows it. Unlit materials, no shadows, not static: it has no part in the light or occlusion bake.
    ///
    /// Model swap: if Art/Models/Home/SM_Home_Street.fbx exists it is used (rebuild with GameOff/Build Economy into Main after the
    /// delivery). Its "Windows" material slot is found, its triangles are split into three windows from left to right as seen from
    /// the street (Mom, Lena, Pip), and a glowing quad is laid just in front of each; the eye looks along the windows' normal.
    /// Without the model (or if its windows can't be found) the placeholder from the asset entry is built: three grey boxes
    /// 3 x 5 x 6 m on a plate, the middle one with three window quads.
    /// </summary>
    public static class HomeDioramaBuilder
    {
        private const string ModelPath = "Assets/_Project/Art/Models/Home/SM_Home_Street.fbx";
        private const string AlbedoPath = "Assets/_Project/Art/Textures/Home/T_Home_Street_Albedo.png";
        private const string MatFolder = "Assets/_Project/Art/Materials/Home/";
        private const string TuningPath = "Assets/_Project/Data/Economy/HomeViewTuning.asset";

        /// <summary>World position of the diorama: far below the station, out of every other view.</summary>
        private static readonly Vector3 Origin = new Vector3(0f, -400f, 0f);
        // The view comes from the front, high up like a telescope from orbit would see it.
        private const float EyeDistance = 24f;
        private const float EyeElevation = 35f;
        private static readonly Vector2 WindowSize = new Vector2(0.7f, 1f);

        public readonly struct Result
        {
            public readonly GameObject Root;
            public readonly Transform Eye;
            public readonly string Log;

            public Result(GameObject root, Transform eye, string log) { Root = root; Eye = eye; Log = log; }
        }

        internal static HomeViewTuning EnsureTuning()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<HomeViewTuning>(TuningPath);
            if (tuning != null) return tuning;
            AssetFolders.EnsureForAsset(TuningPath);
            tuning = ScriptableObject.CreateInstance<HomeViewTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            return tuning;
        }

        public static Result Build(Transform parent, HomeViewTuning tuning)
        {
            var root = new GameObject("HomeDiorama");
            root.transform.SetParent(parent, false);
            root.transform.position = Origin;

            var street = UnlitMaterial("M_Home_Street", new Color(0.16f, 0.17f, 0.21f), AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath));
            var window = UnlitMaterial("M_Home_Window", tuning.darkColor, null);
            var ground = UnlitMaterial("M_Home_Ground", new Color(0.03f, 0.035f, 0.05f), null);

            // Dark ground far around it, so the view never shows the sky below the station.
            var backdrop = Primitive(PrimitiveType.Quad, "Backdrop", root.transform, ground);
            backdrop.transform.localPosition = new Vector3(0f, -0.02f, 0f);
            backdrop.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            backdrop.transform.localScale = new Vector3(160f, 160f, 1f);

            string log;
            List<(Vector3 centre, Vector2 size)> spots = null;
            Vector3 normal = Vector3.back;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (model != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                instance.name = "SM_Home_Street";
                instance.transform.SetParent(root.transform, false);
                foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
                {
                    Quiet(r);
                    var slots = r.sharedMaterials;
                    for (int i = 0; i < slots.Length; i++) slots[i] = IsWindowSlot(slots[i]) ? window : street;
                    r.sharedMaterials = slots;
                }
                foreach (var c in instance.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);
                spots = FindWindows(instance, model, out normal);
                log = spots != null ? "home street: model" : "home street: model, but its 3 'Windows' not found - windows placed like the placeholder";
            }
            else
            {
                BuildPlaceholder(root.transform, street);
                log = "home street: placeholder (SM_Home_Street.fbx not delivered)";
            }

            if (spots == null)
            {
                normal = Vector3.back;
                spots = new List<(Vector3, Vector2)>();
                foreach (float x in new[] { -1f, 0f, 1f }) spots.Add((root.transform.TransformPoint(new Vector3(x, 3.3f, -2f)), WindowSize));
            }

            var windows = new GameObject("Windows").transform;
            windows.SetParent(root.transform, false);
            var renderers = new Renderer[spots.Count];
            string[] names = { "Window_Mom", "Window_Lena", "Window_Pip" };
            for (int i = 0; i < spots.Count; i++)
            {
                var quad = Primitive(PrimitiveType.Quad, i < names.Length ? names[i] : "Window_" + i, windows, window);
                quad.transform.SetPositionAndRotation(spots[i].centre + normal * 0.03f, Quaternion.LookRotation(-normal));
                quad.transform.localScale = new Vector3(spots[i].size.x, spots[i].size.y, 1f);
                renderers[i] = quad.GetComponent<Renderer>();
            }

            Vector3 target = spots.Aggregate(Vector3.zero, (sum, s) => sum + s.centre) / spots.Count;
            Vector3 flat = new Vector3(normal.x, 0f, normal.z).sqrMagnitude > 0.01f ? new Vector3(normal.x, 0f, normal.z).normalized : Vector3.back;
            var eye = new GameObject("Eye").transform;
            eye.SetParent(root.transform, false);
            float up = EyeElevation * Mathf.Deg2Rad;
            eye.position = target + (flat * Mathf.Cos(up) + Vector3.up * Mathf.Sin(up)) * EyeDistance;
            eye.rotation = Quaternion.LookRotation(target - eye.position);

            var lights = root.AddComponent<HomeWindows>();
            var so = new SerializedObject(lights);
            var list = so.FindProperty("windows");
            list.arraySize = renderers.Length;
            for (int i = 0; i < renderers.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = renderers[i];
            so.FindProperty("tuning").objectReferenceValue = tuning;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
            return new Result(root, eye, log);
        }

        /// <summary>From the asset entry: three grey boxes 3 x 5 x 6 m side by side on a flat plate, pivot bottom centre, front to -z.</summary>
        private static void BuildPlaceholder(Transform root, Material street)
        {
            var plate = Primitive(PrimitiveType.Cube, "Plate", root, street);
            plate.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            plate.transform.localScale = new Vector3(12f, 0.1f, 8f);
            foreach (var (name, x) in new[] { ("House_Left", -3.8f), ("House_Family", 0f), ("House_Right", 3.8f) })
            {
                var house = Primitive(PrimitiveType.Cube, name, root, street);
                house.transform.localPosition = new Vector3(x, 2.5f, 1f);
                house.transform.localScale = new Vector3(3f, 5f, 6f);
            }
        }

        /// <summary>
        /// The three windows of the delivered model: the triangles of its "Windows" slot, split at the two widest gaps from left to
        /// right as seen from the front. Null if there is no such slot or it doesn't split into three.
        /// </summary>
        private static List<(Vector3 centre, Vector2 size)> FindWindows(GameObject instance, GameObject model, out Vector3 normal)
        {
            normal = Vector3.back;
            var points = new List<Vector3>();
            Vector3 normalSum = Vector3.zero;
            foreach (var filter in instance.GetComponentsInChildren<MeshFilter>(true))
            {
                var r = filter.GetComponent<Renderer>();
                var mesh = filter.sharedMesh;
                if (r == null || mesh == null) continue;
                // The slot names are on the model's own materials; the instance already wears the diorama's.
                var original = FindOriginal(model, instance, filter);
                var slots = original != null ? original.sharedMaterials : r.sharedMaterials;
                var vertices = mesh.vertices;
                for (int s = 0; s < mesh.subMeshCount && s < slots.Length; s++)
                {
                    if (!IsWindowSlot(slots[s])) continue;
                    var tris = mesh.GetTriangles(s);
                    for (int t = 0; t + 2 < tris.Length; t += 3)
                    {
                        Vector3 a = filter.transform.TransformPoint(vertices[tris[t]]);
                        Vector3 b = filter.transform.TransformPoint(vertices[tris[t + 1]]);
                        Vector3 c = filter.transform.TransformPoint(vertices[tris[t + 2]]);
                        normalSum += Vector3.Cross(b - a, c - a); // area-weighted, Unity's clockwise winding faces this way
                        points.Add(a); points.Add(b); points.Add(c);
                    }
                }
            }
            if (points.Count < 9 || normalSum.sqrMagnitude < 1e-6f) return null;
            normal = normalSum.normalized;
            Vector3 right = Vector3.Cross(Vector3.up, -normal).normalized; // the camera looks along -normal
            if (right.sqrMagnitude < 0.5f) return null;

            var along = points.Select(p => Vector3.Dot(p, right)).OrderBy(v => v).ToList();
            var gaps = new List<(float gap, float at)>();
            for (int i = 1; i < along.Count; i++) gaps.Add((along[i] - along[i - 1], (along[i] + along[i - 1]) * 0.5f));
            var cuts = gaps.OrderByDescending(g => g.gap).Take(2).Select(g => g.at).OrderBy(v => v).ToArray();
            if (cuts.Length < 2 || gaps.OrderByDescending(g => g.gap).ElementAt(1).gap < 0.05f) return null;

            var spots = new List<(Vector3, Vector2)>();
            for (int w = 0; w < 3; w++)
            {
                float lo = w == 0 ? float.MinValue : cuts[w - 1], hi = w == 2 ? float.MaxValue : cuts[w];
                var group = points.Where(p => { float d = Vector3.Dot(p, right); return d > lo && d < hi; }).ToList();
                if (group.Count == 0) return null;
                var bounds = new Bounds(group[0], Vector3.zero);
                foreach (var p in group) bounds.Encapsulate(p);
                float width = group.Max(p => Vector3.Dot(p, right)) - group.Min(p => Vector3.Dot(p, right));
                float height = group.Max(p => p.y) - group.Min(p => p.y);
                spots.Add((bounds.center, new Vector2(Mathf.Max(0.2f, width), Mathf.Max(0.2f, height))));
            }
            return spots;
        }

        /// <summary>The renderer in the model asset that matches the instance's (same path in the hierarchy).</summary>
        private static Renderer FindOriginal(GameObject model, GameObject instance, Component part)
        {
            string path = AnimationUtility.CalculateTransformPath(part.transform, instance.transform);
            var t = path.Length == 0 ? model.transform : model.transform.Find(path);
            return t != null ? t.GetComponent<Renderer>() : null;
        }

        private static bool IsWindowSlot(Material m) => m != null && m.name.ToLowerInvariant().Contains("window");

        private static GameObject Primitive(PrimitiveType type, string name, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            var r = go.GetComponent<Renderer>();
            r.sharedMaterial = material;
            Quiet(r);
            return go;
        }

        // Unlit and far away: no shadows, no probes, nothing for the bake.
        private static void Quiet(Renderer r)
        {
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.lightProbeUsage = LightProbeUsage.Off;
            r.reflectionProbeUsage = ReflectionProbeUsage.Off;
            GameObjectUtility.SetStaticEditorFlags(r.gameObject, 0);
        }

        private static Material UnlitMaterial(string name, Color color, Texture2D albedo)
        {
            string path = MatFolder + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                AssetFolders.EnsureForAsset(path);
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetTexture("_BaseMap", albedo);
            mat.SetColor("_BaseColor", albedo != null ? Color.white : color);
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
