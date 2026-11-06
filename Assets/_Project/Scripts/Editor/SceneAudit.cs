using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Prints what Main.unity costs to draw and finds broken physics (P57): renderers, shadow casters, materials, triangles,
    /// static flags, lights, and mesh colliders without a mesh, which the player would fall through.
    /// </summary>
    public static class SceneAudit
    {
        [MenuItem("GameOff/Audit Main Scene")]
        public static void FromMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Run();
        }

        /// <summary>What is still not final art in every scene that ships: graybox or placeholder materials and names (P60).</summary>
        [MenuItem("GameOff/List Placeholders")]
        public static void ListPlaceholders()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var report = new StringBuilder();
            report.AppendLine("[Placeholders]");
            foreach (string path in new[] { "Assets/_Project/Scenes/Main.unity", "Assets/_Project/Scenes/Menu.unity", "Assets/_Project/Scenes/Boot.unity" })
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                report.AppendLine(path);
                var rows = new SortedDictionary<string, int>();
                foreach (var r in scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Renderer>(true)))
                {
                    if (!r.enabled) continue;
                    var flagged = r.sharedMaterials.Where(m => m != null && (m.name.Contains("Graybox") || m.name.Contains("Placeholder"))).Select(m => m.name).Distinct().ToList();
                    bool named = r.name.Contains("Placeholder");
                    if (flagged.Count == 0 && !named) continue;
                    string group = r.transform.parent != null ? r.transform.parent.name : "-";
                    Bounds b = r.bounds;
                    string key = $"{r.transform.root.name}/{group}/{r.name} [{string.Join(",", flagged)}] size {b.size:F2} at {b.center:F1}";
                    rows.TryGetValue(key, out int n);
                    rows[key] = n + 1;
                }
                foreach (var row in rows) report.AppendLine($"  {row.Value} x {row.Key}");
            }
            System.IO.File.WriteAllText("Logs/Placeholders.txt", report.ToString());
            Debug.Log(report.ToString());
        }

        public static void Run()
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Main.unity", OpenSceneMode.Single);
            var report = new StringBuilder("[SceneAudit]\n");
            var renderers = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Renderer>(true)).ToList();
            var meshes = renderers.OfType<MeshRenderer>().ToList();
            long tris = 0;
            foreach (var r in meshes)
            {
                var filter = r.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                for (int s = 0; s < filter.sharedMesh.subMeshCount; s++) tris += (long)filter.sharedMesh.GetIndexCount(s) / 3;
            }
            report.AppendLine($"renderers {renderers.Count}, active {renderers.Count(r => r.gameObject.activeInHierarchy)}, triangles {tris}");
            report.AppendLine($"shadow casters {renderers.Count(r => r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)}");
            report.AppendLine($"materials {renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null).Distinct().Count()}");
            report.AppendLine($"meshes {meshes.Select(r => r.GetComponent<MeshFilter>()).Where(f => f != null).Select(f => f.sharedMesh).Where(m => m != null).Distinct().Count()}");
            report.AppendLine($"occluder static {renderers.Count(r => GameObjectUtility.GetStaticEditorFlags(r.gameObject).HasFlag(StaticEditorFlags.OccluderStatic))}, occludee static {renderers.Count(r => GameObjectUtility.GetStaticEditorFlags(r.gameObject).HasFlag(StaticEditorFlags.OccludeeStatic))}, batching static {renderers.Count(r => GameObjectUtility.GetStaticEditorFlags(r.gameObject).HasFlag(StaticEditorFlags.BatchingStatic))}, contribute GI {renderers.Count(r => GameObjectUtility.GetStaticEditorFlags(r.gameObject).HasFlag(StaticEditorFlags.ContributeGI))}");

            var lights = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Light>(true)).ToList();
            report.AppendLine($"lights {lights.Count}: " + string.Join(", ", lights.GroupBy(l => $"{l.lightmapBakeType}/{l.shadows}").Select(g => $"{g.Key}={g.Count()}")));
            report.AppendLine("realtime lights: " + string.Join(", ", lights.Where(l => l.lightmapBakeType != LightmapBakeType.Baked).Select(l => $"{l.name} ({l.type}, {l.shadows}, on {l.enabled && l.gameObject.activeInHierarchy})")));

            var byMaterial = renderers.SelectMany(r => r.sharedMaterials).Where(m => m != null)
                .GroupBy(m => m.shader.name).OrderByDescending(g => g.Count()).Take(6);
            report.AppendLine("shaders: " + string.Join(", ", byMaterial.Select(g => $"{g.Key}={g.Count()}")));

            var colliders = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MeshCollider>(true)).ToList();
            var empty = colliders.Where(c => c.sharedMesh == null).ToList();
            report.AppendLine($"mesh colliders {colliders.Count}, without mesh {empty.Count}: " + string.Join(", ", empty.Take(12).Select(c => c.name)));

            var biggest = new List<(string, long)>();
            foreach (var group in meshes.GroupBy(r => r.transform.root.name))
                biggest.Add((group.Key, group.Count()));
            report.AppendLine("by root: " + string.Join(", ", biggest.OrderByDescending(b => b.Item2).Take(8).Select(b => $"{b.Item1}={b.Item2}")));
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponentsInChildren<Renderer>(true).Length < 100) continue;
                for (int c = 0; c < root.transform.childCount; c++)
                {
                    var group = root.transform.GetChild(c);
                    var inGroup = group.GetComponentsInChildren<Renderer>(true);
                    if (inGroup.Length == 0) continue;
                    long groupTris = inGroup.Select(r => r.GetComponent<MeshFilter>()).Where(f => f != null && f.sharedMesh != null)
                        .Sum(f => { long n = 0; for (int s = 0; s < f.sharedMesh.subMeshCount; s++) n += f.sharedMesh.GetIndexCount(s) / 3; return n; });
                    report.AppendLine($"  {root.name}/{group.name}: {inGroup.Length} renderers, {inGroup.Count(r => r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off)} cast, {groupTris} tris");
                }
            }
            Debug.Log(report.ToString());
        }
    }
}
