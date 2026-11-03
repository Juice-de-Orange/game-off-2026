using GameOff.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds the sky rig (sun, planets, moon) and the station exterior (P24) as prefabs and places them in the open scene.
    /// Delivered models (docs/ASSET-REQUESTS.md, area Exterior/Ships) are used when present, otherwise placeholders.
    /// Re-run after new deliveries. Values documented in docs/STATION.md → "Himmel & Außenwelt".
    /// </summary>
    public static class SkyExteriorBuilder
    {
        private const string SkyPrefabPath = "Assets/_Project/Prefabs/Environment/PF_SkyRig.prefab";
        private const string ExteriorPrefabPath = "Assets/_Project/Prefabs/Environment/PF_Exterior.prefab";
        private const string SkyTex = "Assets/_Project/Art/Textures/Sky/";
        private const string SkyMat = "Assets/_Project/Art/Materials/Sky/";
        private const string ExtMat = "Assets/_Project/Art/Materials/Exterior/";
        private const string RingMeshPath = "Assets/_Project/Art/Models/Sky/Mesh_PlanetRing.asset";
        private const string Models = "Assets/_Project/Art/Models/";

        [MenuItem("GameOff/Build Sky & Exterior")]
        public static void Build()
        {
            ConfigureSkyTextures();
            ReplaceInScene(BuildSkyRig(), SkyPrefabPath);
            ReplaceInScene(BuildExterior(), ExteriorPrefabPath);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Sky] Sky rig and exterior built.");
        }

        // ---------------- Sky rig ----------------

        private static GameObject BuildSkyRig()
        {
            var root = new GameObject("PF_SkyRig");

            var glow = Quad(root, "SunGlow", Mat("M_Sky_SunGlow", "GameOff/SkyGlow", m =>
            {
                m.SetTexture("_MainTex", Tex("T_Sun_Glow.png"));
                m.SetColor("_Color", new Color(1f, 0.93f, 0.8f, 1f));
                m.SetFloat("_Intensity", 2.2f);
            }));
            glow.transform.localScale = Vector3.one * 260f;

            var sunGo = new GameObject("SunLight");
            sunGo.transform.SetParent(root.transform, false);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.9f, 0.78f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Hard;
            sun.lightmapBakeType = LightmapBakeType.Realtime; // moves every day (SkyRig.SetDay)

            var home = Planet(root, "HomePlanet", azimuth: 6f, elevation: -7f, distance: 560f, radius: 175f,
                Mat("M_Sky_HomePlanet", "GameOff/SkyPlanet", m =>
                {
                    m.SetTexture("_MainTex", Tex("T_Planet_Home_Albedo.png"));
                    m.SetTexture("_EmissionTex", Tex("T_Planet_Home_Emission.png"));
                    m.SetFloat("_EmissionStrength", 1.6f);
                    m.SetColor("_RimColor", new Color(0.35f, 0.6f, 1f));
                    m.SetFloat("_SpinSpeed", 0.3f);
                }));
            home.transform.localRotation = Quaternion.Euler(12f, 0f, 8f);

            var giant = Planet(root, "GasGiant", azimuth: 54f, elevation: 17f, distance: 760f, radius: 105f,
                Mat("M_Sky_GasGiant", "GameOff/SkyPlanet", m =>
                {
                    m.SetTexture("_MainTex", Tex("T_Planet_GasGiant_Albedo.png"));
                    m.SetColor("_RimColor", new Color(1f, 0.7f, 0.45f));
                    m.SetFloat("_RimStrength", 0.6f);
                    m.SetFloat("_SpinSpeed", 1.2f);
                }));
            giant.transform.localRotation = Quaternion.Euler(18f, 0f, -12f);
            var ring = new GameObject("Ring");
            ring.transform.SetParent(giant.transform, false);
            ring.AddComponent<MeshFilter>().sharedMesh = RingMesh();
            var ringRenderer = ring.AddComponent<MeshRenderer>();
            ringRenderer.sharedMaterial = Mat("M_Sky_Ring", "GameOff/PlanetRing", m => m.SetTexture("_MainTex", Tex("T_Planet_Ring.png")));
            NoShadows(ringRenderer);

            Planet(root, "Moon", azimuth: -64f, elevation: 23f, distance: 470f, radius: 22f,
                Mat("M_Sky_Moon", "GameOff/SkyPlanet", m =>
                {
                    m.SetTexture("_MainTex", Tex("T_Planet_Moon_Albedo.png"));
                    m.SetFloat("_RimStrength", 0.15f);
                    m.SetFloat("_SpinSpeed", 0f);
                }));

            var rig = root.AddComponent<SkyRig>();
            var so = new SerializedObject(rig);
            so.FindProperty("sunGlow").objectReferenceValue = glow.transform;
            so.FindProperty("sunLight").objectReferenceValue = sun;
            so.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }

        private static GameObject Planet(GameObject parent, string name, float azimuth, float elevation, float distance, float radius, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = Quaternion.Euler(-elevation, azimuth, 0f) * Vector3.forward * distance;
            go.transform.localScale = Vector3.one * radius * 2f;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            NoShadows(r);
            return go;
        }

        private static GameObject Quad(GameObject parent, string name, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            NoShadows(r);
            return go;
        }

        /// <summary>Flat ring (annulus) in the XZ plane, inner 1.4 / outer 2.4 planet radii (sphere radius = 0.5 in local units).</summary>
        private static Mesh RingMesh()
        {
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(RingMeshPath);
            if (mesh != null) return mesh;
            const int segments = 96;
            const float inner = 0.5f * 1.4f, outer = 0.5f * 2.4f;
            var verts = new Vector3[(segments + 1) * 2];
            var uvs = new Vector2[verts.Length];
            var normals = new Vector3[verts.Length];
            var tris = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float a = i * Mathf.PI * 2f / segments;
                var d = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                verts[i * 2] = d * inner; verts[i * 2 + 1] = d * outer;
                uvs[i * 2] = new Vector2(0f, i / (float)segments); uvs[i * 2 + 1] = new Vector2(1f, i / (float)segments);
                normals[i * 2] = normals[i * 2 + 1] = Vector3.up;
                if (i == segments) break;
                int t = i * 6, v = i * 2;
                tris[t] = v; tris[t + 1] = v + 2; tris[t + 2] = v + 1;
                tris[t + 3] = v + 1; tris[t + 4] = v + 2; tris[t + 5] = v + 3;
            }
            mesh = new Mesh { name = "Mesh_PlanetRing", vertices = verts, uv = uvs, normals = normals, triangles = tris };
            mesh.RecalculateBounds();
            AssetFolders.EnsureForAsset(RingMeshPath);
            AssetDatabase.CreateAsset(mesh, RingMeshPath);
            return mesh;
        }

        // ---------------- Exterior ----------------

        private static GameObject BuildExterior()
        {
            var root = new GameObject("PF_Exterior");
            var hull = Mat("M_Ext_Hull_Placeholder", "Universal Render Pipeline/Lit", m =>
            {
                m.SetColor("_BaseColor", new Color(0.42f, 0.44f, 0.46f)); m.SetFloat("_Smoothness", 0.25f);
            });
            var windows = Emissive("M_Ext_Windows_Placeholder", new Color(1f, 0.83f, 0.42f), 2f);
            var solar = Mat("M_Ext_Solar_Placeholder", "Universal Render Pipeline/Lit", m =>
            {
                m.SetColor("_BaseColor", new Color(0.12f, 0.2f, 0.32f)); m.SetFloat("_Smoothness", 0.7f);
            });
            var red = Emissive("M_Ext_NavRed", new Color(0.89f, 0.27f, 0.25f), 4f);
            var green = Emissive("M_Ext_NavGreen", new Color(0.61f, 0.88f, 0.35f), 4f);
            var engine = Emissive("M_Ext_EngineGlow", new Color(0.37f, 0.78f, 1f), 3f);

            // Habitat modules west and east of the observation deck (seen from the deck and the bar window)
            ExtProp(root, "Exterior/SM_Ext_HabModule.fbx", "Hab_West", new Vector3(-32f, 3f, 26f), Quaternion.Euler(90f, 20f, 0f),
                () => HabPlaceholder(hull, windows, 6f, 30f));
            ExtProp(root, "Exterior/SM_Ext_HabModule.fbx", "Hab_East", new Vector3(34f, -4f, 36f), Quaternion.Euler(0f, 0f, 90f),
                () => HabPlaceholder(hull, windows, 5f, 26f));
            ExtProp(root, "Exterior/SM_Ext_TrussSegment.fbx", "Truss_West", new Vector3(-17f, 3f, 16f), Quaternion.Euler(0f, -55f, 0f),
                () => BoxGo(hull, new Vector3(2f, 2f, 24f)), modelRot: Quaternion.Euler(90f, -55f, 0f));
            ExtProp(root, "Exterior/SM_Ext_TrussSegment.fbx", "Truss_East", new Vector3(20f, -1f, 22f), Quaternion.Euler(0f, 50f, 0f),
                () => BoxGo(hull, new Vector3(2f, 2f, 26f)), modelRot: Quaternion.Euler(90f, 50f, 0f));
            ExtProp(root, "Exterior/SM_Ext_SolarArray.fbx", "Solar_North", new Vector3(4f, 22f, 48f), Quaternion.Euler(20f, 8f, 0f),
                () => BoxGo(solar, new Vector3(34f, 0.3f, 12f)), modelRot: Quaternion.Euler(110f, 8f, 0f));
            ExtProp(root, "Exterior/SM_Ext_SolarArray.fbx", "Solar_South", new Vector3(-22f, 14f, -74f), Quaternion.Euler(-25f, -15f, 0f),
                () => BoxGo(solar, new Vector3(34f, 0.3f, 12f)), modelRot: Quaternion.Euler(65f, -15f, 0f));
            ExtProp(root, "Exterior/SM_Ext_Antenna.fbx", "Antenna", new Vector3(-12f, 7f, 20f), Quaternion.identity,
                () => BoxGo(hull, new Vector3(0.6f, 18f, 0.6f), new Vector3(0f, 9f, 0f)));
            ExtProp(root, "Exterior/SM_Ext_DockingArm.fbx", "DockingArm", new Vector3(24f, 2f, 32f), Quaternion.identity,
                () => BoxGo(hull, new Vector3(3f, 3f, 16f)));
            var ship = ExtProp(root, "Ships/SM_Ship_Freighter.fbx", "ParkedShip", new Vector3(24f, 2f, 52f), Quaternion.Euler(0f, 90f, 0f),
                () => BoxGo(hull, new Vector3(10f, 6f, 26f)));
            ship.transform.localScale = Kit("Ships/SM_Ship_Freighter.fbx") != null ? Vector3.one * 0.25f : Vector3.one;
            ExtProp(root, "Exterior/SM_Ext_DeckDome.fbx", "DeckDome", new Vector3(0f, -0.5f, 6f), Quaternion.identity, () => null);

            // Frame around the hangar mouth, seen from inside the hangar through the barrier
            ExtProp(root, "Exterior/SM_Ext_HangarMouth.fbx", "HangarMouth", new Vector3(0f, -2f, -42f), Quaternion.identity, () =>
            {
                var frame = new GameObject("HangarMouth_Placeholder");
                Child(BoxGo(hull, new Vector3(36f, 2f, 4f)), frame, new Vector3(0f, 15f, 0f));
                Child(BoxGo(hull, new Vector3(36f, 2f, 4f)), frame, new Vector3(0f, 0f, 0f));
                Child(BoxGo(hull, new Vector3(3f, 16f, 4f)), frame, new Vector3(-16.5f, 7.5f, 0f));
                Child(BoxGo(hull, new Vector3(3f, 16f, 4f)), frame, new Vector3(16.5f, 7.5f, 0f));
                return frame;
            });

            // Blinking navigation lights
            NavLight(root, "Nav_HabWest", new Vector3(-32f, 3f, 41.5f), red, 0f);
            NavLight(root, "Nav_HabEast", new Vector3(47.5f, -4f, 36f), green, 0.3f);
            // The delivered antenna has a red emissive tip at 17.5 m (y 24.5, pipeline M009); the blinking bulb sits on it.
            NavLight(root, "Nav_Antenna", new Vector3(-12f, Kit("Exterior/SM_Ext_Antenna.fbx") != null ? 24.5f : 25.5f, 20f), red, 0.6f);
            NavLight(root, "Nav_Mouth_W", new Vector3(-16.5f, 14.5f, -44f), red, 0.15f);
            NavLight(root, "Nav_Mouth_E", new Vector3(16.5f, 14.5f, -44f), green, 0.45f);
            // Engine glow of the parked ship (placeholder only)
            if (Kit("Ships/SM_Ship_Freighter.fbx") == null)
                Child(BoxGo(engine, new Vector3(8f, 4f, 0.3f)), ship, new Vector3(0f, 0f, -13.2f));
            return root;
        }

        /// <param name="modelRot">Rotation for the delivered model when its axes differ from the placeholder (pipeline M009).</param>
        private static GameObject ExtProp(GameObject parent, string kitPath, string name, Vector3 pos, Quaternion rot, System.Func<GameObject> placeholder,
            Quaternion? modelRot = null)
        {
            GameObject go;
            var model = Kit(kitPath);
            if (model != null) { go = (GameObject)PrefabUtility.InstantiatePrefab(model); rot = modelRot ?? rot; }
            else go = placeholder();
            if (go == null) return null;
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.SetPositionAndRotation(pos, rot);
            foreach (var r in go.GetComponentsInChildren<Renderer>()) r.receiveShadows = true;
            return go;
        }

        private static GameObject HabPlaceholder(Material hull, Material windows, float radius, float length)
        {
            var pb = ShapeGenerator.GenerateCylinder(PivotLocation.Center, 12, radius, length, 0);
            pb.GetComponent<MeshRenderer>().sharedMaterial = hull;
            pb.ToMesh(); pb.Refresh();
            var go = pb.gameObject;
            // Two lit window bands along the module
            Child(BoxGo(windows, new Vector3(0.2f, length * 0.8f, 0.6f)), go, new Vector3(radius * 0.98f, 0f, 0f));
            Child(BoxGo(windows, new Vector3(0.2f, length * 0.8f, 0.6f)), go, new Vector3(-radius * 0.98f, 0f, 0f));
            return go;
        }

        private static void NavLight(GameObject parent, string name, Vector3 pos, Material mat, float phase)
        {
            var bulb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(bulb.GetComponent<Collider>());
            bulb.name = name;
            bulb.transform.SetParent(parent.transform, false);
            bulb.transform.position = pos;
            bulb.transform.localScale = Vector3.one * 0.8f;
            var r = bulb.GetComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            NoShadows(r);
            var holder = new GameObject(name + "_Blink");
            holder.transform.SetParent(parent.transform, false);
            var blink = holder.AddComponent<BlinkLight>();
            var so = new SerializedObject(blink);
            so.FindProperty("bulb").objectReferenceValue = r;
            so.FindProperty("phase").floatValue = phase;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BoxGo(Material mat, Vector3 size, Vector3 offset = default)
        {
            var pb = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            var go = pb.gameObject;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            pb.ToMesh(); pb.Refresh();
            if (offset != default)
            {
                var holder = new GameObject("Box");
                go.transform.SetParent(holder.transform, false);
                go.transform.localPosition = offset;
                return holder;
            }
            return go;
        }

        private static GameObject Child(GameObject child, GameObject parent, Vector3 localPos)
        {
            child.transform.SetParent(parent.transform, false);
            child.transform.localPosition = localPos;
            return child;
        }

        // ---------------- Helpers ----------------

        private static GameObject Kit(string relativePath) => AssetDatabase.LoadAssetAtPath<GameObject>(Models + relativePath);

        private static Texture2D Tex(string file) => AssetDatabase.LoadAssetAtPath<Texture2D>(SkyTex + file);

        private static Material Mat(string name, string shader, System.Action<Material> setup)
        {
            string folder = name.StartsWith("M_Ext") ? ExtMat : SkyMat;
            string path = folder + name + ".mat";
            var found = Shader.Find(shader);
            if (found == null) Debug.LogError($"[Sky] Shader '{shader}' not found for {name}.");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                AssetFolders.Ensure(folder);
                mat = new Material(found);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (found != null && mat.shader != found)
            {
                mat.shader = found; // repair materials created earlier with a missing/other shader
            }
            setup(mat);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material Emissive(string name, Color color, float strength) =>
            Mat(name, "Universal Render Pipeline/Lit", m =>
            {
                m.SetColor("_BaseColor", color * 0.3f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", color * strength);
                m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            });

        private static void NoShadows(Renderer r)
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        /// <summary>Sky textures: mipmaps on (big, distant), point filter except the soft sun glow; ring clamps.</summary>
        private static void ConfigureSkyTextures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { SkyTex.TrimEnd('/') }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("T_Sky_Space.png")) continue; // configured by MainLightingSetup
                var ti = (TextureImporter)AssetImporter.GetAtPath(path);
                bool glow = path.EndsWith("T_Sun_Glow.png"), ring = path.EndsWith("T_Planet_Ring.png");
                var filter = glow ? FilterMode.Bilinear : FilterMode.Point;
                var wrap = ring || glow ? TextureWrapMode.Clamp : TextureWrapMode.Repeat;
                if (ti.mipmapEnabled && ti.filterMode == filter && ti.wrapMode == wrap && ti.alphaIsTransparency == (glow || ring)) continue;
                ti.mipmapEnabled = true;
                ti.filterMode = filter;
                ti.wrapMode = wrap;
                ti.alphaIsTransparency = glow || ring;
                ti.SaveAndReimport();
            }
        }

        private static void ReplaceInScene(GameObject built, string prefabPath)
        {
            string name = System.IO.Path.GetFileNameWithoutExtension(prefabPath);
            AssetFolders.EnsureForAsset(prefabPath);
            PrefabUtility.SaveAsPrefabAsset(built, prefabPath);
            Object.DestroyImmediate(built);
            var scene = EditorSceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
                if (go.name == name) Object.DestroyImmediate(go);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            PrefabUtility.InstantiatePrefab(prefab, scene);
        }
    }
}
