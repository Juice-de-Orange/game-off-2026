using System.Collections.Generic;
using System.Linq;
using GameOff.EditorTools;
using GameOff.Player;
using GameOff.Rest;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P134: the evening guests' spots: at most six, apart from each other, on a floor, not in a wall or a piece of furniture.
    /// P145: a sitting spot is on a real seat at its height, the seated body, knees and feet clear of everything else; the player's
    /// place on a hall bench stays free and a player's seat a guest takes gets closed; the travelers have the sit loop; who fits where.
    /// </summary>
    public class EveningCrowdSpotTests
    {
        private const string StationPrefab = "Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab";
        private const string EconomyPrefab = "Assets/_Project/Prefabs/Economy/PF_Economy.prefab";
        private const string RestSpotsPrefab = "Assets/_Project/Prefabs/Rest/PF_RestSpots.prefab";
        private const float BodyRadius = 0.25f; // a figure's torso; arms may touch a table, the body may not stand in it
        private static readonly float[] BodyHeights = { 0.35f, 0.8f, 1.25f, 1.6f };

        // The observation deck as StationGrayboxBuilder builds it: half circle north of (0, 6), radius 5, floor 0.36 m down,
        // steps up to z 6.8, glass and railing from about 4.4 m out.
        private static readonly Vector2 DeckCentre = new Vector2(0f, 6f);
        private const float DeckFloor = -0.36f, DeckStandRadius = 4.3f, DeckStepsEnd = 6.8f;

        // P145: seat surfaces measured on the delivered meshes (top of the shell or cushion over the floor), by collider name.
        private static readonly (string prefix, float low, float high)[] Seats =
        {
            ("SM_Hall_Bench_", 0.42f, 0.46f), ("SM_Bar_Stool_", 0.72f, 0.76f), ("Bench_Deck_", 0.45f, 0.5f),
        };
        // The seated body of A_NPC_Sit (its own size): spine points over the hips, knee and toe joints ahead of them.
        private static readonly float[] TorsoAboveHips = { 0f, 0.3f, 0.55f };
        private const float TorsoRadius = 0.2f, KneeHeight = 0.5f, KneeRadius = 0.06f, ToesReach = 0.65f, ToesHeight = 0.03f;

        [Test]
        public void AtMostSix_AndNeverTwoInOnePlace()
        {
            var spots = EconomyBuilder.CrowdSpots;
            Assert.That(spots.Length, Is.InRange(1, 6));
            for (int i = 0; i < spots.Length; i++)
                for (int j = i + 1; j < spots.Length; j++)
                    Assert.GreaterOrEqual(Flat(spots[i].position - spots[j].position).magnitude, 0.9f, $"spots {i} and {j} overlap");
        }

        [Test]
        public void PoseAt_MissingEntriesStand()
        {
            Assert.AreEqual(CrowdPose.Stand, EveningCrowd.PoseAt(null, 0));
            Assert.AreEqual(CrowdPose.Stand, EveningCrowd.PoseAt(new[] { CrowdPose.Sit }, 1));
            Assert.AreEqual(CrowdPose.Sit, EveningCrowd.PoseAt(new[] { CrowdPose.Stand, CrowdPose.Sit }, 1));
        }

        [Test]
        public void EverySpot_StandsOnAFloor_ClearOfWallsAndFurniture()
        {
            var boxes = Boxes();
            var spots = EconomyBuilder.CrowdSpots;
            for (int i = 0; i < spots.Length; i++)
            {
                var p = spots[i].position;
                if (p.y < -0.1f)
                {
                    Assert.AreEqual(DeckFloor, p.y, 0.01f, $"spot {i} below the hall floor must be on the deck floor");
                    Assert.LessOrEqual(Vector2.Distance(new Vector2(p.x, p.z), DeckCentre), DeckStandRadius, $"spot {i}: inside the deck's glass and railing");
                    Assert.GreaterOrEqual(p.z, DeckStepsEnd, $"spot {i}: past the deck steps");
                }
                else
                {
                    Assert.AreEqual(0f, p.y, 0.01f, $"spot {i} stands on the floor");
                    Assert.IsTrue(boxes.Exists(b => IsFloorUnder(b, p)), $"spot {i} at {p}: no floor under it");
                }
                if (spots[i].pose == CrowdPose.Sit) continue; // seated bodies: SittingSpots_OnARealSeat_BodyKneesAndFeetClear
                foreach (float height in BodyHeights)
                    foreach (var box in boxes)
                        if (Distance(box, p + Vector3.up * height) < BodyRadius)
                            Assert.Fail($"spot {i} at {p} stands in {Name(box.transform)}");
            }
        }

        [Test]
        public void SittingSpots_OnARealSeat_BodyKneesAndFeetClear()
        {
            var boxes = Boxes();
            var spots = EconomyBuilder.CrowdSpots;
            int sitting = 0;
            for (int i = 0; i < spots.Length; i++)
            {
                var (p, yaw, pose, seat, front) = spots[i];
                if (pose != CrowdPose.Sit) continue;
                sitting++;
                var forward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                var seatBox = boxes.FirstOrDefault(b => SeatOf(b) >= 0 && InFootprint(b, p, 0.05f));
                Assert.IsNotNull(seatBox, $"spot {i} at {p}: no seat under the hips");
                var (_, low, high) = Seats[SeatOf(seatBox)];
                Assert.That(seat, Is.InRange(low, high), $"spot {i}: seat height of {seatBox.name}");
                // 'front' is where the seat ends ahead of the hips (small guests slide forward up to there).
                Assert.IsTrue(InFootprint(seatBox, p + forward * (front - 0.03f), 0f), $"spot {i}: {seatBox.name} ends before 'front'");
                Assert.IsFalse(InFootprint(seatBox, p + forward * (front + 0.03f), 0f), $"spot {i}: {seatBox.name} goes on past 'front'");

                // A guest of the clip's own size on this seat: torso, knees and toes clear of everything but the seat itself.
                float lift = EveningCrowd.SeatLift(seat, 1f);
                var root = p + forward * EveningCrowd.SeatForward(front, 1f) + Vector3.up * lift;
                var hips = root + Vector3.up * EveningCrowd.ClipHipsHeight;
                var knee = root + forward * EveningCrowd.ClipKneeReach + Vector3.up * KneeHeight;
                var toes = root + forward * ToesReach + Vector3.up * ToesHeight;
                foreach (var box in boxes)
                {
                    if (box == seatBox) continue;
                    foreach (float h in TorsoAboveHips)
                        if (Distance(box, hips + Vector3.up * h) < TorsoRadius) Assert.Fail($"spot {i}: the seated body is in {Name(box.transform)}");
                    if (Distance(box, knee) < KneeRadius) Assert.Fail($"spot {i}: the knees are in {Name(box.transform)}");
                    if (Distance(box, toes) <= 0f) Assert.Fail($"spot {i}: the feet are in {Name(box.transform)}");
                }
            }
            Assert.That(sitting, Is.InRange(2, 3), "P145: two or three of the spots are seats");
        }

        [Test]
        public void SittingSpots_LeaveThePlayersBenchPlaceFree_AndCloseAPlayerSeatTheyTake()
        {
            var rest = AssetDatabase.LoadAssetAtPath<GameObject>(RestSpotsPrefab);
            if (rest == null) Assert.Ignore(RestSpotsPrefab + " is not built yet");
            var spots = EconomyBuilder.CrowdSpots;
            for (int i = 0; i < spots.Length; i++)
            {
                if (spots[i].pose != CrowdPose.Sit) continue;
                var p = spots[i].position;
                var seat = p + Vector3.up * spots[i].seat;
                foreach (var view in rest.GetComponentsInChildren<SeatView>(true))
                {
                    var so = new SerializedObject(view);
                    var eye = so.FindProperty("eye").objectReferenceValue as Transform;
                    var use = so.FindProperty("interactable").objectReferenceValue as Interactable;
                    var trigger = use != null ? use.GetComponent<BoxCollider>() : null;
                    if (eye == null || trigger == null) continue; // RestSpotBuilder gives every seat both
                    // EveningCrowd finds the triggers around the guest's seat and closes the ones centred on it.
                    bool closes = Distance(trigger, seat) <= 0f && EveningCrowd.TakesSeat(trigger.transform.TransformPoint(trigger.center), seat);
                    bool cameraInGuest = Flat(eye.position - p).magnitude < 0.5f;
                    if (view.GetComponent<HallBench>() != null)
                    {
                        Assert.IsFalse(cameraInGuest, $"spot {i}: sits on the player's place on {view.name}");
                        Assert.IsFalse(closes, $"spot {i}: would close {view.name} for the evening");
                    }
                    else Assert.AreEqual(cameraInGuest, closes, $"spot {i}: {view.name} must be closed exactly when the player would sit in the guest");
                }
            }
        }

        [Test]
        public void TravelerControllers_HaveTheSitLoop()
        {
            var clip = AssetDatabase.LoadAllAssetsAtPath(TravelerPrefabBuilder.SitClipPath).OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__"));
            if (clip == null) Assert.Ignore(TravelerPrefabBuilder.SitClipPath + " is not imported yet");
            Assert.IsTrue(clip.isHumanMotion, "A_NPC_Sit is not humanoid yet: run GameOff/Build Travelers");
            Assert.IsTrue(clip.isLooping, "A_NPC_Sit does not loop: run GameOff/Build Travelers");
            foreach (string species in TravelerPrefabBuilder.Species)
            {
                string path = TravelerPrefabBuilder.ControllerFolder + $"AC_NPC_{species}.controller";
                var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
                Assert.IsNotNull(controller, path);
                var sit = controller.layers[0].stateMachine.states.Select(s => s.state).FirstOrDefault(s => s.name == TravelerPrefabBuilder.SitState);
                Assert.IsNotNull(sit, $"{path}: no Sit state, run GameOff/Build Travelers");
                Assert.AreEqual(clip, sit.motion, $"{path}: Sit plays another clip");
            }
        }

        [Test]
        public void PickSpot_TakesTheFirstFreeSpotThatFits()
        {
            var poses = new[] { CrowdPose.Stand, CrowdPose.Sit, CrowdPose.Sit, CrowdPose.Stand };
            var seats = new[] { 0f, 0.44f, 0.74f, 0f };
            Assert.AreEqual(0, EveningCrowd.PickSpot(poses, seats, new bool[4], 1f), "anyone stands");
            var firstTaken = new[] { true, false, false, false };
            Assert.AreEqual(1, EveningCrowd.PickSpot(poses, seats, firstTaken, 1f), "the clip's size sits on the bench");
            Assert.AreEqual(1, EveningCrowd.PickSpot(poses, seats, firstTaken, 0.56f), "short legs sit anywhere");
            Assert.AreEqual(2, EveningCrowd.PickSpot(poses, seats, firstTaken, 1.23f), "legs too long for the bench: the bar stool");
            Assert.AreEqual(3, EveningCrowd.PickSpot(poses, seats, firstTaken, float.NaN), "who cannot sit skips the seats");
            Assert.AreEqual(-1, EveningCrowd.PickSpot(poses, seats, new[] { true, false, false, true }, float.NaN), "nothing left that fits");
            Assert.AreEqual(-1, EveningCrowd.PickSpot(new[] { CrowdPose.Sit }, null, new bool[1], 1f), "a seat without a height takes nobody");
        }

        [Test]
        public void SeatLift_LiftsOntoHighSeats_NeverIntoTheFloor()
        {
            Assert.AreEqual(0.74f - EveningCrowd.ClipSeatHeight, EveningCrowd.SeatLift(0.74f, 1f), 1e-4f, "bar stool: the feet dangle");
            Assert.AreEqual(0f, EveningCrowd.SeatLift(0.44f, 1f), 1e-4f, "bench: the feet on the floor");
            Assert.AreEqual(0f, EveningCrowd.SeatLift(0.44f, 1.06f), "legs a little long: hovers, the feet stay out of the floor");
            Assert.Greater(EveningCrowd.SeatLift(0.44f, 0.56f), 0.15f, "a small guest's feet dangle on the bench");
        }

        [Test]
        public void SeatForward_SmallGuestsSlideForward_StayingOnTheSeat()
        {
            Assert.AreEqual(0f, EveningCrowd.SeatForward(0.385f, 1f), 1e-4f, "the clip's size: knees just past the edge already");
            float slide = EveningCrowd.SeatForward(0.385f, 0.56f);
            Assert.AreEqual(0.385f + EveningCrowd.KneeOverhang - EveningCrowd.ClipKneeReach * 0.56f, slide, 1e-4f);
            Assert.Less(slide, 0.385f, "the hips stay behind the seat's edge");
        }

        [Test]
        public void TakesSeat_OnlyTheSeatTheGuestIsOn()
        {
            var seat = new Vector3(13.48f, 0.74f, 1.6f);
            Assert.IsTrue(EveningCrowd.TakesSeat(new Vector3(13.6f, 0.375f, 1.6f), seat), "the bar stool under the guest");
            Assert.IsFalse(EveningCrowd.TakesSeat(new Vector3(13.6f, 0.375f, 3.2f), seat), "the next stool");
            Assert.IsFalse(EveningCrowd.TakesSeat(new Vector3(-2.5f, 0.45f, -3.5f), new Vector3(-1.5f, 0.44f, -3.585f)), "a bench with room in the middle");
        }

        private static List<BoxCollider> Boxes()
        {
            var boxes = new List<BoxCollider>();
            foreach (string path in new[] { StationPrefab, EconomyPrefab })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) Assert.Ignore(path + " is not built yet");
                foreach (var box in prefab.GetComponentsInChildren<BoxCollider>(true))
                    if (box.enabled && !box.isTrigger && Active(box.transform)) boxes.Add(box);
            }
            return boxes;
        }

        private static int SeatOf(BoxCollider box) => System.Array.FindIndex(Seats, s => box.name.StartsWith(s.prefix));

        private static Vector2 Flat(Vector3 v) => new Vector2(v.x, v.z);

        private static bool Active(Transform t)
        {
            for (; t != null; t = t.parent) if (!t.gameObject.activeSelf) return false;
            return true;
        }

        /// <summary>A floor slab whose top is at the spot's height and whose footprint holds the spot.</summary>
        private static bool IsFloorUnder(BoxCollider box, Vector3 point)
        {
            var local = box.transform.InverseTransformPoint(point) - box.center;
            if (Mathf.Abs(local.x) > box.size.x * 0.5f || Mathf.Abs(local.z) > box.size.z * 0.5f) return false;
            float top = box.transform.TransformPoint(box.center + Vector3.up * box.size.y * 0.5f).y;
            return Mathf.Abs(top - point.y) < 0.02f;
        }

        /// <summary>The point lies over the box's footprint, at least <paramref name="inset"/> in from its edges (unscaled furniture).</summary>
        private static bool InFootprint(BoxCollider box, Vector3 point, float inset)
        {
            var local = box.transform.InverseTransformPoint(point) - box.center;
            return Mathf.Abs(local.x) <= box.size.x * 0.5f - inset && Mathf.Abs(local.z) <= box.size.z * 0.5f - inset;
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
