using System.Collections.Generic;
using System.Linq;
using GameOff.EditorTools;
using GameOff.Economy;
using GameOff.Shops;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P32: which model the evidence locker shows for which goods, how they fill the compartments, how models are scaled.</summary>
    public class EvidenceLockerTests
    {
        private readonly List<Object> _created = new List<Object>();
        private ItemTag _weapon, _medicine, _food;
        private GameObject _ownModel, _weaponModel, _pillModel;

        [SetUp]
        public void SetUp()
        {
            _weapon = Make<ItemTag>();
            _medicine = Make<ItemTag>();
            _food = Make<ItemTag>();
            _ownModel = MakeObject("Own");
            _weaponModel = MakeObject("WeaponParts");
            _pillModel = MakeObject("PillBottle");
        }

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private T Make<T>() where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            _created.Add(o);
            return o;
        }

        private GameObject MakeObject(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        private ItemData Item(GameObject model, params ItemTag[] tags)
        {
            var item = Make<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("model").objectReferenceValue = model;
            var list = so.FindProperty("tags");
            list.arraySize = tags.Length;
            for (int i = 0; i < tags.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = tags[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private List<LockerTagModel> TagModels() => new List<LockerTagModel>
        {
            new LockerTagModel(_weapon, _weaponModel),
            new LockerTagModel(_medicine, _pillModel),
        };

        [Test]
        public void ModelFor_OwnModelFirst_ThenFirstMatchingLabel_ElseNullForTheBag()
        {
            Assert.AreSame(_ownModel, EvidenceLockerLayout.ModelFor(Item(_ownModel, _weapon), TagModels()), "own model beats a label");
            Assert.AreSame(_weaponModel, EvidenceLockerLayout.ModelFor(Item(null, _medicine, _weapon), TagModels()),
                "list order decides, not the item's label order");
            Assert.AreSame(_pillModel, EvidenceLockerLayout.ModelFor(Item(null, _medicine), TagModels()));
            Assert.IsNull(EvidenceLockerLayout.ModelFor(Item(null, _food), TagModels()), "no fitting label = evidence bag");
            Assert.IsNull(EvidenceLockerLayout.ModelFor(Item(null), null));
            Assert.IsNull(EvidenceLockerLayout.ModelFor(null, TagModels()));
        }

        [Test]
        public void ModelFor_EntriesWithoutModel_AreSkipped()
        {
            var models = new List<LockerTagModel> { new LockerTagModel(_medicine, null), new LockerTagModel(_medicine, _pillModel) };
            Assert.AreSame(_pillModel, EvidenceLockerLayout.ModelFor(Item(null, _medicine), models));
        }

        [Test]
        public void Place_OneCompartmentPerType_InOrder_CopiesCapped_RestNotShown()
        {
            var a = Item(_ownModel);
            var b = Item(null, _weapon);
            var c = Item(null, _food);
            var goods = new List<(ItemData, int)> { (a, 5), (b, 0), (b, 2), (null, 3), (c, 1) };
            var placed = EvidenceLockerLayout.Place(goods, 2, 3);
            Assert.AreEqual(2, placed.Count, "two compartments only");
            Assert.AreEqual((0, a, 3), (placed[0].Slot, placed[0].Item, placed[0].Copies), "5 of a, capped at 3 models");
            Assert.AreEqual((1, b, 2), (placed[1].Slot, placed[1].Item, placed[1].Copies), "empty entries skipped, slots stay dense");
            Assert.AreEqual(0, EvidenceLockerLayout.Place(null, 6, 3).Count);
            Assert.AreEqual(1, EvidenceLockerLayout.Place(new List<(ItemData, int)> { (a, 4) }, 6, 0)[0].Copies, "at least one model");
        }

        [Test]
        public void Contents_FollowTheLockerInventory()
        {
            var a = Item(_ownModel);
            var b = Item(null, _food);
            var locker = new Inventory();
            locker.Add(a, 2);
            locker.Add(b);
            locker.Add(a);
            var contents = EvidenceLockerLayout.Contents(locker);
            Assert.AreEqual(2, contents.Count);
            Assert.AreEqual((a, 3), contents[0]);
            Assert.AreEqual((b, 1), contents[1]);
            Assert.AreEqual(0, EvidenceLockerLayout.Contents(null).Count);
        }

        [Test]
        public void FitScale_FitsTheBox_CapsUpscaling()
        {
            var box = new Vector3(0.4f, 0.4f, 0.4f);
            Assert.AreEqual(0.5f, EvidenceLockerLayout.FitScale(new Vector3(0.8f, 0.2f, 0.1f), box, 2f), 1e-5f, "too wide: shrunk");
            Assert.AreEqual(1.5f, EvidenceLockerLayout.FitScale(new Vector3(0.1f, 0.1f, 0.1f), box, 1.5f), 1e-5f, "small: grows at most 1.5x");
            Assert.AreEqual(1f, EvidenceLockerLayout.FitScale(Vector3.zero, box, 2f), "no size: unchanged");
        }

        // ---------- P135: the open locker is cut from the closed model ----------

        private const float Front = -0.183f, LeafFront = -0.203f;

        /// <summary>A locker shaped like SM_Counter_Locker: body, top, plinth, label, two leaves with handles and a hasp; UV = (x, y), UV2 = (x + 1, y + 1).</summary>
        private static LockerMeshSplit.Source SyntheticLocker()
        {
            var m = new MeshMaker();
            m.Box(new Vector3(-0.5f, 0.08f, Front), new Vector3(0.5f, 1.76f, 0.248f));
            m.Box(new Vector3(-0.5f, 1.76f, -0.212f), new Vector3(0.5f, 1.8f, 0.248f));
            m.Box(new Vector3(-0.47f, 0f, -0.153f), new Vector3(0.47f, 0.08f, 0.218f));
            m.Box(new Vector3(-0.4f, 1.625f, -0.19f), new Vector3(0.4f, 1.735f, Front), false);
            m.Box(new Vector3(-0.49f, 0.1f, LeafFront), new Vector3(-0.004f, 1.6f, Front), false);
            m.Box(new Vector3(0.004f, 0.1f, LeafFront), new Vector3(0.49f, 1.6f, Front), false);
            m.Box(new Vector3(-0.083f, 0.9f, -0.24f), new Vector3(-0.067f, 1.12f, LeafFront), false);
            m.Box(new Vector3(0.067f, 0.9f, -0.24f), new Vector3(0.083f, 1.12f, LeafFront), false);
            m.Box(new Vector3(-0.045f, 0.975f, -0.208f), new Vector3(0.06f, 1.02f, LeafFront), false);
            return m.Source();
        }

        private static void AreClose(Vector3 expected, Vector3 actual, string message = null)
        {
            Assert.AreEqual(expected.x, actual.x, 1e-4f, message);
            Assert.AreEqual(expected.y, actual.y, 1e-4f, message);
            Assert.AreEqual(expected.z, actual.z, 1e-4f, message);
        }

        [Test]
        public void Split_DoorsWithTheirHandles_HingedAtTheOuterFrontCorners()
        {
            var cut = LockerMeshSplit.Split(SyntheticLocker(), 0.025f);
            Assert.IsNotNull(cut);
            Assert.AreEqual(Front, cut.FrontZ, 1e-4f);
            Assert.AreEqual(0.248f, cut.BackZ, 1e-4f);
            AreClose(new Vector3(-0.49f, 0f, LeafFront), cut.LeftHinge, "left hinge");
            AreClose(new Vector3(0.49f, 0f, LeafFront), cut.RightHinge, "right hinge");
            var left = cut.LeftDoor.Positions.Select(p => p + cut.LeftHinge).ToList();
            var right = cut.RightDoor.Positions.Select(p => p + cut.RightHinge).ToList();
            Assert.IsTrue(left.All(p => p.x < 0f), "the left door keeps to its side");
            Assert.IsTrue(right.All(p => p.x > -0.046f), "the right door keeps to its side (its hasp reaches over the slit)");
            Assert.IsTrue(left.Any(p => Mathf.Approximately(p.z, -0.24f)), "the left handle swings with the left door");
            Assert.IsTrue(right.Any(p => Mathf.Approximately(p.z, -0.208f)), "the hasp goes with the door its middle is on");
            Assert.IsTrue(cut.Shell.Positions.Any(p => Mathf.Approximately(p.y, 1.735f)), "the label above the doors stays on the shell");
            Assert.IsFalse(cut.Shell.Positions.Any(p => Mathf.Approximately(p.z, -0.24f)), "no handle left on the shell");
        }

        [Test]
        public void Split_ReplacesThePanelByAFrame_WithThePanelsUvs()
        {
            var cut = LockerMeshSplit.Split(SyntheticLocker(), 0.025f);
            Assert.IsNotNull(cut);
            var shell = cut.Shell;
            float frameArea = 0f;
            for (int t = 0; t < shell.Triangles.Count; t += 3)
            {
                Vector3 a = shell.Positions[shell.Triangles[t]], b = shell.Positions[shell.Triangles[t + 1]], c = shell.Positions[shell.Triangles[t + 2]];
                var cross = Vector3.Cross(b - a, c - a);
                if (cross.normalized.z > -0.9f || Mathf.Abs(a.z - Front) > 1e-4f) continue;
                frameArea += cross.magnitude * 0.5f;
            }
            // 1.0 x 1.68 panel minus the 0.93 x 1.5 opening, plus the 8 mm slit between the leaves: the panel itself is gone.
            Assert.AreEqual(1.68f - 0.93f * 1.5f + 0.008f * 1.5f, frameArea, 1e-3f);
            Assert.AreEqual(-0.465f, cut.Opening.xMin, 1e-4f);
            Assert.AreEqual(0.465f, cut.Opening.xMax, 1e-4f);
            Assert.AreEqual(0.1f, cut.Opening.yMin, 1e-4f);
            Assert.AreEqual(1.6f, cut.Opening.yMax, 1e-4f);
            for (int i = 0; i < shell.Positions.Count; i++)
            {
                var p = shell.Positions[i];
                if (shell.Normals[i] != Vector3.back || Mathf.Abs(p.z - Front) > 1e-4f) continue;
                Assert.AreEqual(p.x, shell.Uv[i].x, 1e-4f, "frame UV continues the panel's");
                Assert.AreEqual(p.y, shell.Uv[i].y, 1e-4f);
                Assert.AreEqual(p.x + 1f, shell.Uv2[i].x, 1e-4f, "and so does its lightmap UV");
                Assert.AreEqual(p.y + 1f, shell.Uv2[i].y, 1e-4f);
            }
        }

        [Test]
        public void Split_DoorsGetAnInside_LitLikeTheirFront()
        {
            var cut = LockerMeshSplit.Split(SyntheticLocker(), 0.025f);
            Assert.IsNotNull(cut);
            foreach (var (door, hinge) in new[] { (cut.LeftDoor, cut.LeftHinge), (cut.RightDoor, cut.RightHinge) })
            {
                float back = 0f;
                for (int t = 0; t < door.Triangles.Count; t += 3)
                {
                    Vector3 a = door.Positions[door.Triangles[t]], b = door.Positions[door.Triangles[t + 1]], c = door.Positions[door.Triangles[t + 2]];
                    var cross = Vector3.Cross(b - a, c - a);
                    if (cross.normalized.z < 0.9f) continue;
                    back += cross.magnitude * 0.5f;
                    foreach (int k in new[] { door.Triangles[t], door.Triangles[t + 1], door.Triangles[t + 2] })
                    {
                        var p = door.Positions[k] + hinge;
                        Assert.AreEqual(Front, p.z, 1e-4f, "the inside sits where the leaf met the panel");
                        Assert.AreEqual(p.x + 1f, door.Uv2[k].x, 1e-4f, "lightmap of the front");
                        Assert.AreEqual(p.y + 1f, door.Uv2[k].y, 1e-4f, "lightmap of the front");
                    }
                }
                Assert.AreEqual(0.486f * 1.5f, back, 1e-3f, "one inside face over the whole leaf");
            }
        }

        [Test]
        public void Split_SomethingElse_IsRefused()
        {
            var m = new MeshMaker();
            m.Box(new Vector3(-0.5f, 0f, -0.25f), new Vector3(0.5f, 1.8f, 0.25f));
            Assert.IsNull(LockerMeshSplit.Split(m.Source(), 0.025f), "a plain box has no doors");
            Assert.IsNull(LockerMeshSplit.Split(null, 0.025f));
        }

        [Test]
        public void Split_TheDeliveredLocker_FindsBothDoors()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/Counter/SM_Counter_Locker.fbx");
            if (model == null) Assert.Ignore("SM_Counter_Locker.fbx is not imported (Git LFS not pulled?)");
            var cut = LockerMeshSplit.Split(LockerMeshSplit.Read(model), 0.025f);
            Assert.IsNotNull(cut, "two leaves in front of a front panel");
            Assert.AreEqual(-0.49f, cut.LeftHinge.x, 0.02f);
            Assert.AreEqual(0.49f, cut.RightHinge.x, 0.02f);
            Assert.AreEqual(LeafFront, cut.LeftHinge.z, 0.02f);
            Assert.AreEqual(Front, cut.FrontZ, 0.02f);
            Assert.IsTrue(cut.HasUv2, "lightmap UVs, so the open parts can share the closed model's lightmap");
            Assert.Greater(cut.LeftDoor.Triangles.Count, 0);
            Assert.Greater(cut.RightDoor.Triangles.Count, 0);
        }

        /// <summary>Flat-shaded boxes, every face with its own four corners and outward winding.</summary>
        private sealed class MeshMaker
        {
            private readonly List<Vector3> _positions = new List<Vector3>(), _normals = new List<Vector3>();
            private readonly List<Vector2> _uv = new List<Vector2>(), _uv2 = new List<Vector2>();
            private readonly List<int> _triangles = new List<int>();

            public void Box(Vector3 min, Vector3 max, bool back = true)
            {
                var size = max - min;
                Vector3 x = new Vector3(size.x, 0f, 0f), y = new Vector3(0f, size.y, 0f), z = new Vector3(0f, 0f, size.z);
                Face(min, y, z, Vector3.left);
                Face(min + x, y, z, Vector3.right);
                Face(min, x, z, Vector3.down);
                Face(min + y, x, z, Vector3.up);
                Face(min, x, y, Vector3.back);
                if (back) Face(min + z, x, y, Vector3.forward);
            }

            private void Face(Vector3 o, Vector3 u, Vector3 v, Vector3 outward)
            {
                int b = _positions.Count;
                foreach (var p in new[] { o, o + u, o + v, o + u + v })
                {
                    _positions.Add(p);
                    _normals.Add(outward);
                    _uv.Add(new Vector2(p.x, p.y));
                    _uv2.Add(new Vector2(p.x + 1f, p.y + 1f));
                }
                if (Vector3.Dot(Vector3.Cross(u, v), outward) > 0f) _triangles.AddRange(new[] { b, b + 1, b + 2, b + 2, b + 1, b + 3 });
                else _triangles.AddRange(new[] { b, b + 2, b + 1, b + 2, b + 3, b + 1 });
            }

            public LockerMeshSplit.Source Source() =>
                new LockerMeshSplit.Source(_positions.ToArray(), _normals.ToArray(), _uv.ToArray(), _uv2.ToArray(), _triangles.ToArray());
        }
    }
}
