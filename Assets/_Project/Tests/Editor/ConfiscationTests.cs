using System.Collections.Generic;
using GameOff.Economy;
using GameOff.Travelers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P08: cargo → items, the three shift-end answers, disciplinary penalties and morale.</summary>
    public class ConfiscationTests
    {
        private readonly List<Object> _created = new List<Object>();
        private ConfiscationTuning _tuning;
        private ItemData _stim, _stimSeized, _spice;

        [SetUp]
        public void SetUp()
        {
            _tuning = Make<ConfiscationTuning>();
            _stimSeized = MakeItem("Confiscated stimulant vials", "", null);
            _stim = MakeItem("Stimulant vials", "STIMULANT", _stimSeized);
            _spice = MakeItem("Spice", "SPICE", null);
            _tuning.goods = new List<ItemData> { _stim, _spice };
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

        private ItemData MakeItem(string name, string word, ItemData variant)
        {
            var item = Make<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("displayName").stringValue = name;
            so.FindProperty("cargoWord").stringValue = word;
            so.FindProperty("confiscatedVariant").objectReferenceValue = variant;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private List<(ItemData, int)> Seized() => new List<(ItemData, int)> { (_stim, 2), (_spice, 1) };

        [Test]
        public void Cargo_ReadsAmountsByWholeWord_CapsThem_AndIgnoresOtherFreight()
        {
            var read = CargoReader.Read("2x SPARE PARTS CRATE\n 6x CHEM STIMULANT VIALS\nSPICE\n1x SPICED TEA", _tuning.goods, 3);
            Assert.AreEqual(2, read.Count);
            Assert.AreEqual((_stim, 3), read[0], "6 vials, capped at 3");
            Assert.AreEqual((_spice, 1), read[1], "no amount = 1; SPICED is not SPICE");
            Assert.AreEqual(6, CargoReader.Read("6x CHEM STIMULANT VIALS", _tuning.goods, 0)[0].count, "0 = no cap");
            Assert.IsEmpty(CargoReader.Read("", _tuning.goods, 3));
        }

        [Test]
        public void NothingToReport_Caught_LosesEverything_PaysAndGetsADisciplinaryPenalty()
        {
            var o = ShiftReport.Resolve(ShiftChoice.NothingToReport, Seized(), null, _tuning, 0.1f);
            Assert.IsTrue(o.Caught);
            Assert.IsEmpty(o.Kept);
            Assert.AreEqual(_tuning.keepAllFine, o.Fine);
            Assert.AreEqual(1, o.Disciplinary);
        }

        [Test]
        public void NothingToReport_NotCaught_KeepsAllAsConfiscatedVariants()
        {
            var o = ShiftReport.Resolve(ShiftChoice.NothingToReport, Seized(), null, _tuning, 0.95f);
            Assert.IsFalse(o.Caught);
            Assert.AreEqual(0, o.Fine);
            CollectionAssert.AreEqual(new[] { (_stimSeized, 2), (_spice, 1) }, o.Kept, "variant where there is one");
        }

        [Test]
        public void MissedThat_KeepsOnePiece_OrOnlyAFine()
        {
            var ok = ShiftReport.Resolve(ShiftChoice.MissedThat, Seized(), _stim, _tuning, 0.9f);
            CollectionAssert.AreEqual(new[] { (_stimSeized, 1) }, ok.Kept);
            Assert.AreEqual(0, ok.Disciplinary);

            var caught = ShiftReport.Resolve(ShiftChoice.MissedThat, Seized(), _stim, _tuning, 0.1f);
            Assert.IsTrue(caught.Caught);
            Assert.IsEmpty(caught.Kept);
            Assert.AreEqual(_tuning.missedFine, caught.Fine);
            Assert.AreEqual(0, caught.Disciplinary, "only a fine, no disciplinary penalty");
        }

        [Test]
        public void AsOrdered_IsSafe_AndHandsEverythingIn()
        {
            var o = ShiftReport.Resolve(ShiftChoice.AsOrdered, Seized(), null, _tuning, 0f);
            Assert.IsFalse(o.Caught);
            Assert.IsEmpty(o.Kept);
            Assert.AreEqual(0, o.Fine);
            Assert.AreEqual(0, o.Disciplinary);
        }

        [Test]
        public void Conduct_DischargesAtTheLimit_AndTheAirlockCostsMoreOnTheInnocent()
        {
            var c = new Conduct();
            c.AddDisciplinary(2);
            Assert.IsFalse(c.Discharged(_tuning));
            c.AddDisciplinary(1);
            Assert.IsTrue(c.Discharged(_tuning));

            var guilty = new Judgement(true, true, false, "", Verdict.Airlock, Verdict.Airlock);
            var innocent = new Judgement(false, true, false, "x", Verdict.Admit, Verdict.Airlock);
            var admitted = new Judgement(true, false, false, "", Verdict.Admit, Verdict.Admit);
            Assert.AreEqual(_tuning.airlockGuilty, c.Judge(Verdict.Airlock, guilty, _tuning));
            Assert.AreEqual(_tuning.airlockInnocent, c.Judge(Verdict.Airlock, innocent, _tuning));
            Assert.AreEqual(0, c.Judge(Verdict.Admit, admitted, _tuning));
            Assert.AreEqual(_tuning.airlockGuilty + _tuning.airlockInnocent, c.Morale);
            Assert.Less(_tuning.airlockInnocent, _tuning.airlockGuilty, "innocents cost more");
        }

        [Test]
        public void ProjectTuning_ListsEveryCargoItem()
        {
            var tuning = AssetDatabase.LoadAssetAtPath<ConfiscationTuning>("Assets/_Project/Data/Economy/ConfiscationTuning.asset");
            Assert.NotNull(tuning, "ConfiscationTuning missing - run GameOff/Update Economy Data");
            foreach (var guid in AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/_Project/Data/Items" }))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));
                if (!string.IsNullOrEmpty(item.CargoWord)) CollectionAssert.Contains(tuning.goods, item, $"{item.name} can't be confiscated");
            }
        }
    }
}
