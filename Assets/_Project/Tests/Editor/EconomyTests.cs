using System.Collections.Generic;
using GameOff.Economy;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameOff.Tests
{
    /// <summary>
    /// The money/inventory backbone (P46) as tests, so P08, P10, P11 and P13 can build on it safely: wallet rules
    /// (no buying without money, forced payments may go negative, TODAY), inventory stacking, unique items,
    /// vendor prices and refused tags, and all item/stock assets in the project.
    /// </summary>
    public class EconomyTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void CleanUp()
        {
            foreach (var o in _created) Object.DestroyImmediate(o);
            _created.Clear();
        }

        private T Make<T>(params (string field, object value)[] fields) where T : ScriptableObject
        {
            var o = ScriptableObject.CreateInstance<T>();
            _created.Add(o);
            var so = new SerializedObject(o);
            foreach (var (field, value) in fields)
            {
                var p = so.FindProperty(field);
                switch (value)
                {
                    case string s: p.stringValue = s; break;
                    case int i: p.intValue = i; break;
                    case float f: p.floatValue = f; break;
                    case bool b: p.boolValue = b; break;
                    case Object r: p.objectReferenceValue = r; break;
                }
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            return o;
        }

        private static void AddToList(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            p.arraySize++;
            p.GetArrayElementAtIndex(p.arraySize - 1).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------- Wallet ----------

        [Test]
        public void Wallet_StartsWithStartCredits()
        {
            Assert.AreEqual(40, new Wallet(40).Balance);
        }

        [Test]
        public void Wallet_TrySpend_FailsWithoutEnoughMoney_AndChangesNothing()
        {
            var wallet = new Wallet(10);
            Assert.IsFalse(wallet.TrySpend(60, "Scanner"));
            Assert.AreEqual(10, wallet.Balance);
            Assert.AreEqual(0, wallet.Today.Count);
        }

        [Test]
        public void Wallet_TrySpend_PaysAndLogsReason()
        {
            var wallet = new Wallet(40);
            Assert.IsTrue(wallet.TrySpend(8, "Kiosk · Ration pack"));
            Assert.AreEqual(32, wallet.Balance);
            Assert.AreEqual(-8, wallet.Today[0].Amount);
            Assert.AreEqual("Kiosk · Ration pack", wallet.Today[0].Reason);
        }

        [Test]
        public void Wallet_Charge_MayGoBelowZero()
        {
            var wallet = new Wallet(10);
            wallet.Charge(30, "Rent");
            Assert.AreEqual(-20, wallet.Balance);
        }

        [Test]
        public void Wallet_AmountsAreAlwaysSigned_ByMethod()
        {
            var wallet = new Wallet(0);
            wallet.Add(-10, "Wage");      // income stays income even if someone passes a negative number
            wallet.Charge(-5, "Citation"); // a charge stays a charge
            Assert.AreEqual(5, wallet.Balance);
            Assert.AreEqual(10, wallet.Today[0].Amount);
            Assert.AreEqual(-5, wallet.Today[1].Amount);
        }

        [Test]
        public void Wallet_BeginDay_ClearsTodayButKeepsBalance()
        {
            var wallet = new Wallet(40);
            wallet.Add(10, "Wage");
            wallet.BeginDay();
            Assert.AreEqual(0, wallet.Today.Count);
            Assert.AreEqual(50, wallet.Balance);
        }

        [Test]
        public void Wallet_Changed_FiresOnEveryMovement()
        {
            var wallet = new Wallet(40);
            int fired = 0;
            wallet.Changed += () => fired++;
            wallet.Add(5, "a");
            wallet.TrySpend(5, "b");
            wallet.TrySpend(999, "fails");
            Assert.AreEqual(2, fired);
        }

        // ---------- Inventory ----------

        [Test]
        public void Inventory_StacksSameItem_InFirstAddedOrder()
        {
            var cola = Make<ItemData>(("displayName", "Nova Cola"));
            var watch = Make<ItemData>(("displayName", "Pocket watch"));
            var inventory = new Inventory();
            inventory.Add(cola);
            inventory.Add(watch);
            inventory.Add(cola, 2);
            Assert.AreEqual(2, inventory.Items.Count);
            Assert.AreEqual(cola, inventory.Items[0]);
            Assert.AreEqual(3, inventory.Count(cola));
        }

        [Test]
        public void Inventory_Remove_FailsForMissingItems()
        {
            var cola = Make<ItemData>();
            var inventory = new Inventory();
            inventory.Add(cola);
            Assert.IsFalse(inventory.Remove(cola, 2));
            Assert.AreEqual(1, inventory.Count(cola));
            Assert.IsTrue(inventory.Remove(cola));
            Assert.AreEqual(0, inventory.Items.Count);
        }

        [Test]
        public void Inventory_UniqueItem_OnlyOnce()
        {
            var scanner = Make<ItemData>(("unique", true));
            var inventory = new Inventory();
            Assert.IsTrue(inventory.CanAdd(scanner));
            Assert.AreEqual(1, inventory.Add(scanner, 3));
            Assert.IsFalse(inventory.CanAdd(scanner));
            Assert.AreEqual(0, inventory.Add(scanner));
            Assert.AreEqual(1, inventory.Count(scanner));
        }

        // ---------- Vendor prices ----------

        [Test]
        public void VendorStock_PricesUseFactors()
        {
            var watch = Make<ItemData>(("baseValue", 20));
            var stock = Make<VendorStock>(("sellFactor", 1.5f), ("buyFactor", 0.5f));
            Assert.AreEqual(30, stock.PriceFor(watch));
            Assert.AreEqual(10, stock.OfferFor(watch));
        }

        [Test]
        public void VendorStock_RefusesTaggedItems_AndBuysNothingAtFactorZero()
        {
            var contraband = Make<ItemTag>(("displayName", "CONTRABAND"));
            var spice = Make<ItemData>(("baseValue", 12));
            AddToList(spice, "tags", contraband);
            var kiosk = Make<VendorStock>(("buyFactor", 0.5f));
            AddToList(kiosk, "refusedTags", contraband);
            var bar = Make<VendorStock>(("buyFactor", 0f));
            Assert.IsFalse(kiosk.Buys(spice));
            Assert.IsTrue(kiosk.Buys(Make<ItemData>()));
            Assert.IsFalse(bar.Buys(Make<ItemData>()));
        }

        // ---------- Content in the project ----------

        [Test]
        public void AllItemsAndStocksInProject_AreValid()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:ItemData"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.DisplayName), $"{item.name}: no name");
                Assert.GreaterOrEqual(item.BaseValue, 0, $"{item.name}: negative value");
                foreach (var tag in item.Tags) Assert.IsNotNull(tag, $"{item.name}: empty tag slot");
            }
            foreach (var guid in AssetDatabase.FindAssets("t:VendorStock"))
            {
                var stock = AssetDatabase.LoadAssetAtPath<VendorStock>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var item in stock.Items) Assert.IsNotNull(item, $"{stock.name}: empty item slot");
            }
        }
    }
}
