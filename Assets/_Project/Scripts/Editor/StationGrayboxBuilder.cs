using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace GameOff.EditorTools
{
    /// <summary>
    /// Builds the station graybox from the approved floor plan (docs/STATION.md) and saves it as a prefab.
    /// Re-run after changing the plan; the prefab is overwritten.
    /// Axes: X = east, Z = north, Y = up. Origin = center of the hall at floor level.
    /// </summary>
    public static class StationGrayboxBuilder
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab";
        private const string MaterialFolder = "Assets/_Project/Art/Materials/Graybox";
        private const float WallThickness = 0.2f;
        private const float FloorThickness = 0.1f;

        private enum Area { Neutral, Counter, Danger, Motel, Bar, Kiosk, Diner }

        private readonly struct Room
        {
            public readonly string Name;
            public readonly float X0, X1, Z0, Z1, Height;
            public readonly Area Area;
            public Room(string name, float x0, float x1, float z0, float z1, float height, Area area)
            {
                Name = name; X0 = x0; X1 = x1; Z0 = z0; Z1 = z1; Height = height; Area = area;
            }
        }

        /// <summary>Hole in a wall line. OnXLine = wall at constant X (runs along Z).</summary>
        private readonly struct Opening
        {
            public readonly bool OnXLine;
            public readonly float Line, Center, Width, Sill, Head;
            public readonly bool Glass;
            public Opening(bool onXLine, float line, float center, float width, float sill, float head, bool glass = false)
            {
                OnXLine = onXLine; Line = line; Center = center; Width = width; Sill = sill; Head = head; Glass = glass;
            }
        }

        // --- Floor plan (docs/STATION.md) ---
        private static readonly Room[] Rooms =
        {
            new Room("Hall",          -8,   8,  -6,   6, 6, Area.Neutral),
            new Room("ServiceCorridor",-8, -6, -12,  -6, 3, Area.Neutral),
            new Room("Counter",       -6,   0, -12,  -6, 3, Area.Counter),
            new Room("InspectionLock", 0,   4, -12,  -6, 3, Area.Danger),
            new Room("Hangar",       -10,  10, -26, -12, 9, Area.Danger),
            new Room("MotelCorridor",-20,  -8,   0,   2, 3, Area.Motel),
            new Room("MotelRoom101", -12,  -8,   2,   6, 3, Area.Motel),
            new Room("MotelRoom102", -16, -12,   2,   6, 3, Area.Motel),
            new Room("MotelRoom103", -20, -16,   2,   6, 3, Area.Motel),
            new Room("Diner",        -16,  -8,  -6,   0, 3, Area.Diner),
            new Room("Bar",            8,  18,   0,   8, 4, Area.Bar),
            new Room("Kiosk",          8,  12,  -6,  -2, 3, Area.Kiosk),
        };

        private static readonly Opening[] Openings =
        {
            // Doors
            new Opening(true,  -8,   1,   2.0f, 0, 2.6f),   // Hall <-> motel corridor
            new Opening(false,  2, -10,   1.2f, 0, 2.2f),   // Room 101
            new Opening(false,  2, -14,   1.2f, 0, 2.2f),   // Room 102
            new Opening(false,  2, -18,   1.2f, 0, 2.2f),   // Room 103
            new Opening(true,  -8,  -3,   1.6f, 0, 2.2f),   // Hall <-> diner
            new Opening(true,   8,   3,   1.6f, 0, 2.2f),   // Hall <-> bar
            new Opening(true,   8,  -4,   1.6f, 1.0f, 2.2f),// Kiosk hatch (counter height)
            new Opening(false, -6,  -7,   1.2f, 0, 2.2f),   // Hall <-> service corridor
            new Opening(false,-12,  -7,   1.2f, 0, 2.2f),   // Service corridor <-> hangar
            new Opening(true,  -6,  -7.5f,1.2f, 0, 2.2f),   // Service corridor <-> counter
            new Opening(false, -6,   2,   1.6f, 0, 2.2f),   // Lock -> hall (approve)
            new Opening(false,-12,   2,   1.6f, 0, 2.2f),   // Hangar -> lock (arrive / deny)
            new Opening(true,   4,  -9,   1.6f, 0, 2.2f),   // Outer airlock hatch (closed slab added separately)
            new Opening(false,-26,   0,  16.0f, 0, 7.0f),   // Hangar door to space
            // Windows
            new Opening(false,  6,   0,   8.0f, 0.8f, 4.5f, true), // Hall panorama
            new Opening(true,   0,  -9,   4.0f, 0.9f, 2.2f, true), // Counter -> lock
            new Opening(false,-12,  -3,   4.0f, 1.0f, 2.2f, true), // Counter -> hangar
            new Opening(true, -20,   4,   0.6f, 1.2f, 1.8f, true), // Porthole room 103
        };

        private static readonly Dictionary<Area, Color> AreaColors = new Dictionary<Area, Color>
        {
            { Area.Neutral, new Color(0.62f, 0.64f, 0.66f) },
            { Area.Counter, new Color(0.55f, 0.70f, 0.78f) },
            { Area.Danger,  new Color(0.70f, 0.55f, 0.53f) },
            { Area.Motel,   new Color(0.60f, 0.70f, 0.55f) },
            { Area.Bar,     new Color(0.75f, 0.60f, 0.48f) },
            { Area.Kiosk,   new Color(0.66f, 0.58f, 0.72f) },
            { Area.Diner,   new Color(0.76f, 0.70f, 0.50f) },
        };

        private static readonly Dictionary<Area, Color> LightColors = new Dictionary<Area, Color>
        {
            { Area.Neutral, new Color(0.85f, 0.92f, 1.00f) },
            { Area.Counter, new Color(0.37f, 0.78f, 1.00f) },
            { Area.Danger,  new Color(1.00f, 0.45f, 0.40f) },
            { Area.Motel,   new Color(0.61f, 0.88f, 0.35f) },
            { Area.Bar,     new Color(1.00f, 0.60f, 0.24f) },
            { Area.Kiosk,   new Color(0.76f, 0.48f, 0.84f) },
            { Area.Diner,   new Color(1.00f, 0.83f, 0.42f) },
        };

        private static Dictionary<Area, Material> _materials;
        private static Material _glass, _hatch, _forceField, _prop;

        [MenuItem("GameOff/Build Station Graybox")]
        public static void Build()
        {
            CreateMaterials();

            // Replace an existing instance in the open scene so re-running doesn't stack copies.
            foreach (var existing in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (existing != null && existing.parent == null && (existing.name == "Station_Graybox" || existing.name == "PF_Station_Graybox"))
                    Object.DestroyImmediate(existing.gameObject);

            var root = new GameObject("Station_Graybox");
            var floors = Child(root, "Floors");
            var ceilings = Child(root, "Ceilings");
            var walls = Child(root, "Walls");
            var glass = Child(root, "Glass");
            var lights = Child(root, "Lights");
            var props = Child(root, "Props_Placeholder");
            var markers = Child(root, "Markers");

            foreach (var room in Rooms)
            {
                var mat = _materials[room.Area];
                float w = room.X1 - room.X0, d = room.Z1 - room.Z0;
                var center = new Vector3((room.X0 + room.X1) * 0.5f, 0, (room.Z0 + room.Z1) * 0.5f);

                Block(floors, $"{room.Name}_Floor", center + Vector3.down * (FloorThickness * 0.5f), new Vector3(w, FloorThickness, d), mat);
                Block(ceilings, $"{room.Name}_Ceiling", center + Vector3.up * (room.Height + FloorThickness * 0.5f), new Vector3(w, FloorThickness, d), mat);

                var roomWalls = Child(walls, room.Name);
                // West/east walls run the full depth; south/north walls fit between them (no corner overlap).
                BuildWall(roomWalls, room, true, room.X0, room.X0 + WallThickness * 0.5f, room.Z0, room.Z1, mat);
                BuildWall(roomWalls, room, true, room.X1, room.X1 - WallThickness * 0.5f, room.Z0, room.Z1, mat);
                BuildWall(roomWalls, room, false, room.Z0, room.Z0 + WallThickness * 0.5f, room.X0 + WallThickness, room.X1 - WallThickness, mat);
                BuildWall(roomWalls, room, false, room.Z1, room.Z1 - WallThickness * 0.5f, room.X0 + WallThickness, room.X1 - WallThickness, mat);

                AddLight(lights, room);
            }

            // Glass panes (one per window, centered on the wall line)
            foreach (var o in Openings)
            {
                if (!o.Glass) continue;
                float h = o.Head - o.Sill;
                var pos = o.OnXLine ? new Vector3(o.Line, o.Sill + h * 0.5f, o.Center) : new Vector3(o.Center, o.Sill + h * 0.5f, o.Line);
                var size = o.OnXLine ? new Vector3(0.05f, h, o.Width) : new Vector3(o.Width, h, 0.05f);
                Block(glass, $"Glass_{pos.x:0}_{pos.z:0}", pos, size, _glass);
            }

            // Closed outer airlock hatch and hangar force field
            Block(root, "Airlock_OuterHatch (SM_Station_Hatch_Airlock)", new Vector3(4.1f, 1.1f, -9), new Vector3(0.2f, 2.2f, 1.6f), _hatch);
            // The force field's collider keeps the player inside the station.
            Block(root, "Hangar_ForceField (SM_Hangar_Door_Big)", new Vector3(0, 3.5f, -26), new Vector3(16, 7, 0.1f), _forceField);

            BuildProps(props);
            BuildMarkers(markers);

            // Walkable surface for travelers (P14) and reachability checks. Roofs and glass are not walkable.
            var surface = root.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            ceilings.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            glass.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;

            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PrefabPath));
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);
            Selection.activeGameObject = root;
            Debug.Log($"Station graybox built: {PrefabPath}");
        }

        /// <summary>Builds one wall of a room, cutting every opening that lies on this wall line.</summary>
        private static void BuildWall(GameObject parent, Room room, bool onXLine, float line, float offsetLine, float from, float to, Material mat)
        {
            var cuts = new List<Opening>();
            foreach (var o in Openings)
                if (o.OnXLine == onXLine && Mathf.Approximately(o.Line, line) && o.Center - o.Width * 0.5f < to && o.Center + o.Width * 0.5f > from)
                    cuts.Add(o);
            cuts.Sort((a, b) => a.Center.CompareTo(b.Center));

            float cursor = from;
            foreach (var o in cuts)
            {
                float a = Mathf.Max(from, o.Center - o.Width * 0.5f);
                float b = Mathf.Min(to, o.Center + o.Width * 0.5f);
                WallPiece(parent, onXLine, offsetLine, cursor, a, 0, room.Height, mat);
                WallPiece(parent, onXLine, offsetLine, a, b, 0, o.Sill, mat);
                WallPiece(parent, onXLine, offsetLine, a, b, Mathf.Min(o.Head, room.Height), room.Height, mat);
                cursor = b;
            }
            WallPiece(parent, onXLine, offsetLine, cursor, to, 0, room.Height, mat);
        }

        private static void WallPiece(GameObject parent, bool onXLine, float line, float from, float to, float bottom, float top, Material mat)
        {
            float len = to - from, h = top - bottom;
            if (len < 0.01f || h < 0.01f) return;
            float mid = (from + to) * 0.5f;
            var pos = onXLine ? new Vector3(line, bottom + h * 0.5f, mid) : new Vector3(mid, bottom + h * 0.5f, line);
            var size = onXLine ? new Vector3(WallThickness, h, len) : new Vector3(len, h, WallThickness);
            Block(parent, onXLine ? $"Wall_X{line:0.#}_{mid:0.#}" : $"Wall_Z{line:0.#}_{mid:0.#}", pos, size, mat);
        }

        private static void AddLight(GameObject parent, Room room)
        {
            var go = new GameObject($"{room.Name}_Light");
            go.transform.SetParent(parent.transform, false);
            go.transform.position = new Vector3((room.X0 + room.X1) * 0.5f, room.Height - 0.4f, (room.Z0 + room.Z1) * 0.5f);
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = LightColors[room.Area];
            light.range = Mathf.Max(room.X1 - room.X0, room.Z1 - room.Z0) * 0.9f + room.Height;
            light.intensity = room.Height > 5 ? 6f : 3f;
            light.shadows = LightShadows.None;
        }

        /// <summary>Placeholder blocks named after the requested assets (docs/ASSET-REQUESTS.md). Replaced by real prefabs later.</summary>
        private static void BuildProps(GameObject p)
        {
            // Counter
            Prop(p, "SM_Counter_Desk", -0.7f, -9, 1.2f, 4.0f, 0.85f);
            Prop(p, "SM_Counter_Chair", -2, -9, 0.6f, 0.6f, 1.0f);
            Prop(p, "SM_Counter_Locker", -4, -6.45f, 1.0f, 0.5f, 1.8f);
            Prop(p, "SM_Counter_Board", -1.5f, -6.15f, 1.0f, 0.05f, 0.7f, 1.4f);
            // Hall
            Prop(p, "SM_Hall_DepartureBoard", 0, 0, 2, 2, 3.5f);
            Prop(p, "SM_Hall_Bench_A", -2.5f, 3.2f, 2.8f, 0.6f, 0.9f);
            Prop(p, "SM_Hall_Bench_B", 2.5f, 3.2f, 2.8f, 0.6f, 0.9f);
            Prop(p, "SM_Hall_Bench_C", -2.5f, -3.5f, 2.8f, 0.6f, 0.9f);
            Prop(p, "SM_Hall_VendingMachine", -7.4f, 4.5f, 0.8f, 1.0f, 1.9f);
            Prop(p, "SM_Hall_Planter", 7.2f, 5.2f, 1, 1, 1.4f);
            // Kiosk
            Prop(p, "SM_Kiosk_Counter", 8.6f, -4, 0.8f, 2.8f, 1.0f);
            Prop(p, "SM_Kiosk_Shelf", 11.6f, -4, 0.4f, 2.8f, 2.0f);
            // Bar
            Prop(p, "SM_Bar_Counter_A", 14.5f, 2.5f, 0.7f, 3.0f, 1.1f);
            Prop(p, "SM_Bar_Counter_B", 14.5f, 5.5f, 0.7f, 3.0f, 1.1f);
            Prop(p, "SM_Bar_Shelf", 17.6f, 4, 0.3f, 3.0f, 2.2f);
            Prop(p, "SM_Bar_Table_A", 10.5f, 6, 0.8f, 0.8f, 1.1f);
            Prop(p, "SM_Bar_Table_B", 10.5f, 1.5f, 0.8f, 0.8f, 1.1f);
            for (int i = 0; i < 4; i++) Prop(p, $"SM_Bar_Stool_{i + 1}", 13.6f, 1.6f + i * 1.6f, 0.4f, 0.4f, 0.75f);
            // Diner
            Prop(p, "SM_Diner_Counter", -12, -0.6f, 6.0f, 0.7f, 1.1f);
            Prop(p, "SM_Diner_Booth_A", -14, -5.2f, 2.2f, 1.4f, 1.2f);
            Prop(p, "SM_Diner_Booth_B", -10.5f, -5.2f, 2.2f, 1.4f, 1.2f);
            // Motel
            Prop(p, "SM_Motel_Reception", -9, 0.35f, 1.2f, 0.5f, 1.2f);
            Prop(p, "SM_Motel_Bed", -19.4f, 4.9f, 1.0f, 2.0f, 0.6f);
            Prop(p, "SM_Motel_Table", -17, 5.6f, 0.8f, 0.6f, 0.75f);
            Prop(p, "SM_Motel_Locker", -16.5f, 2.5f, 0.6f, 0.5f, 1.9f);
            // Hangar
            Prop(p, "SM_Hangar_Shuttle", 0, -20, 6, 9, 3.5f);
            Prop(p, "SM_Hangar_DronePad", -7.5f, -14.5f, 3, 3, 0.2f);
            Prop(p, "SM_Hangar_CargoDrone", -7.5f, -14.5f, 1.6f, 1.6f, 0.8f, 0.2f);
            Prop(p, "SM_Hangar_Barrier_W", 1, -13.7f, 0.3f, 3.2f, 1);
            Prop(p, "SM_Hangar_Barrier_E", 3, -13.7f, 0.3f, 3.2f, 1);
            Prop(p, "SM_Hangar_Crate_A", -9, -19, 1.2f, 1.2f, 1);
            Prop(p, "SM_Hangar_Crate_B", -9, -20.4f, 1.2f, 1.2f, 1);
            Prop(p, "SM_Hangar_Crate_C", -9, -19.7f, 1.2f, 1.2f, 1, 1);
            Prop(p, "SM_Hangar_Tank", 8.8f, -15, 1.4f, 1.4f, 3.5f);
        }

        private static void BuildMarkers(GameObject parent)
        {
            Marker(parent, "PlayerSpawn", new Vector3(-18, 0, 4), 90);
            Marker(parent, "CounterCameraAnchor", new Vector3(-2, 1.2f, -9), 90);
            Marker(parent, "TravelerStandPoint", new Vector3(2, 0, -9), -90);
            Marker(parent, "ShuttleRamp", new Vector3(0, 0, -15), 0);
        }

        private static void Prop(GameObject parent, string name, float x, float z, float w, float d, float h, float y = 0)
        {
            Block(parent, name, new Vector3(x, y + h * 0.5f, z), new Vector3(w, h, d), _prop);
        }

        private static void Marker(GameObject parent, string name, Vector3 pos, float yaw)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, yaw, 0));
        }

        private static GameObject Block(GameObject parent, string name, Vector3 center, Vector3 size, Material mat)
        {
            var pb = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            var go = pb.gameObject;
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.position = center;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            pb.ToMesh();
            pb.Refresh();
            go.AddComponent<BoxCollider>();
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            return go;
        }

        private static GameObject Child(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void CreateMaterials()
        {
            System.IO.Directory.CreateDirectory(MaterialFolder);
            _materials = new Dictionary<Area, Material>();
            foreach (var kv in AreaColors)
                _materials[kv.Key] = GetOrCreateMaterial($"M_Graybox_{kv.Key}", kv.Value, false);
            _glass = GetOrCreateMaterial("M_Graybox_Glass", new Color(0.37f, 0.78f, 1f, 0.25f), true);
            _forceField = GetOrCreateMaterial("M_Graybox_ForceField", new Color(0.37f, 0.6f, 1f, 0.15f), true);
            _hatch = GetOrCreateMaterial("M_Graybox_Hatch", new Color(0.89f, 0.27f, 0.25f), false);
            _prop = GetOrCreateMaterial("M_Graybox_Prop", new Color(0.35f, 0.37f, 0.40f), false);
            AssetDatabase.SaveAssets();
        }

        private static Material GetOrCreateMaterial(string name, Color color, bool transparent)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(mat, path);
            }
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.1f);
            if (transparent)
            {
                mat.SetFloat("_Surface", 1); // Transparent
                mat.SetFloat("_Blend", 0);   // Alpha
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }
    }
}
