using System.Collections.Generic;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P131: the family's stock at home – every night uses one item per due need, the rest stays for later nights; perishable
    /// leftovers spoil; items for no need just arrive; medicine waits until someone is sick; one item covers one need; customs only
    /// sees the new shipment.
    /// </summary>
    public class HomeStockTests
    {
        private readonly List<Object> _created = new List<Object>();
        private ItemTag _food, _power, _medicine, _perishable, _contraband, _confiscated;
        private ItemData _battery, _ration, _fruit, _antibiotics, _spice, _seizedBattery;

        [SetUp]
        public void SetUp()
        {
            _food = Make<ItemTag>();
            _power = Make<ItemTag>();
            _medicine = Make<ItemTag>();
            _perishable = Make<ItemTag>();
            _contraband = Make<ItemTag>();
            _confiscated = Make<ItemTag>();
            _battery = Item("Battery pack", _power);
            _ration = Item("Ration pack", _food);
            _fruit = Item("Fresh fruit", _food, _perishable);
            _antibiotics = Item("Antibiotics", _medicine);
            _spice = Item("Spice", _contraband);
            _seizedBattery = Item("Confiscated battery pack", _confiscated, _power);
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

        private const int Food = (int)FamilyNeed.Food, Heat = (int)FamilyNeed.Heat, Medicine = (int)FamilyNeed.Medicine;

        /// <summary>Tonight's tags in need order like the settlement: medicine only while someone is sick.</summary>
        private ItemTag[] Needs(bool sick) => new[] { _food, _power, sick ? _medicine : null };

        /// <summary>The tags of all needs, due or not.</summary>
        private ItemTag[] AllNeeds => new[] { _food, _power, _medicine };

        /// <summary>One night like SettlementWindow.Book: the covering items are used up, the rest goes through the night.</summary>
        private ItemData[] Night(HomeDelivery home, bool sick = false)
        {
            var used = home.Covering(Needs(sick), _perishable);
            home.Night(used, AllNeeds, _perishable);
            return used;
        }

        [Test]
        public void ThreeBatteryPacks_HeatThreeNights_TheFourthCostsAgain()
        {
            var home = new HomeDelivery();
            for (int i = 0; i < 3; i++) home.Add(_battery);
            var family = new Household(new[] { false, true, false }); // the second one feels the cold
            for (int night = 1; night <= 3; night++)
            {
                var used = Night(home);
                Assert.AreSame(_battery, used[Heat], $"night {night}: power from home, the bed asks no money for it");
                Assert.AreEqual(3 - night, home.Count(_battery), $"night {night}: one battery pack per night");
                family.Settle(true, used[Heat] != null, false);
            }
            Assert.AreEqual(Household.Well, family.Condition(1), "the one who feels the cold stayed warm");
            Assert.IsNull(Night(home)[Heat], "the fourth night costs money again");
        }

        [Test]
        public void ANight_UsesOnePerNeed_TheRestStays()
        {
            var home = new HomeDelivery();
            home.Add(_ration);
            home.Add(_ration);
            home.Add(_battery);
            home.AddGift(_antibiotics);
            home.Add(_spice);
            Assert.AreEqual("Ration pack ×2, Battery pack ×1, Antibiotics ×1, Spice ×1", home.Summary());
            var used = Night(home);
            CollectionAssert.AreEqual(new[] { _ration, _battery, null }, used);
            CollectionAssert.AreEqual(new[] { _ration, _antibiotics }, home.Items, "one ration and the medicine stay; the spice just arrived");
            Assert.AreEqual("Ration pack ×1, Antibiotics ×1", home.Summary());
        }

        [Test]
        public void Medicine_StaysAtHome_WhileNobodyIsSick()
        {
            var home = new HomeDelivery();
            home.Add(_antibiotics);
            Assert.IsNull(Night(home, sick: false)[Medicine]);
            Assert.AreEqual(1, home.Count(_antibiotics), "not used up while nobody is sick");
            Assert.AreSame(_antibiotics, Night(home, sick: true)[Medicine]);
            Assert.AreEqual(0, home.Count(_antibiotics));
        }

        [Test]
        public void Perishable_IsEatenFirst_AndTheRestSpoilsAtHome()
        {
            var home = new HomeDelivery();
            home.Add(_ration);
            home.Add(_fruit);
            home.Add(_fruit);
            var used = home.Covering(Needs(false), _perishable);
            Assert.AreSame(_fruit, used[Food], "fresh food goes first, it would spoil anyway");
            var spoiled = home.Night(used, AllNeeds, _perishable);
            CollectionAssert.AreEqual(new[] { _fruit }, spoiled);
            CollectionAssert.AreEqual(new[] { _ration }, home.Items, "the ration pack keeps");
            Assert.AreEqual("Fresh fruit ×1", HomeDelivery.Summary(spoiled));
            Assert.IsEmpty(home.Spoil(null), "without a perishable tag nothing spoils");
        }

        [Test]
        public void OneItem_CoversOnlyOneNeed()
        {
            var kit = Item("Field kit", _food, _medicine);
            var home = new HomeDelivery();
            home.Add(kit);
            var used = home.Covering(Needs(true), _perishable);
            Assert.AreSame(kit, used[Food]);
            Assert.IsNull(used[Medicine], "the same kit can't heal as well");

            home.Add(_ration);
            used = home.Covering(Needs(true), _perishable);
            Assert.AreSame(_ration, used[Food], "the plain ration feeds them …");
            Assert.AreSame(kit, used[Medicine], "… so the kit heals");
            home.UseUp(used);
            Assert.IsEmpty(home.Items);
        }

        [Test]
        public void Customs_InspectsOnlyTheNewShipment_NotTheStock()
        {
            var tuning = Make<ConfiscationTuning>();
            tuning.contrabandTag = _contraband;
            tuning.confiscatedTag = _confiscated;
            tuning.droneInspectChance = 0.25f;
            tuning.disciplinaryLimit = 3;
            var conduct = new Conduct();
            var home = new HomeDelivery();
            home.Add(_seizedBattery);
            home.Add(_seizedBattery);
            Assert.IsFalse(DroneCustoms.Inspect(home, conduct, tuning, 0.9f).Caught, "first night: the seized battery packs get through");
            Night(home);
            CollectionAssert.AreEqual(new[] { _seizedBattery }, home.Items, "one is used, one stays at home");

            home.Add(_ration);
            Assert.IsFalse(DroneCustoms.Inspect(home, conduct, tuning, 0f).Caught, "only the new ration pack is inspected, not the stock");
            home.Add(_seizedBattery);
            var caught = DroneCustoms.Inspect(home, conduct, tuning, 0f);
            CollectionAssert.AreEqual(new[] { _seizedBattery }, caught.Lost, "only what was sent since");
            Assert.AreEqual(1, home.Count(_seizedBattery), "customs keeps the new one, the one at home stays");
            Assert.AreEqual(1, home.Count(_ration));
            Assert.AreEqual(1, conduct.Disciplinary);
        }

        [Test]
        public void AfterTheNight_NothingWaitsForCustoms()
        {
            var home = new HomeDelivery();
            home.Add(_spice);
            home.Add(_seizedBattery);
            Night(home, sick: false); // without customs
            Assert.IsEmpty(home.TakeUninspected(), "the drone has left");
            Assert.IsEmpty(home.Items, "the battery pack was used, the spice just arrived");
        }

        [Test]
        public void Restore_ReplacesTheStock_NothingUninspected()
        {
            var home = new HomeDelivery();
            home.Add(_spice);
            home.Restore(new[] { _battery, null, _battery });
            CollectionAssert.AreEqual(new[] { _battery, _battery }, home.Items);
            Assert.IsEmpty(home.TakeUninspected());
        }
    }
}
