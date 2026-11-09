using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Proves that an optimization changes nothing the player can see (P57): renders each view from every walkable spot of the
    /// station twice, with and without it, doors out of the way, and reports views that differ. Saves nothing in the scene.
    /// The worst view lands in Logs/OcclusionCheck_on.png and _off.png.
    /// </summary>
    public static class OcclusionCheck
    {
        private const string MainScene = "Assets/_Project/Scenes/Main.unity";
        private const float GridStep = 1.5f; // every second spot gets checked
        private const float EyeHeight = 1.6f;
        private const int Width = 256, Height = 144;
        private const int ChannelTolerance = 8; // of 255
        private const int PixelTolerance = 12;

        [MenuItem("GameOff/Check Occlusion Bake")]
        public static void FromMenu()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) Run();
        }

        /// <summary>Occlusion on and off, sun included. Run it after a bake.</summary>
        public static void Run() => Compare("occlusion", true, null);

        /// <summary>Geometry alone: the sun's realtime shadows change with culled casters, so they get their own question.</summary>
        public static void RunWithoutSun() => Compare("occlusion without sun", false, null);

        /// <summary>Beyond the shadow distance the sun shines through every wall, so this one can light up long rooms.</summary>
        public static void CheckShadowDistance()
        {
            UseWebQuality();
            var asset = (UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            float current = asset.shadowDistance;
            Compare($"shadow distance {current} against 100", true, far => asset.shadowDistance = far ? 100f : current);
            asset.shadowDistance = current;
        }

        /// <summary>The editor may stand on the PC level, which the web build doesn't have.</summary>
        private static void UseWebQuality()
        {
            int level = Array.IndexOf(QualitySettings.names, "Mobile");
            if (level >= 0) QualitySettings.SetQualityLevel(level, true);
        }

        private static void Compare(string title, bool sun, Action<bool> variant)
        {
            UseWebQuality();
            var scene = EditorSceneManager.OpenScene(MainScene, OpenSceneMode.Single);
            if (!sun)
                foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                    if (l.type == LightType.Directional && l.lightmapBakeType != LightmapBakeType.Baked) l.enabled = false;
            foreach (var door in Object.FindObjectsByType<Station.SlidingDoor>(FindObjectsSortMode.None))
            {
                foreach (var r in door.GetComponentsInChildren<Renderer>(true))
                    r.enabled = false;
                foreach (var c in door.GetComponentsInChildren<Collider>(true))
                    c.enabled = false;
            }
            // Force fields and screens animate with the clock, so two renders of them differ even when nothing is hidden.
            // Without them the check also looks straight through at whatever stands behind.
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                foreach (var m in r.sharedMaterials)
                    if (m != null && m.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent)
                        r.enabled = false;
            Physics.SyncTransforms();

            var go = new GameObject("OcclusionCheckCamera") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.farClipPlane = 5000f;
            cam.clearFlags = CameraClearFlags.Skybox;
            var target = new RenderTexture(Width, Height, 24);
            var on = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            var off = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            var spots = WalkableSpots();
            int views = 0, bad = 0, worst = 0;
            var report = new StringBuilder();
            foreach (var spot in spots)
                for (int yaw = 0; yaw < 360; yaw += 45)
                    foreach (float pitch in new[] { 0f, 35f, -35f })
                    {
                        go.transform.SetPositionAndRotation(spot, Quaternion.Euler(pitch, yaw, 0f));
                        variant?.Invoke(true);
                        Capture(cam, target, on, true);
                        variant?.Invoke(false);
                        Capture(cam, target, off, variant != null); // without a variant the difference is occlusion itself
                        int diff = CountDifferent(on, off);
                        views++;
                        if (diff <= PixelTolerance) continue;
                        if (++bad <= 30) report.AppendLine($"  {spot} yaw {yaw} pitch {pitch}: {diff} px");
                        if (diff > worst)
                        {
                            worst = diff;
                            System.IO.File.WriteAllBytes("Logs/OcclusionCheck_on.png", on.EncodeToPNG());
                            System.IO.File.WriteAllBytes("Logs/OcclusionCheck_off.png", off.EncodeToPNG());
                        }
                    }

            Debug.Log($"[OcclusionCheck] {title}: {spots.Count} spots, {views} views, {bad} differ (worst {worst} of {Width * Height} px)\n{report}");
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(on);
            Object.DestroyImmediate(off);
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single); // brings the doors back
            if (Application.isBatchMode && (bad > 0 || spots.Count == 0)) EditorApplication.Exit(1);
        }

        private static List<Vector3> WalkableSpots()
        {
            var spots = new List<Vector3>();
            // Flood fill from the player: spots outside the hull or inside a wall would report views nobody ever gets.
            var start = Object.FindFirstObjectByType<GameOff.Player.FirstPersonController>().transform.position;
            if (!Physics.Raycast(start + Vector3.up, Vector3.down, out var ground, 3f, ~0, QueryTriggerInteraction.Ignore)) return spots;
            var open = new Queue<(Vector2Int cell, Vector3 eye)>();
            var seen = new HashSet<Vector2Int> { Vector2Int.zero };
            open.Enqueue((Vector2Int.zero, ground.point + Vector3.up * EyeHeight));
            var neighbours = new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
            while (open.Count > 0)
            {
                var (cell, eye) = open.Dequeue();
                if (cell.x % 2 == 0 && cell.y % 2 == 0) spots.Add(eye);
                foreach (var n in neighbours)
                {
                    var next = cell + n;
                    if (seen.Contains(next)) continue;
                    var wanted = eye + new Vector3(n.x, 0f, n.y) * GridStep;
                    if (Physics.Linecast(eye, wanted, ~0, QueryTriggerInteraction.Ignore) || Physics.CheckSphere(wanted, 0.2f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (!Physics.Raycast(wanted, Vector3.down, out var floor, EyeHeight + 0.5f, ~0, QueryTriggerInteraction.Ignore)) continue;
                    if (floor.point.y + EyeHeight - eye.y > 0.4f) continue; // a step up onto furniture, nobody walks there
                    seen.Add(next);
                    open.Enqueue((next, floor.point + Vector3.up * EyeHeight));
                }
            }
            return spots;
        }

        private static void Capture(Camera cam, RenderTexture target, Texture2D into, bool occlusion)
        {
            cam.useOcclusionCulling = occlusion;
            cam.targetTexture = target;
            cam.Render();
            RenderTexture.active = target;
            into.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            RenderTexture.active = null;
            cam.targetTexture = null;
        }

        private static int CountDifferent(Texture2D a, Texture2D b)
        {
            var pa = a.GetPixels32();
            var pb = b.GetPixels32();
            int count = 0;
            for (int i = 0; i < pa.Length; i++)
                if (Mathf.Abs(pa[i].r - pb[i].r) > ChannelTolerance || Mathf.Abs(pa[i].g - pb[i].g) > ChannelTolerance || Mathf.Abs(pa[i].b - pb[i].b) > ChannelTolerance)
                    count++;
            return count;
        }
    }
}
