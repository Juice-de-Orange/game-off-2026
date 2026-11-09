using System.Collections.Generic;
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
    }
}
