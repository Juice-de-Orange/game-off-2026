using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Scene-wide lighting for the open scene (meant for Main.unity): space skybox, dark ambient, fog, a cold
    /// "planet light" through the panorama window, low-res baked lightmaps (fits the retro look), then bake.
    /// Re-run after rebuilding the station graybox. Values documented in docs/STATION.md.
    /// </summary>
    public static class MainLightingSetup
    {
        private const string SkyTexturePath = "Assets/_Project/Art/Textures/Sky/T_Sky_Space.png";
        private const string SkyMaterialPath = "Assets/_Project/Art/Materials/Station/M_Sky_Space.mat";
        private const string LightingSettingsPath = "Assets/_Project/Scenes/Main_LightingSettings.lighting";

        [MenuItem("GameOff/Setup Main Lighting + Bake")]
        public static void SetupAndBake()
        {
            Setup();
            NavMeshRebake();
            Lightmapping.BakeAsync();
            Debug.Log("[Lighting] Bake started. Save the scene when the progress bar in the bottom right is done.");
        }

        public static void Setup()
        {
            // Sky
            var skyTex = AssetDatabase.LoadAssetAtPath<Texture2D>(SkyTexturePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(SkyTexturePath);
            if (importer != null && (importer.filterMode != FilterMode.Point || importer.mipmapEnabled || importer.wrapModeU != TextureWrapMode.Repeat))
            {
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.wrapModeU = TextureWrapMode.Repeat;
                importer.wrapModeV = TextureWrapMode.Clamp;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyMaterialPath);
            if (sky == null)
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SkyMaterialPath));
                sky = new Material(Shader.Find("Skybox/Panoramic"));
                AssetDatabase.CreateAsset(sky, SkyMaterialPath);
            }
            sky.SetTexture("_MainTex", skyTex);
            sky.SetFloat("_Exposure", 1f);
            EditorUtility.SetDirty(sky);
            RenderSettings.skybox = sky;

            // Ambient + fog: dark, slightly blue, like a station running on minimal power
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.10f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.025f;
            RenderSettings.fogColor = new Color(0.05f, 0.06f, 0.09f);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;

            // Planet light: the scene's directional light, cold and low, shining in through the north panorama window
            Light sun = RenderSettings.sun;
            if (sun == null)
                foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                    if (l.type == LightType.Directional) { sun = l; break; }
            if (sun != null)
            {
                sun.name = "PlanetLight";
                sun.color = new Color(0.55f, 0.68f, 0.95f);
                sun.intensity = 0.8f;
                sun.shadows = LightShadows.Hard;
                sun.lightmapBakeType = LightmapBakeType.Baked;
                sun.transform.rotation = Quaternion.LookRotation(new Vector3(0.15f, -0.35f, -1f));
                RenderSettings.sun = sun;
            }

            // Lightmapping: coarse texels on purpose (retro look, fast bake, small download)
            var settings = AssetDatabase.LoadAssetAtPath<LightingSettings>(LightingSettingsPath);
            if (settings == null)
            {
                settings = new LightingSettings { name = "Main_LightingSettings" };
                AssetDatabase.CreateAsset(settings, LightingSettingsPath);
            }
            settings.bakedGI = true;
            settings.realtimeGI = false;
            settings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            settings.lightmapResolution = 4f;
            settings.lightmapPadding = 2;
            settings.lightmapMaxSize = 1024;
            settings.directionalityMode = LightmapsMode.NonDirectional;
            settings.lightmapCompression = LightmapCompression.NormalQuality;
            settings.maxBounces = 1;
            settings.indirectResolution = 1f;
            settings.filteringMode = LightingSettings.FilterMode.Auto;
            settings.mixedBakeMode = MixedLightingMode.IndirectOnly;
            EditorUtility.SetDirty(settings);
            Lightmapping.lightingSettings = settings;

            BuildLightProbes();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Light probes on a 2.5 m grid over every floor of the station graybox, so moving objects
        /// (counter buttons, stamps, travelers) pick up the baked light. Rebuilt on every run.
        /// </summary>
        private static void BuildLightProbes()
        {
            var station = GameObject.Find("Station_Graybox");
            if (station == null) { Debug.LogWarning("[Lighting] No Station_Graybox in scene, no light probes built."); return; }
            var floors = station.transform.Find("Floors");
            var ceilings = station.transform.Find("Ceilings");
            if (floors == null) { Debug.LogWarning("[Lighting] Station_Graybox has no Floors group, no light probes built."); return; }

            var existing = GameObject.Find("LightProbes");
            if (existing != null) Object.DestroyImmediate(existing);
            var go = new GameObject("LightProbes");
            var group = go.AddComponent<LightProbeGroup>();

            const float step = 2.5f;
            var positions = new System.Collections.Generic.List<Vector3>();
            foreach (Transform floor in floors)
            {
                var floorRenderer = floor.GetComponent<Renderer>();
                if (floorRenderer == null) continue;
                var b = floorRenderer.bounds;
                float height = 3f;
                var ceiling = ceilings != null ? ceilings.Find(floor.name.Replace("_Floor", "_Ceiling")) : null;
                var ceilingRenderer = ceiling != null ? ceiling.GetComponent<Renderer>() : null;
                if (ceilingRenderer != null) height = ceilingRenderer.bounds.min.y;

                int nx = Mathf.Max(1, Mathf.CeilToInt((b.size.x - 0.6f) / step));
                int nz = Mathf.Max(1, Mathf.CeilToInt((b.size.z - 0.6f) / step));
                float[] heights = height > 4f ? new[] { 0.6f, 1.7f, height * 0.6f } : new[] { 0.6f, 1.7f };
                for (int ix = 0; ix <= nx; ix++)
                for (int iz = 0; iz <= nz; iz++)
                foreach (float y in heights)
                {
                    float x = Mathf.Lerp(b.min.x + 0.3f, b.max.x - 0.3f, ix / (float)nx);
                    float z = Mathf.Lerp(b.min.z + 0.3f, b.max.z - 0.3f, iz / (float)nz);
                    positions.Add(new Vector3(x, y, z));
                }
            }
            group.probePositions = positions.ToArray();
            Debug.Log($"[Lighting] {positions.Count} light probes placed.");
        }

        private static void NavMeshRebake()
        {
            foreach (var surface in Object.FindObjectsByType<Unity.AI.Navigation.NavMeshSurface>(FindObjectsSortMode.None))
                Unity.AI.Navigation.Editor.NavMeshAssetManager.instance.StartBakingSurfaces(new Object[] { surface });
        }
    }
}
