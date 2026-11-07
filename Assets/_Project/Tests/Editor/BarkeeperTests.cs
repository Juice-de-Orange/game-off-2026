using System.Collections.Generic;
using System.Linq;
using GameOff.Day;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>P65: the barkeeper's wish (by label, cheapest items first) and the rumour (tomorrow's market events, always true).</summary>
    public class BarkeeperTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
            Market.Begin(1, null);
        }

        private T Make<T>() where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            _created.Add(o);
            return o;
        }

        private ItemData Item(int value, ItemTag tag)
        {
            var item = Make<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("baseValue").intValue = value;
            var tags = so.FindProperty("tags");
            tags.arraySize = 1;
            tags.GetArrayElementAtIndex(0).objectReferenceValue = tag;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        [Test]
        public void Give_NeedsEnough_AndTakesTheCheapestFirst()
        {
            var luxury = Make<ItemTag>();
            var cola = Item(2, luxury);
            var wine = Item(20, luxury);
            var inventory = new Inventory();
            inventory.Add(wine);
            inventory.Add(cola);
            var wish = new BarWish { tag = luxury, amount = 2 };
            Assert.AreEqual(2, BarWishRules.Matching(inventory, luxury));
            Assert.IsTrue(BarWishRules.Give(inventory, wish));
            Assert.AreEqual(0, inventory.Count(cola));
            Assert.AreEqual(0, inventory.Count(wine));

            inventory.Add(wine);
            inventory.Add(cola, 2);
            Assert.IsTrue(BarWishRules.Give(inventory, wish));
            Assert.AreEqual(1, inventory.Count(wine), "the two colas went, the wine stays");

            Assert.IsFalse(BarWishRules.Give(inventory, wish), "only one left");
            Assert.AreEqual(1, inventory.Count(wine), "nothing taken when there isn't enough");
        }

        [Test]
        public void Rumour_IsTomorrowsEvents_OrQuiet()
        {
            var a = Make<MarketEvent>();
            a.rumour = "Border closes.";
            var b = Make<MarketEvent>();
            b.rumour = "Cold front.";
            Assert.AreEqual("Border closes. Cold front.", BarWishRules.Rumour(new[] { a, b }));
            Assert.AreEqual(BarWishRules.Quiet, BarWishRules.Rumour(new MarketEvent[0]));
            Assert.AreEqual(BarWishRules.Quiet, BarWishRules.Rumour(null));
        }

        [Test]
        public void ProjectData_EveryEventHasARumour_AndEveryWishCanBeBought()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MarketEvent"))
            {
                var e = AssetDatabase.LoadAssetAtPath<MarketEvent>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.rumour), $"{e.name}: no rumour");
            }
            var stocks = AssetDatabase.FindAssets("t:VendorStock")
                .Select(g => AssetDatabase.LoadAssetAtPath<VendorStock>(AssetDatabase.GUIDToAssetPath(g))).ToList();
            foreach (var guid in AssetDatabase.FindAssets("t:DaySchedule"))
            {
                var schedule = AssetDatabase.LoadAssetAtPath<DaySchedule>(AssetDatabase.GUIDToAssetPath(guid));
                for (int d = 1; d <= schedule.Count; d++)
                {
                    var wish = schedule.Get(d).barWish;
                    if (wish == null || !wish.Active) continue;
                    Assert.IsTrue(stocks.Any(s => s.Items.Any(i => i != null && i.HasTag(wish.tag))),
                        $"{schedule.name} day {d}: nobody sells anything with {wish.tag.DisplayName}");
                }
            }
        }
    }
}
