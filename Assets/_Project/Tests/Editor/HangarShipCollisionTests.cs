using System.Collections.Generic;
using System.Linq;
using GameOff.EditorTools;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P158 (#49): the hangar ships' collision follows their shape: several boxes inside the model's bounds that fill clearly
    /// less than one box around everything, so you can walk closer to wings and noses. Cheap: a few dozen boxes at most.
    /// </summary>
    public class HangarShipCollisionTests
    {
        [Test]
        public void ShipPrefabs_HaveShapedBoxes_InsideTheModel()
        {
            var paths = AssetDatabase.FindAssets("PF_HangarShip_ t:Prefab", new[] { ShipCollisionBuilder.PrefabFolder.TrimEnd('/') })
                .Select(AssetDatabase.GUIDToAssetPath).ToList();
            if (paths.Count == 0) Assert.Ignore("the hangar ship prefabs are not built yet (Build Economy)");
            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var boxes = prefab.GetComponentsInChildren<BoxCollider>(true);
                Assert.That(boxes.Length, Is.InRange(2, 120), path + ": shaped, but not hundreds of boxes");

                var model = ModelBounds(prefab);
                float boxVolume = 0f;
                foreach (var box in boxes)
                {
                    var b = new Bounds(box.transform.TransformPoint(box.center), box.size);
                    Assert.That(b.min.x, Is.GreaterThanOrEqualTo(model.min.x - 0.06f), path);
                    Assert.That(b.max.x, Is.LessThanOrEqualTo(model.max.x + 0.06f), path);
                    Assert.That(b.min.z, Is.GreaterThanOrEqualTo(model.min.z - 0.06f), path);
                    Assert.That(b.max.z, Is.LessThanOrEqualTo(model.max.z + 0.06f), path);
                    Assert.That(b.min.y, Is.GreaterThanOrEqualTo(model.min.y - 0.01f), path);
                    Assert.That(b.max.y, Is.LessThanOrEqualTo(model.max.y + 0.11f), path);
                    boxVolume += b.size.x * b.size.y * b.size.z;
                }
                float whole = model.size.x * model.size.y * model.size.z;
                Assert.That(boxVolume, Is.LessThan(whole * 0.85f), path + ": tighter than one box around the ship");
            }
        }

        [Test]
        public void Boxes_LeaveTheSpaceUnderAHighWingFree()
        {
            // A body on the floor and a thin wing at head height beside it: the column under the wing stays open.
            var root = new GameObject("ShipProbe");
            try
            {
                Block(root, new Vector3(0f, 1f, 0f), new Vector3(1.2f, 2f, 3f));
                Block(root, new Vector3(2.2f, 2.2f, 0f), new Vector3(3f, 0.1f, 1.2f));
                var boxes = ShipCollisionBuilder.Boxes(root);
                var underWing = new Vector3(2.8f, 1f, 0f);
                var inWing = new Vector3(2.8f, 2.2f, 0f);
                var inBody = new Vector3(0f, 1f, 0f);
                Assert.IsFalse(boxes.Any(b => b.Contains(underWing)), "you can stand under the wing");
                Assert.IsTrue(boxes.Any(b => b.Contains(inWing)), "the wing itself is solid");
                Assert.IsTrue(boxes.Any(b => b.Contains(inBody)), "the body is solid");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Boxes_DontFillTheSpaceUnderASlopedWing()
        {
            // A wing rising from 1.0 m at the body to 2.6 m at the tip (dihedral): the space under the tip stays open.
            var root = new GameObject("ShipProbe");
            try
            {
                Block(root, new Vector3(0f, 1f, 0f), new Vector3(1.2f, 2f, 3f));
                var wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(wing.GetComponent<Collider>());
                wing.transform.SetParent(root.transform, false);
                wing.transform.localPosition = new Vector3(2.5f, 1.8f, 0f);
                wing.transform.localRotation = Quaternion.Euler(0f, 0f, 21.8f); // rises 1.6 m over 4 m
                wing.transform.localScale = new Vector3(4.3f, 0.08f, 1.2f);
                var boxes = ShipCollisionBuilder.Boxes(root);
                Assert.IsFalse(boxes.Any(b => b.Contains(new Vector3(4f, 1.2f, 0f))), "under the outer wing: free");
                Assert.IsTrue(boxes.Any(b => b.Contains(new Vector3(4f, 2.4f, 0f))), "the outer wing itself is solid");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static void Block(GameObject parent, Vector3 centre, Vector3 size)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Object.DestroyImmediate(cube.GetComponent<Collider>());
            cube.transform.SetParent(parent.transform, false);
            cube.transform.localPosition = centre;
            cube.transform.localScale = size;
        }

        private static Bounds ModelBounds(GameObject prefab)
        {
            var renderers = prefab.GetComponentsInChildren<MeshFilter>(true).Where(f => f.sharedMesh != null).ToList();
            var all = new List<Vector3>();
            foreach (var f in renderers)
            {
                var b = f.sharedMesh.bounds;
                for (int c = 0; c < 8; c++)
                    all.Add(f.transform.TransformPoint(b.center + Vector3.Scale(b.extents,
                        new Vector3((c & 1) == 0 ? -1f : 1f, (c & 2) == 0 ? -1f : 1f, (c & 4) == 0 ? -1f : 1f))));
            }
            var bounds = new Bounds(all[0], Vector3.zero);
            foreach (var p in all) bounds.Encapsulate(p);
            return bounds;
        }
    }
}
