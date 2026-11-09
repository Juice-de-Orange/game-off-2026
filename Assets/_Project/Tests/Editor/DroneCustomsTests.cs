using System.Collections.Generic;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P110: customs on the cargo drone – only contraband is lost, +1 disciplinary penalty, gifts never inspected.</summary>
    public class DroneCustomsTests
    {
        private readonly List<Object> _created = new List<Object>();
        private ConfiscationTuning _tuning;
        private ItemTag _contraband, _confiscated, _food;
        private ItemData _spice, _seizedStim, _ration;

        [SetUp]
        public void SetUp()
        {
            _contraband = Make<ItemTag>();
            _confiscated = Make<ItemTag>();
            _food = Make<ItemTag>();
            _tuning = Make<ConfiscationTuning>();
            _tuning.contrabandTag = _contraband;
            _tuning.confiscatedTag = _confiscated;
            _tuning.droneInspectChance = 0.25f;
            _tuning.disciplinaryLimit = 3;
            _spice = Item("Spice", _contraband);
            _seizedStim = Item("Confiscated stimulant vials", _confiscated, _food);
            _ration = Item("Ration pack", _food);
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

        private ItemData Item(string name, params ItemTag[] tags)
        {
            var item = Make<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("displayName").stringValue = name;
            var list = so.FindProperty("tags");
            list.arraySize = tags.Length;
            for (int i = 0; i < tags.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = tags[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        [Test]
        public void Roll_BelowTheChance_IsCaught_AboveIsNot()
        {
            var shipment = new List<ItemData> { _spice };
            Assert.IsTrue(DroneCustoms.Resolve(shipment, _tuning, 0.1f).Caught);
            var safe = DroneCustoms.Resolve(shipment, _tuning, 0.9f);
            Assert.IsFalse(safe.Caught);
            Assert.IsEmpty(safe.Lost);
        }

        [Test]
        public void Caught_LosesOnlyContraband_TheRestArrivesAndCovers()
        {
            var d = new HomeDelivery();
            d.Add(_ration);
            d.Add(_spice);
            d.Add(_seizedStim);
            var conduct = new Conduct();
            var r = DroneCustoms.Inspect(d, conduct, _tuning, 0.1f);
            Assert.IsTrue(r.Caught);
            CollectionAssert.AreEquivalent(new[] { _spice, _seizedStim }, r.Lost, "CONTRABAND and CONFISCATED are kept");
            CollectionAssert.AreEqual(new[] { _ration }, d.Items);
            Assert.AreSame(_ration, d.CoveringItem(_food), "the kept confiscated food no longer covers the need, the ration does");
            Assert.AreEqual(1, conduct.Disciplinary);
        }

        [Test]
        public void NotCaught_EverythingArrives_NoPenalty()
        {
            var d = new HomeDelivery();
            d.Add(_seizedStim);
            var conduct = new Conduct();
            Assert.IsFalse(DroneCustoms.Inspect(d, conduct, _tuning, 0.9f).Caught);
            Assert.AreSame(_seizedStim, d.CoveringItem(_food), "an unnoticed delivery covers the need as before");
            Assert.AreEqual(0, conduct.Disciplinary);
        }

        [Test]
        public void NoContraband_IsNeverCaught()
        {
            var d = new HomeDelivery();
            d.Add(_ration);
            var conduct = new Conduct();
            Assert.IsFalse(DroneCustoms.Inspect(d, conduct, _tuning, 0f).Caught);
            Assert.AreEqual(1, d.Items.Count);
            Assert.AreEqual(0, conduct.Disciplinary);
        }

        [Test]
        public void Gifts_AreNeverInspected()
        {
            var d = new HomeDelivery();
            d.AddGift(_spice);
            var conduct = new Conduct();
            Assert.IsFalse(DroneCustoms.Inspect(d, conduct, _tuning, 0f).Caught);
            CollectionAssert.AreEqual(new[] { _spice }, d.Items);
            Assert.AreEqual(0, conduct.Disciplinary);
        }

        [Test]
        public void AShipment_IsInspectedOnce_OnlyNewItemsLater()
        {
            var d = new HomeDelivery();
            d.Add(_spice);
            var conduct = new Conduct();
            Assert.IsFalse(DroneCustoms.Inspect(d, conduct, _tuning, 0.9f).Caught);
            Assert.IsFalse(DroneCustoms.Inspect(d, conduct, _tuning, 0f).Caught, "RETURN and open again: no second roll");
            Assert.AreEqual(1, d.Items.Count);
            d.Add(_seizedStim);
            var later = DroneCustoms.Inspect(d, conduct, _tuning, 0f);
            CollectionAssert.AreEqual(new[] { _seizedStim }, later.Lost, "only what was sent since");
            CollectionAssert.AreEqual(new[] { _spice }, d.Items);
            d.Clear();
            Assert.IsEmpty(d.TakeUninspected());
        }

        [Test]
        public void Strikes_CountTowardTheDischarge()
        {
            var conduct = new Conduct();
            conduct.AddDisciplinary(_tuning.disciplinaryLimit - 1);
            var d = new HomeDelivery();
            d.Add(_spice);
            Assert.IsFalse(conduct.Discharged(_tuning));
            DroneCustoms.Inspect(d, conduct, _tuning, 0.1f);
            Assert.IsTrue(conduct.Discharged(_tuning));
        }

        [Test]
        public void LoadingHint_OnlyForContraband_AndItemListReads()
        {
            Assert.AreEqual("Customs may inspect this.", DroneCustoms.LoadingHint(_spice, _tuning));
            Assert.AreEqual(_tuning.droneLoadingHint, DroneCustoms.LoadingHint(_seizedStim, _tuning));
            Assert.AreEqual("", DroneCustoms.LoadingHint(_ration, _tuning));
            Assert.AreEqual("", DroneCustoms.LoadingHint(_spice, null));
            Assert.AreEqual("spice", DroneCustoms.ItemList(new[] { _spice }));
            Assert.AreEqual("spice ×2 and ration pack", DroneCustoms.ItemList(new[] { _spice, _ration, _spice }));
            Assert.AreEqual("spice, ration pack and confiscated stimulant vials",
                DroneCustoms.ItemList(new[] { _spice, _ration, _seizedStim }));
        }

        [Test]
        public void ProjectTuning_HasTheChanceAndBothTags()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<ConfiscationTuning>("Assets/_Project/Data/Economy/ConfiscationTuning.asset");
            if (tuning == null) Assert.Ignore("ConfiscationTuning not built yet - run GameOff/Update Economy Data");
            Assert.AreEqual(0.25f, tuning.droneInspectChance, 1e-6f);
            Assert.NotNull(tuning.contrabandTag, "contrabandTag missing - run GameOff/Update Economy Data");
            Assert.NotNull(tuning.confiscatedTag, "confiscatedTag missing - run GameOff/Update Economy Data");
            Assert.AreEqual("CONTRABAND", tuning.contrabandTag.DisplayName);
            Assert.AreEqual("CONFISCATED", tuning.confiscatedTag.DisplayName);
        }
    }
}
