using System.Collections.Generic;
using System.Linq;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// P10 and P11 as tests: the family's night rules (food, heating, medicine, recovery), the wage with penalties,
    /// the vendor shelf (buy, sold out, morning top-up, selling to the vendor) and the household asset in the project.
    /// </summary>
    public class SettlementTests
    {
        private readonly List<Object> _created = new List<Object>();

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

        // ---------- Household ----------

        private static Household Family() => new Household(new[] { false, true, false }); // the second one feels the cold

        [Test]
        public void NoFood_EveryoneGetsWorse()
        {
            var h = Family();
            h.Settle(food: false, heat: true, medicine: false);
            for (int i = 0; i < h.Count; i++) Assert.AreEqual(Household.Weak, h.Condition(i));
        }

        [Test]
        public void NoHeating_OnlyTheColdSensitiveGetWorse()
        {
            var h = Family();
            h.Settle(food: true, heat: false, medicine: false);
            Assert.AreEqual(Household.Well, h.Condition(0));
            Assert.AreEqual(Household.Weak, h.Condition(1));
        }

        [Test]
        public void NothingPaid_ColdSensitiveGetsSick_AndItIsCappedAtCritical()
        {
            var h = Family();
            h.Settle(false, false, false);
            Assert.AreEqual(Household.Sick, h.Condition(1));
            Assert.IsTrue(h.AnySick);
            h.Settle(false, false, false);
            h.Settle(false, false, false);
            Assert.AreEqual(Household.Critical, h.Condition(1));
        }

        [Test]
        public void Medicine_HealsTheSick_FoodAndHeat_LetTheWeakRecover()
        {
            var h = Family();
            h.Settle(false, false, false); // 1, 2, 1
            h.Settle(true, true, true);    // weak recover, sick heals one step
            Assert.AreEqual(Household.Well, h.Condition(0));
            Assert.AreEqual(Household.Weak, h.Condition(1));
            h.Settle(true, true, false);
            Assert.AreEqual(Household.Well, h.Condition(1));
        }

        [Test]
        public void Preview_ChangesNothing()
        {
            var h = Family();
            var next = h.Preview(false, false, false);
            Assert.AreEqual(Household.Sick, next[1]);
            Assert.AreEqual(Household.Well, h.Condition(1));
        }

        // ---------- Payroll ----------

        [Test]
        public void Wage_PerTraveler_PenaltyOnlyBeyondFreeWarnings()
        {
            var t = Make<HouseholdTuning>();
            t.wagePerTraveler = 15;
            t.penaltyPerCitation = 10;
            Assert.AreEqual(90, Payroll.Wage(6, t));
            Assert.AreEqual(0, Payroll.Penalty(2, 2, t));
            Assert.AreEqual(20, Payroll.Penalty(4, 2, t));
            Assert.AreEqual(0, Payroll.Wage(-1, t));
        }

        // ---------- Vendor supply ----------

        [Test]
        public void StockEntry_FromDay_HiddenAndEmptyBeforeThatDay()
        {
            var item = Make<ItemData>();
            var stock = StockWith(item, 1);
            var so = new SerializedObject(stock);
            so.FindProperty("entries").GetArrayElementAtIndex(0).FindPropertyRelative("fromDay").intValue = 4;
            so.ApplyModifiedPropertiesWithoutUndo();

            Market.Begin(3, null);
            var supply = new VendorSupply(stock);
            Assert.AreEqual(0, supply.Count(item));
            CollectionAssert.DoesNotContain(supply.Items, item);

            Market.Begin(4, null);
            supply.Refill();
            Assert.AreEqual(1, supply.Count(item));
            CollectionAssert.Contains(supply.Items, item);
            Market.Begin(1, null); // Market is static
        }

        private VendorStock StockWith(ItemData item, int amount)
        {
            var stock = Make<VendorStock>();
            var so = new SerializedObject(stock);
            var entries = so.FindProperty("entries");
            entries.arraySize = 1;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("item").objectReferenceValue = item;
            entries.GetArrayElementAtIndex(0).FindPropertyRelative("dailyAmount").intValue = amount;
            so.ApplyModifiedPropertiesWithoutUndo();
            return stock;
        }

        [Test]
        public void Supply_SellsOut_AndIsToppedUpNextMorning()
        {
            var ration = Make<ItemData>();
            var supply = new VendorSupply(StockWith(ration, 2));
            Assert.AreEqual(2, supply.Count(ration));
            Assert.IsTrue(supply.Take(ration));
            Assert.IsTrue(supply.Take(ration));
            Assert.IsFalse(supply.Take(ration));
            Assert.AreEqual(1, supply.Items.Count(), "sold-out stock items stay listed");
            supply.Refill();
            Assert.AreEqual(2, supply.Count(ration));
        }

        [Test]
        public void Supply_SoldToTheVendor_IsOnTheShelf_AndGoneWhenBoughtBack()
        {
            var ration = Make<ItemData>();
            var watch = Make<ItemData>();
            var supply = new VendorSupply(StockWith(ration, 1));
            supply.Put(watch);
            supply.Put(ration);
            Assert.AreEqual(1, supply.Count(watch));
            Assert.AreEqual(2, supply.Count(ration), "selling may go above the daily amount");
            CollectionAssert.Contains(supply.Items.ToList(), watch);
            Assert.IsTrue(supply.Take(watch));
            CollectionAssert.DoesNotContain(supply.Items.ToList(), watch);
            supply.Refill();
            Assert.AreEqual(1, supply.Count(ration), "every morning the shelf holds exactly the daily amount (P68)");
        }

        // ---------- Drone delivery (P13) ----------

        private ItemData ItemWith(ItemTag tag, int value)
        {
            var item = Make<ItemData>();
            var so = new SerializedObject(item);
            so.FindProperty("baseValue").intValue = value;
            if (tag != null)
            {
                var tags = so.FindProperty("tags");
                tags.arraySize = 1;
                tags.GetArrayElementAtIndex(0).objectReferenceValue = tag;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        [Test]
        public void Delivery_CoversTheNeedWithTheTag_AndClears()
        {
            var food = Make<ItemTag>();
            var power = Make<ItemTag>();
            var ration = ItemWith(food, 8);
            var d = new HomeDelivery();
            d.Add(ItemWith(null, 20));
            d.Add(ration);
            Assert.AreSame(ration, d.CoveringItem(food));
            Assert.IsFalse(d.Covers(power));
            Assert.IsFalse(d.Covers(null));
            d.Clear();
            Assert.IsFalse(d.Covers(food));
        }

        [Test]
        public void MoneyAtTheBed_Is35PercentAboveTheVendorValue()
        {
            Assert.AreEqual(11, HouseholdTuning.MoneyCost(ItemWith(null, 8), 1.35f));
            Assert.AreEqual(15, HouseholdTuning.MoneyCost(ItemWith(null, 11), 1.35f));
            Assert.AreEqual(20, HouseholdTuning.MoneyCost(ItemWith(null, 15), 1.35f));
            Assert.AreEqual(0, HouseholdTuning.MoneyCost(null, 1.35f));
        }

        // ---------- Market price (P70) ----------

        [TearDown]
        public void ResetMarketDay() => GameOff.Economy.Market.Begin(1, null);

        private ItemData Changing(int value, params float[] change)
        {
            var item = ItemWith(null, value);
            var so = new SerializedObject(item);
            var p = so.FindProperty("dailyChange");
            p.arraySize = change.Length;
            for (int i = 0; i < change.Length; i++) p.GetArrayElementAtIndex(i).floatValue = change[i];
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        [Test]
        public void MarketPrice_FollowsTheFixedChangeOfTheDay_AndVendorsAndBedStartFromIt()
        {
            var ration = Changing(8, 0f, 0.1f, 0.25f);
            var kiosk = Make<VendorStock>();
            var so = new SerializedObject(kiosk);
            so.FindProperty("sellFactor").floatValue = 1f;
            so.FindProperty("buyFactor").floatValue = 0.5f;
            so.ApplyModifiedPropertiesWithoutUndo();

            GameOff.Economy.Market.Day = 1;
            Assert.AreEqual(8, kiosk.PriceFor(ration));
            Assert.AreEqual(11, HouseholdTuning.MoneyCost(ration, 1.35f));
            GameOff.Economy.Market.Day = 3;
            Assert.AreEqual(10, kiosk.PriceFor(ration));
            Assert.AreEqual(5, kiosk.OfferFor(ration));
            Assert.AreEqual(14, HouseholdTuning.MoneyCost(ration, 1.35f), "8 × 1.25 × 1.35 = 13.5, rounded once at the end");
            GameOff.Economy.Market.Day = 4;
            Assert.AreEqual(8, kiosk.PriceFor(ration), "days without a value = no change");
        }

        // ---------- Content ----------

        [Test]
        public void HouseholdAndStocksInProject_AreValid()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:HouseholdTuning"))
            {
                var t = AssetDatabase.LoadAssetAtPath<HouseholdTuning>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.Greater(t.members.Count, 0, $"{t.name}: no family");
                Assert.AreEqual(4, t.letterLines.Length, $"{t.name}: one letter line per condition");
                foreach (var m in t.members) Assert.IsFalse(string.IsNullOrWhiteSpace(m.name), $"{t.name}: nameless family member");
                var kiosk = AssetDatabase.LoadAssetAtPath<VendorStock>("Assets/_Project/Data/Economy/Stock_Kiosk_Test.asset");
                foreach (FamilyNeed need in System.Enum.GetValues(typeof(FamilyNeed)))
                {
                    if (kiosk != null)
                        Assert.GreaterOrEqual(kiosk.Items.Count(i => i != null && i.HasTag(t.TagOf(need))), 2, $"kiosk: fewer than two goods for {need}");
                    Assert.IsNotNull(t.TagOf(need), $"{t.name}: {need} has no tag");
                    Assert.IsNotNull(t.PriceItemOf(need), $"{t.name}: {need} has no price item");
                    Assert.IsTrue(t.PriceItemOf(need).HasTag(t.TagOf(need)), $"{t.name}: the {need} price item doesn't carry the {need} tag");
                }
            }
            foreach (var guid in AssetDatabase.FindAssets("t:VendorStock"))
            {
                var stock = AssetDatabase.LoadAssetAtPath<VendorStock>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var e in stock.Entries) Assert.GreaterOrEqual(e.dailyAmount, 1, $"{stock.name}: daily amount below 1");
            }
            var food = AssetDatabase.LoadAssetAtPath<ItemTag>("Assets/_Project/Data/Items/Tags/Tag_FOOD.asset");
            foreach (var guid in AssetDatabase.FindAssets("t:ItemData"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));
                int days = new SerializedObject(item).FindProperty("dailyChange").arraySize;
                Assert.IsTrue(days == 0 || days == 3, $"{item.name}: daily change for {days} days (expected none or 3)");
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.Description), $"{item.name}: no description");
                if (item.name == "Item_NovaCola") Assert.IsFalse(item.HasTag(food), "Nova Cola must not cover the family's food");
            }
        }
    }
}
