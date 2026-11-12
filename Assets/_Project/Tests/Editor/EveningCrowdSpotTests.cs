using System.Collections.Generic;
using GameOff.EditorTools;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P134: the evening guests' spots: at most six, apart from each other, on a floor, not in a wall or a piece of furniture.</summary>
    public class EveningCrowdSpotTests
    {
        private const string StationPrefab = "Assets/_Project/Prefabs/Station/PF_Station_Graybox.prefab";
        private const string EconomyPrefab = "Assets/_Project/Prefabs/Economy/PF_Economy.prefab";
        private const float BodyRadius = 0.25f; // a figure's torso; arms may touch a table, the body may not stand in it
        private static readonly float[] BodyHeights = { 0.35f, 0.8f, 1.25f, 1.6f };

        // The observation deck as StationGrayboxBuilder builds it: half circle north of (0, 6), radius 5, floor 0.36 m down,
        // steps up to z 6.8, glass and railing from about 4.4 m out.
        private static readonly Vector2 DeckCentre = new Vector2(0f, 6f);
        private const float DeckFloor = -0.36f, DeckStandRadius = 4.3f, DeckStepsEnd = 6.8f;

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
            var boxes = new List<BoxCollider>();
            foreach (string path in new[] { StationPrefab, EconomyPrefab })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) Assert.Ignore(path + " is not built yet");
                foreach (var box in prefab.GetComponentsInChildren<BoxCollider>(true))
                    if (box.enabled && !box.isTrigger && Active(box.transform)) boxes.Add(box);
            }

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
                foreach (float height in BodyHeights)
                    foreach (var box in boxes)
                        if (Distance(box, p + Vector3.up * height) < BodyRadius)
                            Assert.Fail($"spot {i} at {p} stands in {Name(box.transform)}");
            }
        }

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
