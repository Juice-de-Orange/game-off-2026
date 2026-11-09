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
            new Opening(true,   8,  -4,   2.8f, 1.0f, 2.2f),// Kiosk hatch: counter height up to the sign strip under the lintel
            new Opening(false, -6,  -7,   1.2f, 0, 2.2f),   // Hall <-> service corridor
            new Opening(false,-12,  -7,   1.2f, 0, 2.2f),   // Service corridor <-> hangar
            new Opening(true,  -6,  -9,   1.2f, 0, 2.2f),   // Service corridor <-> counter (cell center, slides into the wall)
            new Opening(false, -6,   2,   1.6f, 0, 2.2f),   // Lock -> hall (approve)
            new Opening(false,-12,   2,   1.6f, 0, 2.2f),   // Hangar -> lock (arrive / deny)
            new Opening(true,   4,  -9,   1.6f, 0, 2.2f),   // Outer airlock hatch (closed slab added separately)
            new Opening(false,-40,   0,  28.0f, 0, 12.0f),  // Hangar mouth to space (energy barrier, P23)
            new Opening(false,  6,   0,  10.0f, 0, 6.0f),   // Hall → observation deck (full height, deck built separately)
            new Opening(true, -20,   1,   1.2f, 0, 2.2f),   // Motel corridor -> pressure tube of the hab module (P53)
            // Windows
            new Opening(false,  8,  14,   8.0f, 0.9f, 2.4f, true), // Bar window (north), matches SM_Station_Wall_Window_2x3
            new Opening(true,   0,  -9,   4.0f, 0.9f, 2.2f, true), // Counter -> lock
            new Opening(false,-12,  -4,   4.0f, 0.9f, 2.4f, true), // Counter -> hangar: two SM_Station_Wall_Window_2x3 cells
            new Opening(true, -20,   5,   1.2f, 0.9f, 2.1f, true), // Porthole room 103 (cell center, SM_Station_Wall_Porthole_Large_2x3, P51)
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
            DoorModuleSides.Clear();

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
                string floorTile = room.Name == "ServiceCorridor" && Kit(KitFloorGrate) != null ? KitFloorGrate : set.Floor;
                if (floorKit) TileRoom(roomVisual, room, floorTile, -KitHeight(floorTile), floorCell);
                // Vaulted corridor ceiling: its pivot sits on the wall top (= room height), the vault rises above it.
                if (ceilingKit && IsCorridor(room) && Kit(KitCeilingArch) != null) ArchCeiling(roomVisual, room);
                else if (ceilingKit) TileRoom(roomVisual, room, set.Ceiling, room.Height - set.CeilingDepth, ceilingCell);
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
                    !(IsWindowModuleOpening(o) && Kit(KitWindowWall) != null) && !(IsPortholeOpening(o) && Kit(KitWallPorthole) != null)
                    && ModuleForOpening(o) == null;
            }
            PlaceOpeningModules(Child(visual, "WindowModules"));

            BuildObservationDeck(root, floors, ceilings, walls, visual, glass, lights, fixtures);

            // Closed outer airlock hatch and hangar barrier
            var hatch = Block(root, "Airlock_OuterHatch_Collider", new Vector3(4.1f, 1.1f, -9), new Vector3(0.2f, 2.2f, 1.6f), _hatch);
            hatch.GetComponent<MeshRenderer>().enabled = Kit(KitHatch) == null;
            // Pivot hinge_left: the hatch spans local X 0..1.6 (= world -Z at yaw 90), so the hinge sits at the opening's north edge.
            var hatchModel = PlaceKit(visual, KitHatch, new Vector3(4f, 0f, -9f + KitDoorWidth * 0.5f), 90f); // front (local -Z) faces into the lock
            if (hatchModel != null) // P105: it moves, so it is not static
            {
                foreach (var t in hatchModel.GetComponentsInChildren<Transform>())
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
                hatchModel.AddComponent<GameOff.Station.AirlockHatch>();
            }
            BuildEnergyBarrier(root, visual);
            BuildDoors(root);
            BuildDoorSigns(visual);
            BuildRoundDetails(visual);
            BuildHull(root);

            BuildProps(props);
            BuildDecor(Child(props, "Decor"));
            BuildShuttleDock(props, lights);
            BuildDecals(Child(props, "Decals"));
            BuildMarkers(markers);

            // Walkable surface for travelers (P14) and reachability checks. Roofs and glass are not walkable.
            var surface = root.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            ceilings.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            glass.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            fixtures.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;

            OcclusionSetup.MarkStatic(root);
            StaticOcclusionCulling.Clear(); // the bake refers to the station that was just deleted

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

        /// <summary>P114: what the menu can't see from outside: every room but the hangar, just inside its walls so the hull stays.</summary>
        public static Bounds[] InteriorBounds()
        {
            var list = new List<Bounds>();
            foreach (var room in Rooms)
            {
                if (room.Name == "Hangar") continue;
                var bounds = new Bounds();
                bounds.SetMinMax(new Vector3(room.X0 + 0.05f, -0.05f, room.Z0 + 0.05f), new Vector3(room.X1 - 0.05f, room.Height - 0.02f, room.Z1 - 0.05f));
                list.Add(bounds);
            }
            return list.ToArray();
        }

        /// <summary>The floor plan's area of a room, with some room above and below for whoever stands in it.</summary>
        public static Bounds RoomBounds(string name)
        {
            foreach (var room in Rooms)
                if (room.Name == name)
                {
                    var bounds = new Bounds();
                    bounds.SetMinMax(new Vector3(room.X0, -1f, room.Z0), new Vector3(room.X1, room.Height + 1f, room.Z1));
                    return bounds;
                }
            throw new System.ArgumentException($"No room {name}");
        }

        private static bool IsHallWallEnd(bool onXLine, float line, float mid, float len) =>
            !onXLine && Mathf.Approximately(line, 5.9f) && Mathf.Approximately(len, 1f) && Mathf.Approximately(Mathf.Abs(mid), 5.5f);

        private static void WallPiece(GameObject parent, bool onXLine, float line, float from, float to, float bottom, float top,
            Material mat, bool visible, bool collider)
        {
            float len = to - from, h = top - bottom;
            if (len < 0.01f || h < 0.01f) return;
            float mid = (from + to) * 0.5f;
            if (visible && IsHallWallEnd(onXLine, line, mid, len) && Kit(KitHallWallEnd) != null)
            {
                if (bottom > 0f) return; // the 6 m piece also replaces the upper fill
                var end = PlaceKit(parent, KitHallWallEnd, new Vector3(mid, 0f, line), 0f);
                if (mid > 0f) end.transform.localScale = new Vector3(-1f, 1f, 1f); // mirrored, so the rib stays on the passage side
                return;
            }
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
        private const string KitDoorWallNarrow = "Station/SM_Station_Wall_Door_2x3_Narrow.fbx";
        private const string KitLintel = "Station/SM_Station_Lintel_4x08.fbx";
        private const string KitCounterWindowWall = "Counter/SM_Counter_WindowWall_6x3.fbx";
        private const string KitLockWindowWall = "Station/SM_Lock_WindowWall_6x3.fbx";
        private const string KitHallWallEnd = "Hall/SM_Hall_WallEnd_1x6.fbx";
        private const string KitLightStrip = "Station/SM_Station_LightStrip.fbx";
        private const string KitLockLamp = "Station/SM_Station_LockLamp.fbx";
        private const string HullFillerA = "Assets/_Project/Art/Textures/Exterior/T_Ext_HullFiller_A_Albedo.png";
        private const string HullFillerB = "Assets/_Project/Art/Textures/Exterior/T_Ext_HullFiller_B_Albedo.png";
        private const float HullFillerTileMeters = 4f;
        private const string KitWallPorthole = "Station/SM_Station_Wall_Porthole_Large_2x3.fbx";
        private const string KitWindowWall = "Station/SM_Station_Wall_Window_2x3.fbx";
        private const string KitHangarWall = "Hangar/SM_Hangar_Wall_4x4.fbx";
        private const string KitHangarFloor = "Hangar/SM_Hangar_Floor_4x4.fbx";
        private const string KitHangarCeiling = "Hangar/SM_Hangar_Ceiling_4x4.fbx";
        private const string KitDeckWindow = "Station/SM_Station_Window_Curved.fbx";
        private const string KitDeckFloor = "Station/SM_Station_Floor_DeckRound.fbx";
        private const string KitDeckCeiling = "Station/SM_Station_Ceiling_DeckRound.fbx";
        private const string KitSteps = "Station/SM_Station_Steps_2m.fbx";
        private const float KitCell = 2f, KitWallHeight = 3f, KitDoorWidth = 1.6f, KitDoorWidthNarrow = 1.2f, KitDoorHeight = 2.2f;
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

        // --- Wall variety and rounder shapes (M007). Everything is optional: missing files fall back to the base modules. ---
        private const string KitWallPadded = "Station/SM_Station_Wall_2x3_Padded.fbx";
        private const string KitWallPanel = "Station/SM_Station_Wall_2x3_Panel.fbx";
        private const string KitWallPipes = "Station/SM_Station_Wall_2x3_Pipes.fbx";
        private const string KitWallVent = "Station/SM_Station_Wall_2x3_Vent.fbx";
        private const string KitWallDamaged = "Station/SM_Station_Wall_2x3_Damaged.fbx";
        private const string KitWallScreen = "Station/SM_Station_Wall_2x3_Screen.fbx";
        private const string KitWallLockers = "Station/SM_Station_Wall_2x3_Lockers.fbx";
        private const string KitWallFiller = "Station/SM_Station_Wall_2x1.fbx";
        private const string KitFloorGrate = "Station/SM_Station_Floor_2x2_Grate.fbx";
        private const string KitCeilingLight = "Station/SM_Station_Ceiling_2x2_Light.fbx";
        private const string KitCeilingArch = "Station/SM_Station_Ceiling_Arch_2x2.fbx";
        private const string KitCornerRound = "Station/SM_Station_Wall_CornerRound.fbx";
        private const string KitPillarRound = "Station/SM_Station_Pillar_Round.fbx";
        private const string KitDoorTrim = "Station/SM_Station_DoorTrim_Round.fbx";
        private const string KitDoorTrimNarrow = "Station/SM_Station_DoorTrim_Round_Narrow.fbx";
        private const string KitWallArch = "Station/SM_Station_Wall_Arch_2x3.fbx";
        private const float ArchHead = 2.6f; // opening height of SM_Station_Wall_Arch_2x3
        private const string KitDeckArch = "Station/SM_Station_DeckArch.fbx";
        private const string KitHallDome = "Hall/SM_Hall_CeilingDome.fbx";
        private const string KitDeckRailing = "Station/SM_Deck_Railing.fbx";
        private const string KitHangarRib = "Hangar/SM_Hangar_RibArch.fbx";

        /// <summary>Stable small hash (string.GetHashCode is not guaranteed stable), so rebuilds look the same.</summary>
        private static int StableHash(string s, float a, float b, float c)
        {
            unchecked
            {
                int h = 17;
                foreach (char ch in s) h = h * 31 + ch;
                h = h * 31 + Mathf.RoundToInt(a * 10f);
                h = h * 31 + Mathf.RoundToInt(b * 10f);
                h = h * 31 + Mathf.RoundToInt(c * 10f);
                return h & 0x7fffffff;
            }
        }

        /// <summary>
        /// Picks the wall module for one cell: the room type's base wall (padded in motel/counter, wood panels in bar/diner),
        /// and in about a third of the cells a detail variant that fits the room. Upper rows only get pipes/vents.
        /// </summary>
        private static string WallVariant(Room room, float line, float mid, float bottom, bool allowDecor = true)
        {
            string baseWall = room.Area == Area.Motel || room.Area == Area.Counter ? KitWallPadded
                : room.Area == Area.Bar || room.Area == Area.Diner ? KitWallPanel : KitWall;
            if (Kit(baseWall) == null) baseWall = KitWall;
            if (!allowDecor) return baseWall;

            string[] decor;
            if (bottom > 0f) decor = new[] { KitWallPipes, KitWallVent };
            else if (room.Name.Contains("Corridor")) decor = new[] { KitWallPipes, KitWallVent, KitWallLockers, KitWallDamaged };
            else if (room.Area == Area.Neutral) decor = new[] { KitWallScreen, KitWallVent, KitWallPipes, KitWallDamaged };
            else if (room.Area == Area.Motel) decor = new[] { KitWallDamaged, KitWallVent };
            else if (room.Area == Area.Bar || room.Area == Area.Diner) decor = new[] { KitWallScreen, KitWallVent };
            else if (room.Area == Area.Counter) decor = new[] { KitWallPipes, KitWallScreen };
            else decor = new[] { KitWallPipes, KitWallVent };

            var available = new List<string>();
            foreach (var d in decor) if (Kit(d) != null) available.Add(d);
            int h = StableHash(room.Name, line, mid, bottom);
            if (available.Count > 0 && h % 100 < 35) return available[(h / 100) % available.Count];
            return baseWall;
        }

        /// <summary>Rounded inner corners, round pillars, door trims, arches, dome, railings and hangar ribs (when delivered).</summary>
        private static void BuildRoundDetails(GameObject visual)
        {
            var group = Child(visual, "RoundDetails");
            if (Kit(KitCornerRound) != null)
            {
                foreach (var room in Rooms)
                {
                    if (room.Name == "Hangar") continue; // hangar gets ribs instead
                    float t = WallThickness;
                    var corners = new[]
                    {
                        (new Vector3(room.X0 + t, 0f, room.Z0 + t), 0f), (new Vector3(room.X1 - t, 0f, room.Z0 + t), -90f),
                        (new Vector3(room.X1 - t, 0f, room.Z1 - t), 180f), (new Vector3(room.X0 + t, 0f, room.Z1 - t), 90f),
                    };
                    foreach (var (pos, yaw) in corners)
                        for (float y = 0f; y + KitWallHeight <= room.Height + 0.01f; y += KitWallHeight)
                            if (!OpeningNearCorner(room, pos, y))
                                PlaceKit(group, KitCornerRound, pos + Vector3.up * y, yaw);
                }
            }
            if (Kit(KitPillarRound) != null)
                foreach (var x in new[] { -5.6f, 5.6f })
                    for (float y = 0f; y < HallHeight - 0.01f; y += KitWallHeight)
                        PlaceKit(group, KitPillarRound, new Vector3(x, y, 5.5f), 0f);
            // Door trims only on wall sides without a door-wall module (those bring their own frame), set onto the wall face
            // instead of the wall center, where they used to z-fight with the graybox reveal.
            foreach (var d in Doors)
            {
                string trim = d.Width < 1.4f ? KitDoorTrimNarrow : KitDoorTrim;
                foreach (int side in new[] { -1, 1 })
                {
                    if (DoorModuleSides.Contains(DoorSideKey(d.OnXLine, d.Line, d.Center, side))) continue;
                    if (d.Name == "Door_HabTube" && side < 0) continue; // the tube's frame stands on that side
                    float across = d.Line + side * WallFace;
                    float yaw = d.OnXLine ? (side < 0 ? 90f : -90f) : (side < 0 ? 0f : 180f); // front (-Z) faces that side's room
                    PlaceKit(group, trim, d.OnXLine ? new Vector3(across, 0f, d.Center) : new Vector3(d.Center, 0f, across), yaw);
                }
            }
            PlaceKit(group, KitDeckArch, new Vector3(0f, 0f, DeckCenter.z), 0f);
            PlaceKit(group, KitHallDome, new Vector3(0f, HallHeight, 0f), 0f);
            if (Kit(KitDeckRailing) != null)
                for (int i = 0; i < DeckGlassSegments; i++)
                {
                    float a = (i + 0.5f) * Mathf.PI / DeckGlassSegments;
                    var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                    float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg; // front (-Z) faces the deck center
                    var pos = DeckCenter + dir * (DeckRadius * Mathf.Cos(Mathf.PI / (2f * DeckGlassSegments)) - 0.45f) + Vector3.up * -DeckDrop;
                    PlaceKit(group, KitDeckRailing, pos, yaw);
                }
            if (Kit(KitHangarRib) != null)
                for (float z = -15f; z > -40f; z -= 7f)
                    PlaceKit(group, KitHangarRib, new Vector3(0f, 16f, z), 0f);
        }

        /// <summary>
        /// True if a door, arch or window on one of the two walls of this inner corner reaches into the 0.6 m of the round
        /// corner piece (it would block the passage or cover the frame). Only openings of this room's wall and only in the
        /// corner piece's row (rowBottom..+3 m) count.
        /// </summary>
        private static bool OpeningNearCorner(Room room, Vector3 corner, float rowBottom)
        {
            const float reach = 0.6f + WallFace + 0.05f;
            foreach (var o in Openings)
            {
                float along = o.OnXLine ? corner.z : corner.x, across = o.OnXLine ? corner.x : corner.z;
                if (Mathf.Abs(across - o.Line) > WallFace + 0.01f) continue;
                float wallFrom = o.OnXLine ? room.Z0 : room.X0, wallTo = o.OnXLine ? room.Z1 : room.X1;
                if (o.Center < wallFrom || o.Center > wallTo) continue;        // opening belongs to a neighboring room
                if (o.Head <= rowBottom || o.Sill >= rowBottom + KitWallHeight) continue;
                if (Mathf.Abs(along - o.Center) - o.Width * 0.5f < reach) return true;
            }
            return false;
        }

        /// <summary>2 m wide corridors get a vaulted ceiling instead of flat tiles (arch spans the corridor width).</summary>
        private static bool IsCorridor(Room room) => Mathf.Approximately(Mathf.Min(room.X1 - room.X0, room.Z1 - room.Z0), KitCell);

        private static void ArchCeiling(GameObject parent, Room room)
        {
            bool alongZ = (room.Z1 - room.Z0) > (room.X1 - room.X0);
            float cx = (room.X0 + room.X1) * 0.5f, cz = (room.Z0 + room.Z1) * 0.5f;
            if (alongZ)
                for (float z = room.Z0 + KitCell * 0.5f; z < room.Z1; z += KitCell) PlaceKit(parent, KitCeilingArch, new Vector3(cx, room.Height, z), 0f);
            else
                for (float x = room.X0 + KitCell * 0.5f; x < room.X1; x += KitCell) PlaceKit(parent, KitCeilingArch, new Vector3(x, room.Height, cz), 90f);
        }

        // --- Window/porthole modules that fill one specific opening (placed once per opening, not per room) ---
        private const string KitHatch = "Station/SM_Station_Hatch_Airlock.fbx";
        private const string KitWindowSmall = "Station/SM_Station_Window_Small.fbx";
        private const string KitWindowSlot = "Station/SM_Station_Window_Small_Slot.fbx";
        private const string KitPorthole = "Station/SM_Station_Porthole.fbx";

        /// <summary>The module that fills this opening, or null. Counter windows (4 m, up to 2.2 m) and the porthole.</summary>
        private static string ModuleForOpening(Opening o)
        {
            if (!o.Glass) return null;
            // Openings that wall modules fill completely (window walls, porthole wall) need no separate module.
            if (IsWindowModuleOpening(o) && Kit(KitWindowWall) != null) return null;
            if (IsPortholeOpening(o) && Kit(KitWallPorthole) != null) return null;
            string path = null;
            if (o.OnXLine && Mathf.Approximately(o.Line, 0f) && Mathf.Approximately(o.Width, 4f)) path = KitWindowSlot;      // counter -> lock
            else if (o.OnXLine && Mathf.Approximately(o.Line, -20f) && o.Width < 1.5f) path = KitPorthole;                     // room 103
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
        private const float DeskYaw = 0f, ChairYaw = 90f;

        private static AudioClip DoorClip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/_Project/Audio/SFX/SFX_Door_{name}.ogg");

        private static GameObject Kit(string relativePath) => AssetDatabase.LoadAssetAtPath<GameObject>(KitRoot + relativePath);

        /// <summary>Kit models need lightmap UVs for the baked lighting. Sets the import flag once per model.</summary>
        /// <summary>What Build sets up for the helpers it shares, for a builder that runs without it.</summary>
        public static void PrepareForExtraBuild()
        {
            CreateMaterials();
            PrepareKitImport();
        }

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

        public static GameObject PlaceKit(GameObject parent, string relativePath, Vector3 position, float yaw)
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
            // Every third 2 m ceiling tile is the light variant (if delivered) for a less uniform ceiling.
            bool lightTiles = tile == KitCeiling && Kit(KitCeilingLight) != null && room.Area != Area.Diner; // P92: lights off
            int i = 0;
            for (float x = room.X0 + cell * 0.5f; x < room.X1; x += cell)
            for (float z = room.Z0 + cell * 0.5f; z < room.Z1; z += cell, i++)
                PlaceKit(parent, lightTiles && i % 3 == 1 ? KitCeilingLight : tile, new Vector3(x, y, z), 0f);
        }

        // --- Sliding doors (P27) ---
        private const string KitDoor = "Station/SM_Station_Door_Slide.fbx";
        private const string KitDoorNarrow = "Station/SM_Station_Door_Slide_Narrow.fbx";

        private readonly struct DoorSpec
        {
            public readonly string Name; public readonly bool OnXLine; public readonly float Line, Center, Width;
            public readonly GameOff.Station.DoorAccess Access;
            public readonly GameOff.Station.SlidingDoor.Role Role;
            public DoorSpec(string name, bool onXLine, float line, float center, float width, GameOff.Station.DoorAccess access,
                GameOff.Station.SlidingDoor.Role role = GameOff.Station.SlidingDoor.Role.None)
            {
                Name = name; OnXLine = onXLine; Line = line; Center = center; Width = width; Access = access; Role = role;
            }
        }

        // Must match door openings in the Openings table (docs/STATION.md).
        private static readonly DoorSpec[] Doors =
        {
            new DoorSpec("Door_HabTube",        true,  -20,   1,   1.2f, GameOff.Station.DoorAccess.Public),
            new DoorSpec("Door_Room101",        false,   2, -10,   1.2f, GameOff.Station.DoorAccess.Locked),
            new DoorSpec("Door_Room102",        false,   2, -14,   1.2f, GameOff.Station.DoorAccess.Locked),
            new DoorSpec("Door_Room103",        false,   2, -18,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Diner",          true,   -8,  -3,   1.6f, GameOff.Station.DoorAccess.Public),
            new DoorSpec("Door_Bar",            true,    8,   3,   1.6f, GameOff.Station.DoorAccess.Public),
            new DoorSpec("Door_Service_Hall",   false,  -6,  -7,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Service_Hangar", false, -12,  -7,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Service_Counter",true,   -6,  -9,   1.2f, GameOff.Station.DoorAccess.PlayerOnly),
            new DoorSpec("Door_Lock_Hall",      false,  -6,   2,   1.6f, GameOff.Station.DoorAccess.Scripted, GameOff.Station.SlidingDoor.Role.LockHall),
            new DoorSpec("Door_Lock_Hangar",    false, -12,   2,   1.6f, GameOff.Station.DoorAccess.Scripted, GameOff.Station.SlidingDoor.Role.LockHangar),
        };

        private static void BuildDoors(GameObject root)
        {
            var group = Child(root, "Doors");
            group.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true; // doorways stay walkable in the bake
            var lampMat = EmissiveMaterial(new Color(0.89f, 0.27f, 0.25f));

            foreach (var d in Doors) BuildDoor(group, d, 0f, lampMat);
        }

        /// <summary>A door of the hab module: it stands on another floor than the station's.</summary>
        public static void BuildDoorAt(GameObject group, string name, bool onXLine, float line, float center, float width, GameOff.Station.DoorAccess access, float floorY) =>
            BuildDoor(group, new DoorSpec(name, onXLine, line, center, width, access), floorY, EmissiveMaterial(new Color(0.89f, 0.27f, 0.25f)));

        private static void BuildDoor(GameObject group, DoorSpec d, float floorY, Material lampMat)
        {
            var door = new GameObject(d.Name);
            door.transform.SetParent(group.transform, false);
            door.transform.SetPositionAndRotation(
                d.OnXLine ? new Vector3(d.Line, floorY, d.Center) : new Vector3(d.Center, floorY, d.Line),
                Quaternion.Euler(0f, d.OnXLine ? 90f : 0f, 0f));

            // Sensing area on both sides of the door
            var trigger = door.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.2f, 0f);
            // Locked doors only react right in front of them, so walking down the 2 m motel corridor stays quiet.
            trigger.size = new Vector3(d.Width + 0.8f, 2.4f, d.Access == GameOff.Station.DoorAccess.Locked ? 1.4f : 3.2f);

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
                // Deeper than the 0.4 m wall so it shows on both wall faces.
                lamp = PlaceKit(door, KitLockLamp, Vector3.zero, 0f);
                if (lamp == null)
                {
                    lamp = Block(door, "LockedLamp", Vector3.zero, new Vector3(0.12f, 0.08f, 2f * WallFace + 0.04f), lampMat);
                    Object.DestroyImmediate(lamp.GetComponent<Collider>());
                }
                lamp.transform.localRotation = Quaternion.identity;
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

            if (d.Access == GameOff.Station.DoorAccess.Scripted)
            {
                // P91: "The player never enters the lock" (STATION.md). Travelers walk without colliders, so this only stops the player.
                var blocker = door.AddComponent<BoxCollider>();
                blocker.isTrigger = false;
                blocker.center = new Vector3(0f, 1.1f, 0f);
                blocker.size = new Vector3(d.Width, 2.2f, 0.2f);
            }

            var sliding = door.AddComponent<GameOff.Station.SlidingDoor>();
            var so = new SerializedObject(sliding);
            so.FindProperty("access").enumValueIndex = (int)d.Access;
            so.FindProperty("role").enumValueIndex = (int)d.Role;
            so.FindProperty("panel").objectReferenceValue = leaf;
            so.FindProperty("slideDistance").floatValue = d.Width;
            so.FindProperty("audioSource").objectReferenceValue = audio;
            so.FindProperty("openClip").objectReferenceValue = DoorClip("Open");
            so.FindProperty("closeClip").objectReferenceValue = DoorClip("Close");
            so.FindProperty("lockedClip").objectReferenceValue = DoorClip("Locked");
            so.FindProperty("lockedLamp").objectReferenceValue = lamp;
            so.FindProperty("obstacle").objectReferenceValue = obstacle;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // --- Outer hull (P39): plates over every face of the station that is seen from outside ---
        private const string KitHullPanel = "Exterior/SM_Ext_HullPanel_4x4.fbx";
        private const string KitHullPanelRibbed = "Exterior/SM_Ext_HullPanel_4x4_Ribbed.fbx";
        private const string KitRoofCap = "Exterior/SM_Ext_RoofCap_4x4.fbx";
        private const string KitHangarRibOuter = "Exterior/SM_Ext_HangarRib_Outer.fbx";
        private const string KitGreebleVent = "Exterior/SM_Ext_Greeble_VentBox.fbx";
        private const string KitGreebleTank = "Exterior/SM_Ext_Greeble_Tank.fbx";
        private const string KitPipeLarge = "Exterior/SM_Ext_Pipe_Large_4.fbx";
        private const float HullCell = 4f, HullGap = 0.04f, HullOffset = 0.15f, HullThickness = 0.2f;
        private static Material _hullA, _hullB, _hullTrim;

        /// <summary>
        /// Outer hull: every wall part not covered by a neighboring room (also above lower neighbors), every roof and the
        /// underside, in 4 m plates (delivered SM_Ext_HullPanel_4x4 / RoofCap, else graybox plates in two shades).
        /// Openings to space (windows, hangar mouth, deck, outer hatch) stay open. Hangar gets outer ribs, roofs get greebles.
        /// Not part of the baked GI (seen from far away, lit by the planet light); no colliders.
        /// </summary>
        private static void BuildHull(GameObject root)
        {
            var hull = Child(root, "Hull");
            hull.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true;
            _hullA = GetOrCreateMaterial("M_Graybox_Hull_A", new Color(0.40f, 0.42f, 0.44f), false);
            _hullB = GetOrCreateMaterial("M_Graybox_Hull_B", new Color(0.33f, 0.34f, 0.36f), false);
            _hullTrim = GetOrCreateMaterial("M_Graybox_Hull_Trim", new Color(0.22f, 0.22f, 0.24f), false);
            ApplyHullFiller(_hullA, HullFillerA);
            ApplyHullFiller(_hullB, HullFillerB);

            foreach (var room in Rooms)
            {
                float roofTop = RoofTop(room);
                foreach (var (onXLine, line, outward, from, to) in RoomSides(room))
                    foreach (var (a, b, coveredTo) in ExposedSpans(room, onXLine, line, outward, from, to))
                    {
                        // At an outer corner the plates run on past the corner (closing the corner gap), unless another room
                        // sits there (inner corner) or the facade continues in line; above a lower in-line neighbor only a strip.
                        float bottom = coveredTo > 0f ? coveredTo : -FloorThickness - 0.3f;
                        float ext = HullOffset + HullThickness * 0.5f;
                        float a2 = a, b2 = b;
                        foreach (var (atStart, corner) in new[] { (true, from), (false, to) })
                        {
                            if (!Mathf.Approximately(atStart ? a : b, corner)) continue;
                            float dir = atStart ? -1f : 1f;
                            if (RoomAt(onXLine, corner + dir * 0.1f, line + outward * 0.1f)) continue; // inner corner
                            float inLine = InLineNeighborTop(room, onXLine, line, outward, corner);
                            if (inLine < 0f) { if (atStart) a2 = a - ext; else b2 = b + ext; }
                            else if (inLine < roofTop - 0.01f)
                                HullWall(hull, room, onXLine, line, outward, atStart ? a - ext : b, atStart ? a : b + ext, Mathf.Max(bottom, inLine), roofTop);
                        }
                        HullWall(hull, room, onXLine, line, outward, a2, b2, bottom, roofTop);
                    }
                HullHorizontal(hull, room, roofTop, true);
                HullHorizontal(hull, room, -FloorThickness - 0.3f, false);
            }
            HangarRibs(hull);
            RoofGreebles(hull);
            foreach (var r in hull.GetComponentsInChildren<Renderer>())
                GameObjectUtility.SetStaticEditorFlags(r.gameObject, StaticEditorFlags.BatchingStatic);
        }

        /// <summary>Top of a room's ceiling from outside (tiles, corridor vault or the hangar's 1 m ceiling truss).</summary>
        private static float RoofTop(Room room)
        {
            if (room.Name == "Hangar") return room.Height;
            if (IsCorridor(room) && Kit(KitCeilingArch) != null) return room.Height + KitHeight(KitCeilingArch);
            return room.Height + 0.2f;
        }

        private static IEnumerable<(bool, float, int, float, float)> RoomSides(Room room)
        {
            yield return (true, room.X0, -1, room.Z0, room.Z1);
            yield return (true, room.X1, 1, room.Z0, room.Z1);
            yield return (false, room.Z0, -1, room.X0, room.X1);
            yield return (false, room.Z1, 1, room.X0, room.X1);
        }

        /// <summary>
        /// Parts of one room side seen from outside: split at neighbor boundaries; coveredTo = height up to which a neighbor
        /// on the other side hides the wall (0 = open to space). Fully hidden parts are left out.
        /// </summary>
        private static List<(float, float, float)> ExposedSpans(Room room, bool onXLine, float line, int outward, float from, float to)
        {
            var neighbors = new List<Room>();
            var cuts = new List<float> { from, to };
            foreach (var other in Rooms)
            {
                if (other.Name == room.Name) continue;
                float otherLine = onXLine ? (outward > 0 ? other.X0 : other.X1) : (outward > 0 ? other.Z0 : other.Z1);
                float o0 = onXLine ? other.Z0 : other.X0, o1 = onXLine ? other.Z1 : other.X1;
                if (!Mathf.Approximately(otherLine, line) || o1 <= from + 0.01f || o0 >= to - 0.01f) continue;
                neighbors.Add(other);
                cuts.Add(Mathf.Clamp(o0, from, to));
                cuts.Add(Mathf.Clamp(o1, from, to));
            }
            cuts.Sort();
            var spans = new List<(float, float, float)>();
            for (int i = 0; i + 1 < cuts.Count; i++)
            {
                float a = cuts[i], b = cuts[i + 1];
                if (b - a < 0.05f) continue;
                float covered = 0f;
                foreach (var n in neighbors)
                {
                    float o0 = onXLine ? n.Z0 : n.X0, o1 = onXLine ? n.Z1 : n.X1;
                    if (o0 <= a + 0.01f && o1 >= b - 0.01f) covered = Mathf.Max(covered, RoofTop(n));
                }
                if (covered < RoofTop(room) - 0.01f) spans.Add((a, b, covered)); // vaulted corridors stick out above flat neighbors
            }
            return spans;
        }

        /// <summary>One exposed wall part as 4 m plates, set just outside the wall line; openings on this line are cut out.</summary>
        private static void HullWall(GameObject hull, Room room, bool onXLine, float line, int outward, float a, float b, float bottom, float top)
        {
            float across = line + outward * HullOffset;
            float yaw = onXLine ? (outward > 0 ? -90f : 90f) : (outward > 0 ? 180f : 0f); // panel front (-Z) faces outward
            for (float u = a; u < b - 0.01f; u += HullCell)
            for (float v = bottom; v < top - 0.01f; v += HullCell)
            {
                float u1 = Mathf.Min(u + HullCell, b), v1 = Mathf.Min(v + HullCell, top);
                if ((v + v1) * 0.5f < 4f && IsDockViewportCell(room, onXLine, line, (u + u1) * 0.5f)) continue; // P106: open behind the window
                bool cut = false;
                foreach (var o in Openings)
                    if (o.OnXLine == onXLine && Mathf.Approximately(o.Line, line) && o.Center - o.Width * 0.5f < u1 && o.Center + o.Width * 0.5f > u
                        && o.Sill < v1 && o.Head > v)
                        cut = true;
                var mat = StableHash(room.Name, line, u, v) % 3 == 0 ? _hullB : _hullA;
                if (!cut && HullPanel(hull, room.Name == "Hangar" ? KitHullPanelRibbed : KitHullPanel,
                        onXLine ? new Vector3(across, (v + v1) * 0.5f, (u + u1) * 0.5f) : new Vector3((u + u1) * 0.5f, (v + v1) * 0.5f, across),
                        Quaternion.Euler(0f, yaw, 0f), u1 - u, v1 - v))
                    continue;
                // Graybox plate(s); BuildWall cuts the openings (hangar mouth, windows, deck, outer hatch).
                BuildWall(hull, room, onXLine, line, across, u + HullGap, u1 - HullGap, v + HullGap, v1 - HullGap, mat, true, false);
            }
        }

        /// <summary>
        /// Roof top of the room whose wall on the same side continues this facade in line at the given corner, or -1 if none.
        /// </summary>
        private static float InLineNeighborTop(Room room, bool onXLine, float line, int outward, float corner)
        {
            float top = -1f;
            foreach (var other in Rooms)
            {
                if (other.Name == room.Name) continue;
                float otherLine = onXLine ? (outward > 0 ? other.X1 : other.X0) : (outward > 0 ? other.Z1 : other.Z0);
                float o0 = onXLine ? other.Z0 : other.X0, o1 = onXLine ? other.Z1 : other.X1;
                if (Mathf.Approximately(otherLine, line) && (Mathf.Approximately(o0, corner) || Mathf.Approximately(o1, corner)))
                    top = Mathf.Max(top, RoofTop(other));
            }
            return top;
        }

        /// <summary>True if the floor-plan point (along the wall, across it) lies inside any room.</summary>
        private static bool RoomAt(bool onXLine, float along, float across)
        {
            float x = onXLine ? across : along, z = onXLine ? along : across;
            foreach (var r in Rooms)
                if (x > r.X0 && x < r.X1 && z > r.Z0 && z < r.Z1) return true;
            return false;
        }

        /// <summary>
        /// Overhang of a roof/underside on one side: only where no neighbor of the same (or greater) height sits there, so plates
        /// of neighboring rooms never overlap in one plane (z-fighting).
        /// </summary>
        private static float HullPad(Room room, bool onXLine, float line, int outward, float y, bool roof)
        {
            foreach (var other in Rooms)
            {
                if (other.Name == room.Name) continue;
                float otherLine = onXLine ? (outward > 0 ? other.X0 : other.X1) : (outward > 0 ? other.Z0 : other.Z1);
                float o0 = onXLine ? other.Z0 : other.X0, o1 = onXLine ? other.Z1 : other.X1;
                float r0 = onXLine ? room.Z0 : room.X0, r1 = onXLine ? room.Z1 : room.X1;
                if (!Mathf.Approximately(otherLine, line) || o1 <= r0 + 0.01f || o0 >= r1 - 0.01f) continue;
                if (!roof || RoofTop(other) >= y - 0.01f) return 0f;
            }
            return HullOffset + HullThickness * 0.5f;
        }

        /// <summary>Roof (or underside) of a room in 4 m plates, reaching over the wall plates on open sides.</summary>
        private static void HullHorizontal(GameObject hull, Room room, float y, bool roof)
        {
            float x0 = room.X0 - HullPad(room, true, room.X0, -1, y, roof), x1 = room.X1 + HullPad(room, true, room.X1, 1, y, roof);
            float z0 = room.Z0 - HullPad(room, false, room.Z0, -1, y, roof), z1 = room.Z1 + HullPad(room, false, room.Z1, 1, y, roof);
            float cy = roof ? y + HullThickness * 0.5f + 0.02f : y + HullThickness * 0.5f;
            for (float x = x0; x < x1 - 0.01f; x += HullCell)
            for (float z = z0; z < z1 - 0.01f; z += HullCell)
            {
                float xe = Mathf.Min(x + HullCell, x1), ze = Mathf.Min(z + HullCell, z1);
                var center = new Vector3((x + xe) * 0.5f, cy, (z + ze) * 0.5f);
                if (roof && Kit(KitRoofCap) != null)
                {
                    // Roof cap: 4 x 4 m flat plate (0.4 m), pivot bottom center
                    var cap = PlaceKit(hull, KitRoofCap, new Vector3(center.x, y + 0.02f, center.z), 0f);
                    cap.transform.localScale = new Vector3((xe - x) / HullCell, 1f, (ze - z) / HullCell);
                    continue;
                }
                if (roof && HullPanel(hull, KitHullPanel, center, Quaternion.Euler(90f, 0f, 0f), xe - x, ze - z)) continue;
                var mat = StableHash(room.Name, x, z, y) % 3 == 0 ? _hullB : _hullA;
                var plate = Block(hull, $"Hull_{room.Name}_{(roof ? "Roof" : "Under")}_{x:0}_{z:0}", center,
                    new Vector3(xe - x - HullGap * 2f, HullThickness, ze - z - HullGap * 2f), mat);
                Object.DestroyImmediate(plate.GetComponent<Collider>());
            }
        }

        /// <summary>Delivered hull panel scaled to the cell (pivot center, front -Z); false if not delivered yet.</summary>
        private static bool HullPanel(GameObject hull, string kit, Vector3 center, Quaternion rotation, float width, float height)
        {
            if (Kit(kit) == null) return false;
            var go = PlaceKit(hull, kit, center, 0f);
            go.transform.rotation = rotation;
            go.transform.localScale = new Vector3(width / HullCell, height / HullCell, 1f);
            return true;
        }

        /// <summary>Outer ribs on the hangar's long sides every 7 m (matching the inner rib arches), with a beam over the roof.</summary>
        private static void HangarRibs(GameObject hull)
        {
            var hangar = System.Array.Find(Rooms, r => r.Name == "Hangar");
            float top = RoofTop(hangar);
            for (float z = -15f; z > hangar.Z0; z -= 7f)
            {
                foreach (int side in new[] { -1, 1 })
                {
                    if (side < 0 && Mathf.Approximately(z, DockViewportZ)) continue; // P106: would stand right in front of the window
                    float x = side < 0 ? hangar.X0 - 0.75f : hangar.X1 + 0.75f;
                    if (Kit(KitHangarRibOuter) != null)
                    {
                        // Delivered rib is 2 m deep: its center stands 1 m outside the hull plates
                        float kx = side < 0 ? hangar.X0 - HullOffset - HullThickness * 0.5f - 1f : hangar.X1 + HullOffset + HullThickness * 0.5f + 1f;
                        PlaceKit(hull, KitHangarRibOuter, new Vector3(kx, -0.4f, z), side < 0 ? 90f : -90f);
                        continue;
                    }
                    var rib = Block(hull, $"HangarRib_{(side < 0 ? "W" : "E")}_{-z:0}", new Vector3(x, top * 0.5f, z), new Vector3(1f, top + 0.8f, 1.6f), _hullTrim);
                    Object.DestroyImmediate(rib.GetComponent<Collider>());
                }
                if (Kit(KitHangarRibOuter) != null) continue;
                var beam = Block(hull, $"HangarRib_Roof_{-z:0}", new Vector3(0f, top + 0.7f, z), new Vector3(hangar.X1 - hangar.X0 + 2.5f, 0.8f, 1.6f), _hullTrim);
                Object.DestroyImmediate(beam.GetComponent<Collider>());
            }
        }

        /// <summary>Technical clutter on the roofs (vent boxes, tanks, pipe runs) so they read as machinery, not flat boxes.</summary>
        private static void RoofGreebles(GameObject hull)
        {
            var group = Child(hull, "Greebles");
            float hangarTop = RoofTop(System.Array.Find(Rooms, r => r.Name == "Hangar")) + HullThickness + 0.02f;
            float hallTop = HallHeight + 0.2f + HullThickness + 0.02f;
            // (position on the roof, kind, yaw): 0 = vent box, 1 = tank lying, 2 = pipe run 4 m
            var items = new (Vector3 Pos, int Kind, float Yaw)[]
            {
                (new Vector3(-12f, hangarTop, -18f), 0, 0f), (new Vector3(-6f, hangarTop, -18f), 0, 0f), (new Vector3(10f, hangarTop, -20f), 0, 90f),
                (new Vector3(12f, hangarTop, -34f), 0, 0f), (new Vector3(-13f, hangarTop, -33f), 1, 90f), (new Vector3(-9f, hangarTop, -33f), 1, 90f),
                (new Vector3(4f, hangarTop, -26f), 2, 0f), (new Vector3(8f, hangarTop, -26f), 2, 0f), (new Vector3(-4f, hangarTop, -26f), 2, 0f),
                (new Vector3(-4f, hallTop, -4f), 0, 0f), (new Vector3(5.5f, hallTop, -4.5f), 1, 0f),
                (new Vector3(13f, RoofTop(System.Array.Find(Rooms, r => r.Name == "Bar")) + HullThickness + 0.02f, 6f), 0, 90f), (new Vector3(-14f, RoofTop(System.Array.Find(Rooms, r => r.Name == "Diner")) + HullThickness + 0.02f, -3f), 0, 0f),
            };
            foreach (var (pos, kind, yaw) in items)
            {
                string kit = kind == 0 ? KitGreebleVent : kind == 1 ? KitGreebleTank : KitPipeLarge;
                if (Kit(kit) != null)
                {
                    var go = PlaceKit(group, kit, pos, yaw);
                    if (kind == 1)
                    {
                        // Tank (2 m wide, 4 m tall, pivot bottom center) lies on its side, centered on the spot, resting on the roof
                        var lying = Quaternion.Euler(0f, yaw, 90f);
                        go.transform.SetPositionAndRotation(pos + Vector3.up * 1f - lying * new Vector3(0f, 2f, 0f), lying);
                    }
                    else if (kind == 2) go.transform.position = pos + Vector3.up * 0.4f; // pipe pivot is its center
                    continue;
                }
                GameObject block;
                if (kind == 0) block = Block(group, "Greeble_VentBox", pos + Vector3.up * 0.75f, new Vector3(2f, 1.5f, 1.2f), _hullTrim);
                else if (kind == 1) block = Block(group, "Greeble_Tank", pos + Vector3.up * 1f, new Vector3(4f, 2f, 2f), _hullA);
                else block = Block(group, "Greeble_Pipe", pos + Vector3.up * 0.4f, new Vector3(4f, 0.6f, 0.6f), _hullTrim);
                block.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                Object.DestroyImmediate(block.GetComponent<Collider>());
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

        /// <summary>
        /// Z range (min, max) of a kit model relative to its pivot, in the scale it is placed with; (0, 0) if missing.
        /// Same convention as KitHeight: world offsets from the asset root (unrotated), so root scale is included.
        /// </summary>
        private static (float Min, float Max) KitZRange(string relativePath)
        {
            var model = Kit(relativePath);
            if (model == null) return (0f, 0f);
            float min = float.MaxValue, max = float.MinValue;
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>())
            {
                if (filter.sharedMesh == null) continue;
                var b = filter.sharedMesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var corner = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                    float z = filter.transform.TransformPoint(corner).z - model.transform.position.z;
                    min = Mathf.Min(min, z);
                    max = Mathf.Max(max, z);
                }
            }
            return max > min ? (min, max) : (0f, 0f);
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
        /// Visual wall in module cells (2 x 3 m, or 4 x 4 m in the hangar): plain cell → wall module; window cell → window-wall module;
        /// door cell → door-wall module (1.6 m or narrow 1.2 m). On the bottom row the 2 m grid is shifted around doors that are not
        /// on a cell center, so every door gets its module; the leftover strips get the base wall squeezed to their width.
        /// Hangar: bottom cells with doors/windows become a station facade (2 m modules + 1 m filler). The kiosk hatch gets its own front.
        /// Anything else → visible graybox fill for that cell.
        /// </summary>
        private static void BuildWallVisual(GameObject parent, Room room, ModuleSet set, bool onXLine, float line, float offsetLine,
            float from, float to, float yaw, Material mat)
        {
            // The wall with the counter window is one delivered piece per side. Its opening goes down to the floor: the window
            // module fills it, so without that module the graybox has to stay.
            if (onXLine && Mathf.Approximately(line, 0f) && (room.Name == "Counter" || room.Name == "InspectionLock") && Kit(KitWindowSlot) != null)
            {
                var whole = PlaceKit(parent, room.Name == "Counter" ? KitCounterWindowWall : KitLockWindowWall,
                    new Vector3(offsetLine, 0f, (from + to) * 0.5f), yaw);
                if (whole != null) return;
            }

            var w = new WallRun
            {
                Parent = parent, Room = room, OnXLine = onXLine, Line = line, OffsetLine = offsetLine, From = from, To = to, Yaw = yaw, Mat = mat,
                // South/north walls stop at the side walls for the graybox fill (no overlapping corners).
                FillFrom = onXLine ? from : from + WallThickness, FillTo = onXLine ? to : to - WallThickness,
                Side = offsetLine > line ? 1 : -1, // the room lies on this side of the wall line
                RoundCorners = room.Name != "Hangar" && Kit(KitCornerRound) != null,
            };
            for (float bottom = 0f; bottom < room.Height - 0.01f; bottom += set.Height)
            {
                float top = Mathf.Min(bottom + set.Height, room.Height);
                var spans = CellSpans(from, to, set.Cell, onXLine, line, bottom == 0f);
                for (int i = 0; i < spans.Count; i++)
                {
                    var (cell, cellEnd) = spans[i];
                    var ops = OpeningsIn(onXLine, line, cell, cellEnd, bottom, top);
                    if (set.Cell == KitCell)
                    {
                        StationCell(w, cell, cellEnd, bottom, top, ops);
                        continue;
                    }
                    // Hangar: a station facade where small openings (doors, counter window) are, the big 4 x 4 m modules elsewhere.
                    // Neighboring facade cells are merged, so a door on a 4 m cell boundary still gets its 2 m cell.
                    if (IsFacadeCell(ops, bottom, top))
                    {
                        while (i + 1 < spans.Count && IsFacadeCell(OpeningsIn(onXLine, line, spans[i + 1].Item1, spans[i + 1].Item2, bottom, top), bottom, top))
                            cellEnd = spans[++i].Item2;
                        foreach (var (a, b) in CellSpans(cell, cellEnd, KitCell, onXLine, line, true))
                        {
                            StationCell(w, a, b, 0f, KitWallHeight, OpeningsIn(onXLine, line, a, b, 0f, KitWallHeight));
                            var filler = PlaceKit(parent, KitWallFiller, w.At((a + b) * 0.5f, KitWallHeight), yaw);
                            if (filler != null) filler.transform.localScale = new Vector3((b - a) / KitCell, 1f, 1f);
                            else BuildWall(parent, room, onXLine, line, offsetLine, a, b, KitWallHeight, top, mat, true, false);
                        }
                        continue;
                    }
                    bool full = Mathf.Approximately(cellEnd - cell, set.Cell) && Mathf.Approximately(top - bottom, set.Height);
                    bool viewport = bottom == 0f && IsDockViewportCell(room, onXLine, line, (cell + cellEnd) * 0.5f) && Kit(KitHangarViewport) != null;
                    if (full && ops.Count == 0 && viewport) PlaceKit(parent, KitHangarViewport, w.At((cell + cellEnd) * 0.5f, bottom), yaw);
                    else if (full && ops.Count == 0 && Kit(set.Wall) != null) PlaceKit(parent, set.Wall, w.At((cell + cellEnd) * 0.5f, bottom), yaw);
                    else BuildWall(parent, room, onXLine, line, offsetLine, Mathf.Max(cell, w.FillFrom), Mathf.Min(cellEnd, w.FillTo), bottom, top, mat, true, false);
                }
            }
        }

        /// <summary>Bottom hangar cell (4 m high) whose openings all fit into station modules (up to 3 m).</summary>
        private static bool IsFacadeCell(List<Opening> ops, float bottom, float top) =>
            bottom == 0f && ops.Count > 0 && ops.TrueForAll(o => o.Head <= KitWallHeight + 0.01f)
            && Kit(KitWall) != null && Mathf.Approximately(top, KitWallHeight + 1f);

        /// <summary>One wall line of one room, as seen by the visual pass.</summary>
        private class WallRun
        {
            public GameObject Parent; public Room Room; public bool OnXLine; public float Line, OffsetLine, From, To, FillFrom, FillTo, Yaw;
            public Material Mat; public int Side; public bool RoundCorners;
            public Vector3 At(float along, float y) => OnXLine ? new Vector3(OffsetLine, y, along) : new Vector3(along, y, OffsetLine);
        }

        private static List<Opening> OpeningsIn(bool onXLine, float line, float from, float to, float bottom, float top)
        {
            var ops = new List<Opening>();
            foreach (var raw in Openings)
                if (VisualOpening(raw) is var o && o.OnXLine == onXLine && Mathf.Approximately(o.Line, line)
                    && o.Center - o.Width * 0.5f < to && o.Center + o.Width * 0.5f > from && o.Sill < top && o.Head > bottom)
                    ops.Add(o);
            return ops;
        }

        /// <summary>A 2 m (or shorter) station wall cell between bottom and top.</summary>
        private static void StationCell(WallRun w, float cell, float cellEnd, float bottom, float top, List<Opening> ops)
        {
            float mid = (cell + cellEnd) * 0.5f;
            bool wallKit = Kit(KitWall) != null;
            bool fullCell = Mathf.Approximately(cellEnd - cell, KitCell) && Mathf.Approximately(top - bottom, KitWallHeight);
            var pos = w.At(mid, bottom);
            if (bottom == 0f && wallKit && ops.Exists(IsKioskHatch))
            {
                // Built once for the whole hatch (it spans two cells); the second cell is covered by it.
                var kiosk = ops.Find(IsKioskHatch);
                if (cell <= kiosk.Center - KitCell + 0.01f) BuildKioskFront(w, kiosk);
                return;
            }
            if (fullCell && ops.Count == 0 && wallKit)
            {
                // Detail variants stick out of the wall: not in corner cells (round corner pieces) or behind wall props.
                bool cornerCell = w.RoundCorners && (cell <= w.From + 0.01f || cellEnd >= w.To - 0.01f);
                bool decor = !cornerCell && !HasWallProp(w.OnXLine, w.Line, cell, cellEnd, bottom, top);
                PlaceKit(w.Parent, WallVariant(w.Room, w.Line, mid, bottom, decor), pos, w.Yaw);
                return;
            }
            // Strip next to a shifted door cell: the plain base wall squeezed to the strip width (local X runs along the wall).
            if (ops.Count == 0 && wallKit && cellEnd - cell < KitCell - 0.01f && Mathf.Approximately(top - bottom, KitWallHeight))
            {
                PlaceKit(w.Parent, WallVariant(w.Room, w.Line, mid, bottom, false), pos, w.Yaw).transform.localScale =
                    new Vector3((cellEnd - cell) / KitCell, 1f, 1f);
                return;
            }
            // 1 m top strip in 4 m rooms (bar)
            if (ops.Count == 0 && wallKit && Mathf.Approximately(cellEnd - cell, KitCell) && Mathf.Approximately(top - bottom, 1f)
                && Kit(KitWallFiller) != null)
            {
                PlaceKit(w.Parent, KitWallFiller, pos, w.Yaw);
                return;
            }
            // Open round arch (hall ↔ motel corridor): one 2 m opening centered in the cell, up to 2.6 m
            if (fullCell && ops.Count == 1 && bottom == 0f && Kit(KitWallArch) != null
                && Mathf.Approximately(ops[0].Width, KitCell) && Mathf.Approximately(ops[0].Center, mid) && !ops[0].Glass
                && ops[0].Sill == 0f && Mathf.Approximately(ops[0].Head, ArchHead))
            {
                PlaceKit(w.Parent, KitWallArch, pos, w.Yaw);
                return;
            }
            if (fullCell && Kit(KitWindowWall) != null && ops.Count == 1 && bottom == 0f && IsWindowModuleOpening(ops[0])
                && ops[0].Center - ops[0].Width * 0.5f <= cell + 0.01f && ops[0].Center + ops[0].Width * 0.5f >= cellEnd - 0.01f)
            {
                PlaceKit(w.Parent, KitWindowWall, pos, w.Yaw);
                return;
            }
            if (fullCell && ops.Count == 1 && bottom == 0f && IsPortholeOpening(ops[0]) && Mathf.Approximately(ops[0].Center, mid)
                && Kit(KitWallPorthole) != null)
            {
                PlaceKit(w.Parent, KitWallPorthole, pos, w.Yaw);
                return;
            }
            if (fullCell && ops.Count == 1 && bottom == 0f && IsDoorOpening(ops[0])
                && Mathf.Approximately(ops[0].Center, mid) && DoorWallFor(ops[0]) is string doorWall)
            {
                PlaceKit(w.Parent, doorWall, pos, w.Yaw);
                DoorModuleSides.Add(DoorSideKey(w.OnXLine, w.Line, mid, w.Side));
                return;
            }
            BuildWall(w.Parent, w.Room, w.OnXLine, w.Line, w.OffsetLine, Mathf.Max(cell, w.FillFrom), Mathf.Min(cellEnd, w.FillTo),
                bottom, top, w.Mat, true, false);
        }

        /// <summary>
        /// Cell spans along one wall: the regular grid, except on the bottom row of 2 m walls where a door that is not on a cell
        /// center gets its own 2 m span (centered on the door) and the neighbors become shorter strips.
        /// </summary>
        private static List<(float, float)> CellSpans(float from, float to, float cell, bool onXLine, float line, bool bottomRow)
        {
            var cuts = new List<float>();
            for (float c = from; c < to - 0.01f; c += cell) cuts.Add(c);
            cuts.Add(to);
            if (bottomRow && Mathf.Approximately(cell, KitCell))
                foreach (var o in Openings)
                {
                    if (!IsDoorOpening(o) || o.OnXLine != onXLine || !Mathf.Approximately(o.Line, line)) continue;
                    float a = o.Center - KitCell * 0.5f, b = o.Center + KitCell * 0.5f;
                    if (a < from - 0.01f || b > to + 0.01f) continue;
                    cuts.RemoveAll(c => c > a + 0.01f && c < b - 0.01f);
                    cuts.Add(a);
                    cuts.Add(b);
                }
            cuts.Sort();
            var spans = new List<(float, float)>();
            for (int i = 0; i + 1 < cuts.Count; i++)
                if (cuts[i + 1] - cuts[i] > 0.05f) spans.Add((cuts[i], cuts[i + 1]));
            return spans;
        }

        private static bool IsDoorOpening(Opening o) =>
            !o.Glass && o.Sill == 0f && Mathf.Approximately(o.Head, KitDoorHeight)
            && (Mathf.Approximately(o.Width, KitDoorWidth) || Mathf.Approximately(o.Width, KitDoorWidthNarrow));

        /// <summary>Door-wall module for a door opening (1.6 m or narrow 1.2 m), or null if not delivered.</summary>
        private static string DoorWallFor(Opening o)
        {
            string path = Mathf.Approximately(o.Width, KitDoorWidthNarrow) ? KitDoorWallNarrow : KitDoorWall;
            return Kit(path) != null ? path : null;
        }

        /// <summary>Distance of a room's wall face from the wall line (two 0.2 m modules back to back).</summary>
        private const float WallFace = 0.2f;

        /// <summary>
        /// Things on or against walls (onXLine, line, position along the wall, height). Cells behind them always get the plain
        /// base wall, because detail variants (vents, pipes, lockers) stick out up to 0.25 m and would clip into them.
        /// </summary>
        private static readonly (bool OnXLine, float Line, float Along, float Y)[] WallPropAnchors =
        {
            (false, -6, -1.5f, 1.75f), (false, -6, -2.5f, 2.45f), (false, -6, -4f, 0.9f),       // counter: board, speaker, locker
            (false, 0, -15f, 0.5f), (false, 0, -13f, 0.5f), (false, 0, -11f, 0.5f), (false, 0, -9f, 0.5f), // diner counter
            (false, 0, -12f, 2.3f),                                                               // diner menu board
            (false, -6, -14.5f, 0.6f), (false, -6, -13.5f, 0.6f), (false, -6, -11f, 0.6f), (false, -6, -10f, 0.6f), // booths
            (true, 18, 3f, 1f), (true, 18, 5f, 1f), (true, 18, 4f, 3.1f),                          // bar shelf, neon sign
            (true, -8, 4.5f, 1f),                                                                 // vending machine
            (true, 12, -5f, 1f), (true, 12, -3f, 1f),                                             // kiosk shelf
            (true, -20, 3.9f, 0.3f), (true, -20, 5.5f, 0.3f), (false, 6, -17f, 0.4f),             // room 103: bed, table
            (false, 6, -18.42f, 1.5f), (false, 6, -17.645f, 1.5f), (false, 2, -16.5f, 1f),         // room 103: paintings, locker
            (false, 0, -9f, 0.6f),                                                                // motel reception
            // Posters and stains (BuildDecals) stay on plain wall modules
            (false, -6, -1f, 1.6f), (true, 8, 0.6f, 1.6f), (true, -8, -9.2f, 1.5f), (false, 0, -14f, 1.6f), (false, 0, 12f, 1.9f),
            (true, -6, -8f, 2.1f), (false, 2, -12f, 1.4f), (true, -6, -10.6f, 1.2f), (false, -6, -11f, 1.9f),
            // Decor (BuildDecor), long pieces at both ends
            (false, -6, -3f, 3.2f), (false, -6, 4.5f, 4.6f), (false, -12, -1f, 2.2f), (true, -6, -7.2f, 0.8f), (false, -6, -5.2f, 2.3f),
            (true, -8, -10f, 2.3f), (true, -8, -8f, 2.3f), (true, -8, -7.5f, 1.1f), (true, -6, -11.7f, 2.5f), (true, -6, -9.7f, 2.5f), (true, -6, -7.4f, 2.8f),
            (false, 0, -15.5f, 2.5f), (false, 0, -13.5f, 2.5f), (false, 0, -11.6f, 2.3f), (false, 0, -17f, 1.1f), (false, 0, -12.6f, 0.8f), (false, 0, -18.6f, 2.4f), (false, 2, -12f, 2.8f),
            (true, 8, 3f, 2.55f), (true, -8, -3f, 2.55f), (true, 8, -2.1f, 1.7f), (true, 18, -25f, 3.2f), (true, 18, -23f, 3.2f), (false, -12, -0.4f, 1.1f), (false, -12, 0.5f, 0.8f),
        };

        private static bool HasWallProp(bool onXLine, float line, float cell, float cellEnd, float bottom, float top)
        {
            foreach (var a in WallPropAnchors)
                if (a.OnXLine == onXLine && Mathf.Approximately(a.Line, line) && a.Along > cell - 0.3f && a.Along < cellEnd + 0.3f
                    && a.Y > bottom - 0.5f && a.Y < top + 0.5f)
                    return true;
            return false;
        }

        /// <summary>
        /// A wall-mounted prop with its back on the wall face of the given side (front = local -Z faces that room).
        /// along/y: position along the wall and pivot height; w/h: graybox size if the model is missing.
        /// </summary>
        private static void WallProp(GameObject parent, string kitPath, string name, bool onXLine, float line, int side,
            float along, float y, float w, float h)
        {
            // The back (local max Z) touches the wall face, whatever the pivot depth; graybox fallback 0.1 m deep, centered.
            var (zMin, zMax) = KitZRange(kitPath);
            float depth = zMax > zMin ? zMax - zMin : 0.1f, back = zMax > zMin ? zMax : 0.05f;
            float across = line + side * (WallFace + back + 0.005f);
            float yaw = onXLine ? (side < 0 ? 90f : -90f) : (side < 0 ? 0f : 180f);
            KitProp(parent, kitPath, name, onXLine ? across : along, onXLine ? along : across,
                onXLine ? depth : w, onXLine ? w : depth, h, y, yaw);
        }

        private static bool IsPortholeOpening(Opening o) => o.Glass && o.Width < 1.5f;

        private static bool IsKioskHatch(Opening o) => !o.Glass && o.OnXLine && Mathf.Approximately(o.Line, 8f) && o.Sill > 0.5f;

        /// <summary>Wall sides (per door) that got a door-wall module; door trims only go where there is none.</summary>
        private static readonly HashSet<string> DoorModuleSides = new HashSet<string>();
        private static string DoorSideKey(bool onXLine, float line, float center, int side) => $"{onXLine}|{line:0.00}|{center:0.00}|{side}";

        /// <summary>
        /// Kiosk hatch front (hall and kiosk side): 1 m wall strips under the counter, the 4 m lintel over the hatch
        /// (the sign strip of SM_Kiosk_Counter shows just below it) and narrow side posts. Covers the two cells of the hatch.
        /// </summary>
        private static void BuildKioskFront(WallRun w, Opening hatch)
        {
            float c = hatch.Center, half = hatch.Width * 0.5f;
            string baseWall = WallVariant(w.Room, hatch.Line, c, 0f, false);
            foreach (float along in new[] { c - KitCell * 0.5f, c + KitCell * 0.5f })
                if (PlaceKit(w.Parent, KitWallFiller, w.At(along, 0f), w.Yaw) == null)
                    PlaceKit(w.Parent, baseWall, w.At(along, 0f), w.Yaw).transform.localScale = new Vector3(1f, hatch.Sill / KitWallHeight, 1f);
            float postWidth = KitCell - half; // from the cell edge to the hatch edge
            foreach (float along in new[] { c - half - postWidth * 0.5f, c + half + postWidth * 0.5f })
            {
                var post = PlaceKit(w.Parent, baseWall, w.At(along, hatch.Sill), w.Yaw);
                post.transform.localScale = new Vector3(postWidth / KitCell, (hatch.Head - hatch.Sill) / KitWallHeight, 1f); // lintel above
            }
            // Lintel from the hatch head up to the top of the row; graybox strip if the model is missing.
            if (PlaceKit(w.Parent, KitLintel, w.At(c, hatch.Head), w.Yaw) is GameObject lintel)
                lintel.transform.localScale = new Vector3(1f, (KitWallHeight - hatch.Head) / KitHeight(KitLintel), 1f);
            else
                WallPiece(w.Parent, w.OnXLine, w.OffsetLine, c - KitCell, c + KitCell, hatch.Head, KitWallHeight, w.Mat, true, false);
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

            if (room.Area == Area.Diner)
            {
                // P92: out of service, lights off. One dim lamp so the room isn't a black hole.
                BakedLight(lights, "Diner_Dim", new Vector3(-12f, 2.6f, -3f), new Color(0.55f, 0.6f, 0.7f), 7f, 0.35f);
                return;
            }

            for (int ix = 0; ix < nx; ix++)
            for (int iz = 0; iz < nz; iz++)
            {
                var pos = new Vector3(room.X0 + w * (ix + 0.5f) / nx, lightY, room.Z0 + d * (iz + 0.5f) / nz);
                Color color = RoomLightColor(room, ix + iz);
                float range = spacing * 1.6f + room.Height * (room.Height > 10 ? 0.9f : 0.5f);
                BakedLight(lights, $"{room.Name}_Light_{ix}_{iz}", pos, color, range, RoomLightIntensity(room));

                AddLightStrip(fixtures, $"{room.Name}_Strip_{ix}_{iz}", pos + Vector3.up * 0.2f, alongX, EmissiveMaterial(StripColor(room, ix + iz)));
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

        /// <summary>A baked lamp in the motel's light, for the hab module.</summary>
        public static void MotelLight(GameObject parent, string name, Vector3 pos, float range)
        {
            var room = System.Array.Find(Rooms, r => r.Name == "MotelRoom103");
            BakedLight(parent, name, pos, RoomLightColor(room, 0), range, RoomLightIntensity(room));
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

        /// <summary>Ceiling light: the delivered strip with its glow slot in the room's color, or a plain glowing bar. Center is the bar's center.</summary>
        private static void AddLightStrip(GameObject parent, string name, Vector3 center, bool alongX, Material glow)
        {
            const float height = 0.06f;
            var strip = PlaceKit(parent, KitLightStrip, center + Vector3.down * (height * 0.5f), alongX ? 0f : 90f);
            if (strip == null)
            {
                var size = alongX ? new Vector3(1.4f, height, 0.2f) : new Vector3(0.2f, height, 1.4f);
                Object.DestroyImmediate(Block(parent, name, center, size, glow).GetComponent<Collider>());
                return;
            }
            strip.name = name;
            bool colored = false;
            foreach (var r in strip.GetComponentsInChildren<Renderer>())
            {
                var materials = r.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                    if (materials[i] != null && materials[i].name.Contains("Glow"))
                    {
                        materials[i] = glow;
                        colored = true;
                    }
                r.sharedMaterials = materials;
            }
            if (!colored) Debug.LogWarning($"[Station] {KitLightStrip} has no Glow slot: {name} keeps the delivered color instead of the room's.");
        }

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
        // --- P26: posters and grime from the delivered atlases. One small mesh per atlas cell and one cut-out material per atlas, so they batch. ---
        private const string DecalMeshPath = "Assets/_Project/Art/Materials/Decals/DecalQuads.asset";

        private readonly struct WallDecal
        {
            public readonly bool Poster, OnXLine;
            public readonly int Col, Row, Side;
            public readonly float Line, Along, Y, W, H;
            public WallDecal(bool poster, int col, int row, bool onXLine, float line, int side, float along, float y, float w, float h)
            {
                Poster = poster; Col = col; Row = row; OnXLine = onXLine; Line = line; Side = side; Along = along; Y = y; W = w; H = h;
            }
        }

        private static readonly WallDecal[] WallDecals =
        {
            new WallDecal(true, 0, 0, false, -6f, 1, -1f, 1.6f, 0.7f, 1f),       // hall: A GOOD OFFICER (the Arrivals door is at x 2)
            new WallDecal(true, 1, 1, true, 8f, -1, 0.6f, 1.6f, 0.7f, 1f),       // hall: NOVA COLA
            new WallDecal(true, 0, 1, true, -8f, 1, -9.2f, 1.5f, 0.7f, 1f),      // service corridor: WANTED
            new WallDecal(true, 1, 0, false, 0f, 1, -14f, 1.6f, 0.7f, 1f),       // motel corridor: VISIT HOME
            new WallDecal(true, 1, 1, false, 0f, 1, 12f, 1.9f, 0.7f, 1f),        // bar: NOVA COLA
            new WallDecal(true, 1, 0, true, -18f, 1, -30.5f, 1.7f, 0.7f, 1f),    // hangar, by the ticket machine
            new WallDecal(false, 2, 1, true, -6f, -1, -8f, 2.1f, 0.9f, 0.9f),    // service corridor: rust runs
            new WallDecal(false, 2, 3, true, -6f, -1, -10.6f, 1.2f, 0.5f, 0.5f), // service corridor: tally marks
            new WallDecal(false, 1, 3, false, 2f, -1, -12f, 1.4f, 0.9f, 0.6f),   // motel corridor: WHO WATCHES?
            new WallDecal(false, 0, 3, false, -6f, 1, -11f, 1.9f, 1f, 1f),       // diner: graffiti
            new WallDecal(false, 1, 1, true, 18f, -1, -26f, 3f, 1.4f, 1.4f),     // hangar east wall: rust stain
        };

        private static readonly (int col, int row, float x, float z, float size, float yaw)[] FloorDecals =
        {
            (3, 1, -4f, -18f, 1.6f, 20f),   // hangar: oil
            (1, 2, 7f, -30f, 2f, 70f),      // hangar: scratches
            (2, 0, -3f, -3f, 1.3f, 0f),     // hall: dried puddle
            (3, 3, -7f, -9f, 0.9f, 180f),   // service corridor: footprints
            (3, 0, -15f, 1f, 0.8f, 40f),    // motel corridor: stains
        };

        private static void BuildDecals(GameObject parent)
        {
            var posters = DecalMaterial("Posters");
            var grime = DecalMaterial("Grime");
            if (posters == null || grime == null) return;
            // Fresh on every run; the prefab that references the meshes is rebuilt in the same run. The empty main object keeps the cell names findable.
            AssetDatabase.DeleteAsset(DecalMeshPath);
            AssetFolders.EnsureForAsset(DecalMeshPath);
            AssetDatabase.CreateAsset(new Mesh { name = "DecalQuads" }, DecalMeshPath);
            foreach (var d in WallDecals)
            {
                float across = d.Line + d.Side * (WallFace + 0.02f);
                float yaw = d.OnXLine ? (d.Side < 0 ? 90f : -90f) : (d.Side < 0 ? 0f : 180f);
                Decal(parent, d.Poster ? "Posters" : "Grime", d.Poster ? posters : grime, d.Poster ? 2 : 4, d.Col, d.Row,
                    d.OnXLine ? new Vector3(across, d.Y, d.Along) : new Vector3(d.Along, d.Y, across), Quaternion.Euler(0f, yaw, 0f), d.W, d.H);
            }
            foreach (var (col, row, x, z, size, yaw) in FloorDecals)
                Decal(parent, "Grime", grime, 4, col, row, new Vector3(x, 0.012f, z), Quaternion.Euler(90f, yaw, 0f), size, size);
        }

        private static void Decal(GameObject parent, string atlas, Material material, int cells, int col, int row, Vector3 position, Quaternion rotation, float w, float h)
        {
            var go = new GameObject($"Decal_{atlas}_{col}_{row}", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent.transform, false);
            go.transform.SetPositionAndRotation(position, rotation);
            go.transform.localScale = new Vector3(w, h, 1f);
            go.GetComponent<MeshFilter>().sharedMesh = DecalMesh(atlas, cells, col, row);
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        private static Material DecalMaterial(string atlas)
        {
            string path = $"Assets/_Project/Art/Materials/Decals/M_Decal_{atlas}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/_Project/Art/Textures/Decals/T_Decal_{atlas}.png");
            if (texture == null) return null;
            AssetFolders.EnsureForAsset(path);
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_Smoothness", 0f);
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        /// <summary>A 1 x 1 m quad facing local -Z showing one atlas cell (row 0 = top), with lightmap UVs; kept as sub-assets of one file.</summary>
        private static Mesh DecalMesh(string atlas, int cells, int col, int row)
        {
            string name = $"Decal_{atlas}_{col}_{row}";
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(DecalMeshPath))
                if (o is Mesh existing && existing.name == name) return existing;
            float u0 = col / (float)cells, u1 = (col + 1) / (float)cells, v0 = 1f - (row + 1) / (float)cells, v1 = 1f - row / (float)cells;
            var mesh = new Mesh { name = name };
            mesh.vertices = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, -0.5f, 0f) };
            mesh.uv = new[] { new Vector2(u0, v0), new Vector2(u0, v1), new Vector2(u1, v1), new Vector2(u1, v0) };
            mesh.uv2 = new[] { Vector2.zero, Vector2.up, Vector2.one, Vector2.right };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back };
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            AssetDatabase.AddObjectToAsset(mesh, DecalMeshPath);
            return mesh;
        }

        // --- P106: the staff shuttle, docked outside the west wall (hull and collar: SkyExteriorBuilder, hatch and seat: DepartureBuilder) ---
        private const string KitHangarViewport = "Hangar/SM_Hangar_Wall_Viewport.fbx";
        private const string KitShuttleCabin = "Hangar/SM_Shuttle_Cabin_Interior.fbx";
        private const float DockViewportZ = -22f, DockHatchZ = -26f;
        // Inside the hull, raised so that the cabin's underside doesn't show below the belly; the hatch fades, so its floor need not match the hangar's.
        public static readonly Vector3 ShuttleCabinCentre = new Vector3(-23.2f, 0.6f, -28f);
        private const float ShuttleCabinYaw = 180f; // the pilot's seat points to the nose (north); checked visually
        private const float CabinAisleHalfWidth = 0.5f, CabinHalfLength = 4f;

        /// <summary>The hangar wall cell with the window onto the shuttle.</summary>
        private static bool IsDockViewportCell(Room room, bool onXLine, float line, float mid) =>
            room.Name == "Hangar" && onXLine && Mathf.Approximately(line, room.X0) && Mathf.Abs(mid - DockViewportZ) < 2f;

        /// <summary>The closed hatch in the hangar wall and the cabin: only its aisle is walkable (the seats are in the model).</summary>
        private static void BuildShuttleDock(GameObject props, GameObject lights)
        {
            // A closed sliding door (the airlock hatch model says VACUUM BEYOND); it opens by fade, ShuttleHatch.
            PlaceKit(props, KitDoor, new Vector3(-17.84f, 0f, DockHatchZ), 90f);
            var cabin = PlaceKit(props, KitShuttleCabin, ShuttleCabinCentre, ShuttleCabinYaw);
            if (cabin == null) return;
            cabin.name = "SM_Shuttle_Cabin_Interior";
            var c = ShuttleCabinCentre;
            float wall = CabinAisleHalfWidth + 0.1f;
            foreach (var (name, centre, size) in new[]
            {
                ("Floor", c + new Vector3(0f, -0.1f, 0f), new Vector3(3.2f, 0.2f, 2f * CabinHalfLength)),
                ("AisleWest", c + new Vector3(-wall, 1.25f, 0f), new Vector3(0.2f, 2.5f, 2f * CabinHalfLength)),
                ("AisleEast", c + new Vector3(wall, 1.25f, 0f), new Vector3(0.2f, 2.5f, 2f * CabinHalfLength)),
                ("EndSouth", c + new Vector3(0f, 1.25f, -CabinHalfLength), new Vector3(1.4f, 2.5f, 0.2f)),
                ("EndNorth", c + new Vector3(0f, 1.25f, CabinHalfLength - 1.2f), new Vector3(1.4f, 2.5f, 0.2f)), // in front of the pilot's seat
            })
                Block(props, "ShuttleCabin_" + name, centre, size, _hatch).GetComponent<MeshRenderer>().enabled = false;
            BakedLight(lights, "ShuttleCabin_Light", c + new Vector3(0f, 2.2f, 0f), new Color(0.85f, 0.9f, 1f), 7f, 1.2f);
        }

        private static void BuildProps(GameObject p)
        {
            // Counter
            KitProp(p, "Counter/SM_Counter_Desk.fbx", "SM_Counter_Desk", -0.7f, -9, 1.2f, 4.0f, 0.85f, 0, DeskYaw);
            KitProp(p, "Counter/SM_Counter_Chair.fbx", "SM_Counter_Chair", -2, -9, 0.6f, 0.6f, 1.0f, 0, ChairYaw);
            KitProp(p, "Counter/SM_Counter_Locker.fbx", "SM_Counter_Locker", -4, -6.45f, 1.0f, 0.5f, 1.8f);
            WallProp(p, "Counter/SM_Counter_Board.fbx", "SM_Counter_Board", false, -6, -1, -1.5f, 1.4f, 1.0f, 0.7f);
            // The document drawer (SM_Counter_DocSlot) lives in PF_CounterStation (animated, P06).
            WallProp(p, "Counter/SM_Counter_Speaker.fbx", "SM_Counter_Speaker", false, -6, -1, -2.5f, 2.3f, 0.4f, 0.3f);
            KitProp(p, "Counter/SM_Counter_WarningLamp.fbx", "SM_Counter_WarningLamp", 2f, -9f, 0.25f, 0.25f, 0.3f, 3f); // pivot top_center → hangs from the ceiling
            // Hall (benches face north toward the observation deck: module front = local -Z → yaw 180)
            KitProp(p, "Hall/SM_Hall_DepartureBoard.fbx", "SM_Hall_DepartureBoard", 0, 0, 2, 2, 3.5f);
            KitProp(p, "Hall/SM_Hall_Bench.fbx", "SM_Hall_Bench_A", -2.5f, 3.2f, 2.8f, 0.6f, 0.9f, 0, 180f);
            KitProp(p, "Hall/SM_Hall_Bench.fbx", "SM_Hall_Bench_B", 2.5f, 3.2f, 2.8f, 0.6f, 0.9f, 0, 180f);
            KitProp(p, "Hall/SM_Hall_Bench.fbx", "SM_Hall_Bench_C", -2.5f, -3.5f, 2.8f, 0.6f, 0.9f, 0, 180f);
            KitProp(p, "Hall/SM_Hall_VendingMachine.fbx", "SM_Hall_VendingMachine", -7.4f, 4.5f, 0.8f, 1.0f, 1.9f, 0, -90f);
            BuildWayfinding(p); // P88: hanging signs with the right room and arrow
            KitProp(p, "Hall/SM_Hall_Planter.fbx", "SM_Hall_Planter", 7.2f, 5.2f, 1, 1, 1.4f);
            // Kiosk
            // Sign and glass face local -X = the hall side; the grille box tops out at 2.4 m.
            KitProp(p, "Kiosk/SM_Kiosk_Counter.fbx", "SM_Kiosk_Counter", 8.6f, -4, 0.8f, 2.8f, 2.4f);
            KitProp(p, "Kiosk/SM_Kiosk_Shelf.fbx", "SM_Kiosk_Shelf", 11.6f, -4, 0.4f, 2.8f, 2.0f);
            // Bar
            // The bar counter model is 3 m along X with the guest side at local -Z → yaw 90 faces the guests (west).
            KitProp(p, "Bar/SM_Bar_Counter.fbx", "SM_Bar_Counter_A", 14.5f, 2.5f, 0.7f, 3.0f, 1.1f, 0, 90);
            KitProp(p, "Bar/SM_Bar_Counter.fbx", "SM_Bar_Counter_B", 14.5f, 5.5f, 0.7f, 3.0f, 1.1f, 0, 90);
            KitProp(p, "Bar/SM_Bar_Shelf.fbx", "SM_Bar_Shelf", 17.6f, 4, 0.3f, 3.0f, 2.2f, 0, 90f);
            WallProp(p, "Bar/SM_Bar_NeonSign.fbx", "SM_Bar_NeonSign", true, 18, -1, 4f, 3.1f, 2.0f, 0.6f); // pivot center
            // Standing tables along the north window
            KitProp(p, "Bar/SM_Bar_Table.fbx", "SM_Bar_Table_A", 10.8f, 7.1f, 0.8f, 0.8f, 1.1f);
            KitProp(p, "Bar/SM_Bar_Table.fbx", "SM_Bar_Table_B", 12.5f, 7.1f, 0.8f, 0.8f, 1.1f);
            for (int i = 0; i < 4; i++) KitProp(p, "Bar/SM_Bar_Stool.fbx", $"SM_Bar_Stool_{i + 1}", 13.6f, 1.6f + i * 1.6f, 0.4f, 0.4f, 0.75f);
            // Diner
            // Diner: counter along the north wall (guest side faces south), booths against the south wall open to the north
            KitProp(p, "Diner/SM_Diner_Counter.fbx", "SM_Diner_Counter", -12, -0.6f, 6.0f, 0.7f, 1.1f);
            KitProp(p, "Diner/SM_Diner_Booth.fbx", "SM_Diner_Booth_A", -14, -5.2f, 2.2f, 1.4f, 1.2f, 0, 180f);
            KitProp(p, "Diner/SM_Diner_Booth.fbx", "SM_Diner_Booth_B", -10.5f, -5.2f, 2.2f, 1.4f, 1.2f, 0, 180f);
            for (int i = 0; i < 4; i++) KitProp(p, "Bar/SM_Bar_Stool.fbx", $"SM_Diner_Stool_{i + 1}", -14.2f + i * 1.5f, -1.4f, 0.4f, 0.4f, 0.75f);
            SwitchOffDinerCounter(p);
            // Motel
            // Motel: reception against the corridor's south wall, facing guests (north); room 103 furniture against its walls
            KitProp(p, "Motel/SM_Motel_Reception.fbx", "SM_Motel_Reception", -9, 0.45f, 1.2f, 0.5f, 1.2f, 0, 180f);
            KitProp(p, "Motel/SM_Motel_Bed.fbx", "SM_Motel_Bed", -19.29f, 4.8f, 1.0f, 2.0f, 0.6f);
            KitProp(p, "Motel/SM_Motel_Table.fbx", "SM_Motel_Table", -17, 5.5f, 0.8f, 0.6f, 0.75f);
            KitProp(p, "Motel/SM_Motel_Locker.fbx", "SM_Motel_Locker", -16.5f, 2.45f, 0.6f, 0.5f, 1.9f, 0, 180f);
            // Room 103: cat painting on the north wall (position chosen by Max), pivot center
            WallProp(p, "Motel/SM_Motel_CatPainting.fbx", "SM_Motel_CatPainting", false, 6, -1, -18.42f, 1.5f, 0.53f, 0.63f);
            // Elo, Max's dog, beside the cat with 20 cm between the frames, same center height
            WallProp(p, "Motel/SM_Motel_DogPainting.fbx", "SM_Motel_DogPainting", false, 6, -1, -17.645f, 1.5f, 0.63f, 0.53f);
            // Hangar
            // Hangar (36 x 28 m): queue lane to the inspection lock door (x 1..3); the staff shuttle docks outside the west wall (P106)
            // Drone pad west of the service door (door at x -7, z -12), so the way into the hangar stays free
            KitProp(p, "Hangar/SM_Hangar_DronePad.fbx", "SM_Hangar_DronePad", -13f, -16.5f, 3, 3, 0.2f);
            KitProp(p, "Hangar/SM_Hangar_CargoDrone.fbx", "SM_Hangar_CargoDrone", -13f, -16.5f, 1.6f, 1.6f, 0.8f, 0.2f); // on the pad top (M011)
            // Queue lanes from the shuttle ramp to the lock door: 2 m barrier segments along Z (model is 2 m along X → yaw 90)
            for (int i = 0; i < 5; i++)
            {
                float z = -13f - i * 2f;
                KitProp(p, "Hangar/SM_Hangar_Barrier.fbx", $"SM_Hangar_Barrier_W{i}", 1, z, 0.3f, 2f, 1, 0, 90f);
                KitProp(p, "Hangar/SM_Hangar_Barrier.fbx", $"SM_Hangar_Barrier_E{i}", 3, z, 0.3f, 2f, 1, 0, 90f);
            }
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_A", -16.5f, -31, 1.2f, 1.2f, 1); // kept clear of the shuttle hatch at z -26
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_B", -16.5f, -32.4f, 1.2f, 1.2f, 1, 0, 90f);
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_C", -16.5f, -31.7f, 1.2f, 1.2f, 1, 1, 15f);
            KitProp(p, "Hangar/SM_Hangar_Crate.fbx", "SM_Hangar_Crate_D", -15.2f, -33, 1.2f, 1.2f, 1, 0, -10f);
            KitProp(p, "Hangar/SM_Hangar_Crate_Small.fbx", "SM_Hangar_Crate_Small_A", -15.4f, -31.2f, 0.6f, 0.6f, 0.5f);
            KitProp(p, "Hangar/SM_Hangar_Crate_Small.fbx", "SM_Hangar_Crate_Small_B", -13.9f, -33.4f, 0.6f, 0.6f, 0.5f, 0, 30f);
            KitProp(p, "Hangar/SM_Hangar_Crate_Small.fbx", "SM_Hangar_Crate_Small_C", -11.2f, -14.6f, 0.6f, 0.6f, 0.5f); // next to the drone pad
            KitProp(p, "Hangar/SM_Hangar_Tank.fbx", "SM_Hangar_Tank_A", 16.3f, -16, 1.4f, 1.4f, 3.5f);
            KitProp(p, "Hangar/SM_Hangar_Tank.fbx", "SM_Hangar_Tank_B", 16.3f, -18.5f, 1.4f, 1.4f, 3.5f, 0, 40f);
            KitProp(p, "Hangar/SM_Hangar_Scaffold.fbx", "SM_Hangar_Scaffold", 12, -30, 2f, 4f, 5f, 0, 90f); // model is 4 m along X
            // Crane rail under the ceiling tiles (y 15), between the rib arches at z -22 and -29
            KitProp(p, "Hangar/SM_Hangar_CraneRail.fbx", "SM_Hangar_CraneRail", 0, -25.5f, 36f, 2f, 1.5f, 15f);
        }

        // --- Decor (P26): small delivered pieces that make the rooms lived in. Nothing to do with them yet. ---
        private const string KitClock = "Props/SM_Prop_WallClock.fbx", KitSpeaker = "Props/SM_Prop_PASpeaker.fbx", KitPlant = "Props/SM_Prop_PlantDead.fbx",
            KitBin = "Props/SM_Prop_TrashBin.fbx", KitBot = "Props/SM_Prop_CleaningBot.fbx", KitExtinguisher = "Props/SM_Prop_FireExtinguisher.fbx",
            KitVent = "Props/SM_Prop_VentGrate.fbx", KitCable = "Props/SM_Prop_CableBundle.fbx", KitPipe = "Props/SM_Prop_PipeBundle_2m.fbx",
            KitPipeElbow = "Props/SM_Prop_PipeElbow.fbx", KitJunction = "Props/SM_Prop_JunctionBox.fbx", KitBucket = "Props/SM_Prop_MopBucket.fbx",
            KitTrashPile = "Props/SM_Prop_TrashPile.fbx", KitFan = "Props/SM_Prop_CeilingFan.fbx", KitNeon = "Props/SM_Prop_NeonSign_Open.fbx",
            KitCan = "Props/SM_Prop_SodaCan.fbx", KitDeckBench = "Props/SM_Prop_DeckBench.fbx", KitTelescope = "Props/SM_Prop_CoinTelescope.fbx",
            KitFloppyPlayer = "Motel/SM_Motel_FloppyPlayer.fbx", KitTv = "Motel/SM_Motel_TV.fbx", KitDoorSign = "Motel/SM_Motel_DoorSign.fbx";

        private static void BuildDecor(GameObject p)
        {
            // Hall: y 0..6, doors west z 1 and -3, east z 3 and -4 (hatch), south x -7 and 2
            AddClockDisplay(WallDecor(p, KitClock, "Clock_Hall", false, -6, 1, -3f, 3.2f));
            WallDecor(p, KitSpeaker, "Speaker_Hall", false, -6, 1, 4.5f, 4.6f);
            FloorDecor(p, KitPlant, "Plant_Hall", 5.6f, -5.4f, 0.5f, 0.5f, 1f);
            FloorDecor(p, KitBin, "Bin_Hall", -7.55f, 2.7f, 0.5f, 0.5f, 0.9f);
            FloorDecor(p, KitBot, "CleaningBot", 7.4f, 0.6f, 0.6f, 0.6f, 0.4f, 0f, -90f);
            // A loop around the hall, clear of the benches, the board, the plants and the evening guests' spots (#31).
            StartBot(p.transform.Find("CleaningBot"), new Vector3(6.2f, 0f, -1.2f), new Vector3(6.2f, 0f, 4.3f), new Vector3(-6.2f, 0f, 4.8f),
                new Vector3(-6.2f, 0f, -5.2f), new Vector3(4.6f, 0f, -5.2f));

            // Counter: west door z -9, south window x -6..-2, north board and speaker x -1.5, -2.5
            AddClockDisplay(WallDecor(p, KitClock, "Clock_Counter", false, -12, 1, -1f, 2.2f));
            WallDecor(p, KitExtinguisher, "Extinguisher_Counter", true, -6, 1, -7.2f, 0.8f);
            WallDecor(p, KitVent, "Vent_Counter", false, -6, -1, -5.2f, 2.3f);

            // Service corridor (x -8..-6): doors north and south at x -7, east at z -9
            WallDecor(p, KitPipe, "Pipes_Service", true, -8, 1, -9f, 2.3f);
            WallDecor(p, KitJunction, "Junction_Service", true, -8, 1, -7.5f, 1.1f);
            WallDecor(p, KitCable, "Cables_Service", true, -6, -1, -10.7f, 2.5f);
            WallDecor(p, KitSpeaker, "Speaker_Service", true, -6, -1, -7.4f, 2.8f);
            FloorDecor(p, KitBucket, "Bucket_Service", -7.55f, -10.6f, 0.5f, 0.4f, 1.2f, 0f, 90f);

            // Motel corridor (z 0..2): doors north at x -10, -14, -18, the west end stays free for the hab door
            WallDecor(p, KitCable, "Cables_Motel", false, 0, 1, -14.5f, 2.5f);
            WallDecor(p, KitPipeElbow, "PipeElbow_Motel", false, 0, 1, -11.6f, 2.3f);
            WallDecor(p, KitJunction, "Junction_Motel", false, 0, 1, -17f, 1.1f);
            WallDecor(p, KitExtinguisher, "Extinguisher_Motel", false, 0, 1, -12.6f, 0.8f);
            WallDecor(p, KitVent, "Vent_Motel", false, 0, 1, -18.6f, 2.4f);
            WallDecor(p, KitSpeaker, "Speaker_Motel", false, 2, -1, -12f, 2.8f);
            for (int i = 0; i < 3; i++)
            {
                float x = -10f - 4f * i;
                var sign = WallDecor(p, KitDoorSign, $"DoorSign_10{i + 1}", false, 2, -1, x, 2.42f);
                var material = DoorSignMaterial(i);
                if (sign != null && material != null) sign.GetComponentInChildren<Renderer>().sharedMaterial = material;
            }

            // Room 103
            // P117: the floppy player replaces the TV as soon as it is delivered; the TV stands in until then.
            PlaceDecor(p, Kit(KitFloppyPlayer) != null ? KitFloppyPlayer : KitTv, "TV_Room103", new Vector3(-17f, 0.75f, 5.55f), 0f);
            FloorDecor(p, KitPlant, "Plant_Room103", -16.4f, 4.2f, 0.5f, 0.5f, 1f);

            // Bar and diner: fans between the ceiling strips
            Spin(PlaceDecor(p, KitFan, "Fan_Bar", new Vector3(10.5f, 4f, 4f), 0f));
            PlaceDecor(p, KitFan, "Fan_Diner", new Vector3(-12f, 3f, -3f), 0f);
            WallDecor(p, KitNeon, "Neon_Bar", true, 8, -1, 3f, 2.55f);
            BuildDinerOutOfService(p);
            WallDecor(p, KitNeon, "Neon_Kiosk", true, 8, -1, -2.1f, 1.7f);
            FloorDecor(p, KitBin, "Bin_Bar", 8.6f, 0.6f, 0.5f, 0.5f, 0.9f);
            FloorDecor(p, KitBin, "Bin_Diner", -15.5f, -2.4f, 0.5f, 0.5f, 0.9f);
            PlaceDecor(p, KitCan, "Can_BarTable", new Vector3(10.95f, 1.1f, 7f), 30f);
            PlaceDecor(p, KitCan, "Can_BarCounter", new Vector3(14.5f, 1.1f, 2.1f), 0f);
            PlaceDecor(p, KitCan, "Can_DinerCounter", new Vector3(-12.7f, 1.1f, -0.6f), 70f);

            // Hangar
            PlaceDecor(p, KitTrashPile, "Trash_Hangar_A", new Vector3(9f, 0f, -14.2f), 20f);
            PlaceDecor(p, KitTrashPile, "Trash_Hangar_B", new Vector3(-9f, 0f, -37.5f), -40f);
            WallDecor(p, KitPipe, "Pipes_Hangar", true, 18, -1, -24f, 3.2f);
            WallDecor(p, KitJunction, "Junction_Hangar", false, -12, -1, -0.4f, 1.1f);
            WallDecor(p, KitExtinguisher, "Extinguisher_Hangar", false, -12, -1, 0.5f, 0.8f);

            // Observation deck (floor y -0.36, half circle z 6..11)
            FloorDecor(p, KitDeckBench, "Bench_Deck_W", -2.2f, 7.6f, 2.4f, 0.6f, 0.5f, -DeckDrop, 180f);
            FloorDecor(p, KitDeckBench, "Bench_Deck_E", 2.2f, 7.6f, 2.4f, 0.6f, 0.5f, -DeckDrop, 180f);
            FloorDecor(p, KitTelescope, "Telescope_Deck", 0f, 9.6f, 0.5f, 0.6f, 1.5f, -DeckDrop, 180f);
        }

        /// <summary>Stands on the floor and blocks the player, or a block where the model is missing.</summary>
        private static void FloorDecor(GameObject parent, string kitPath, string name, float x, float z, float w, float d, float h, float y = 0f, float yaw = 0f) =>
            KitProp(parent, kitPath, name, x, z, w, d, h, y, yaw);

        /// <summary>A piece without collider at an exact pose; missing models are skipped, decor has no placeholder.</summary>
        private static GameObject PlaceDecor(GameObject parent, string kitPath, string name, Vector3 position, float yaw)
        {
            var go = PlaceKit(parent, kitPath, position, yaw);
            if (go != null) go.name = name;
            return go;
        }

        private enum DoorSignStyle { Neon, Stencil, Restricted }

        /// <summary>P50: doors without a sign of their own (diner and bar have theirs). (door centre on its wall line, side of the wall the text is read from, text, style).</summary>
        private static readonly (bool onXLine, float line, float center, int side, string text, DoorSignStyle style)[] DoorSigns =
        {
            (true, -20f, 1f, 1, "HAB MODULE", DoorSignStyle.Neon),
            (false, -6f, -7f, 1, "STAFF ONLY", DoorSignStyle.Restricted),
            (false, -6f, 2f, 1, "ARRIVALS", DoorSignStyle.Stencil),
            (false, -12f, -7f, 1, "HANGAR", DoorSignStyle.Stencil),
            (true, -6f, -9f, -1, "CUSTOMS", DoorSignStyle.Restricted),
            (false, -6f, -7f, -1, "HALL", DoorSignStyle.Stencil),
            (false, -12f, -7f, -1, "SERVICE", DoorSignStyle.Stencil),
        };

        /// <summary>P50: a lettered plate over each door; three looks so they don't all read the same.</summary>
        private static void BuildDoorSigns(GameObject parent)
        {
            var group = new GameObject("DoorSigns");
            group.transform.SetParent(parent.transform, false);
            var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(SignFont);
            var stencil = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/_Project/Fonts/ShareTechMono SDF.asset");
            foreach (var (onXLine, line, center, side, text, style) in DoorSigns)
            {
                float across = line + side * (WallFace + 0.04f);
                float yaw = onXLine ? (side < 0 ? 90f : -90f) : (side < 0 ? 0f : 180f);
                var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
                plate.name = "DoorSign_" + text.Replace(" ", "");
                Object.DestroyImmediate(plate.GetComponent<Collider>());
                plate.transform.SetParent(group.transform, false);
                plate.transform.SetPositionAndRotation(onXLine ? new Vector3(across, 2.5f, center) : new Vector3(center, 2.5f, across), Quaternion.Euler(0f, yaw, 0f));
                plate.transform.localScale = new Vector3(0.28f * text.Length * 0.42f + 0.25f, 0.26f, 0.03f);
                Color back = style == DoorSignStyle.Restricted ? new Color(0.45f, 0.08f, 0.06f) : style == DoorSignStyle.Stencil ? new Color(0.62f, 0.5f, 0.12f) : new Color(0.03f, 0.03f, 0.05f);
                plate.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("M_DoorSign_" + style, back, false);
                GameObjectUtility.SetStaticEditorFlags(plate, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
                var label = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshPro));
                label.transform.SetParent(group.transform, false);
                label.transform.SetPositionAndRotation(plate.transform.position + plate.transform.forward * -0.02f, plate.transform.rotation);
                label.name = plate.name + "_Text";
                ((RectTransform)label.transform).sizeDelta = new Vector2(plate.transform.localScale.x, 0.26f);
                var tmp = label.GetComponent<TMPro.TextMeshPro>();
                tmp.font = style == DoorSignStyle.Neon ? font : stencil;
                tmp.text = text;
                tmp.fontSize = 1.5f;
                tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
                tmp.color = style == DoorSignStyle.Neon ? new Color(0.61f, 0.88f, 0.35f) : style == DoorSignStyle.Restricted ? new Color(0.98f, 0.9f, 0.82f) : new Color(0.07f, 0.06f, 0.05f);
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
            }
        }

        /// <summary>P26: what moves must not be static (and is left out of the light bake).</summary>
        private static void Spin(GameObject fan)
        {
            if (fan == null) return;
            foreach (var t in fan.GetComponentsInChildren<Transform>()) GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
            fan.AddComponent<GameOff.Station.Spinner>();
        }

        private static void StartBot(Transform bot, params Vector3[] route)
        {
            if (bot == null) return;
            foreach (var t in bot.GetComponentsInChildren<Transform>()) GameObjectUtility.SetStaticEditorFlags(t.gameObject, 0);
            var patrol = bot.gameObject.AddComponent<GameOff.Station.PatrolBot>();
            var so = new SerializedObject(patrol);
            var points = so.FindProperty("route");
            points.arraySize = route.Length;
            for (int i = 0; i < route.Length; i++) points.GetArrayElementAtIndex(i).vector3Value = route[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            bot.position = route[0];
        }

        /// <summary>P92: the counter's strip light is emission in its material, so the closed diner's counter gets a copy without it.</summary>
        private static void SwitchOffDinerCounter(GameObject p)
        {
            var counter = p.transform.Find("SM_Diner_Counter");
            if (counter == null) return;
            foreach (var r in counter.GetComponentsInChildren<Renderer>(true))
            {
                var materials = r.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] == null || !materials[i].IsKeywordEnabled("_EMISSION")) continue;
                    string path = $"Assets/_Project/Art/Materials/Diner/{materials[i].name}_Off.mat";
                    var off = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (off == null)
                    {
                        off = new Material(materials[i]);
                        AssetDatabase.CreateAsset(off, path);
                    }
                    off.CopyPropertiesFromMaterial(materials[i]);
                    off.SetTexture("_EmissionMap", null);
                    off.SetColor("_EmissionColor", Color.black);
                    off.DisableKeyword("_EMISSION");
                    off.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                    EditorUtility.SetDirty(off);
                    materials[i] = off;
                }
                r.sharedMaterials = materials;
            }
        }

        private const string DinerCobwebTexture = "Assets/_Project/Art/Textures/Diner/T_Diner_Cobweb_Albedo.png";
        private const string DinerCobwebMaterial = "Assets/_Project/Art/Materials/Diner/M_Diner_Cobweb.mat";
        private const string SignFont = "Assets/_Project/Fonts/Orbitron SDF.asset";

        /// <summary>P92: the diner is out of service. A sign over its door in the hall (where the neon was), cobwebs in the dark room. The door stays open.</summary>
        private static void BuildDinerOutOfService(GameObject p)
        {
            var sign = new GameObject("Diner_OutOfService");
            sign.transform.SetParent(p.transform, false);
            sign.transform.localPosition = new Vector3(-7.76f, 2.55f, -3f);
            sign.transform.localRotation = Quaternion.Euler(0f, -90f, 0f); // text toward the hall (+X)
            var plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "Plate";
            Object.DestroyImmediate(plate.GetComponent<Collider>());
            plate.transform.SetParent(sign.transform, false);
            plate.transform.localScale = new Vector3(1.5f, 0.32f, 0.03f);
            plate.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("M_Diner_OutOfService", new Color(0.07f, 0.08f, 0.1f), false);
            GameObjectUtility.SetStaticEditorFlags(plate, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            var text = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshPro));
            text.transform.SetParent(sign.transform, false);
            text.transform.localPosition = new Vector3(0f, 0f, -0.03f);
            ((RectTransform)text.transform).sizeDelta = new Vector2(1.4f, 0.28f);
            var tmp = text.GetComponent<TMPro.TextMeshPro>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(SignFont);
            tmp.text = "OUT OF SERVICE";
            tmp.fontSize = 1.1f;
            tmp.color = new Color(0.85f, 0.25f, 0.15f);
            tmp.alignment = TMPro.TextAlignmentOptions.Center;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(DinerCobwebTexture);
            if (texture == null) return;
            var material = AssetDatabase.LoadAssetAtPath<Material>(DinerCobwebMaterial);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                AssetDatabase.CreateAsset(material, DinerCobwebMaterial);
            }
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", new Color(0.24f, 0.24f, 0.26f)); // unlit, so it must stay dim in the dark room
            material.SetFloat("_AlphaClip", 1f);
            material.SetFloat("_Cutoff", 0.5f);
            material.SetFloat("_Cull", 0f);
            material.EnableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
            EditorUtility.SetDirty(material);
            (Vector3 at, float yaw, float size)[] webs =
            {
                (new Vector3(-15.45f, 2.45f, -0.55f), 135f, 1f), (new Vector3(-15.45f, 2.45f, -5.45f), 45f, 1f), // room corners under the ceiling
                (new Vector3(-8.55f, 2.45f, -0.55f), 45f, 1f), (new Vector3(-8.55f, 2.45f, -5.45f), 135f, 1f),
            };
            for (int i = 0; i < webs.Length; i++)
            {
                var web = GameObject.CreatePrimitive(PrimitiveType.Quad);
                web.name = "Diner_Cobweb_" + i;
                Object.DestroyImmediate(web.GetComponent<Collider>());
                web.transform.SetParent(p.transform, false);
                web.transform.localPosition = webs[i].at;
                web.transform.localRotation = Quaternion.Euler(0f, webs[i].yaw, 0f);
                web.transform.localScale = Vector3.one * webs[i].size;
                web.GetComponent<MeshRenderer>().sharedMaterial = material;
                GameObjectUtility.SetStaticEditorFlags(web, StaticEditorFlags.BatchingStatic);
            }
        }

        // ---------- Wayfinding (P88) ----------
        // SM_Hall_Sign is a double-sided hanging sign (pivot top center). Its atlas T_Hall_Sign_* has one 40 px row per destination
        // and the model shows the first; both faces show the arrow pointing to the sign's local -X. Signs hang across the way, so the
        // arrow points left or right of someone walking up to them.
        private enum SignRow { Motel, Diner, Bar, Kiosk, HangarStaff, Customs }
        private const string KitHallSign = "Hall/SM_Hall_Sign.fbx";
        private const float SignHeight = 0.4f, SignGap = 0.05f, SignHalfWidth = 0.6f;

        private static void BuildWayfinding(GameObject p)
        {
            var north = Vector3.forward; var south = Vector3.back; var east = Vector3.right; var west = Vector3.left;
            // Hall, at the motel entrance: the player comes in from room 103 heading east (doors: diner x -8 z -3, service x -7 z -6,
            // kiosk x 8 z -4, bar x 8 z 3). Hall ceiling 6 m.
            HangSigns(p, "Motel", new Vector3(-5.5f, 4.2f, 1f), 6f,
                (SignRow.Customs, south), (SignRow.Diner, south), (SignRow.Kiosk, south), (SignRow.Bar, north));
            // Hall, in front of the service door: coming back from the counter heading north (motel door x -8 z 1).
            HangSigns(p, "Service", new Vector3(-6f, 4.2f, -3.5f), 6f,
                (SignRow.Motel, west), (SignRow.Diner, west), (SignRow.Bar, east), (SignRow.Kiosk, east));
            // Service corridor (x -8..-6, 3 m high): from the hall the counter door is on the right (x -6 z -9) ...
            HangSigns(p, "CorridorHall", new Vector3(-7f, 2.95f, -7.4f), 3f, (SignRow.Customs, east));
            // ... and leaving the counter, the hangar door is to the left (z -12).
            HangSigns(p, "CorridorCounter", new Vector3(-7f, 2.95f, -9.6f), 3f, (SignRow.HangarStaff, south));
        }

        /// <summary>A stack of signs hanging from the ceiling at <paramref name="top"/>; all in one stack point along one axis.</summary>
        private static void HangSigns(GameObject parent, string name, Vector3 top, float ceiling, params (SignRow row, Vector3 arrow)[] signs)
        {
            if (Kit(KitHallSign) == null) return;
            var group = new GameObject("Wayfinding_" + name);
            group.transform.SetParent(parent.transform, false);
            float yaw = 0f;
            for (int i = 0; i < signs.Length; i++)
            {
                var (row, arrow) = signs[i];
                // Local -X must point along the arrow: yaw = atan2(z, -x).
                yaw = Mathf.Atan2(arrow.z, -arrow.x) * Mathf.Rad2Deg;
                var sign = PlaceDecor(group, KitHallSign, $"SM_Hall_Sign_{row}", top + Vector3.down * (i * (SignHeight + SignGap)), yaw);
                var material = HallSignMaterial(row);
                var renderer = sign != null ? sign.GetComponentInChildren<Renderer>() : null;
                if (renderer != null && material != null) renderer.sharedMaterial = material;
            }
            // Two thin hangers from the top sign to the ceiling.
            float length = ceiling - top.y;
            if (length <= 0.01f) return;
            var along = Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
            foreach (float side in new[] { -0.45f, 0.45f })
            {
                var rod = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rod.name = "Hanger";
                Object.DestroyImmediate(rod.GetComponent<Collider>());
                rod.transform.SetParent(group.transform, false);
                rod.transform.SetPositionAndRotation(top + along * side + Vector3.up * (length * 0.5f), Quaternion.Euler(0f, yaw, 0f));
                rod.transform.localScale = new Vector3(0.02f, length, 0.02f);
                rod.GetComponent<MeshRenderer>().sharedMaterial = _prop;
                GameObjectUtility.SetStaticEditorFlags(rod, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI);
            }
        }

        /// <summary>One material per atlas row (40 of 256 px); the first row is the delivered material itself.</summary>
        private static Material HallSignMaterial(SignRow row)
        {
            const string source = "Assets/_Project/Art/Materials/Hall/M_Hall_Sign.mat";
            if (row == SignRow.Motel || AssetDatabase.LoadAssetAtPath<Material>(source) == null) return null;
            string path = $"Assets/_Project/Art/Materials/Hall/M_Hall_Sign_{row}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null) AssetDatabase.CopyAsset(source, path);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            material.SetTextureOffset("_BaseMap", new Vector2(0f, -40f / 256f * (int)row)); // URP's Lit uses it for the emission too
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>The door sign's atlas has one row per room, and the model shows the first: 101 keeps its material, the others shift it.</summary>
        private static Material DoorSignMaterial(int row)
        {
            const string source = "Assets/_Project/Art/Materials/Motel/M_Motel_DoorSign.mat";
            if (row == 0 || AssetDatabase.LoadAssetAtPath<Material>(source) == null) return null;
            string path = $"Assets/_Project/Art/Materials/Motel/M_Motel_DoorSign_10{row + 1}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(path) == null) AssetDatabase.CopyAsset(source, path);
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            material.SetTextureOffset("_BaseMap", new Vector2(0f, -0.25f * row)); // 16 of 64 pixels; URP's Lit uses it for the emission too
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>A piece on a wall face without collider: its back touches the wall, y is the height of its pivot.</summary>
        /// <summary>P97: a live readout over the painted digits of the wall clock (its front looks along local -Z).</summary>
        private static void AddClockDisplay(GameObject clock)
        {
            if (clock == null) return;
            float front = KitZRange(KitClock).Item1 - 0.002f;
            var screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = "Screen";
            Object.DestroyImmediate(screen.GetComponent<Collider>());
            screen.transform.SetParent(clock.transform, false);
            screen.transform.localPosition = new Vector3(0f, 0.02f, front);
            screen.transform.localScale = new Vector3(0.31f, 0.23f, 1f);
            screen.GetComponent<MeshRenderer>().sharedMaterial = GetOrCreateMaterial("M_Clock_Screen", new Color(0.01f, 0.01f, 0.012f), false);
            var text = new GameObject("Text", typeof(RectTransform), typeof(TMPro.TextMeshPro));
            text.transform.SetParent(clock.transform, false);
            text.transform.localPosition = new Vector3(0f, 0.02f, front - 0.003f);
            ((RectTransform)text.transform).sizeDelta = new Vector2(0.3f, 0.2f);
            var tmp = text.GetComponent<TMPro.TextMeshPro>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/_Project/Fonts/ShareTechMono SDF.asset");
            tmp.text = "08:00";
            tmp.fontSize = 1.1f;
            tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            tmp.color = new Color(0.9f, 0.16f, 0.12f);
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            var display = clock.AddComponent<GameOff.Day.ClockDisplay>();
            var so = new SerializedObject(display);
            so.FindProperty("text").objectReferenceValue = tmp;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject WallDecor(GameObject parent, string kitPath, string name, bool onXLine, float line, int side, float along, float y)
        {
            var (zMin, zMax) = KitZRange(kitPath);
            float across = line + side * (WallFace + (zMax > zMin ? zMax : 0.05f) + 0.005f);
            float yaw = onXLine ? (side < 0 ? 90f : -90f) : (side < 0 ? 0f : 180f);
            return PlaceDecor(parent, kitPath, name, onXLine ? new Vector3(across, y, along) : new Vector3(along, y, across), yaw);
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
        public static void KitProp(GameObject parent, string kitPath, string name, float x, float z, float w, float d, float h,
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

        /// <summary>Hull pieces that are no whole plate get a tiling skin; blocks have 1 UV per meter.</summary>
        private static void ApplyHullFiller(Material mat, string texturePath)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null) return;
            mat.SetColor("_BaseColor", Color.white);
            mat.SetTexture("_BaseMap", texture);
            mat.SetTextureScale("_BaseMap", Vector2.one / HullFillerTileMeters);
            EditorUtility.SetDirty(mat);
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
