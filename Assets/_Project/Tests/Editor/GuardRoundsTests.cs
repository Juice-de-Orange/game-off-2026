using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.EditorTools;
using GameOff.Station;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.AI;

namespace GameOff.Tests
{
    /// <summary>
    /// P148: the guards' rounds: at most two, every waypoint on a floor and clear of walls and furniture, inside its own rooms and never
    /// in room 103, behind the counter or in the lock, away from the ships on the hangar pads and from where the arrest guard comes in;
    /// on the baked NavMesh every leg can be walked without leaving the round's rooms. The line pool is not empty; duty, pauses,
    /// looking around and giving way.
    /// </summary>
    public class GuardRoundsTests
    {
        private const string StationPrefab = "Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab";
        private const string EconomyPrefab = "Assets/_Project/Prefabs/Economy/PF_Economy.prefab";
        private const string StationNavMesh = "Assets/_Project/Scenes/Main/NavMesh-Station_Graybox.asset";
        private const float AgentRadius = 0.3f;    // as baked (STATION.md) and set on the guards' agents
        private const float ShipClearance = 0.5f;  // walking, the guard's body never touches a ship (ships are not in the NavMesh)
        private const float WaypointShipClearance = 1.5f; // standing, he keeps some room
        private static readonly float[] BodyHeights = { 0.35f, 0.8f, 1.25f, 1.6f };
        // P104: the arrest guard comes in through the lock's hall door (2, -6) and starts 0.9 m in front of it in the hall.
        private static readonly Vector3 LockHallDoor = new Vector3(2f, 0f, -6f);

        [Test]
        public void AtMostTwoGuards_EachWithARealRound()
        {
            var routes = GuardRoundsBuilder.Routes;
            Assert.That(routes.Length, Is.InRange(1, GuardRounds.MaxGuards));
            foreach (var (name, rooms, points) in routes)
            {
                Assert.GreaterOrEqual(points.Length, 2, $"{name}: a round needs at least two waypoints");
                Assert.IsNotEmpty(rooms, name);
                for (int i = 0; i < points.Length; i++)
                {
                    var next = points[(i + 1) % points.Length].position;
                    Assert.GreaterOrEqual(Flat(points[i].position - next).magnitude, 2f, $"{name}: waypoints {i} and the next are too close");
                }
            }
        }

        [Test]
        public void EveryWaypoint_StandsOnAFloor_ClearOfWallsAndFurniture()
        {
            var boxes = Boxes();
            foreach (var (name, _, points) in GuardRoundsBuilder.Routes)
                for (int i = 0; i < points.Length; i++)
                {
                    var p = points[i].position;
                    Assert.AreEqual(0f, p.y, 0.01f, $"{name} {i}: on the floor");
                    Assert.IsTrue(boxes.Exists(b => IsFloorUnder(b, p)), $"{name} {i} at {p}: no floor under it");
                    foreach (float height in BodyHeights)
                        foreach (var box in boxes)
                            if (Distance(box, p + Vector3.up * height) < AgentRadius)
                                Assert.Fail($"{name} {i} at {p} stands in {Name(box.transform)}");
                }
        }

        [Test]
        public void EveryWaypoint_InsideItsRooms_NeverInRoom103_BehindTheCounter_OrInTheLock()
        {
            foreach (var (name, rooms, points) in GuardRoundsBuilder.Routes)
                for (int i = 0; i < points.Length; i++)
                {
                    var p = points[i].position;
                    // Wall half thickness (0.1) and the body: the whole guard is in the room.
                    Assert.IsTrue(rooms.Any(r => Inside(StationGrayboxBuilder.RoomBounds(r), p, 0.1f + AgentRadius)),
                        $"{name} {i} at {p} is not inside {string.Join("/", rooms)}");
                    foreach (string forbidden in GuardRoundsBuilder.ForbiddenRooms)
                        Assert.IsFalse(Inside(StationGrayboxBuilder.RoomBounds(forbidden), p, -AgentRadius), $"{name} {i} at {p} is in {forbidden}");
                }
            foreach (var (_, rooms, _) in GuardRoundsBuilder.Routes)
                foreach (string forbidden in GuardRoundsBuilder.ForbiddenRooms)
                    CollectionAssert.DoesNotContain(rooms, forbidden);
        }

        [Test]
        public void Waypoints_KeepAwayFromTheArrestGuardsDoor()
        {
            foreach (var (name, _, points) in GuardRoundsBuilder.Routes)
                for (int i = 0; i < points.Length; i++)
                    Assert.GreaterOrEqual(Flat(points[i].position - LockHallDoor).magnitude, 3f, $"{name} {i}: in the way of the arrest guard (P104)");
        }

        [Test]
        public void Waypoints_KeepClearOfTheShipsOnTheHangarPads()
        {
            var ships = ShipFootprints();
            foreach (var (name, _, points) in GuardRoundsBuilder.Routes)
                for (int i = 0; i < points.Length; i++)
                    foreach (var ship in ships)
                        Assert.GreaterOrEqual(ship.Distance(points[i].position), AgentRadius + WaypointShipClearance,
                            $"{name} {i} at {points[i].position}: stands right next to {ship.Name}");
        }

        [Test]
        public void OnTheBakedNavMesh_EveryLegCanBeWalked_WithoutLeavingTheRoundsRooms()
        {
            var data = AssetDatabase.LoadAssetAtPath<NavMeshData>(StationNavMesh);
            if (data == null) Assert.Ignore(StationNavMesh + " is not baked yet");
            var ships = ShipFootprints();
            var boxes = Boxes(EconomyPrefab); // runtime things on the floor (the station's own are in the NavMesh already)
            var instance = NavMesh.AddNavMeshData(data);
            try
            {
                if (!NavMesh.SamplePosition(new Vector3(3f, 0f, -2f), out _, 0.5f, NavMesh.AllAreas))
                    Assert.Ignore("no NavMesh queries here (the hall itself is not found)");
                var path = new NavMeshPath();
                foreach (var (name, rooms, points) in GuardRoundsBuilder.Routes)
                {
                    var areas = rooms.Select(StationGrayboxBuilder.RoomBounds).ToArray();
                    var forbidden = GuardRoundsBuilder.ForbiddenRooms.Select(StationGrayboxBuilder.RoomBounds).ToArray();
                    for (int i = 0; i < points.Length; i++)
                    {
                        var from = points[i].position;
                        var to = points[(i + 1) % points.Length].position;
                        Assert.IsTrue(NavMesh.SamplePosition(from, out var hit, 0.3f, NavMesh.AllAreas), $"{name} {i} at {from}: not on the NavMesh");
                        Assert.Less(Flat(hit.position - from).magnitude, 0.2f, $"{name} {i}: the NavMesh is {hit.position}, not under the waypoint");
                        Assert.IsTrue(NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path), $"{name} {i}: no path to the next waypoint");
                        Assert.AreEqual(NavMeshPathStatus.PathComplete, path.status, $"{name} {i}: the way to the next waypoint is cut");
                        foreach (var p in Along(path.corners, 0.25f))
                        {
                            Assert.IsTrue(areas.Any(a => a.Contains(p)), $"{name} leg {i}: leaves {string.Join("/", rooms)} at {p}");
                            Assert.IsFalse(forbidden.Any(f => f.Contains(p)), $"{name} leg {i}: passes a forbidden room at {p}");
                            foreach (var ship in ships)
                                Assert.GreaterOrEqual(ship.Distance(p), AgentRadius + ShipClearance, $"{name} leg {i}: brushes {ship.Name} at {p}");
                            foreach (float height in BodyHeights)
                                foreach (var box in boxes)
                                    if (Distance(box, p + Vector3.up * height) < AgentRadius)
                                        Assert.Fail($"{name} leg {i}: walks through {Name(box.transform)} at {p}");
                        }
                    }
                }
            }
            finally { NavMesh.RemoveNavMeshData(instance); }
        }

        [Test]
        public void LinePools_AreNotEmpty()
        {
            var defaults = ScriptableObject.CreateInstance<GuardTuning>();
            try
            {
                Assert.IsNotEmpty(defaults.lines);
                Assert.IsNotEmpty(defaults.curfewLines);
                foreach (string line in defaults.lines.Concat(defaults.curfewLines)) Assert.IsFalse(string.IsNullOrWhiteSpace(line));
            }
            finally { Object.DestroyImmediate(defaults); }
            var tuning = AssetDatabase.LoadAssetAtPath<GuardTuning>(GuardRoundsBuilder.TuningPath);
            if (tuning == null) return; // made by GameOff/Build Guard Rounds into Main
            Assert.IsNotEmpty(tuning.lines, $"{GuardRoundsBuilder.TuningPath}: no lines (Build Guard Rounds fills an empty pool)");
            Assert.IsTrue(tuning.lines.All(l => !string.IsNullOrWhiteSpace(l)), $"{GuardRoundsBuilder.TuningPath}: an empty line");
            Assert.That(tuning.pauseSeconds.x, Is.GreaterThan(0f).And.LessThanOrEqualTo(tuning.pauseSeconds.y));
            Assert.Greater(tuning.walkSpeed, 0f);
        }

        [Test]
        public void GuardFigure_HasIdleAndWalk()
        {
            string path = TravelerPrefabBuilder.ControllerFolder + "AC_NPC_Guard.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) Assert.Ignore(path + " is not built yet (GameOff/Build Travelers)");
            var states = controller.layers[0].stateMachine.states.Select(s => s.state.name).ToList();
            CollectionAssert.Contains(states, "Idle");
            CollectionAssert.Contains(states, "Walk");
        }

        [Test]
        public void OnDuty_OnlyFromTheShiftToTheEndOfTheEvening()
        {
            foreach (DayManager.Phase phase in System.Enum.GetValues(typeof(DayManager.Phase)))
                Assert.IsFalse(GuardRounds.OnDuty(0, phase), $"menu ({phase}): no guards");
            Assert.IsTrue(GuardRounds.OnDuty(1, DayManager.Phase.Shift));
            Assert.IsTrue(GuardRounds.OnDuty(3, DayManager.Phase.Evening), "the evening, curfew too");
            Assert.IsFalse(GuardRounds.OnDuty(1, DayManager.Phase.Morning));
            Assert.IsFalse(GuardRounds.OnDuty(2, DayManager.Phase.Night));
            Assert.IsFalse(GuardRounds.OnDuty(2, DayManager.Phase.Finished));
        }

        [Test]
        public void Pause_TwoToFiveSeconds_LookingBothWaysAndBack()
        {
            var range = new Vector2(2f, 5f);
            Assert.AreEqual(2f, GuardRounds.PauseSeconds(range, 0f), 1e-4f);
            Assert.AreEqual(5f, GuardRounds.PauseSeconds(range, 1f), 1e-4f);
            Assert.AreEqual(5f, GuardRounds.PauseSeconds(range, 7f), 1e-4f, "clamped");

            Assert.AreEqual(90f, GuardRounds.LookYaw(90f, 45f, 0f, 4f), 1e-3f, "starts at the waypoint's facing");
            Assert.AreEqual(135f, GuardRounds.LookYaw(90f, 45f, 1f, 4f), 1e-3f, "one side");
            Assert.AreEqual(45f, GuardRounds.LookYaw(90f, 45f, 3f, 4f), 1e-3f, "the other side");
            Assert.AreEqual(90f, GuardRounds.LookYaw(90f, 45f, 4f, 4f), 1e-3f, "back when he walks on");
            Assert.AreEqual(90f, GuardRounds.LookYaw(90f, 45f, 9f, 4f), 1e-3f, "held after the pause");
            Assert.AreEqual(90f, GuardRounds.LookYaw(90f, 45f, 1f, 0f), 1e-3f, "no pause, no looking");
            for (float t = 0f; t <= 4f; t += 0.1f)
                Assert.That(GuardRounds.LookYaw(90f, 45f, t, 4f), Is.InRange(45f - 1e-3f, 135f + 1e-3f));
        }

        [Test]
        public void PickLine_CurfewPoolAfter23_FallsBack_NeverOutOfRange()
        {
            var lines = new[] { "a", "b" };
            var curfew = new[] { "c" };
            Assert.AreEqual("a", GuardRounds.PickLine(lines, curfew, false, 0f));
            Assert.AreEqual("b", GuardRounds.PickLine(lines, curfew, false, 1f), "random 1.0 is the last one");
            Assert.AreEqual("c", GuardRounds.PickLine(lines, curfew, true, 0.7f));
            Assert.AreEqual("b", GuardRounds.PickLine(lines, new string[0], true, 0.9f), "no curfew lines: a normal one");
            Assert.AreEqual("a", GuardRounds.PickLine(lines, null, true, 0f));
            Assert.IsNull(GuardRounds.PickLine(new string[0], null, false, 0.5f));
            Assert.IsNull(GuardRounds.PickLine(null, null, true, 0.5f));
        }

        [Test]
        public void Blocks_OnlyAPlayerCloseInFront()
        {
            var guard = new Vector3(0f, 0f, 0f);
            var north = Vector3.forward;
            Assert.IsTrue(GuardRounds.Blocks(guard, north, new Vector3(0f, 0f, 1f), 1.2f), "right in front");
            Assert.IsTrue(GuardRounds.Blocks(guard, north, new Vector3(0.5f, 0.9f, 1f), 1.2f), "a little to the side, eye height");
            Assert.IsFalse(GuardRounds.Blocks(guard, north, new Vector3(0f, 0f, 2f), 1.2f), "further away");
            Assert.IsFalse(GuardRounds.Blocks(guard, north, new Vector3(0f, 0f, -1f), 1.2f), "behind him");
            Assert.IsFalse(GuardRounds.Blocks(guard, north, new Vector3(1f, 0f, 0f), 1.2f), "beside him");
            Assert.IsFalse(GuardRounds.Blocks(guard, north, new Vector3(0f, 3f, 1f), 1.2f), "on another floor");
            Assert.IsTrue(GuardRounds.Blocks(guard, north, guard, 1.2f), "on top of each other");
        }

        // ---------- Helpers ----------

        /// <summary>
        /// Every ship model that can stand on every pad (HangarShips in PF_Economy, P107): its box as HangarShips fits it (all its
        /// meshes, in the ship's own space) and where that space is when the ship stands on the pad.
        /// </summary>
        private static List<Ship> ShipFootprints()
        {
            var economy = AssetDatabase.LoadAssetAtPath<GameObject>(EconomyPrefab);
            if (economy == null) Assert.Ignore(EconomyPrefab + " is not built yet");
            var hangar = economy.GetComponentInChildren<HangarShips>(true);
            var result = new List<Ship>();
            if (hangar == null) return result;
            var so = new SerializedObject(hangar);
            var pads = Objects<Transform>(so.FindProperty("pads"));
            foreach (var model in Objects<GameObject>(so.FindProperty("ships")))
            {
                if (!ShipBox(model, out var box)) continue;
                var scale = model.transform.localScale; // Instantiate keeps the model's own scale, the pad gives position and rotation
                foreach (var pad in pads)
                    result.Add(new Ship { Name = $"{model.name} on the pad at {pad.position}", Box = box, Scale = scale,
                        ToShip = Matrix4x4.TRS(pad.position, pad.rotation, scale).inverse });
            }
            return result;
        }

        private class Ship
        {
            public string Name;
            public Bounds Box;
            public Vector3 Scale;
            public Matrix4x4 ToShip;

            /// <summary>Distance on the floor from the point to the ship's footprint (m); 0 under it.</summary>
            public float Distance(Vector3 point)
            {
                var local = ToShip.MultiplyPoint3x4(point);
                float dx = Mathf.Max(0f, Mathf.Abs(local.x - Box.center.x) - Box.extents.x) * Mathf.Abs(Scale.x);
                float dz = Mathf.Max(0f, Mathf.Abs(local.z - Box.center.z) - Box.extents.z) * Mathf.Abs(Scale.z);
                return Mathf.Sqrt(dx * dx + dz * dz);
            }
        }

        /// <summary>Like HangarShips.FitCollider: one box around all of the model's meshes, in the model's space.</summary>
        private static bool ShipBox(GameObject model, out Bounds bounds)
        {
            var toLocal = model.transform.worldToLocalMatrix;
            bounds = new Bounds();
            bool any = false;
            foreach (var filter in model.GetComponentsInChildren<MeshFilter>(true))
            {
                if (filter.sharedMesh == null) continue;
                var b = filter.sharedMesh.bounds;
                var matrix = toLocal * filter.transform.localToWorldMatrix;
                for (int c = 0; c < 8; c++)
                {
                    var corner = matrix.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents, new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f)));
                    if (any) bounds.Encapsulate(corner);
                    else { bounds = new Bounds(corner, Vector3.zero); any = true; }
                }
            }
            return any;
        }

        private static List<T> Objects<T>(SerializedProperty list) where T : Object
        {
            var result = new List<T>();
            for (int i = 0; list != null && i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue is T o) result.Add(o);
            return result;
        }

        /// <summary>Points every <paramref name="step"/> metres along a path, its corners included.</summary>
        private static IEnumerable<Vector3> Along(Vector3[] corners, float step)
        {
            for (int c = 1; c < corners.Length; c++)
            {
                float length = Vector3.Distance(corners[c - 1], corners[c]);
                for (float s = 0f; s < length; s += step) yield return Vector3.Lerp(corners[c - 1], corners[c], s / length);
            }
            if (corners.Length > 0) yield return corners[corners.Length - 1];
        }

        /// <summary>Inside the room's floor plan, at least <paramref name="inset"/> in from its walls (negative: also a little outside).</summary>
        private static bool Inside(Bounds room, Vector3 p, float inset) =>
            p.x >= room.min.x + inset && p.x <= room.max.x - inset && p.z >= room.min.z + inset && p.z <= room.max.z - inset;

        private static List<BoxCollider> Boxes(params string[] paths)
        {
            if (paths.Length == 0) paths = new[] { StationPrefab, EconomyPrefab };
            var boxes = new List<BoxCollider>();
            foreach (string path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) Assert.Ignore(path + " is not built yet");
                foreach (var box in prefab.GetComponentsInChildren<BoxCollider>(true))
                    if (box.enabled && !box.isTrigger && Active(box.transform)) boxes.Add(box);
            }
            return boxes;
        }

        private static Vector2 Flat(Vector3 v) => new Vector2(v.x, v.z);

        private static bool Active(Transform t)
        {
            for (; t != null; t = t.parent) if (!t.gameObject.activeSelf) return false;
            return true;
        }

        /// <summary>A floor slab whose top is at the point's height and whose footprint holds the point.</summary>
        private static bool IsFloorUnder(BoxCollider box, Vector3 point)
        {
            var local = box.transform.InverseTransformPoint(point) - box.center;
            if (Mathf.Abs(local.x) > box.size.x * 0.5f || Mathf.Abs(local.z) > box.size.z * 0.5f) return false;
            float top = box.transform.TransformPoint(box.center + Vector3.up * box.size.y * 0.5f).y;
            return Mathf.Abs(top - point.y) < 0.02f;
        }

        /// <summary>Distance from a point to the (rotated, scaled) box; 0 inside.</summary>
        private static float Distance(BoxCollider box, Vector3 point)
        {
            var m = box.transform.localToWorldMatrix;
            var centre = m.MultiplyPoint3x4(box.center);
            var offset = point - centre;
            var closest = centre;
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 direction = m.GetColumn(axis);
                float scale = direction.magnitude;
                if (scale < 1e-6f) continue;
                direction /= scale;
                float half = box.size[axis] * 0.5f * scale;
                closest += direction * Mathf.Clamp(Vector3.Dot(offset, direction), -half, half);
            }
            return Vector3.Distance(point, closest);
        }

        private static string Name(Transform t)
        {
            string name = t.name;
            for (t = t.parent; t != null; t = t.parent) name = t.name + "/" + name;
            return name;
        }
    }
}
