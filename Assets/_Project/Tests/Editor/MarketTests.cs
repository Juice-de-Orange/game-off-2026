using System.Collections.Generic;
using System.Linq;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// The market of P68: label base factors, the day's events (price, amount, buy price, assortment), rounding, the bed price,
    /// and the market events in the project (every event has its texts).
    /// </summary>
    public class MarketTests
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

        private ItemTag Tag(float factor)
        {
            var tag = Make<ItemTag>();
            var so = new SerializedObject(tag);
            so.FindProperty("priceFactor").floatValue = factor;
            so.ApplyModifiedPropertiesWithoutUndo();
            return tag;
        }

        private ItemData Item(int value, params ItemTag[] tags)
        {
            var item = Make<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("baseValue").intValue = value;
            var list = so.FindProperty("tags");
            list.arraySize = tags.Length;
            for (int i = 0; i < tags.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = tags[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private VendorStock Stock(float sell, float buy, ItemData item, int amount)
        {
            var stock = Make<VendorStock>();
            var so = new SerializedObject(stock);
            so.FindProperty("sellFactor").floatValue = sell;
            so.FindProperty("buyFactor").floatValue = buy;
            var entries = so.FindProperty("entries");
            entries.arraySize = 1;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("item").objectReferenceValue = item;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("dailyAmount").intValue = amount;
            so.ApplyModifiedPropertiesWithoutUndo();
            return stock;
        }

        [Test]
        public void LabelFactors_MultiplyForEveryLabel_AndTheVendorComesOnTop()
        {
            var contraband = Tag(1.3f);
            var drug = Tag(1f);
            var spice = Item(12, contraband, drug);
            var dealer = Stock(1.5f, 1.2f, spice, 3);
            Market.Begin(1, null);
            Assert.AreEqual(23, dealer.PriceFor(spice), "12 × 1.3 × 1.5 = 23.4");
            Assert.AreEqual(19, dealer.OfferFor(spice), "12 × 1.3 × 1.2 = 18.7");
        }

        [Test]
        public void Events_ChangePriceBuyPriceAndTheBed_OnlyOnTheirDay()
        {
            var drug = Tag(1f);
            var contraband = Tag(1.3f);
            var spice = Item(12, contraband, drug);
            var dealer = Stock(1.5f, 1.2f, spice, 3);
            var busted = Make<MarketEvent>();
            busted.labels.Add(new LabelEffect { tag = drug, price = 1.8f });
            busted.labels.Add(new LabelEffect { tag = contraband, buy = 1.3f });

            Market.Begin(3, new[] { busted });
            Assert.AreEqual(42, dealer.PriceFor(spice), "12 × 1.3 × 1.8 × 1.5 = 42.1");
            Assert.AreEqual(44, dealer.OfferFor(spice), "12 × 1.3 × 1.8 × 1.2 × 1.3 = 43.8 – the rumour pays");
            Assert.AreEqual(38, HouseholdTuning.MoneyCost(spice, 1.35f), "the bed follows the event: 28.08 × 1.35 = 37.9");
            Market.Begin(4, null);
            Assert.AreEqual(23, dealer.PriceFor(spice));
        }

        [Test]
        public void Amount_IsRoundedUp_AndZeroOnlyWhenAnEventTakesItAway()
        {
            var food = Tag(1f);
            var fruit = Item(14, food);
            var halve = Make<MarketEvent>();
            halve.labels.Add(new LabelEffect { tag = food, amount = 0.5f });
            Market.Begin(2, new[] { halve });
            Assert.AreEqual(1, Market.Amount(fruit, 1));
            Assert.AreEqual(2, Market.Amount(fruit, 3));
            Assert.AreEqual(2, Market.Amount(fruit, 4));
            var none = Make<MarketEvent>();
            none.labels.Add(new LabelEffect { tag = food, amount = 0f });
            Market.Begin(2, new[] { none });
            Assert.AreEqual(0, Market.Amount(fruit, 3));
        }

        [Test]
        public void StockChanges_TakeItemsAwayOrAddThemForTheDay()
        {
            var food = Tag(1f);
            var fruit = Item(14, food);
            var spice = Item(12);
            var kiosk = Stock(1f, 0.5f, fruit, 1);
            var dealer = Stock(1.5f, 1.2f, spice, 3);
            var stew = Item(12, food);
            var e = Make<MarketEvent>();
            e.stock.Add(new StockChange { stock = kiosk, item = fruit, amount = 0 });
            e.stock.Add(new StockChange { stock = kiosk, item = stew, amount = 2 });
            e.stock.Add(new StockChange { stock = dealer, item = spice, amount = 2 });
            Market.Begin(2, new[] { e });

            var k = new VendorSupply(kiosk);
            Assert.AreEqual(0, k.Count(fruit), "missing today");
            Assert.AreEqual(2, k.Count(stew), "on the shelf for the day");
            CollectionAssert.Contains(k.Items.ToList(), stew);
            Assert.AreEqual(5, new VendorSupply(dealer).Count(spice), "3 a day + 2 from the event");
        }

        [Test]
        public void EventItems_LeaveAtTheNextRefill_AndAreNotSoldExtras()
        {
            var spice = Item(12);
            var stew = Item(12);
            var kiosk = Stock(1f, 0.5f, spice, 1);
            var e = Make<MarketEvent>();
            e.stock.Add(new StockChange { stock = kiosk, item = stew, amount = 2 });
            Market.Begin(2, new[] { e });
            var k = new VendorSupply(kiosk);
            Assert.AreEqual(2, k.Count(stew));
            CollectionAssert.IsEmpty(k.Extras, "the event's items were not sold by the player (the save keeps extras, P128)");

            Market.Begin(3, null);
            k.Refill();
            Assert.AreEqual(0, k.Count(stew));
            CollectionAssert.DoesNotContain(k.Items.ToList(), stew, "only on the shelf for the day of the event");
        }

        [Test]
        public void EventItem_SoldBackByThePlayer_OnlyThatOneStays()
        {
            var spice = Item(12);
            var stew = Item(12);
            var kiosk = Stock(1f, 0.5f, spice, 1);
            var e = Make<MarketEvent>();
            e.stock.Add(new StockChange { stock = kiosk, item = stew, amount = 2 });
            Market.Begin(2, new[] { e });
            var k = new VendorSupply(kiosk);
            k.Put(stew);
            Assert.AreEqual(3, k.Count(stew));
            Assert.AreEqual(1, k.SoldCount(stew), "what the save keeps");
            Assert.AreEqual(1, k.Items.Count(i => i == stew), "listed once");
            Assert.IsTrue(k.Take(stew));
            Assert.AreEqual(1, k.SoldCount(stew), "a buyer takes the event's units first");

            Market.Begin(3, null);
            k.Refill();
            Assert.AreEqual(1, k.Count(stew), "the event's units are gone, the sold one stays");
            CollectionAssert.AreEqual(new[] { stew }, k.Extras);
            Assert.AreEqual(1, k.SoldCount(stew));
            Assert.IsTrue(k.Take(stew));
            CollectionAssert.DoesNotContain(k.Items.ToList(), stew, "bought back: gone like any extra");
        }

        [Test]
        public void MarketEventsInProject_HaveTheirTexts()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:MarketEvent"))
            {
                var e = AssetDatabase.LoadAssetAtPath<MarketEvent>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.bulletinLine), $"{e.name}: no bulletin line");
                Assert.IsFalse(string.IsNullOrWhiteSpace(e.announcement), $"{e.name}: no announcement");
                foreach (var l in e.labels) Assert.IsNotNull(l.tag, $"{e.name}: label effect without a tag");
                foreach (var c in e.stock) Assert.IsTrue(c.stock != null && c.item != null, $"{e.name}: assortment change without vendor or item");
            }
        }
    }
}
