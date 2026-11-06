using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GameOff.EditorTools
{
    /// <summary>
    /// The walkable hab module (P53, docs/station/wohnmodul.md): pressure tube from the motel corridor, deck, corridor and 12 rooms
    /// inside Hab_West. Own prefab, because only Main needs it: the menu sees the hab from outside. Like the rest of the station the
    /// models carry no collision, plain boxes do. Part of Rebuild All; bake the light afterwards.
    /// </summary>
    public static class StationHabBuilder
    {
        private const string RootName = "Hab_Interior";
        private const string PrefabPath = "Assets/_Project/Prefabs/Station/PF_Station_Hab.prefab";

        private const float DeckY = HabLayout.DeckY;
        private const float Thick = 0.2f;
        private const float WallHeight = 3.6f;
        // Corridor between its walls and the area of the deck
        private const float CorridorEast = -35.4f, CorridorWest = -37.6f, DeckEast = -31.3f, DeckWest = -41.7f, DeckSouth = 14f, DeckNorth = 38f;
        // Tube: its center line runs west along z 1, bends north around (BendX, BendZ) and goes on along x -36.5
        private const float TubeZ = 1f, TubeX = -36.5f, TubeHalfWidth = 1.3f, LandingEnd = -22.5f, BendX = -34.5f, BendZ = 3f, BendRadius = 2f;
        private const float DoorHalfWidth = 0.6f;
        private const float LightRange = 6f;

        [MenuItem("GameOff/Build Hab Module into Main")]
        public static void BuildFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Main.unity", OpenSceneMode.Single);
            StationGrayboxBuilder.PrepareForExtraBuild();
            Build();
            EditorSceneManager.SaveScene(scene);
        }

        public static void Build()
        {
            foreach (var old in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (old != null && old.parent == null && (old.name == RootName || old.name == "PF_Station_Hab")) Object.DestroyImmediate(old.gameObject);

            var root = new GameObject(RootName);
            var pieces = Group(root, "Pieces");
            var furniture = Group(root, "Furniture");
            var lights = Group(root, "Lights");
            var collision = Group(root, "Collision");
            var doors = Group(root, "Doors");

            foreach (var piece in HabLayout.Inside)
                StationGrayboxBuilder.PlaceKit(pieces, piece.Kit, new Vector3(piece.X, piece.Y, piece.Z), piece.Yaw);
            int number = 0;
            foreach (var piece in HabLayout.Furniture)
                StationGrayboxBuilder.KitProp(furniture, piece.Kit, $"Furniture_{number++}", piece.X, piece.Z, 0.5f, 0.5f, 1f, piece.Y, piece.Yaw);

            foreach (var door in HabLayout.Doors)
                StationGrayboxBuilder.BuildDoorAt(doors, door.Name, door.OnXLine, door.Line, door.Center, door.Width, door.Access, DeckY);

            BuildLights(lights);
            BuildDeckCollision(collision);
            BuildTubeCollision(collision);
            BuildSunBlockers(Group(root, "SunBlockers"));
            BuildNavigation(root, doors);

            OcclusionSetup.MarkStatic(root);
            StaticOcclusionCulling.Clear(); // the bake refers to what was just deleted

            AssetFolders.EnsureForAsset(PrefabPath);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);
            Debug.Log($"[Hab] Built: {HabLayout.Inside.Length} pieces, {HabLayout.Furniture.Length} furniture, {HabLayout.Doors.Length} doors");
        }

        private static GameObject Group(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static void BuildLights(GameObject parent)
        {
            float lampY = DeckY + 2.6f;
            foreach (var room in HabLayout.Rooms)
                StationGrayboxBuilder.MotelLight(parent, $"Room{room.Number}_Light", new Vector3((room.X0 + room.X1) * 0.5f, lampY, (room.Z0 + room.Z1) * 0.5f), LightRange);
            float corridorX = (CorridorEast + CorridorWest) * 0.5f;
            for (float z = DeckSouth + 4f; z < DeckNorth; z += 8f)
                StationGrayboxBuilder.MotelLight(parent, $"Corridor_Light_{z:0}", new Vector3(corridorX, lampY, z), LightRange * 1.5f);
            StationGrayboxBuilder.MotelLight(parent, "Tube_Light_Low", new Vector3(-24f, 2.2f, TubeZ), LightRange);
            StationGrayboxBuilder.MotelLight(parent, "Tube_Light_Mid", new Vector3(-30f, 5.8f, TubeZ), LightRange);
            StationGrayboxBuilder.MotelLight(parent, "Tube_Light_High", new Vector3(TubeX, lampY - 0.8f, 7f), LightRange);
        }

        /// <summary>Own walkable surface, as this prefab isn't a child of the station's. The link joins the two through the tube door.</summary>
        private static void BuildNavigation(GameObject root, GameObject doors)
        {
            var surface = root.AddComponent<Unity.AI.Navigation.NavMeshSurface>();
            surface.collectObjects = Unity.AI.Navigation.CollectObjects.Children;
            surface.useGeometry = UnityEngine.AI.NavMeshCollectGeometry.PhysicsColliders;
            doors.AddComponent<Unity.AI.Navigation.NavMeshModifier>().ignoreFromBuild = true; // doorways stay walkable in the bake

            var link = Group(root, "Link_MotelCorridor").AddComponent<Unity.AI.Navigation.NavMeshLink>();
            link.startPoint = new Vector3(-19.4f, 0f, TubeZ);
            link.endPoint = new Vector3(-20.8f, 0f, TubeZ);
            link.width = DoorHalfWidth * 2f - 0.2f;
            link.bidirectional = true;
        }

        /// <summary>Doors aren't static: without probes they would take their light from the far station floors.</summary>
        public static List<Vector3> ProbePositions()
        {
            var positions = new List<Vector3>();
            void Add(float x, float z)
            {
                positions.Add(new Vector3(x, DeckY + 0.6f, z));
                positions.Add(new Vector3(x, DeckY + 1.7f, z));
            }
            const float step = 2.5f;
            float corridorX = (CorridorEast + CorridorWest) * 0.5f;
            for (float z = DeckSouth + 1f; z < DeckNorth; z += step) Add(corridorX, z);
            foreach (var room in HabLayout.Rooms) Add((room.X0 + room.X1) * 0.5f, (room.Z0 + room.Z1) * 0.5f);
            for (float z = BendZ; z < DeckSouth; z += step) Add(TubeX, z);
            RampFrame(out var bottom, out var top, out _);
            positions.Add(new Vector3((-20f + LandingEnd) * 0.5f, 1.2f, TubeZ));
            for (float t = 0f; t <= 1f; t += 0.2f) positions.Add(Vector3.Lerp(bottom, top, t) + Vector3.up * 1.2f);
            return positions;
        }

        /// <summary>The slope of the tube's stairs, in world space.</summary>
        private static void RampFrame(out Vector3 bottom, out Vector3 top, out Vector3 up)
        {
            bottom = new Vector3(LandingEnd, 0f, TubeZ);
            top = new Vector3(BendX, DeckY, TubeZ);
            up = Vector3.Cross(Vector3.forward, (top - bottom).normalized).normalized;
            if (up.y < 0f) up = -up;
        }

        private static void BuildDeckCollision(GameObject parent)
        {
            float centerX = (DeckEast + DeckWest) * 0.5f, centerZ = (DeckSouth + DeckNorth) * 0.5f;
            Box(parent, "Deck", new Vector3(centerX, DeckY - 0.15f, centerZ), new Vector3(DeckEast - DeckWest, 0.3f, DeckNorth - DeckSouth));

            // Outer limits; only the south bulkhead has a door, in the corridor
            WallAlongZ(parent, "Wall_East", DeckEast, DeckSouth, DeckNorth);
            WallAlongZ(parent, "Wall_West", DeckWest, DeckSouth, DeckNorth);
            WallAlongX(parent, "Wall_North", DeckNorth, DeckWest, DeckEast);
            WallAlongX(parent, "Bulkhead_East", DeckSouth, TubeX + DoorHalfWidth, DeckEast);
            WallAlongX(parent, "Bulkhead_West", DeckSouth, DeckWest, TubeX - DoorHalfWidth);

            // Corridor walls with a gap at every door
            CorridorWall(parent, CorridorEast);
            CorridorWall(parent, CorridorWest);

            // Between the rooms
            for (float z = DeckSouth + 4f; z < DeckNorth - 0.1f; z += 4f)
            {
                WallAlongX(parent, $"Divider_East_{z:0}", z, CorridorEast, DeckEast);
                WallAlongX(parent, $"Divider_West_{z:0}", z, DeckWest, CorridorWest);
            }
        }

        private static void CorridorWall(GameObject parent, float x)
        {
            var doorCenters = new List<float>();
            foreach (var door in HabLayout.Doors)
                if (door.OnXLine && Mathf.Approximately(door.Line, x)) doorCenters.Add(door.Center);
            doorCenters.Sort();
            float from = DeckSouth;
            foreach (float center in doorCenters)
            {
                WallAlongZ(parent, $"Corridor_{x:0.0}_{from:0.0}", x, from, center - DoorHalfWidth);
                from = center + DoorHalfWidth;
            }
            WallAlongZ(parent, $"Corridor_{x:0.0}_{from:0.0}", x, from, DeckNorth);
        }

        private static void BuildTubeCollision(GameObject parent)
        {
            // Landing behind the door, on the level of the motel corridor
            Box(parent, "Tube_Landing", new Vector3((-20f + LandingEnd) * 0.5f, -0.15f, TubeZ), new Vector3(-20f - LandingEnd, 0.3f, TubeHalfWidth * 2f));

            // The stairs are one smooth ramp for the capsule: 31.7 degrees, under the player's slope limit of 45
            RampFrame(out var bottom, out var top, out var up);
            const float rampThickness = 0.4f;
            var rampCenter = (bottom + top) * 0.5f - up * (rampThickness * 0.5f);
            Box(parent, "Tube_Ramp", rampCenter, new Vector3((top - bottom).magnitude, rampThickness, TubeHalfWidth * 2f), Quaternion.LookRotation(Vector3.forward, up));

            // Side walls of the straight part going west, tall enough for the whole rise
            float length = -20f - BendX, height = DeckY + WallHeight + 1f;
            foreach (int side in new[] { -1, 1 })
                Box(parent, side < 0 ? "Tube_Wall_South" : "Tube_Wall_North",
                    new Vector3((-20f + BendX) * 0.5f, height * 0.5f - 1f, TubeZ + side * (TubeHalfWidth + Thick * 0.5f)), new Vector3(length, height, Thick));

            // Bend: flat floor, outer and inner wall as three straight pieces each
            float bendSize = BendRadius + TubeHalfWidth + 0.2f;
            Box(parent, "Tube_BendFloor", new Vector3(BendX - bendSize * 0.5f, DeckY - 0.15f, BendZ - bendSize * 0.5f), new Vector3(bendSize, 0.3f, bendSize));
            float outer = BendRadius + TubeHalfWidth + Thick * 0.5f, inner = BendRadius - TubeHalfWidth - Thick * 0.5f;
            const int bendParts = 3;
            for (int i = 0; i < bendParts; i++)
            {
                float angle = (i + 0.5f) * 90f / bendParts * Mathf.Deg2Rad;
                var outward = new Vector3(-Mathf.Sin(angle), 0f, -Mathf.Cos(angle));
                var center = new Vector3(BendX, DeckY + WallHeight * 0.5f, BendZ);
                float arc = 90f / bendParts * Mathf.Deg2Rad;
                Box(parent, $"Tube_BendOuter_{i}", center + outward * outer, new Vector3(outer * arc + Thick, WallHeight, Thick), Quaternion.LookRotation(outward));
                Box(parent, $"Tube_BendInner_{i}", center + outward * inner, new Vector3(inner * arc + Thick, WallHeight, Thick), Quaternion.LookRotation(outward));
            }

            // Straight north to the bulkhead
            Box(parent, "Tube_Floor_North", new Vector3(TubeX, DeckY - 0.15f, (BendZ + DeckSouth) * 0.5f), new Vector3(TubeHalfWidth * 2f, 0.3f, DeckSouth - BendZ));
            foreach (int side in new[] { -1, 1 })
                Box(parent, side < 0 ? "Tube_Wall_West" : "Tube_Wall_East",
                    new Vector3(TubeX + side * (TubeHalfWidth + Thick * 0.5f), DeckY + WallHeight * 0.5f, (BendZ + DeckSouth) * 0.5f), new Vector3(Thick, WallHeight, DeckSouth - BendZ));
        }

        /// <summary>
        /// The module has no windows, so no sun belongs inside. The outer hull lies closer to the inner walls than the shadow tolerance
        /// and left bright patches. These boxes only exist in the shadow map, never get culled, overlap at the joints, and stick out
        /// of the tube on purpose: its outside is hardly seen from Main, and the menu doesn't have them.
        /// </summary>
        private static void BuildSunBlockers(GameObject parent)
        {
            const float halfWidth = 1.75f, height = 3.5f, below = 0.2f, overlap = 1.5f;
            var material = AssetDatabase.LoadAssetAtPath<Material>(DeliveryIntegrator.HabMaterialPath);

            RampFrame(out var bottom, out var top, out var up);
            const float landingStart = -20.3f; // the motel corridor stays out of it
            Blocker(parent, material, "Blocker_Landing", new Vector3((landingStart + LandingEnd) * 0.5f, height * 0.5f - below, TubeZ), new Vector3(landingStart - LandingEnd, height, halfWidth * 2f));
            Blocker(parent, material, "Blocker_Stairs", (bottom + top) * 0.5f + up * (height * 0.5f - below),
                new Vector3((top - bottom).magnitude + overlap * 2f, height, halfWidth * 2f), Quaternion.LookRotation(Vector3.forward, up));
            float bendEast = BendX + overlap, bendWest = BendX - BendRadius - halfWidth, bendSouth = BendZ - BendRadius - halfWidth, bendNorth = BendZ + overlap;
            Blocker(parent, material, "Blocker_Bend", new Vector3((bendEast + bendWest) * 0.5f, DeckY + height * 0.5f - below, (bendSouth + bendNorth) * 0.5f),
                new Vector3(bendEast - bendWest, height, bendNorth - bendSouth));
            Blocker(parent, material, "Blocker_North", new Vector3(TubeX, DeckY + height * 0.5f - below, (BendZ + DeckSouth) * 0.5f), new Vector3(halfWidth * 2f, height, DeckSouth - BendZ));

            float roomTop = DeckY + 3.9f;
            Blocker(parent, material, "Blocker_Module", new Vector3((DeckEast + DeckWest) * 0.5f, (roomTop + DeckY - below) * 0.5f, (DeckSouth + DeckNorth) * 0.5f),
                new Vector3(DeckEast - DeckWest + 0.6f, roomTop - DeckY + below, DeckNorth - DeckSouth + overlap * 2f));
        }

        private static void Blocker(GameObject parent, Material material, string name, Vector3 center, Vector3 size, Quaternion? rotation = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent.transform, false);
            go.transform.SetPositionAndRotation(center, rotation ?? Quaternion.identity);
            go.transform.localScale = size;
            var r = go.GetComponent<MeshRenderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            r.allowOcclusionWhenDynamic = false;
        }

        private static void WallAlongZ(GameObject parent, string name, float x, float z0, float z1) =>
            Box(parent, name, new Vector3(x, DeckY + WallHeight * 0.5f, (z0 + z1) * 0.5f), new Vector3(Thick, WallHeight, z1 - z0));

        private static void WallAlongX(GameObject parent, string name, float z, float x0, float x1) =>
            Box(parent, name, new Vector3((x0 + x1) * 0.5f, DeckY + WallHeight * 0.5f, z), new Vector3(x1 - x0, WallHeight, Thick));

        private static void Box(GameObject parent, string name, Vector3 center, Vector3 size, Quaternion? rotation = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.SetPositionAndRotation(center, rotation ?? Quaternion.identity);
            go.AddComponent<BoxCollider>().size = size;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
        }
    }
}
