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
            new Room("Hangar",       -18,  18, -40, -12, 16, Area.Danger),
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
            new Opening(false,-40,   0,  28.0f, 0, 12.0f),  // Hangar mouth to space (energy barrier, P23)
            new Opening(false,  6,   0,  10.0f, 0, 6.0f),   // Hall → observation deck (full height, deck built separately)
            // Windows
            new Opening(false,  8,  14,   8.0f, 0.9f, 2.4f, true), // Bar window (north), matches SM_Station_Wall_Window_2x3
            new Opening(true,   0,  -9,   4.0f, 0.9f, 2.2f, true), // Counter -> lock
            new Opening(false,-12,  -3,   4.0f, 0.9f, 2.2f, true), // Counter -> hangar (sill 0.9 = SM_Station_Window_Small)
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

            PrepareKitImport();

            var root = new GameObject("Station_Graybox");
            var floors = Child(root, "Floors");
            var ceilings = Child(root, "Ceilings");
            var walls = Child(root, "Walls");
            var visual = Child(root, "Visual"); // kit modules + visible graybox where no module exists yet
            var glass = Child(root, "Glass");
            var lights = Child(root, "Lights");
            var fixtures = Child(root, "LightFixtures");
            var props = Child(root, "Props_Placeholder");
            var markers = Child(root, "Markers");

            foreach (var room in Rooms)
            {
                var mat = _materials[room.Area];
                float w = room.X1 - room.X0, d = room.Z1 - room.Z0;
                var center = new Vector3((room.X0 + room.X1) * 0.5f, 0, (room.Z0 + room.Z1) * 0.5f);

                // Collision layer: graybox slabs/walls, invisible once kit visuals exist (walking + NavMesh stay identical).
                bool floorKit = Kit(KitFloor) != null, ceilingKit = Kit(KitCeiling) != null;
                var floor = Block(floors, $"{room.Name}_Floor", center + Vector3.down * (FloorThickness * 0.5f), new Vector3(w, FloorThickness, d), mat);
                floor.GetComponent<MeshRenderer>().enabled = !floorKit;
                var ceiling = Block(ceilings, $"{room.Name}_Ceiling", center + Vector3.up * (room.Height + FloorThickness * 0.5f), new Vector3(w, FloorThickness, d), mat);
                ceiling.GetComponent<MeshRenderer>().enabled = !ceilingKit;

                var roomWalls = Child(walls, room.Name);
                // West/east walls run the full depth; south/north walls fit between them (no corner overlap).
                BuildWall(roomWalls, room, true, room.X0, room.X0 + WallThickness * 0.5f, room.Z0, room.Z1, 0, room.Height, mat, false, true);
                BuildWall(roomWalls, room, true, room.X1, room.X1 - WallThickness * 0.5f, room.Z0, room.Z1, 0, room.Height, mat, false, true);
                BuildWall(roomWalls, room, false, room.Z0, room.Z0 + WallThickness * 0.5f, room.X0 + WallThickness, room.X1 - WallThickness, 0, room.Height, mat, false, true);
                BuildWall(roomWalls, room, false, room.Z1, room.Z1 - WallThickness * 0.5f, room.X0 + WallThickness, room.X1 - WallThickness, 0, room.Height, mat, false, true);

                // Visual layer
                var roomVisual = Child(visual, room.Name);
                var set = ModulesFor(room);
                float floorCell = set.Floor == KitHangarFloor ? 4f : KitCell;
                float ceilingCell = set.Ceiling == KitHangarCeiling ? 4f : KitCell;
                if (floorKit) TileRoom(roomVisual, room, set.Floor, -KitHeight(set.Floor), floorCell);
                if (ceilingKit) TileRoom(roomVisual, room, set.Ceiling, room.Height - set.CeilingDepth, ceilingCell);
                // Kit walls face into the room: the module's front is local -Z.
                BuildWallVisual(roomVisual, room, set, true, room.X0, room.X0 + WallThickness * 0.5f, room.Z0, room.Z1, -90f, mat);
                BuildWallVisual(roomVisual, room, set, true, room.X1, room.X1 - WallThickness * 0.5f, room.Z0, room.Z1, 90f, mat);
                BuildWallVisual(roomVisual, room, set, false, room.Z0, room.Z0 + WallThickness * 0.5f, room.X0, room.X1, 180f, mat);
                BuildWallVisual(roomVisual, room, set, false, room.Z1, room.Z1 - WallThickness * 0.5f, room.X0, room.X1, 0f, mat);

                AddRoomLights(lights, fixtures, room);
            }

            // Tired neon tubes in the staff corridors (realtime so they can flicker).
            AddFlickerLight(lights, "ServiceCorridor_Flicker", new Vector3(-7, 2.6f, -10), LightColors[Area.Neutral]);
            AddFlickerLight(lights, "MotelCorridor_Flicker", new Vector3(-15, 2.6f, 1), LightColors[Area.Motel]);

            // Glass panes (one per window, centered on the wall line). Invisible where a window module brings its own glass.
            foreach (var o in Openings)
            {
                if (!o.Glass) continue;
                float h = o.Head - o.Sill;
                var pos = o.OnXLine ? new Vector3(o.Line, o.Sill + h * 0.5f, o.Center) : new Vector3(o.Center, o.Sill + h * 0.5f, o.Line);
                var size = o.OnXLine ? new Vector3(0.05f, h, o.Width) : new Vector3(o.Width, h, 0.05f);
                var pane = Block(glass, $"Glass_{pos.x:0}_{pos.z:0}", pos, size, _glass);
                pane.GetComponent<MeshRenderer>().enabled =
                    !(IsWindowModuleOpening(o) && Kit(KitWindowWall) != null) && ModuleForOpening(o) == null;
            }
            PlaceOpeningModules(Child(visual, "WindowModules"));

            BuildObservationDeck(root, floors, ceilings, walls, visual, glass, lights, fixtures);

            // Closed outer airlock hatch and hangar barrier
            var hatch = Block(root, "Airlock_OuterHatch_Collider", new Vector3(4.1f, 1.1f, -9), new Vector3(0.2f, 2.2f, 1.6f), _hatch);
            hatch.GetComponent<MeshRenderer>().enabled = Kit(KitHatch) == null;
            PlaceKit(visual, KitHatch, new Vector3(4f, 0f, -9f), 90f); // front (local -Z) faces into the inspection lock
            BuildEnergyBarrier(root, visual);
            BuildDoors(root);

            BuildProps(props);
            BuildMarkers(markers);

            // Walkable surface for travelers (P14) and reachability checks. Roofs and glass are not walkable.
            var surface = root.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            ceilings.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            glass.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            fixtures.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;

            AssetFolders.EnsureForAsset(PrefabPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);
            Selection.activeGameObject = root;
            Debug.Log($"Station graybox built: {PrefabPath}");
        }

        /// <summary>
        /// Builds the part [from..to] x [bottom..top] of one wall line, cutting every opening on that line.
        /// visible/collider choose between the collision layer (invisible, collider) and graybox visual fill (visible, no collider).
        /// </summary>
        private static void BuildWall(GameObject parent, Room room, bool onXLine, float line, float offsetLine, float from, float to,
            float bottom, float top, Material mat, bool visible, bool collider)
        {
            var cuts = new List<Opening>();
            foreach (var raw in Openings)
            {
                var o = collider ? raw : VisualOpening(raw); // visual fill leaves room for window modules
                if (o.OnXLine == onXLine && Mathf.Approximately(o.Line, line) && o.Center - o.Width * 0.5f < to && o.Center + o.Width * 0.5f > from)
                    cuts.Add(o);
            }
            cuts.Sort((a, b) => a.Center.CompareTo(b.Center));

            float cursor = from;
            foreach (var o in cuts)
            {
                float a = Mathf.Max(from, o.Center - o.Width * 0.5f);
                float b = Mathf.Min(to, o.Center + o.Width * 0.5f);
                WallPiece(parent, onXLine, offsetLine, cursor, a, bottom, top, mat, visible, collider);
                WallPiece(parent, onXLine, offsetLine, a, b, bottom, Mathf.Min(o.Sill, top), mat, visible, collider);
                WallPiece(parent, onXLine, offsetLine, a, b, Mathf.Max(bottom, Mathf.Min(o.Head, room.Height)), top, mat, visible, collider);
                cursor = b;
            }
            WallPiece(parent, onXLine, offsetLine, cursor, to, bottom, top, mat, visible, collider);
        }

        private static void WallPiece(GameObject parent, bool onXLine, float line, float from, float to, float bottom, float top,
            Material mat, bool visible, bool collider)
        {
            float len = to - from, h = top - bottom;
            if (len < 0.01f || h < 0.01f) return;
            float mid = (from + to) * 0.5f;
            var pos = onXLine ? new Vector3(line, bottom + h * 0.5f, mid) : new Vector3(mid, bottom + h * 0.5f, line);
            var size = onXLine ? new Vector3(WallThickness, h, len) : new Vector3(len, h, WallThickness);
            var go = Block(parent, onXLine ? $"Wall_X{line:0.#}_{mid:0.#}_{bottom:0.#}" : $"Wall_Z{line:0.#}_{mid:0.#}_{bottom:0.#}", pos, size, mat);
            go.GetComponent<MeshRenderer>().enabled = visible;
            if (!collider) Object.DestroyImmediate(go.GetComponent<Collider>());
        }

        // --- Kit modules (docs/ASSET-REQUESTS.md). Missing files simply fall back to graybox. ---
        private const string KitRoot = "Assets/_Project/Art/Models/";
        private const string KitFloor = "Station/SM_Station_Floor_2x2.fbx";
        private const string KitCeiling = "Station/SM_Station_Ceiling_2x2.fbx";
        private const string KitWall = "Station/SM_Station_Wall_2x3.fbx";
        private const string KitDoorWall = "Station/SM_Station_Wall_Door_2x3.fbx";
        private const string KitWindowWall = "Station/SM_Station_Wall_Window_2x3.fbx";
        private const string KitHangarWall = "Hangar/SM_Hangar_Wall_4x4.fbx";
        private const string KitHangarFloor = "Hangar/SM_Hangar_Floor_4x4.fbx";
        private const string KitHangarCeiling = "Hangar/SM_Hangar_Ceiling_4x4.fbx";
        private const string KitDeckWindow = "Station/SM_Station_Window_Curved.fbx";
        private const string KitDeckFloor = "Station/SM_Station_Floor_DeckRound.fbx";
        private const string KitDeckCeiling = "Station/SM_Station_Ceiling_DeckRound.fbx";
        private const string KitSteps = "Station/SM_Station_Steps_2m.fbx";
        private const float KitCell = 2f, KitWallHeight = 3f, KitDoorWidth = 1.6f, KitDoorHeight = 2.2f;
        private const float WindowSill = 0.9f, WindowHead = 2.4f; // SM_Station_Wall_Window_2x3

        /// <summary>Module set used for a room: the hangar uses big 4 x 4 m modules once they exist.</summary>
        private readonly struct ModuleSet
        {
            public readonly string Wall, Floor, Ceiling;
            public readonly float Cell, Height, CeilingDepth;
            public ModuleSet(string wall, string floor, string ceiling, float cell, float height, float ceilingDepth)
            {
                Wall = wall; Floor = floor; Ceiling = ceiling; Cell = cell; Height = height; CeilingDepth = ceilingDepth;
            }
        }

        private static ModuleSet ModulesFor(Room room)
        {
            if (room.Name == "Hangar" && Kit(KitHangarWall) != null)
                return new ModuleSet(KitHangarWall, Kit(KitHangarFloor) != null ? KitHangarFloor : KitFloor,
                    Kit(KitHangarCeiling) != null ? KitHangarCeiling : KitCeiling, 4f, 4f, Kit(KitHangarCeiling) != null ? 1f : 0f);
            return new ModuleSet(KitWall, KitFloor, KitCeiling, KitCell, KitWallHeight, 0f);
        }

        /// <summary>
        /// True if the window module can fill this opening completely: matching sill/head and the opening starts and
        /// ends on the 2 m grid (rooms are on that grid), so every covered cell gets a module with its own glass.
        /// </summary>
        private static bool IsWindowModuleOpening(Opening o)
        {
            if (!o.Glass || !Mathf.Approximately(o.Sill, WindowSill) || !Mathf.Approximately(o.Head, WindowHead)) return false;
            float start = o.Center - o.Width * 0.5f, end = o.Center + o.Width * 0.5f;
            return OnGrid(start) && OnGrid(end);
        }

        private static bool OnGrid(float v) => Mathf.Abs(v / KitCell - Mathf.Round(v / KitCell)) < 0.001f;

        // --- Window/porthole modules that fill one specific opening (placed once per opening, not per room) ---
        private const string KitHatch = "Station/SM_Station_Hatch_Airlock.fbx";
        private const string KitWindowSmall = "Station/SM_Station_Window_Small.fbx";
        private const string KitWindowSlot = "Station/SM_Station_Window_Small_Slot.fbx";
        private const string KitPorthole = "Station/SM_Station_Porthole.fbx";

        /// <summary>The module that fills this opening, or null. Counter windows (4 m, up to 2.2 m) and the porthole.</summary>
        private static string ModuleForOpening(Opening o)
        {
            if (!o.Glass) return null;
            string path = null;
            if (o.OnXLine && Mathf.Approximately(o.Line, 0f) && Mathf.Approximately(o.Width, 4f)) path = KitWindowSlot;      // counter -> lock
            else if (!o.OnXLine && Mathf.Approximately(o.Line, -12f) && Mathf.Approximately(o.Width, 4f)) path = KitWindowSmall; // counter -> hangar
            else if (o.OnXLine && Mathf.Approximately(o.Line, -20f) && o.Width < 1f) path = KitPorthole;                     // room 103
            return path != null && Kit(path) != null ? path : null;
        }

        /// <summary>
        /// In the visual pass a window module covers the wall from the floor up to its own height, so the graybox fill must
        /// leave that whole area open (the module brings its own sill). Other openings stay as they are.
        /// </summary>
        private static Opening VisualOpening(Opening o)
        {
            string module = ModuleForOpening(o);
            if (module == KitWindowSmall || module == KitWindowSlot)
                return new Opening(o.OnXLine, o.Line, o.Center, o.Width, 0f, KitHeight(module), o.Glass);
            return o;
        }

        private static void PlaceOpeningModules(GameObject parent)
        {
            foreach (var o in Openings)
            {
                string module = ModuleForOpening(o);
                if (module == null) continue;
                float y = module == KitPorthole ? (o.Sill + o.Head) * 0.5f : 0f; // porthole pivot is its center
                var pos = o.OnXLine ? new Vector3(o.Line, y, o.Center) : new Vector3(o.Center, y, o.Line);
                PlaceKit(parent, module, pos, o.OnXLine ? 90f : 0f);
            }
        }
        // Counter furniture orientation (officer sits at x = -2 looking +X; checked visually).
        private const float DeskYaw = 0f, ChairYaw = 90f, DocSlotYaw = 90f;
        // Shuttle: rear ramp must face north toward the queue lanes and the inspection lock (checked visually).
        private const float ShuttleYaw = 0f;

        private static GameObject Kit(string relativePath) => AssetDatabase.LoadAssetAtPath<GameObject>(KitRoot + relativePath);

        /// <summary>Kit models need lightmap UVs for the baked lighting. Sets the import flag once per model.</summary>
        private static void PrepareKitImport()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { KitRoot.TrimEnd('/') }))
            {
                var importer = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as ModelImporter;
                if (importer == null || importer.generateSecondaryUV) continue;
                importer.generateSecondaryUV = true;
                importer.SaveAndReimport();
            }
        }

        private static GameObject PlaceKit(GameObject parent, string relativePath, Vector3 position, float yaw)
        {
            var model = Kit(relativePath);
            if (model == null) return null;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(model);
            go.transform.SetParent(parent.transform, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            foreach (var t in go.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            return go;
        }

        /// <summary>Fills a room's floor (or ceiling) with square tiles of the given size. Rooms are on the grid, so tiles fit exactly.</summary>
        private static void TileRoom(GameObject parent, Room room, string tile, float y, float cell = KitCell)
        {
            for (float x = room.X0 + cell * 0.5f; x < room.X1; x += cell)
            for (float z = room.Z0 + cell * 0.5f; z < room.Z1; z += cell)
                PlaceKit(parent, tile, new Vector3(x, y, z), 0f);
        }

        // --- Sliding doors (P27) ---
        private const string KitDoor = "Station/SM_Station_Door_Slide.fbx";
        private const string KitDoorNarrow = "Station/SM_Station_Door_Slide_Narrow.fbx";

        private readonly struct DoorSpec
        {
            public readonly string Name; public readonly bool OnXLine; public readonly float Line, Center, Width;
            public readonly GameOff.Station.DoorAccess Access;
            public DoorSpec(string name, bool onXLine, float line, float center, float width, GameOff.Station.DoorAccess access)
            {
                Name = name; OnXLine = onXLine; Line = line; Center = center; Width = width; Access = access;
            }
        }

        // Must match door openings in the Openings table (docs/STATION.md).
        private static readonly DoorSpec[] Doors =
        {
            new DoorSpec("Door_Room101",        false,   2, -10,   1.2f, GameOff.Station.DoorAccess.Locked),
            new DoorSpec("Door_Room102",        false,   2, -14,   1.2f, GameOff.Station.DoorAccess.Locked),
            new DoorSpec("Door_Room103",        false,   2, -18,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Diner",          true,   -8,  -3,   1.6f, GameOff.Station.DoorAccess.Public),
            new DoorSpec("Door_Bar",            true,    8,   3,   1.6f, GameOff.Station.DoorAccess.Public),
            new DoorSpec("Door_Service_Hall",   false,  -6,  -7,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Service_Hangar", false, -12,  -7,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Service_Counter",true,   -6,  -7.5f,1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Lock_Hall",      false,  -6,   2,   1.6f, GameOff.Station.DoorAccess.Scripted),
            new DoorSpec("Door_Lock_Hangar",    false, -12,   2,   1.6f, GameOff.Station.DoorAccess.Scripted),
        };

        private static void BuildDoors(GameObject root)
        {
            var group = Child(root, "Doors");
            group.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true; // doorways stay walkable in the bake
            var lampMat = EmissiveMaterial(new Color(0.89f, 0.27f, 0.25f));

            foreach (var d in Doors)
            {
                var door = new GameObject(d.Name);
                door.transform.SetParent(group.transform, false);
                door.transform.SetPositionAndRotation(
                    d.OnXLine ? new Vector3(d.Line, 0f, d.Center) : new Vector3(d.Center, 0f, d.Line),
                    Quaternion.Euler(0f, d.OnXLine ? 90f : 0f, 0f));

                // Sensing area on both sides of the door
                var trigger = door.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
                trigger.center = new Vector3(0f, 1.2f, 0f);
                trigger.size = new Vector3(d.Width + 0.8f, 2.4f, 3.2f);

                // Door leaf: delivered model or placeholder box, with its own solid collider
                string kit = d.Width < 1.4f ? KitDoorNarrow : KitDoor;
                Transform leaf;
                if (Kit(kit) != null)
                {
                    var model = (GameObject)PrefabUtility.InstantiatePrefab(Kit(kit));
                    model.transform.SetParent(door.transform, false);
                    leaf = model.transform;
                }
                else
                {
                    leaf = Block(door, "Leaf_Placeholder", new Vector3(0f, 1.1f, 0f), new Vector3(d.Width, 2.2f, 0.12f), _hatch).transform;
                    Object.DestroyImmediate(leaf.GetComponent<Collider>());
                    leaf.localPosition = new Vector3(0f, 1.1f, 0f);
                    GameObjectUtility.SetStaticEditorFlags(leaf.gameObject, 0); // moves at runtime: must not be static-batched
                }
                leaf.name = "Leaf";
                var leafCollider = leaf.gameObject.AddComponent<BoxCollider>();
                leafCollider.center = leaf.InverseTransformPoint(door.transform.TransformPoint(new Vector3(0f, 1.1f, 0f)));
                leafCollider.size = new Vector3(d.Width, 2.2f, 0.14f);

                GameObject lamp = null;
                if (d.Access == GameOff.Station.DoorAccess.Locked)
                {
                    lamp = Block(door, "LockedLamp", Vector3.zero, new Vector3(0.12f, 0.08f, 0.32f), lampMat);
                    Object.DestroyImmediate(lamp.GetComponent<Collider>());
                    lamp.transform.localPosition = new Vector3(d.Width * 0.5f + 0.15f, 1.6f, 0f);
                }

                // Travelers (NavMesh agents) must not route through staff or locked doors, nor through a closed scripted door
                UnityEngine.AI.NavMeshObstacle obstacle = null;
                if (d.Access != GameOff.Station.DoorAccess.Public)
                {
                    obstacle = door.AddComponent<UnityEngine.AI.NavMeshObstacle>();
                    obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box;
                    obstacle.center = new Vector3(0f, 1.1f, 0f);
                    obstacle.size = new Vector3(d.Width, 2.2f, 0.4f);
                    obstacle.carving = true;
                }

                var audio = door.AddComponent<AudioSource>();
                audio.playOnAwake = false;
                audio.spatialBlend = 1f;
                audio.maxDistance = 15f;

                var sliding = door.AddComponent<GameOff.Station.SlidingDoor>();
                var so = new SerializedObject(sliding);
                so.FindProperty("access").enumValueIndex = (int)d.Access;
                so.FindProperty("panel").objectReferenceValue = leaf;
                so.FindProperty("slideDistance").floatValue = d.Width;
                so.FindProperty("audioSource").objectReferenceValue = audio;
                so.FindProperty("lockedLamp").objectReferenceValue = lamp;
                so.FindProperty("obstacle").objectReferenceValue = obstacle;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        // --- Energy barrier in the hangar mouth (P23) ---
        private const string BarrierMaterialPath = "Assets/_Project/Art/Materials/Hangar/M_Hangar_EnergyBarrier.mat";
        private const string KitBarrierEmitter = "Hangar/SM_Hangar_BarrierEmitter.fbx";
        private static readonly Vector3 BarrierCenter = new Vector3(0f, 6f, -40f);
        private static readonly Vector2 BarrierSize = new Vector2(28f, 12f);

        private static void BuildEnergyBarrier(GameObject root, GameObject visual)
        {
            var group = Child(root, "EnergyBarrier");

            // Invisible blocker: keeps the player inside (the field itself has no collider).
            var blocker = Block(group, "Barrier_Collider", BarrierCenter, new Vector3(BarrierSize.x, BarrierSize.y, 0.2f), _forceField);
            blocker.GetComponent<MeshRenderer>().enabled = false;

            // The animated field
            var field = GameObject.CreatePrimitive(PrimitiveType.Quad);
            field.name = "Barrier_Field";
            Object.DestroyImmediate(field.GetComponent<Collider>());
            field.transform.SetParent(group.transform, false);
            field.transform.SetPositionAndRotation(BarrierCenter, Quaternion.identity);
            field.transform.localScale = new Vector3(BarrierSize.x, BarrierSize.y, 1f);
            var renderer = field.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = BarrierMaterial();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            field.AddComponent<GameOff.Core.EnergyBarrier>();

            // Emitter frame: modules if delivered (4 m segments), otherwise graybox bars with a glowing inner lens.
            var frame = Child(visual, "BarrierFrame");
            float halfW = BarrierSize.x * 0.5f, top = BarrierCenter.y + BarrierSize.y * 0.5f, z = BarrierCenter.z;
            if (Kit(KitBarrierEmitter) != null)
            {
                for (float x = -halfW + 2f; x < halfW; x += 4f)
                {
                    PlaceKit(frame, KitBarrierEmitter, new Vector3(x, top, z), 180f);                         // top row, lens down
                    PlaceKit(frame, KitBarrierEmitter, new Vector3(x, 0f, z), 0f);                            // bottom row, lens up
                }
                for (float y = 2f; y < BarrierSize.y; y += 4f)
                {
                    var left = PlaceKit(frame, KitBarrierEmitter, new Vector3(-halfW, y, z), 0f);
                    left.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    var right = PlaceKit(frame, KitBarrierEmitter, new Vector3(halfW, y, z), 0f);
                    right.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                }
                return;
            }
            var lens = EmissiveMaterial(new Color(0.37f, 0.78f, 1f));
            FrameBar(frame, "Emitter_Top", new Vector3(0f, top + 0.4f, z), new Vector3(BarrierSize.x + 1.6f, 0.8f, 1.2f), lens, Vector3.down);
            FrameBar(frame, "Emitter_Bottom", new Vector3(0f, 0.3f, z), new Vector3(BarrierSize.x + 1.6f, 0.6f, 1.2f), lens, Vector3.up);
            FrameBar(frame, "Emitter_West", new Vector3(-halfW - 0.4f, BarrierCenter.y, z), new Vector3(0.8f, BarrierSize.y, 1.2f), lens, Vector3.right);
            FrameBar(frame, "Emitter_East", new Vector3(halfW + 0.4f, BarrierCenter.y, z), new Vector3(0.8f, BarrierSize.y, 1.2f), lens, Vector3.left);
        }

        /// <summary>A dark emitter bar with a thin glowing lens strip on the side facing the field.</summary>
        private static void FrameBar(GameObject parent, string name, Vector3 center, Vector3 size, Material lens, Vector3 towardField)
        {
            var bar = Block(parent, name, center, size, _prop);
            Object.DestroyImmediate(bar.GetComponent<Collider>());
            var lensSize = new Vector3(
                towardField.x != 0f ? 0.06f : size.x - 0.4f,
                towardField.y != 0f ? 0.06f : size.y - 0.4f,
                0.5f);
            var lensPos = center + Vector3.Scale(towardField, size) * 0.5f + towardField * 0.03f;
            var strip = Block(parent, name + "_Lens", lensPos, lensSize, lens);
            Object.DestroyImmediate(strip.GetComponent<Collider>());
        }

        private static Material BarrierMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(BarrierMaterialPath);
            if (mat != null) return mat;
            AssetFolders.EnsureForAsset(BarrierMaterialPath);
            mat = new Material(Shader.Find("GameOff/EnergyBarrier"));
            mat.SetVector("_Size", new Vector4(BarrierSize.x, BarrierSize.y, 0f, 0f));
            AssetDatabase.CreateAsset(mat, BarrierMaterialPath);
            return mat;
        }

        // --- Observation deck (docs/STATION.md 2.0): half circle north of the hall, two steps down ---
        private const float DeckRadius = 5f, DeckDrop = 0.36f, StepHeight = 0.18f, StepDepth = 0.4f, DeckSillHeight = 0.5f;
        private const int DeckGlassSegments = 8, DeckArcSegments = 16;
        private static readonly Vector3 DeckCenter = new Vector3(0f, 0f, 6f);
        private const float HallHeight = 6f;

        private static void BuildObservationDeck(GameObject root, GameObject floors, GameObject ceilings, GameObject walls,
            GameObject visual, GameObject glass, GameObject lights, GameObject fixtures)
        {
            var mat = _materials[Area.Neutral];
            var deckVisual = Child(visual, "ObservationDeck");

            // Floor, ceiling and the one step between hall floor (y 0) and deck floor (y -0.36)
            var floor = HalfDisc(floors, "Deck_Floor", DeckCenter + Vector3.up * (-DeckDrop - FloorThickness), DeckRadius, FloorThickness, mat);
            floor.GetComponent<MeshRenderer>().enabled = Kit(KitDeckFloor) == null;
            var ceiling = HalfDisc(ceilings, "Deck_Ceiling", DeckCenter + Vector3.up * HallHeight, DeckRadius, FloorThickness, mat);
            ceiling.GetComponent<MeshRenderer>().enabled = Kit(KitDeckCeiling) == null;
            var step = Block(floors, "Deck_Step", new Vector3(0f, -DeckDrop + StepHeight * 0.5f, DeckCenter.z + StepDepth * 0.5f),
                new Vector3(DeckRadius * 2f, StepHeight, StepDepth), mat);
            step.GetComponent<MeshRenderer>().enabled = Kit(KitSteps) == null;

            PlaceKit(deckVisual, KitDeckFloor, new Vector3(0f, -DeckDrop - KitHeight(KitDeckFloor), DeckCenter.z + DeckRadius * 0.5f), 0f);
            PlaceKit(deckVisual, KitDeckCeiling, new Vector3(0f, HallHeight, DeckCenter.z + DeckRadius * 0.5f), 0f);
            // Step module: 2 risers over 0.8 m depth (z 6.0..6.8). Its pivot is the center of its footprint → z 6.4,
            // so the upper tread lines up with the walkable Deck_Step (z 6.0..6.4, top -0.18) and the lower one with the deck floor.
            if (Kit(KitSteps) != null)
                for (float x = -DeckRadius + 1f; x < DeckRadius; x += 2f)
                    PlaceKit(deckVisual, KitSteps, new Vector3(x, -DeckDrop, DeckCenter.z + StepDepth), 180f);

            // Glass front: 8 segments around the half circle, each a low sill wall + glass pane (colliders keep the player in).
            var deckWalls = Child(walls, "ObservationDeck");
            bool windowKit = Kit(KitDeckWindow) != null;
            float chord = 2f * DeckRadius * Mathf.Sin(Mathf.PI / (2f * DeckGlassSegments));
            for (int i = 0; i < DeckGlassSegments; i++)
            {
                float a = (i + 0.5f) * Mathf.PI / DeckGlassSegments;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var inward = -dir;
                float yaw = Mathf.Atan2(-inward.x, -inward.z) * Mathf.Rad2Deg; // module front (local -Z) faces the deck
                var rot = Quaternion.Euler(0f, yaw, 0f);
                var basePos = DeckCenter + dir * (DeckRadius * Mathf.Cos(Mathf.PI / (2f * DeckGlassSegments)) - WallThickness * 0.5f);

                var sill = Block(deckWalls, $"Deck_Sill_{i}", basePos + Vector3.up * (-DeckDrop + DeckSillHeight * 0.5f),
                    new Vector3(chord, DeckSillHeight, WallThickness), mat);
                sill.transform.rotation = rot;
                sill.GetComponent<MeshRenderer>().enabled = !windowKit;
                float paneHeight = HallHeight - (DeckSillHeight - DeckDrop);
                var pane = Block(glass, $"Deck_Glass_{i}", basePos + Vector3.up * (DeckSillHeight - DeckDrop + paneHeight * 0.5f),
                    new Vector3(chord, paneHeight, 0.05f), _glass);
                pane.transform.rotation = rot;
                pane.GetComponent<MeshRenderer>().enabled = !windowKit;
                PlaceKit(deckVisual, KitDeckWindow, basePos + Vector3.up * -DeckDrop, yaw);
            }

            // Light: cold glow of the home planet in front of the glass, plus two soft ceiling lights.
            BakedLight(lights, "Deck_PlanetGlow", DeckCenter + new Vector3(0f, 3f, 3.5f), new Color(0.45f, 0.6f, 0.9f), 9f, 1.5f);
            BakedLight(lights, "Deck_Light_W", DeckCenter + new Vector3(-2.5f, HallHeight - 0.4f, 2f), LightColors[Area.Neutral], 8f, 1.8f);
            BakedLight(lights, "Deck_Light_E", DeckCenter + new Vector3(2.5f, HallHeight - 0.4f, 2f), LightColors[Area.Neutral], 8f, 1.8f);
        }

        /// <summary>
        /// A half-disc slab (north half, +Z side of center) of given radius and thickness, bottom at center.y.
        /// Built as a ProBuilder mesh so it persists in the prefab; mesh collider for walking.
        /// </summary>
        private static GameObject HalfDisc(GameObject parent, string name, Vector3 bottomCenter, float radius, float thickness, Material mat)
        {
            var positions = new List<Vector3>();
            // 0: top center, 1: bottom center, then pairs (top, bottom) along the arc from +X (angle 0) to -X (angle 180)
            positions.Add(new Vector3(0f, thickness, 0f));
            positions.Add(Vector3.zero);
            for (int i = 0; i <= DeckArcSegments; i++)
            {
                float a = i * Mathf.PI / DeckArcSegments;
                var p = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
                positions.Add(p + Vector3.up * thickness);
                positions.Add(p);
            }
            int Top(int i) => 2 + i * 2;
            int Bottom(int i) => 3 + i * 2;

            var top = new List<int>(); var bottom = new List<int>(); var rim = new List<int>(); var back = new List<int>();
            for (int i = 0; i < DeckArcSegments; i++)
            {
                AddTriangle(top, positions, 0, Top(i), Top(i + 1), Vector3.up);
                AddTriangle(bottom, positions, 1, Bottom(i), Bottom(i + 1), Vector3.down);
                var outward = positions[Top(i)] + positions[Top(i + 1)];
                AddTriangle(rim, positions, Top(i), Top(i + 1), Bottom(i + 1), outward);
                AddTriangle(rim, positions, Top(i), Bottom(i + 1), Bottom(i), outward);
            }
            // Straight back edge along the center line (faces -Z)
            AddTriangle(back, positions, Top(0), Top(DeckArcSegments), Bottom(DeckArcSegments), Vector3.back);
            AddTriangle(back, positions, Top(0), Bottom(DeckArcSegments), Bottom(0), Vector3.back);

            var faces = new List<Face> { new Face(top.ToArray()), new Face(bottom.ToArray()), new Face(rim.ToArray()), new Face(back.ToArray()) };
            var pb = ProBuilderMesh.Create(positions, faces);
            var go = pb.gameObject;
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.position = bottomCenter;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            pb.ToMesh();
            pb.Refresh();
            UnityEditor.ProBuilder.EditorMeshUtility.Optimize(pb, true);
            go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            return go;
        }

        /// <summary>Adds a triangle wound so that its normal points along 'expected' (Unity: normal = cross(b - a, c - a)).</summary>
        private static void AddTriangle(List<int> list, List<Vector3> positions, int a, int b, int c, Vector3 expected)
        {
            var n = Vector3.Cross(positions[b] - positions[a], positions[c] - positions[a]);
            if (Vector3.Dot(n, expected) >= 0f) { list.Add(a); list.Add(b); list.Add(c); }
            else { list.Add(a); list.Add(c); list.Add(b); }
        }

        private static float KitHeight(string relativePath)
        {
            // Mesh bounds are reliable on an asset that was never placed in a scene (Renderer.bounds may not be).
            var model = Kit(relativePath);
            if (model == null) return 0f;
            float min = float.MaxValue, max = float.MinValue;
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                var b = filter.sharedMesh.bounds;
                float scale = filter.transform.lossyScale.y;
                float offset = filter.transform.position.y - model.transform.position.y;
                min = Mathf.Min(min, offset + b.min.y * scale);
                max = Mathf.Max(max, offset + b.max.y * scale);
            }
            return max > min ? max - min : 0f;
        }

        /// <summary>
        /// Visual wall in module cells (2 x 3 m, or 4 x 4 m in the hangar): plain cell → wall module; window cell → window-wall module; cell with a centered 1.6 x 2.2 m door → door-wall module;
        /// anything else (windows, narrow doors, partial heights) → visible graybox fill for that cell.
        /// </summary>
        private static void BuildWallVisual(GameObject parent, Room room, ModuleSet set, bool onXLine, float line, float offsetLine,
            float from, float to, float yaw, Material mat)
        {
            bool wallKit = Kit(set.Wall) != null, doorKit = set.Cell == KitCell && Kit(KitDoorWall) != null;
            bool windowKit = set.Cell == KitCell && Kit(KitWindowWall) != null;
            // South/north walls stop at the side walls for the graybox fill (no overlapping corners).
            float fillFrom = onXLine ? from : from + WallThickness, fillTo = onXLine ? to : to - WallThickness;

            for (float cell = from; cell < to - 0.01f; cell += set.Cell)
            {
                float cellEnd = Mathf.Min(cell + set.Cell, to), mid = (cell + cellEnd) * 0.5f;
                for (float bottom = 0f; bottom < room.Height - 0.01f; bottom += set.Height)
                {
                    float top = Mathf.Min(bottom + set.Height, room.Height);
                    var ops = new List<Opening>();
                    foreach (var raw in Openings)
                        if (VisualOpening(raw) is var o && o.OnXLine == onXLine && Mathf.Approximately(o.Line, line)
                            && o.Center - o.Width * 0.5f < cellEnd && o.Center + o.Width * 0.5f > cell
                            && o.Sill < top && o.Head > bottom)
                            ops.Add(o);

                    bool fullCell = Mathf.Approximately(cellEnd - cell, set.Cell) && Mathf.Approximately(top - bottom, set.Height);
                    var pos = onXLine ? new Vector3(offsetLine, bottom, mid) : new Vector3(mid, bottom, offsetLine);
                    if (fullCell && ops.Count == 0 && wallKit)
                    {
                        PlaceKit(parent, set.Wall, pos, yaw);
                        continue;
                    }
                    if (fullCell && windowKit && ops.Count == 1 && bottom == 0f && IsWindowModuleOpening(ops[0])
                        && ops[0].Center - ops[0].Width * 0.5f <= cell + 0.01f && ops[0].Center + ops[0].Width * 0.5f >= cellEnd - 0.01f)
                    {
                        PlaceKit(parent, KitWindowWall, pos, yaw);
                        continue;
                    }
                    if (fullCell && doorKit && ops.Count == 1 && bottom == 0f
                        && Mathf.Approximately(ops[0].Width, KitDoorWidth) && Mathf.Approximately(ops[0].Center, mid)
                        && ops[0].Sill == 0f && Mathf.Approximately(ops[0].Head, KitDoorHeight))
                    {
                        PlaceKit(parent, KitDoorWall, pos, yaw);
                        continue;
                    }
                    BuildWall(parent, room, onXLine, line, offsetLine, Mathf.Max(cell, fillFrom), Mathf.Min(cellEnd, fillTo), bottom, top, mat, true, false);
                }
            }
        }

        /// <summary>
        /// Baked ceiling lights on a grid (about one per 4-5 m) in the room's color, each with an emissive strip below
        /// the ceiling as placeholder for SM_Station_Ceiling_2x2_Light. Special rooms get accent colors.
        /// </summary>
        private static void AddRoomLights(GameObject lights, GameObject fixtures, Room room)
        {
            float w = room.X1 - room.X0, d = room.Z1 - room.Z0;
            float spacing = room.Height > 10 ? 7f : room.Height > 5 ? 5f : 4f;
            float lightY = room.Name == "Hangar" ? room.Height - 1.4f : room.Height - 0.25f; // below the hangar's ceiling truss
            int nx = Mathf.Max(1, Mathf.RoundToInt(w / spacing));
            int nz = Mathf.Max(1, Mathf.RoundToInt(d / spacing));
            bool alongX = w >= d; // strips run along the longer side

            for (int ix = 0; ix < nx; ix++)
            for (int iz = 0; iz < nz; iz++)
            {
                var pos = new Vector3(room.X0 + w * (ix + 0.5f) / nx, lightY, room.Z0 + d * (iz + 0.5f) / nz);
                Color color = RoomLightColor(room, ix + iz);
                float range = spacing * 1.6f + room.Height * (room.Height > 10 ? 0.9f : 0.5f);
                BakedLight(lights, $"{room.Name}_Light_{ix}_{iz}", pos, color, range, RoomLightIntensity(room));

                var stripSize = alongX ? new Vector3(1.4f, 0.06f, 0.2f) : new Vector3(0.2f, 0.06f, 1.4f);
                var strip = Block(fixtures, $"{room.Name}_Strip_{ix}_{iz}", pos + Vector3.up * 0.2f, stripSize, EmissiveMaterial(StripColor(room, ix + iz)));
                Object.DestroyImmediate(strip.GetComponent<Collider>());
            }

            // Accents
            if (room.Name == "Hangar")
            {
                BakedLight(lights, "Hangar_DoorRed_W", new Vector3(-14.5f, 10f, -39), LightColors[Area.Danger], 14f, 4f);
                BakedLight(lights, "Hangar_DoorRed_E", new Vector3(14.5f, 10f, -39), LightColors[Area.Danger], 14f, 4f);
                BakedLight(lights, "Hangar_BarrierGlow", new Vector3(0, 6f, -38.5f), new Color(0.37f, 0.6f, 1f), 16f, 3f);
            }
            if (room.Name == "InspectionLock")
                BakedLight(lights, "Lock_WarningLamp (SM_Counter_WarningLamp)", new Vector3(3.6f, 2.5f, -9), LightColors[Area.Danger], 4f, 2.5f);
        }

        /// <summary>How strongly a room's light is tinted with its accent color (1 = full color). Strips always use full color.</summary>
        private static readonly Dictionary<Area, float> LightTint = new Dictionary<Area, float>
        {
            { Area.Neutral, 1f }, { Area.Counter, 0.5f }, { Area.Danger, 1f }, { Area.Motel, 0.4f },
            { Area.Bar, 0.8f }, { Area.Kiosk, 0.6f }, { Area.Diner, 0.75f },
        };

        /// <summary>The light's tint: the strip color, pulled toward white per area.</summary>
        private static Color RoomLightColor(Room room, int index)
        {
            if (room.Name == "Hangar") return StripColor(room, index);
            return Color.Lerp(Color.white, StripColor(room, index), LightTint[room.Area]);
        }

        /// <summary>Full accent color of a ceiling strip: hangar white work light, bar alternates orange/violet.</summary>
        private static Color StripColor(Room room, int index)
        {
            if (room.Name == "Hangar") return new Color(0.92f, 0.95f, 1f);
            return room.Area == Area.Bar && index % 2 == 1 ? new Color(0.76f, 0.48f, 0.84f) : LightColors[room.Area];
        }

        private static float RoomLightIntensity(Room room)
        {
            if (room.Name == "Hangar") return 6f;
            if (room.Height > 5) return 3f;
            return room.Area == Area.Bar || room.Area == Area.Diner ? 1.8f : 1.5f;
        }

        private static void BakedLight(GameObject parent, string name, Vector3 pos, Color color, float range, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            light.lightmapBakeType = LightmapBakeType.Baked;
        }

        private static void AddFlickerLight(GameObject parent, string name, Vector3 pos, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.position = pos;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = 4f;
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            go.AddComponent<GameOff.Core.LightFlicker>();
        }

        private static readonly Dictionary<Color, Material> EmissiveCache = new Dictionary<Color, Material>();

        private static Material EmissiveMaterial(Color color)
        {
            if (EmissiveCache.TryGetValue(color, out var cached) && cached != null) return cached;
            string name = $"M_Graybox_Emit_{ColorUtility.ToHtmlStringRGB(color)}";
            var mat = GetOrCreateMaterial(name, color, false);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
            EditorUtility.SetDirty(mat);
            EmissiveCache[color] = mat;
            return mat;
        }

        /// <summary>Placeholder blocks named after the requested assets (docs/ASSET-REQUESTS.md). Replaced by real prefabs later.</summary>
        private static void BuildProps(GameObject p)
        {
            // Counter
            KitProp(p, "Counter/SM_Counter_Desk.fbx", "SM_Counter_Desk", -0.7f, -9, 1.2f, 4.0f, 0.85f, 0, DeskYaw);
            KitProp(p, "Counter/SM_Counter_Chair.fbx", "SM_Counter_Chair", -2, -9, 0.6f, 0.6f, 1.0f, 0, ChairYaw);
            KitProp(p, "Counter/SM_Counter_Locker.fbx", "SM_Counter_Locker", -4, -6.45f, 1.0f, 0.5f, 1.8f);
            KitProp(p, "Counter/SM_Counter_Board.fbx", "SM_Counter_Board", -1.5f, -6.15f, 1.0f, 0.05f, 0.7f, 1.4f);
            KitProp(p, "Counter/SM_Counter_DocSlot.fbx", "SM_Counter_DocSlot", -0.3f, -9, 0.4f, 0.5f, 0.15f, 0.85f, DocSlotYaw);
            KitProp(p, "Counter/SM_Counter_Speaker.fbx", "SM_Counter_Speaker", -2.5f, -6.25f, 0.4f, 0.15f, 0.3f, 2.3f);
            KitProp(p, "Counter/SM_Counter_WarningLamp.fbx", "SM_Counter_WarningLamp", 2f, -9f, 0.25f, 0.25f, 0.3f, 3f); // pivot top_center → hangs from the ceiling
            // Hall (benches face north toward the observation deck: module front = local -Z → yaw 180)
            KitProp(p, "Hall/SM_Hall_DepartureBoard.fbx", "SM_Hall_DepartureBoard", 0, 0, 2, 2, 3.5f);
            KitProp(p, "Hall/SM_Hall_Bench.fbx", "SM_Hall_Bench_A", -2.5f, 3.2f, 2.8f, 0.6f, 0.9f, 0, 180f);
            KitProp(p, "Hall/SM_Hall_Bench.fbx", "SM_Hall_Bench_B", 2.5f, 3.2f, 2.8f, 0.6f, 0.9f, 0, 180f);
            KitProp(p, "Hall/SM_Hall_Bench.fbx", "SM_Hall_Bench_C", -2.5f, -3.5f, 2.8f, 0.6f, 0.9f, 0, 180f);
            KitProp(p, "Hall/SM_Hall_VendingMachine.fbx", "SM_Hall_VendingMachine", -7.4f, 4.5f, 0.8f, 1.0f, 1.9f, 0, -90f);
            KitProp(p, "Hall/SM_Hall_Sign.fbx", "SM_Hall_Sign_Staff", -7f, -5.6f, 1.2f, 0.1f, 0.4f, 3.2f, 180f); // pivot top_center
            KitProp(p, "Hall/SM_Hall_Sign.fbx", "SM_Hall_Sign_Motel", -7.6f, 1f, 0.1f, 1.2f, 0.4f, 3.2f, -90f);
            KitProp(p, "Hall/SM_Hall_Sign.fbx", "SM_Hall_Sign_Bar", 7.6f, 3f, 0.1f, 1.2f, 0.4f, 3.2f, 90f);
            Prop(p, "SM_Hall_Planter", 7.2f, 5.2f, 1, 1, 1.4f);
            // Kiosk
            Prop(p, "SM_Kiosk_Counter", 8.6f, -4, 0.8f, 2.8f, 1.0f);
            Prop(p, "SM_Kiosk_Shelf", 11.6f, -4, 0.4f, 2.8f, 2.0f);
            // Bar
            // The bar counter model is 3 m along X with the guest side at local -Z → yaw 90 faces the guests (west).
            KitProp(p, "Bar/SM_Bar_Counter.fbx", "SM_Bar_Counter_A", 14.5f, 2.5f, 0.7f, 3.0f, 1.1f, 0, 90);
            KitProp(p, "Bar/SM_Bar_Counter.fbx", "SM_Bar_Counter_B", 14.5f, 5.5f, 0.7f, 3.0f, 1.1f, 0, 90);
            KitProp(p, "Bar/SM_Bar_Shelf.fbx", "SM_Bar_Shelf", 17.6f, 4, 0.3f, 3.0f, 2.2f, 0, 90f);
            KitProp(p, "Bar/SM_Bar_NeonSign.fbx", "SM_Bar_NeonSign", 17.8f, 4f, 0.1f, 2.0f, 0.6f, 3.1f, 90f);
            // Standing tables along the north window
            KitProp(p, "Bar/SM_Bar_Table.fbx", "SM_Bar_Table_A", 10.8f, 7.1f, 0.8f, 0.8f, 1.1f);
            KitProp(p, "Bar/SM_Bar_Table.fbx", "SM_Bar_Table_B", 12.5f, 7.1f, 0.8f, 0.8f, 1.1f);
            for (int i = 0; i < 4; i++) KitProp(p, "Bar/SM_Bar_Stool.fbx", $"SM_Bar_Stool_{i + 1}", 13.6f, 1.6f + i * 1.6f, 0.4f, 0.4f, 0.75f);
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
            // Hangar (36 x 28 m): shuttle in the middle, queue lane from its ramp to the inspection lock door (x 1..3)
            KitProp(p, "Hangar/SM_Hangar_Shuttle.fbx", "SM_Hangar_Shuttle", 0, -28, 6, 9, 3.5f, 0, ShuttleYaw);
            Prop(p, "SM_Hangar_DronePad", -7.5f, -14.5f, 3, 3, 0.2f);
            Prop(p, "SM_Hangar_CargoDrone", -7.5f, -14.5f, 1.6f, 1.6f, 0.8f, 0.2f);
            // Queue lanes from the shuttle ramp to the lock door: 2 m barrier segments along Z (model is 2 m along X → yaw 90)
            for (int i = 0; i < 5; i++)
            {
                float z = -13f - i * 2f;
                KitProp(p, "Hangar/SM_Hangar_Barrier.fbx", $"SM_Hangar_Barrier_W{i}", 1, z, 0.3f, 2f, 1, 0, 90f);
                KitProp(p, "Hangar/SM_Hangar_Barrier.fbx", $"SM_Hangar_Barrier_E{i}", 3, z, 0.3f, 2f, 1, 0, 90f);
            }
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_A", -16.5f, -26, 1.2f, 1.2f, 1);
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_B", -16.5f, -27.4f, 1.2f, 1.2f, 1, 0, 90f);
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_C", -16.5f, -26.7f, 1.2f, 1.2f, 1, 1, 15f);
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_D", -15.2f, -33, 1.2f, 1.2f, 1, 0, -10f);
            KitProp(p, "Hangar/SM_Hangar_Crate_Small.fbx", "SM_Hangar_Crate_Small_A", -15.4f, -26.2f, 0.6f, 0.6f, 0.5f);
            KitProp(p, "Hangar/SM_Hangar_Crate_Small.fbx", "SM_Hangar_Crate_Small_B", -14.6f, -33.4f, 0.6f, 0.6f, 0.5f, 0, 30f);
            KitProp(p, "Hangar/SM_Hangar_Crate_Small.fbx", "SM_Hangar_Crate_Small_C", -6f, -12.6f, 0.6f, 0.6f, 0.5f);
            Prop(p, "SM_Hangar_Tank_A", 16.3f, -16, 1.4f, 1.4f, 3.5f);
            Prop(p, "SM_Hangar_Tank_B", 16.3f, -18.5f, 1.4f, 1.4f, 3.5f);
            Prop(p, "SM_Hangar_Scaffold", 12, -30, 2f, 4f, 5f);
        }

        private static void BuildMarkers(GameObject parent)
        {
            Marker(parent, "PlayerSpawn", new Vector3(-18, 0, 4), 90);
            Marker(parent, "CounterCameraAnchor", new Vector3(-2, 1.2f, -9), 90);
            Marker(parent, "TravelerStandPoint", new Vector3(2, 0, -9), -90);
            Marker(parent, "ShuttleRamp", new Vector3(0, 0, -23), 0);
            Marker(parent, "ShuttleSpawnOutside", new Vector3(0, 6, -90), 0);   // P25: shuttle flies in from here through the barrier
            Marker(parent, "ShuttleLandingPoint", new Vector3(0, 0, -28), 0);
        }

        private static void Prop(GameObject parent, string name, float x, float z, float w, float d, float h, float y = 0)
        {
            Block(parent, name, new Vector3(x, y + h * 0.5f, z), new Vector3(w, h, d), _prop);
        }

        /// <summary>
        /// Places the delivered model if it exists (with a box collider around it), otherwise the graybox block.
        /// w/d/h describe the graybox block in world axes; yaw only applies to the model (module front = local -Z).
        /// </summary>
        private static void KitProp(GameObject parent, string kitPath, string name, float x, float z, float w, float d, float h,
            float y = 0, float yaw = 0)
        {
            var model = Kit(kitPath);
            if (model == null)
            {
                Prop(parent, name, x, z, w, d, h, y);
                return;
            }
            var go = PlaceKit(parent, kitPath, Vector3.zero, 0f);
            go.name = name;
            // Collider fitted to the model's local bounds (computed before the final pose is applied).
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var r in renderers) bounds.Encapsulate(r.bounds);
                var box = go.AddComponent<BoxCollider>();
                box.center = bounds.center;
                box.size = bounds.size;
            }
            go.transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(0f, yaw, 0f));
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
            var meshRenderer = go.GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = mat;
            if (mat.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent)
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // glass must not block light
            pb.ToMesh();
            pb.Refresh();
            UnityEditor.ProBuilder.EditorMeshUtility.Optimize(pb, true); // generates lightmap UVs for baking
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
            AssetFolders.Ensure(MaterialFolder);
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
