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
            Debug.Log(report.ToString());
        }
    }
}
