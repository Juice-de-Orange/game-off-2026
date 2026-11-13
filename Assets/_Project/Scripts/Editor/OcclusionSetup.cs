using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Occlusion culling for the station (P57): of its 1,600 renderers only the current room is visible, the rest cost CPU every
    /// frame. Static geometry may be hidden, big opaque pieces may hide others. Rebuilding the station clears the old data,
    /// because stale occluders could hide things that were moved; Setup Main Lighting + Bake (or Bake Occlusion (Main)) bakes it again.
    /// </summary>
    public static class OcclusionSetup
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const float MinOccluderSize = 1f; // smaller pieces hide next to nothing and only slow the bake
        private const float SmallestOccluder = 1.5f;
        private const float SmallestHole = 0.25f;

        /// <summary>Called by the station builder after everything is placed, so doors and other moving parts are already excluded.</summary>
        public static void MarkStatic(GameObject root)
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var flags = GameObjectUtility.GetStaticEditorFlags(r.gameObject);
                if ((flags & StaticEditorFlags.BatchingStatic) == 0) continue;
                flags |= StaticEditorFlags.OccludeeStatic;
                if (IsOccluder(r)) flags |= StaticEditorFlags.OccluderStatic;
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, flags);
            }
        }

        private static bool IsOccluder(Renderer r)
        {
            foreach (var m in r.sharedMaterials)
                if (m == null || m.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.AlphaTest) return false;
            Vector3 s = r.bounds.size;
            float smallest = Mathf.Min(s.x, s.y, s.z), largest = Mathf.Max(s.x, s.y, s.z);
            float middle = s.x + s.y + s.z - smallest - largest;
            return middle >= MinOccluderSize; // wall-like: two long sides
        }

        /// <summary>Bakes the open scene. The scene must be saved afterwards.</summary>
        public static void BakeOpenScene()
        {
            StaticOcclusionCulling.smallestOccluder = SmallestOccluder;
            StaticOcclusionCulling.smallestHole = SmallestHole;
            StaticOcclusionCulling.backfaceThreshold = 100f;
            bool baked = StaticOcclusionCulling.Compute();
            Debug.Log($"[Occlusion] Bake finished: {baked}, {StaticOcclusionCulling.umbraDataSize / 1024} KB");
        }

        [MenuItem("GameOff/Bake Occlusion (Main)")]
        public static void BakeMainFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            BakeOpenScene();
            EditorSceneManager.SaveScene(scene);
        }
    }
}
